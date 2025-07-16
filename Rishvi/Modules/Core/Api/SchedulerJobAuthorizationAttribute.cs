using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Authorization;

namespace Rishvi.Modules.Core.Api
{
    public class SchedulerJobAuthorizationAttribute : IDashboardAuthorizationFilter
    {
        private readonly JwtSetting _jwtSetting;
        private readonly string _permission;

        private static readonly string _hangFireCookieName = "HangFireCookie";
        private static readonly string _permissions = "permissions";

        public SchedulerJobAuthorizationAttribute(JwtSetting jwtSetting, string permission)
        {
            _jwtSetting = jwtSetting;
            _permission = permission;
        }
        public bool Authorize([NotNull] DashboardContext context)
        {
            var request = context.GetHttpContext().Request;

            var token = request.Cookies[_hangFireCookieName];
            if (!string.IsNullOrEmpty(token))
            {
                var key = Encoding.ASCII.GetBytes(_jwtSetting.SecretKey);
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var tokenExpiresAt = jwtToken.ValidTo;
                if (tokenExpiresAt < DateTimeOffset.UtcNow)
                {
                    return false;
                }
                else if (!jwtToken.Claims.Any(claim => claim.Type == _permissions))
                {
                    return false;
                }
                else
                {
                    var jwtPermissions = JsonConvert.DeserializeObject<string[]>(jwtToken.Claims.First(claim => claim.Type == _permissions).Value);
                    return jwtPermissions.Contains(_permission);
                }
            }
            else
            {
                return false;
            }
        }
    }
}
