using Rishvi.Modules.ShippingIntegrations.Models;
using System.Text;

namespace Rishvi.Modules.Core.Helpers
{
    public static class CodeHelper
    {
        public static string MaxLenght(string val, int max)
        {
            if (val.Length > max)
            {
                return val.Substring(0, max);
            }
            else
            {
                return val;
            }
        }

        public static string FormatAddress(GenerateLabelRequest request)
        {
            System.Text.StringBuilder addressOutput = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(request.Name))
                addressOutput.AppendLine(request.Name);
            if (!string.IsNullOrEmpty(request.CompanyName))
                addressOutput.AppendLine(request.CompanyName);
            if (!string.IsNullOrEmpty(request.AddressLine1))
                addressOutput.AppendLine(request.AddressLine1);
            if (!string.IsNullOrEmpty(request.AddressLine2))
                addressOutput.AppendLine(request.AddressLine2);
            if (!string.IsNullOrEmpty(request.Town))
                addressOutput.AppendLine(request.Town);
            if (!string.IsNullOrEmpty(request.Region))
                addressOutput.AppendLine(request.Region);
            if (!string.IsNullOrEmpty(request.Postalcode))
                addressOutput.AppendLine(request.Postalcode.ToUpper());

            return addressOutput.ToString();
        }

        public static string GenerateUniqueCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }
    }
}
