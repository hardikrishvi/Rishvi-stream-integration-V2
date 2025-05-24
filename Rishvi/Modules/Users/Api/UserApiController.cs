using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Users.Models.DTOs;
using Rishvi.Modules.Users.Services;
using System;
using System.Threading.Tasks;

namespace Rishvi.Modules.Users.Api
{
    [ApiController]
    [Route("api/users/v1")]
    public class UserApiController : BaseApiController
    {
        public readonly IUserService _userService;
        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet, Route("install-user-app")]
        public ActionResult InstallAppUser(string token)
        {
            return Result(_userService.InstallAppUser(token));
        }

        [HttpGet, Route("authorized-by-token/{token}")]
        public async Task<IActionResult> AuthorizedByApplicationToken(Guid token)
        {
            var data = await _userService.AuthorizedByApplicationToken(token);
            return Result(data);
        }

        [HttpGet, Route("get-user-ftp/{token}")]

        public async Task<IActionResult> GetUserWiseFtp(Guid token)
        {
            return Result(await _userService.GetUserWiseFtpAsync(token));
        }

        [HttpPost, Route("update-user-ftp/{token}")]

        public async Task<IActionResult> UpdateUserFTP(Guid token, [FromBody] UserWiseFtpUpdateDto dto)
        {
            return Result(await _userService.UpdateUserFTPAsync(token, dto));
        }


    }
}
