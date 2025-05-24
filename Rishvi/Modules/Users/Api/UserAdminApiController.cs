using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.AdminUsers.Data.Permissions;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.Users.Models.DTOs;
using Rishvi.Modules.Users.Services;
using System;
using System.Threading.Tasks;

namespace Rishvi.Modules.Users.Api
{
    [ApiController]
    [Route("api/admin/users/v1")]
    public class UserAdminApiController : BaseApiController
    {
        public readonly IUserAdminService _userAdminService;
        public UserAdminApiController(IUserAdminService userAdminService)
        {
            _userAdminService = userAdminService;
        }

        [HttpGet("")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.List })]
        public async Task<IActionResult> Get([FromQuery] UserAdminFilterDto dto)
        {
            return Result(await _userAdminService.ListAsync(dto));
        }

        [HttpGet("{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Edit })]
        public async Task<IActionResult> Get(Guid id)
        {
            return Result(await _userAdminService.ByIdAsync(id));
        }

        [HttpPost, Route("")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Create })]
        public async Task<IActionResult> Post([FromBody] UserAdminCreateDto dto)
        {
            return Result(await _userAdminService.CreateAsync(dto));
        }

        [HttpPut("")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Edit })]
        public async Task<IActionResult> Put([FromBody] UserAdminEditDto dto)
        {
            return Result(await _userAdminService.EditAsync(dto));
        }

        [HttpPost, Route("active")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Edit })]
        public async Task<IActionResult> Active([FromBody] IdsDto dto)
        {
            return Result(await _userAdminService.ActiveAsync(dto));
        }

        [HttpPost, Route("inactive")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Edit })]
        public async Task<IActionResult> InActive([FromBody] IdsDto dto)
        {
            return Result(await _userAdminService.InActiveAsync(dto));
        }

        [HttpPost, Route("delete")]
        [AuthorizeApiAdminUser(new[] { UsersPermission.Delete })]
        public async Task<IActionResult> Delete([FromBody] IdsDto dto)
        {
            return Result(await _userAdminService.DeleteAsync(dto));
        }

        [HttpGet("get-email-and-token/{emailAddress}")]
        public async Task<IActionResult> GetKlaviyoEmailAndTokenByEmailId(string emailAddress)
        {
            return Result(await _userAdminService.GetKlaviyoEmailAndTokenByEmailIdAsync(emailAddress));
        }


        [HttpPut("update-email-and-token")]
        public async Task<IActionResult> UpdateKlaviyoEmailAndTokenByEmailIdAsync([FromBody] UserAdminEmailDto dto)
        {
            return Result(await _userAdminService.UpdateKlaviyoEmailAndTokenByEmailIdAsync(dto));
        }
    }
}
