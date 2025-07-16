using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.Core.Api;
using Rishvi.Modules.Core.DTOs;
using Rishvi.Modules.ErrorLogs.Data.Permissions;
using Rishvi.Modules.ErrorLogs.Models.DTOs;
using Rishvi.Modules.ErrorLogs.Services;
namespace Rishvi.Modules.ErrorLogs.Api
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/admin/systemlog/v1")]
    public class SystemLogApiController : BaseApiController
    {
        public readonly ISystemLogService _systemLogService;
        public SystemLogApiController(ISystemLogService systemLogService)
        {
            _systemLogService = systemLogService;
        }
        [HttpGet("")]
        [AuthorizeApiAdminUser(new[] { SystemLogPermission.List })]
        public async Task<IActionResult> Get([FromQuery] SystemLogFilterDto dto)
        {
            return Result(await _systemLogService.ListAsync(dto));
        }
        [HttpGet("{id:Guid}")]
        [AuthorizeApiAdminUser(new[] { SystemLogPermission.Edit })]
        public async Task<IActionResult> Get(Guid id)
        {
            return Result(await _systemLogService.ByIdAsync(id));
        }
        [HttpPost, Route("delete")]
        [AuthorizeApiAdminUser(new[] { SystemLogPermission.Delete })]
        public async Task<IActionResult> Delete([FromBody] IdsDto dto)
        {
            return Result(await _systemLogService.DeleteAsync(dto));
        }
        [HttpGet, Route("status")]
        [AuthorizeApiAdminUser(new[] { SystemLogPermission.List })]
        public async Task<IActionResult> GetStatus()
        {
            return Result(await _systemLogService.GetStatus());
        }
    }
}
