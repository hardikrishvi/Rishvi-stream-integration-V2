using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Rishvi.Modules.Core.Authorization;

namespace Rishvi.Modules.Core.Api
{
    public class AuthorizeApiAdminUserAttribute : TypeFilterAttribute
    {
        public AuthorizeApiAdminUserAttribute(params string[] permissions) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] { permissions };
        }

        private class ClaimRequirementFilter : IActionFilter
        {
            private readonly string[] _permissions;
            private readonly JwtSetting _jwtSetting;
            private string[] _jwtPermissions;
            private int _count;
            public ClaimRequirementFilter(IOptions<JwtSetting> jwtSetting, string[] permissions)
            {
                _permissions = permissions;
                _jwtSetting = jwtSetting.Value;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                try
                {
                    var request = context.HttpContext.Request;
                    var authorizationHeader = Convert.ToString(request.Headers["Authorization"]);
                    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith(Auth.AuthHeaderStartWith))
                    {
                        var token = authorizationHeader[Auth.AuthHeaderStartWith.Length..];
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
                        var tokenExpiresAt = validatedToken.ValidTo;
                        if (tokenExpiresAt < DateTimeOffset.UtcNow)
                        {
                            context.Result = new ContentResult()
                            {
                                ContentType = "application/json",
                                StatusCode = (int)HttpStatusCode.RequestTimeout
                            };
                        }
                        else if (!jwtToken.Claims.Any(claim => claim.Type == "permissions"))
                        {
                            context.Result = new ContentResult()
                            {
                                ContentType = "application/json",
                                StatusCode = (int)HttpStatusCode.Unauthorized
                            };
                        }
                        else if (_permissions.Length > 0)
                        {
                            _jwtPermissions = JsonConvert.DeserializeObject<string[]>(jwtToken.Claims.First(claim => claim.Type == "permissions").Value);
                            _count = 0;
                            foreach (string item in _permissions)
                            {
                                if (_jwtPermissions.Contains(item))
                                {
                                    _count++;
                                }
                            }

                            if (_count != _permissions.Length)
                            {
                                context.Result = new ContentResult()
                                {
                                    ContentType = "application/json",
                                    StatusCode = (int)HttpStatusCode.Unauthorized
                                };
                            }
                        }

                    }
                    else
                    {
                        context.Result = new ContentResult()
                        {
                            ContentType = "application/json",
                            StatusCode = (int)HttpStatusCode.Unauthorized
                        };
                    }
                }
                catch (Exception ex)
                {

                    context.Result = new ContentResult()
                    {
                        Content = JsonConvert.SerializeObject(ex.Message),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }
            }
            public void OnActionExecuted(ActionExecutedContext context) { }
        }
    }
}
