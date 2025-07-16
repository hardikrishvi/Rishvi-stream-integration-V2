using System.Text.RegularExpressions;

namespace Rishvi.Modules.Core.Extensions
{
    public static class StringExtensions
    {
        public static string NullSafeToLower(this string s)
        {
            s ??= string.Empty;

            return s.ToLower();
        }

        public static string GenerateSlug(this string phrase)
        {
            var str = phrase.RemoveAccent().ToLower();
            // invalid chars
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();

            return Regex.Replace(str, @"\s", "-"); // hyphens
        }

        public static string RemoveAccent(this string txt)
        {
            var bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);

            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string TrimEnd(this string source, string value)
        {
            return !source.EndsWith(value)
                ? source
                : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }

        public static string StripHtml(this string txt)
        {
            return !string.IsNullOrEmpty(txt) ? Regex.Replace(txt, "<(.|\\n)*?>", string.Empty) : string.Empty;
        }

        public static string GenerateOrderNumber(this int orderId, string prefix)
        {
            return orderId > 0 ? prefix + orderId.ToString("000000000") : string.Empty;
        }

        public static string GenerateEmailMask(this string email)
        {
            string pattern = @"(?<=[\w]{2})[\w-\._\+%\\]*(?=[\w]{2}@)|(?<=@[\w]{1})[\w-_\+%]*(?=\.)";

            return !email.Contains("@")
                ? new String('*', email.Length)
                : email.Split('@')[0].Length < 4
                    ? @"*@*.*"
                    : Regex.Replace(email, pattern, m => new string('*', m.Length));
        }
    }
}