using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dumper
{
    public class Serializer
    {
        public string Code<T>(T target) where T : class, new()
        {
            var type = target.GetType();
            return "var " + type.Name + " = new " + type.FullName + " { " + Process(target) + " };";
        }

        private string Process<T>(T obj)
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

                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) &&
                    property.PropertyType != typeof(string))
                {
                    var arrayCode = new StringBuilder("new " + property.PropertyType.FullName + " {");
                    var lines = new List<string>();

                    foreach (var array in (IEnumerable) value)
                    {
                        lines.Add(Format(array));
                    }

                    arrayCode.AppendLine(string.Join($",{System.Environment.NewLine}", lines));
                    arrayCode.AppendLine("}");
                    format = arrayCode.ToString();
                }
                else
                {
                    format = Format(value);
                }

                code.Add($"{property.Name} = {format}");
            }

            return string.Join($",{System.Environment.NewLine}", code);
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

            if (obj is string)
            {
                return $"\"{obj}\"";
            }

            if (obj is decimal decimalValue)
            {
                return $"{decimalValue:F}M";
            }

            if (obj is bool)
            {
                return $"{obj.ToString()?.ToLowerInvariant()}";
            }

            if (obj is DateTime dateTime)
            {
                return $"new System.DateTime({dateTime.Year}, {dateTime.Month}, {dateTime.Day}, {dateTime.Hour}, {dateTime.Minute}, {dateTime.Second}, {dateTime.Millisecond}, System.DateTimeKind.{dateTime.Kind})";
            }

            if (obj.GetType().IsEnum)
            {
                return $"{obj.GetType().FullName}.{obj.ToString()}";
            }

            if (obj.GetType().IsValueType)
            {
                return obj.ToString();
            }

            var data = Process(obj);
            return $"new " + obj.GetType().FullName + " {" + data + "}";
        }
    }
}