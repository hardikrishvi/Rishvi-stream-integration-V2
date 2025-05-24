using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Services;
using Rishvi.Modules.AdminUsers.Admin.Services.Auth;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.Authorization;

namespace Rishvi.Modules.AdminUsers.Admin.Controllers

{
    [Route("admin/auth")]
    public class AdminAuthController : BaseController
    {
        private readonly IAdminLoginService _adminLoginService;
        private readonly IAdminForgotPasswordService _adminForgotPasswordService;
        private readonly IAdminResetPasswordService _adminResetPasswordService;
        private readonly IAdminUserService _adminUserService;
        private readonly JwtSetting _jwtSetting;

        public AdminAuthController(
            IAdminLoginService adminLoginService,
            IAdminForgotPasswordService adminForgotPasswordService,
            IAdminResetPasswordService adminResetPasswordService,
            IAdminUserService adminUserService,
            IOptions<JwtSetting> jwtSetting)
        {
            _adminLoginService = adminLoginService;
            _adminForgotPasswordService = adminForgotPasswordService;
            _adminResetPasswordService = adminResetPasswordService;
            _adminUserService = adminUserService;
            _jwtSetting = jwtSetting.Value;
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            (Result result, AdminUser adminUser, List<LoginUserPermissions> permissions) = await _adminLoginService.LoginAsync(dto);

            AdminLoginResponseDto loginResponse = new AdminLoginResponseDto();
            if (result.Success)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SecretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var permissionData = await _adminLoginService.GetPermissionsAsync(adminUser.Id);
                var claims = new[] {
                     new Claim(JwtRegisteredClaimNames.Sub, adminUser.Name),
                     new Claim("name", adminUser.Name),
                     new Claim("id", adminUser.Id.ToString()),
                     new Claim("permissions", JsonConvert.SerializeObject(permissionData.Data)),
                     new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                     new Claim(JwtRegisteredClaimNames.Email, adminUser.Email),
                     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                 };

                var token = new JwtSecurityToken(_jwtSetting.Issuer,
                _jwtSetting.Issuer,
                claims,
                expires: DateTime.Now.Add(_jwtSetting.TokenLifeTime),

                signingCredentials: credentials);

                loginResponse.AuthorizationKey = new JwtSecurityTokenHandler().WriteToken(token);

                loginResponse.UserId = adminUser.Id;
                loginResponse.Name = adminUser.Name;
                loginResponse.Email = adminUser.Email;
                loginResponse.Permissions = permissions;

                result.Data = loginResponse;
            }
            else
            {
                result.Data = null;
            }

            return Result(result);
        }

        [HttpPost, Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] AdminForgotPasswordDto dto)
        {
            return Result(await _adminForgotPasswordService.ForgotPasswordAsync(dto));
        }

        [HttpPost, Route("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword(string token, [FromBody] AdminResetPasswordDto dto)
        {
            return Result(await _adminResetPasswordService.ResetPasswordAsync(token, dto));
        }

        [HttpGet, Route("is-valid/{token}")]
        public async Task<bool> IsValidToken(string token)
        {
            return await _adminResetPasswordService.IsValidTokenAsync(token);
        }
    }
}
