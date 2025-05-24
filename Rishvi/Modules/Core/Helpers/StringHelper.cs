using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rishvi.Modules.Core.Helpers
{
    public static class StringHelper
    {
        private static readonly Random _random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static string PrepareAddressLine3(string city, string state, string zip)
        {
            var addressLine3 = !string.IsNullOrWhiteSpace(city) ? $"{city}, " : string.Empty;

            addressLine3 += !string.IsNullOrWhiteSpace(state) ? $"{state}, " : string.Empty;

            if (!string.IsNullOrWhiteSpace(zip))
            {
                addressLine3 += $"{zip}, ";
            }

            if (!string.IsNullOrWhiteSpace(addressLine3) && addressLine3.Length > 1)
            {
                addressLine3 = addressLine3[0..^2];
            }

            return addressLine3;
        }

        public static string PrepareAddressLine1(string addressLine1, string addressLine2)
        {
            var addressLine = !string.IsNullOrWhiteSpace(addressLine1) ? $"{addressLine1}, " : string.Empty;

            addressLine += !string.IsNullOrWhiteSpace(addressLine2) ? $"{addressLine2}, " : string.Empty;

            if (!string.IsNullOrWhiteSpace(addressLine) && addressLine.Length > 1)
            {
                addressLine = addressLine[0..^2];
            }

            return addressLine;
        }
        public static int[] ToIntArray(this string Value)
        {
            return Value.ToIntArray(",");
        }

        public static int[] ToIntArray(this string Value, string Separator)
        {
            try
            {
                string[] strArray = Value.Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int[] numArray = new int[strArray.Length];
                for (int index = 0; index < strArray.Length; ++index)
                {
                    numArray[index] = Convert.ToInt32(strArray[index]);
                }

                return numArray;
            }
            catch
            {
                throw new Exception("Invalid string array format.");
            }
        }

        public static bool IsIntArray(this string Value, string Separator)
        {
            try
            {
                string[] strArray = Value.Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int[] numArray = new int[strArray.Length];
                for (int index = 0; index < strArray.Length; ++index)
                {
                    numArray[index] = Convert.ToInt32(strArray[index]);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string CamelCaseToString(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => ((int)m.Value[0]).ToString() + " " + char.ToLower(m.Value[1]));
        }

        public static string CamelCaseSplitByUnderscore(this string input)
        {
            return Regex.Replace(input, "([A-Z])", "_$1", RegexOptions.Compiled).Trim().Trim('_');
        }

        public static bool IsNullOrWhiteSpace(this string Value)
        {
            return string.IsNullOrEmpty(Value) || Value.Trim().Length == 0;
        }

        public static bool IsEmail(this string value)
        {
            string RegExPattern = "^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}$";
            return value.IsEmail(RegExPattern);
        }

        public static bool IsEmail(this string value, string RegExPattern)
        {
            return Regex.Match(value.Trim(), RegExPattern, RegexOptions.IgnoreCase).Success;
        }

        public static bool IsUrl(this string value)
        {
            string RegExPattern = "^(?:http:[/][/]|https:[/][/])?([a-zA-Z0-9\\.\\-]+(\\:[a-zA-Z0-9\\.&amp;%\\$\\-]+)*@)?((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|([a-zA-Z0-9\\-]+\\.)*[a-zA-Z0-9\\-]+\\.[a-zA-Z]{2,4})(\\:[0-9]+)?(/[^/][a-zA-Z0-9\\.\\,\\?\\'\\\\/\\+&amp;%\\$#\\=~_\\-@]*)*$";
            return value.IsUrl(RegExPattern);
        }

        public static bool IsUrl(this string value, string RegExPattern)
        {
            return Regex.Match(value.Trim(), RegExPattern).Success;
        }

        public static bool IsIn(this string[] values, string inValue)
        {
            bool flag = false;
            foreach (string str in values)
            {
                if (str.Equals(inValue))
                {
                    return true;
                }
            }

            return flag;
        }

        public static bool IsIn(this string value, params string[] inValues)
        {
            bool flag = false;
            foreach (string inValue in inValues)
            {
                if (inValue.Equals(value))
                {
                    return true;
                }
            }

            return flag;
        }

        public static bool Contains(this string value, params string[] inValues)
        {
            bool flag = false;
            foreach (string inValue in inValues)
            {
                if (value.Contains(inValue))
                {
                    return true;
                }
            }

            return flag;
        }

        public static bool IsBoolean(this string value)
        {
            bool result = false;
            return bool.TryParse(value, out result);
        }

        public static string TrimStart(this string value, string strData)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            try
            {
                return value.StartsWith(strData)
                        ? value[strData.Length..]
                        : value;
            }
            catch
            {
            }

            return value;
        }

        public static string TrimEnd(this string value, params string[] strDatas)
        {
            string str = value;
            if (value.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            try
            {
                foreach (string strData in strDatas)
                {
                    if (value.EndsWith(strData) && value.IndexOf(strData, value.Length - strData.Length) > 0)
                    {
                        str = str.Substring(0, str.Length - strData.Length);
                    }
                }
            }
            catch
            {
            }

            return str;
        }

        public static bool IsValidChars(
          this string inputValue,
          bool Letters,
          bool Numbers,
          string Symbol)
        {
            bool flag = false;
            StringBuilder stringBuilder = new StringBuilder("");
            stringBuilder.Append("^[");
            if (!Letters && !Numbers && Symbol == "")
            {
                return false;
            }

            if (Letters)
            {
                stringBuilder.Append("a-zA-Z");
            }

            if (Numbers)
            {
                stringBuilder.Append("0-9");
            }

            if (Symbol != "")
            {
                Symbol = Symbol.Replace("\\", "\\\\");
                Symbol = Symbol.Replace(" ", "\\ ");
                Symbol = Symbol.Replace(".", "\\.");
                Symbol = Symbol.Replace("(", "\\(");
                Symbol = Symbol.Replace(")", "\\)");
                Symbol = Symbol.Replace("[", "\\[");
                Symbol = Symbol.Replace("]", "\\]");
                stringBuilder.Append(Symbol);
            }

            if (new Regex(stringBuilder.ToString() + "]+$").IsMatch(inputValue))
            {
                flag = true;
            }

            return flag;
        }

        public static string Remove(this string value, string strValue)
        {
            return value.Replace(strValue, string.Empty);
        }

        public static string RemoveHTML(this string value)
        {
            return Regex.Replace(value, "<[^>]*>", string.Empty);
        }

        public static string RemoveHTML(this string value, string onlyAllowTags)
        {
            string pattern = "</?(?(?=" + onlyAllowTags + ")notag|[a-zA-Z0-9]+)(?:\\s[a-zA-Z0-9\\-]+=?(?:([\"']?).*?\\1?)?)*\\s*/?>";
            return Regex.Replace(value, pattern, "");
        }

        public static string TakeChars(this string value, int Length)
        {
            return value.Length <= Length
                    ? value
                    : value.Substring(0, Length);
        }

        public static string TakeChars(this string value, int Length, string ifExtraThenAddChars)
        {
            return value.Length <= Length
                    ? value
                    : value.Substring(0, Length) + ifExtraThenAddChars;
        }
    }
}
