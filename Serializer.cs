using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumper
{
    public class Serializer
    {
        private readonly string _separator = $",{Environment.NewLine}";

        public string Code<T>(T target) where T : class, new()
        {
            var type = target.GetType();
            return "var " + type.Name + " = new " + type.FullName + " { " + Process(target) + " };";
        }

        private string Process(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            var code = new List<string>();

            foreach (var property in obj.GetType().GetProperties())
            {
                var value = property.GetValue(obj);

                if (IsDefault(value))
                {
                    continue;
                }

                string format;

                var propertyType = property.PropertyType;
                
                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                {
                    var arrayCode = new StringBuilder("new " + PrepareClassCreating(propertyType) + " {");
                    var lines = new List<string>();

                    foreach (var array in (IEnumerable) value)
                    {
                        lines.Add(Format(array));
                    }

                    arrayCode.AppendLine(string.Join(_separator, lines));
                    arrayCode.AppendLine("}");
                    format = arrayCode.ToString();
                }
                else
                {
                    format = Format(value);
                }

                code.Add($"{property.Name} = {format}");
            }

            return string.Join(_separator, code);
        }

        private static string PrepareClassCreating(Type propertyType)
        {
            if (propertyType.IsGenericType)
            {
                return $"{propertyType.FullName.Split('`')[0]}<" + string.Join(", ", propertyType.GetGenericArguments().Select(a => a.FullName)) + ">";
            }
            
            return propertyType.FullName;
        }

        private bool IsDefault(object value)
        {
            if (value == null)
            {
                return true;
            }

            var type = value.GetType();

            if (type.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(type);
                // ReSharper disable once PossibleNullReferenceException
                return defaultValue.Equals(value);
            }

            return false;
        }

        private string Format<T>(T obj)
        {
            if (obj == null)
            {
                return "null";
            }

            switch (obj)
            {
                case string _:
                    return $"\"{obj}\"";
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

            var data = Process(obj);
            
            return $"new " + PrepareClassCreating(type) + " {" + data + "}";
        }
    }
}
