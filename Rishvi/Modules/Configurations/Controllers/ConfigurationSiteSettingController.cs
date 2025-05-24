using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rishvi.Modules.Configurations.Admin.Services;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core.Api;

namespace Rishvi.Modules.Configurations.Admin.Controllers
{
    [Route("admin/configuration/site-settings/api")]
    [AuthorizeApiAdminUser]
    public class ConfigurationSiteSettingController : BaseController
    {
        private readonly IConfigurationSiteSettingService _configurationSiteSettingService;

        public ConfigurationSiteSettingController(
            IConfigurationSiteSettingService configurationSiteSettingService)
        {
            _configurationSiteSettingService = configurationSiteSettingService;
        }

        [HttpGet, Route("")]
        public async Task<IActionResult> Get()
        {
            return Result(await _configurationSiteSettingService.GetAsync());
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> Post([FromBody] SiteSetting siteSetting)
        {
            return Result(await _configurationSiteSettingService.SetAsync(siteSetting));
        }
    }
}