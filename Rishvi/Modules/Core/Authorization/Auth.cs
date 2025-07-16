using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Rishvi.Modules.Core.Authorization
{
    public static class Auth
    {
        public const string AuthHeaderStartWith = "Bearer ";
        public static AuthUser LoggedInUser(this HttpRequest request, JwtSetting _jwtSetting)
        {
            try
            {
                var token = request.GetHeader("Authorization")[AuthHeaderStartWith.Length..];
                return VerifyAndGetLoggedInUser(token, _jwtSetting);
            }
            catch
            {
                return null;
            }
        }
        private static AuthUser VerifyAndGetLoggedInUser(string token, JwtSetting jwtSetting)
        {
            try
            {
                string secret = jwtSetting.SecretKey;
                var key = Encoding.ASCII.GetBytes(jwtSetting.SecretKey);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var user = new AuthUser();

                var dicClaim = jwtToken.Claims.GroupBy(claim => claim.Type) // Desired Key
                .ToDictionary(group => group.Key, group => group.Last());

                var jObject = dicClaim["email"].Value;
                user.Name = dicClaim["name"].Value;
                user.Email = dicClaim["email"].Value;
                user.Id = Convert.ToInt32(dicClaim["id"].Value);

                return user;
            }
            catch (Exception)
            {
                return new AuthUser();
            }
        }
    }

    public static class HttpRequestExtension
    {
        public static string GetHeader(this HttpRequest request, string key)
        {
            return request.Headers.FirstOrDefault(x => x.Key == key).Value.FirstOrDefault();
        }
    }

    public class AuthUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
