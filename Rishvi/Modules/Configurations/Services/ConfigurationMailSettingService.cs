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
    public interface IConfigurationMailSettingService
    {
        Task<MailSetting> GetAsync();
        Task<Result> SetAsync(MailSetting siteSetting);
    }

    public class ConfigurationMailSettingService : IConfigurationMailSettingService
    {
        private readonly IRepository<Configuration> _configurationRepository;
        private readonly ConfigurationMailSettingValidator _validator;
        private readonly IUnitOfWork _unitOfWork;

        public ConfigurationMailSettingService(
            IRepository<Configuration> configurationRepository,
            ConfigurationMailSettingValidator validator,
            IUnitOfWork unitOfWork)
        {
            _configurationRepository = configurationRepository;
            _validator = validator;
            _unitOfWork = unitOfWork;
        }

        public async Task<MailSetting> GetAsync()
        {
            return await ByConfigurationTypeAsync<MailSetting>(ConfigurationType.MailSetting);
        }

        public async Task<Result> SetAsync(MailSetting model)
        {
            var mailSetting = model ?? new MailSetting();

            var result = await _validator.ValidateResultAsync(mailSetting);
            if (!result.Success)
            {
                return result;
            }

            var entity = await _configurationRepository.AsNoTracking()
                .FirstAsync(s => s.ConfigurationType == ConfigurationType.MailSetting);

            entity.Value = JsonConvert.SerializeObject(mailSetting);

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