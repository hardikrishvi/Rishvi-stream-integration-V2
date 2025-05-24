using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Admin.Services;
using Rishvi.Modules.AdminRolePermissions.Data.Permissions;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.DTOs;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Controllers
{
    [Route("admin/admin-roles/api")]
    public class AdminRolesController : BaseController
    {
        private readonly IAdminRoleService _adminRoleService;

        public AdminRolesController(
            IAdminRoleService adminRoleService)
        {
            _adminRoleService = adminRoleService;
        }

        [HttpGet, Route("all")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.List })]
        public async Task<IActionResult> All()
        {
            return Result(await _adminRoleService.GetRolesAsync());
        }

        [HttpGet("")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.List })]
        public async Task<IActionResult> Get([FromQuery] AdminRoleFilterDto dto)
        {
            return Result(await _adminRoleService.ListAsync(dto));
        }

        [HttpGet("{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.Edit })]
        public async Task<IActionResult> Get(Guid id)
        {
            return Result(await _adminRoleService.ByIdAsync(id));
        }

        [HttpPost, Route("")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.Create })]
        public async Task<IActionResult> Post([FromBody] AdminRoleCreateDto dto)
        {
            return Result(await _adminRoleService.CreateAsync(dto));
        }

        [HttpPut("")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.Edit })]
        public async Task<IActionResult> Put([FromBody] AdminRoleEditDto dto)
        {
            return Result(await _adminRoleService.EditAsync(dto));
        }

        [HttpPost, Route("delete")]
        [AuthorizeApiAdminUser(new[] { AdminRolePermission.Delete })]
        public async Task<IActionResult> Delete([FromBody] IdsDto dto)
        {
            return Result(await _adminRoleService.DeleteAsync(dto.Ids));
        }
    }
}