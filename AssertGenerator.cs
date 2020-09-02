using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dumper
{
    public class AssertGenerator
    {
        private readonly string _separator = $"\r\n";

        public string Generate<T>(T actual) where T : class, new()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{nameof(actual)}.Should().NotBeNull();");

            var processed = Process(actual, nameof(actual));
            processed = StripDoubleEmptyLines(processed);
            sb.AppendLine(processed);

            return sb.ToString();
        }

        private string Process(object obj, string path)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            var code = new List<string>();

            List<string> subItems = new List<string>();
            foreach (var property in obj.GetType().GetProperties())
            {
                var value = property.GetValue(obj);

                var propertyType = property.PropertyType;

                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                {
                    var genericList = (IEnumerable)value;
                    if (genericList == null)
                    {
                        code.Add($"{path}.{property.Name}.Should().BeNull();");
                    }
                    else
                    {
                        var firstItemPath = $"{path}.{property.Name}.First()";

                        var firstItem = genericList?.Cast<dynamic>()?.FirstOrDefault();
                        if (firstItem == null)
                        {
                            subItems.Add($"{path}.{property.Name}.Should().BeEmpty();");
                        }
                        else
                        {
                            subItems.Add("\r\n");
                            var propertyNameLower = GenerateVarName(property.Name);
                            var newVar = $"var {propertyNameLower} = {firstItemPath};";
                            subItems.Add(newVar);

                            if (firstItem is string)
                            {
                                subItems.Add($"{propertyNameLower}.Should().Be(\"{firstItem}\");");
                            }
                            else
                            {
                                var arrayItemCode = Process(firstItem, propertyNameLower);
                                subItems.Add(arrayItemCode);
                            }
                        }
                    }
                }
                else
                {
                    if (value == null)
                    {
                        code.Add($"{path}.{property.Name}.Should().BeNull();");
                    }
                    else
                    {
                        var format = Format(value, path);
                        if (format != null)
                        {
                            code.Add($"{path}.{property.Name}.Should().Be({format});");
                        }
                        else
                        {
                            var itemPath = $"{path}.{property.Name}";
                            var addData = new List<string>();
                            if (propertyType == typeof(object))
                            {
                                var objPath = path.Split(".").Last();

                                var propertyNameLower = GenerateVarName($"{objPath}{property.Name}");
                                var newVar = $"var {propertyNameLower} = ({value.GetType().FullName}){itemPath};";

                                addData.Add(newVar);
                                itemPath = propertyNameLower;
                            }

                            var data = Process(value, itemPath);
                            if (!string.IsNullOrEmpty(data))
                            {
                                if (addData.Any())
                                {
                                    code.AddRange(addData);
                                }

                                code.Add($"{itemPath}.Should().NotBeNull();");
                                code.Add(data);
                            }
                        }
                    }
                }
            }

            if (subItems.Any())
            {
                code.AddRange(subItems);
            }

            return string.Join(_separator, code);
        }

        private string Format<T>(T obj, string path)
        {
            if (obj == null)
            {
                return "null";
            }

            switch (obj)
            {
                case string _:
                    return $"\"{obj}\"";
                case char _:
                    return $"'{obj}'";
                case decimal decimalValue:
                    return $"{decimalValue.ToString(CultureInfo.InvariantCulture)}M";
                case bool _:
                    return $"{obj.ToString()?.ToLowerInvariant()}";
                case DateTime dateTime:
                    return $"System.DateTime.Parse(\"{dateTime:s}\")";
                case TimeSpan _:
                    return $"System.TimeSpan.Parse(\"{obj.ToString()}\")";
                case Guid _:
                    return $"System.Guid.Parse(\"{obj.ToString()}\")";
            }

            var type = obj.GetType();

            if (type.IsEnum)
            {
                return $"{type.FullName}.{obj.ToString()}";
            }

            if (type.IsValueType)
            {
                return obj.ToString();
            }

            return null;
        }

        public static string GenerateVarName(string str)
        {
            var name = str.Substring(0, 1).ToLower() + (str.Length > 1 ? str.Substring(1) : "");
            if (name.EndsWith("es"))
            {
                return $"{name.TrimEnd('e', 's')}";
            }
            if (name.EndsWith("s"))
            {
                return $"{name.TrimEnd('s')}";
            }

            return name;
        }
        private string StripDoubleEmptyLines(string processed)
        {
            for (int i = 0; i < 10; i++)
            {
                processed = processed.Replace("\r\n\r\n\r\n", "\r\n\r\n");
            }

            return processed;
        }
    }
}