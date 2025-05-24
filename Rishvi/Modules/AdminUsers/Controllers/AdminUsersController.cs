using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Services;
using Rishvi.Modules.AdminUsers.Data.Permissions;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.AdminUsers.Admin.Controllers
{
    [Route("admin/admin-users/api")]
    public class AdminUsersController : BaseController
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(
            IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet("")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.List })]
        public async Task<IActionResult> Get([FromQuery] AdminUserFilterDto dto)
        {
            return Result(await _adminUserService.ListAsync(dto));
        }

        [HttpGet("{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> Get(Guid id)
        {
            return Result(await _adminUserService.ByIdAsync(id));
        }

        [HttpPost, Route("")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Create })]
        public async Task<IActionResult> Post([FromBody] AdminUserCreateDto dto)
        {
            return Result(await _adminUserService.CreateAsync(dto));
        }

        [HttpPut("")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> Put([FromBody] AdminUserEditDto dto)
        {
            return Result(await _adminUserService.EditAsync(dto));
        }

        [HttpPost, Route("active")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> Active([FromBody] IdsDto dto)
        {
            return Result(await _adminUserService.ActiveAsync(dto));
        }

        [HttpPost, Route("inactive")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> InActive([FromBody] IdsDto dto)
        {
            return Result(await _adminUserService.InActiveAsync(dto));
        }

        [HttpPost, Route("delete")]
        public async Task<IActionResult> Delete([FromBody] IdsDto dto)
        {
            return Result(await _adminUserService.DeleteAsync(dto));
        }

        [HttpGet, Route("edit-profile/{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> EditProfile(Guid id)
        {
            return Result(await _adminUserService.GetEditProfileAsync(id));
        }

        [HttpPost, Route("edit-profile")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> EditProfile([FromBody] AdminEditProfileDto dto)
        {
            return Result(await _adminUserService.SaveEditProfileAsync(dto));
        }

        [HttpGet, Route("change-password/{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> ChangePassword(Guid id)
        {
            return Result(await _adminUserService.GetChangePasswordAsync(id));
        }

        [HttpPost, Route("change-password")]
        [AuthorizeApiAdminUser(new[] { AdminUsersPermission.Edit })]
        public async Task<IActionResult> ChangePassword([FromBody] AdminChangePasswordDto dto)
        {
            return Result(await _adminUserService.SaveChangePasswordAsync(dto));
        }
    }
}
