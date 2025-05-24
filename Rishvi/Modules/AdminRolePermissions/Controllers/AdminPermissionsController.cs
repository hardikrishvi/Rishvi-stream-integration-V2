using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Admin.Services;
using Rishvi.Modules.AdminRolePermissions.Data.Permissions;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Controllers
{
    [Route("admin/admin-permissions/api")]
    public class AdminPermissionsController : BaseController
    {
        private readonly IAdminPermissionService _adminPermissionService;

        public AdminPermissionsController(
            IAdminPermissionService adminPermissionService)
        {
            _adminPermissionService = adminPermissionService;
        }

        [HttpGet, Route("all")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.List })]
        public async Task<IActionResult> All()
        {
            return Result(await _adminPermissionService.GetAdminPermissionsAsync());
        }

        [HttpGet("")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.List })]
        public async Task<IActionResult> Get(AdminPermissionFilterDto dto)
        {
            return Result(await _adminPermissionService.ListAsync(dto));
        }

        [HttpGet("{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Edit })]
        public async Task<IActionResult> Get(Guid id)
        {
            return Result(await _adminPermissionService.ByIdAsync(id));
        }

        [HttpPost, Route("")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Create })]
        public async Task<IActionResult> Post([FromBody] AdminPermissionDto dto)
        {
            return Result(await _adminPermissionService.CreateAsync(dto));
        }

        [HttpPut("")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Edit })]
        public async Task<IActionResult> Put([FromBody] AdminPermissionDto dto)
        {
            return Result(await _adminPermissionService.EditAsync(dto));
        }

        [HttpPost, Route("delete")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Delete })]
        public async Task<IActionResult> Delete([FromBody] IdsDto dto)
        {
            return Result(await _adminPermissionService.DeleteAsync(dto.Ids));
        }

        [HttpGet, Route("sequence")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Edit })]
        public async Task<IActionResult> GetSequence()
        {
            return Result(await _adminPermissionService.GetSequenceDataAsync());
        }

        [HttpPost, Route("save-sequence")]
        [AuthorizeApiAdminUser(new[] { AdminPermissionPermission.Edit })]
        public async Task SaveSequence([FromBody] IList<AdminPermissionSequenceDto> data)
        {
            await _adminPermissionService.SaveSequenceDataAsync(data);
        }
    }
}