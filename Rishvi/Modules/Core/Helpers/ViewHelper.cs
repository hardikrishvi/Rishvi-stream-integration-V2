using System.Text.RegularExpressions;

namespace Rishvi.Modules.Core.Helpers
{
    public static class ViewHelper
    {
        public static string GenerateMetaTitle(string metaTitle, string elseTitle)
        {
            return !string.IsNullOrEmpty(metaTitle) ? metaTitle : elseTitle;
        }

        public static string GenerateMetaDescription(string metaDescription, string elseContent)
        {
            if (!string.IsNullOrEmpty(metaDescription))
            {
                return metaDescription;
            }

            if (!string.IsNullOrWhiteSpace(elseContent))
            {
                var stripHtmlContent = Regex.Replace(elseContent, "<.*?>", string.Empty);
                stripHtmlContent = Regex.Replace(stripHtmlContent, @"\t|\n|\r", "");

                var take300Char = stripHtmlContent;

                if (stripHtmlContent.Length > 300)
                {
                    take300Char = stripHtmlContent.Substring(0, 300);
                }

                return take300Char;
            }

            return string.Empty;
        }
    }
}