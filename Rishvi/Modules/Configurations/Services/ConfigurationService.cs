using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rishvi.Modules.Configurations.Admin.CacheManagers;
using Rishvi.Modules.Configurations.Models;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core.Data;
using Z.EntityFramework.Plus;

namespace Spinx.Web.Modules.Configurations.Admin.Services
{
    public interface IConfigurationService
    {
        Task<MailSetting> MailSettingAsync();
        Task<SiteSetting> SiteSettingAsync();
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IRepository<Configuration> _configurationRepository;

        public ConfigurationService(
            IRepository<Configuration> configurationRepository
            )
        {
            _configurationRepository = configurationRepository;
        }

        public async Task<MailSetting> MailSettingAsync()
        {
            return await ByConfigurationTypeAsync<MailSetting>(ConfigurationType.MailSetting);
        }

        public async Task<SiteSetting> SiteSettingAsync()
        {
            return await ByConfigurationTypeAsync<SiteSetting>(ConfigurationType.SiteSetting);
        }

        public async Task<T> ByConfigurationTypeAsync<T>(ConfigurationType configurationType)
        {
            var query = await _configurationRepository.AsNoTracking().FromCacheAsync(ConfigurationCacheManager.Name);
            var value = query.FirstOrDefault(t => t.ConfigurationType == configurationType)?.Value;

            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}