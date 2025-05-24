using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.Configurations.Admin.Services;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core.Api;

namespace Rishvi.Modules.Configurations.Admin.Controllers
{
    [Route("admin/configuration/mail-settings/api")]
    [AuthorizeApiAdminUser]
    public class ConfigurationMailSettingController : BaseController
    {
        private readonly IConfigurationMailSettingService _configurationMailSettingService;

        public ConfigurationMailSettingController(
            IConfigurationMailSettingService configurationMailSettingService)
        {
            _configurationMailSettingService = configurationMailSettingService;
        }

        [HttpGet, Route("")]
        public async Task<IActionResult> Get()
        {
            return Result(await _configurationMailSettingService.GetAsync());
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> Post([FromBody] MailSetting mailSetting)
        {
            return Result(await _configurationMailSettingService.SetAsync(mailSetting));
        }
    }
}