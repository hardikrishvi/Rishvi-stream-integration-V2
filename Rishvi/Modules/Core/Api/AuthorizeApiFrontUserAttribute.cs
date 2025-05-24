using System;
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
    public class AuthorizeApiFrontUserAttribute : TypeFilterAttribute
    {
        public AuthorizeApiFrontUserAttribute() : base(typeof(ClaimRequirementFilter))
        {
        }

        private class ClaimRequirementFilter : IActionFilter
        {
            private readonly JwtSetting _jwtSetting;
            public ClaimRequirementFilter(IOptions<JwtSetting> jwtSetting)
            {
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
                                StatusCode = (int)HttpStatusCode.BadRequest
                            };
                        }
                    }
                    else
                    {
                        context.Result = new ContentResult()
                        {
                            ContentType = "application/json",
                            StatusCode = (int)HttpStatusCode.BadRequest
                        };
                    }

                }
                catch (Exception ex)
                {
                    context.Result = new ContentResult()
                    {
                        Content = JsonConvert.SerializeObject(ex.Message),
                        ContentType = "application/json",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }
            }
            public void OnActionExecuted(ActionExecutedContext context)
            {
            }
        }
    }

}
