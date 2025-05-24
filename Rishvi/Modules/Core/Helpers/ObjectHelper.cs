using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rishvi.Modules.Core.Helpers
{
    public static class ObjectHelper
    {
        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            var objects = Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                .Select(type => (T)Activator.CreateInstance(type, constructorArgs))
                .ToList();

            objects.Sort();
            return objects;
        }

        public static string ParseString(this object obj)
        {
            return Convert.ToString(obj);
        }

        public static string ParseString(this object obj, string ifNullThenString)
        {
            return obj.ParseString().Length == 0
                ? ifNullThenString
                : Convert.ToString(obj);
        }
        public static int ParseInt(this object obj)
        {
            int result = 0;
            return int.TryParse(Convert.ToString(obj), out result)
                    ? result
                    : 0;
        }

        public static int ParseInt(this object obj, int DefaultValue)
        {
            int result = 0;
            return int.TryParse(Convert.ToString(obj), out result)
                    ? result
                    : DefaultValue;
        }

        public static long ParseLong(this object obj)
        {
            long result = 0;
            return long.TryParse(Convert.ToString(obj), out result)
                    ? result
                    : 0;
        }

        public static long ParseLong(this object obj, long DefaultValue)
        {
            long result = 0;
            return long.TryParse(Convert.ToString(obj), out result)
                    ? result
                    : DefaultValue;
        }

        public static bool IsDateTime(this object obj)
        {
            if (obj == null)
            {
                return false;
            }

            try
            {
                Convert.ToDateTime(obj);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is Decimal;
        }

        public static bool IsString(this object value)
        {
            return value is string;
        }

        public static DateTime ParseDateTime(this object obj)
        {
            DateTime result;
            return DateTime.TryParse(Convert.ToString(obj), out result)
                    ? result
                    : new DateTime();
        }

        public static string ParseDateTime(this object obj, string Format)
        {
            DateTime result;
            return DateTime.TryParse(Convert.ToString(obj), out result)
                    ? result.ToString(Format)
                    : "";
        }
    }
}
