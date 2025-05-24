using System.Security.Cryptography;
using System.Text;

namespace Rishvi.Modules.Core.Lib
{
    public static class Gravatar
    {
        public static string Get(string email, int size = 30)
        {
            return $"https://www.gravatar.com/avatar/{Hash(email)}?size=30&d=mm";
        }

        private static string Hash(string email)
        {
            var md5Hasher = MD5.Create();

            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(email));

            var sBuilder = new StringBuilder();

            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}