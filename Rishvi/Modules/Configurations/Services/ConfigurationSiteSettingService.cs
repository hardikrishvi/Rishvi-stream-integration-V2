using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rishvi.Modules.Configurations.Admin.CacheManagers;
using Rishvi.Modules.Configurations.Admin.Validators;
using Rishvi.Modules.Configurations.Models;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;
using Z.EntityFramework.Plus;

namespace Rishvi.Modules.Configurations.Admin.Services
{
    public interface IConfigurationSiteSettingService
    {
        Task<SiteSetting> GetAsync();
        Task<Result> SetAsync(SiteSetting siteSetting);
    }

    public class ConfigurationSiteSettingService : IConfigurationSiteSettingService
    {
        private readonly IRepository<Configuration> _configurationRepository;
        private readonly ConfigurationSiteSettingValidator _validator;
        private readonly IUnitOfWork _unitOfWork;

        public ConfigurationSiteSettingService(
            IRepository<Configuration> configurationRepository,
            ConfigurationSiteSettingValidator validator,
            IUnitOfWork unitOfWork)
        {
            _configurationRepository = configurationRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<SiteSetting> GetAsync()
        {
            return await ByConfigurationTypeAsync<SiteSetting>(ConfigurationType.SiteSetting);
        }

        public async Task<Result> SetAsync(SiteSetting model)
        {
            var siteSetting = model ?? new SiteSetting();

            var result = await _validator.ValidateResultAsync(siteSetting);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _configurationRepository.AsNoTracking()
                .FirstAsync(s => s.ConfigurationType == ConfigurationType.SiteSetting);

            entity.Value = JsonConvert.SerializeObject(siteSetting);

            await _configurationRepository.UpdateAsync(entity);

            await _unitOfWork.CommitAsync();
            ConfigurationCacheManager.ClearCache();

            return await result.SetSuccessAsync(Messages.RecordSaved);
        }

        public async Task<T> ByConfigurationTypeAsync<T>(ConfigurationType configurationType)
        {
            var query = await _configurationRepository.AsNoTracking().FromCacheAsync(ConfigurationCacheManager.Name);
            var value = query.FirstOrDefault(t => t.ConfigurationType == configurationType)?.Value;

            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}