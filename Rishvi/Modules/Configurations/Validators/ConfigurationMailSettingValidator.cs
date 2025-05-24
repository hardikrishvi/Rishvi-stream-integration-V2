using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Rishvi.Modules.Configurations.Models.Configs;

namespace Rishvi.Modules.Configurations.Admin.Validators
{
    public class ConfigurationMailSettingValidator : AbstractValidator<MailSetting>
    {
        public ConfigurationMailSettingValidator()
        {
            WhenAsync(CheckEnabledAsync, () =>
            {
                RuleFor(v => v.FromName).MaximumLength(100);
                RuleFor(v => v.FromEmail).NotEmpty().EmailAddress().MaximumLength(256);
                RuleFor(v => v.ContactEmail).NotEmpty().EmailAddress().MaximumLength(256);

                RuleFor(v => v.Host).NotEmpty().MaximumLength(100);
                RuleFor(v => v.Port).NotEqual(0).WithMessage("'Port' should not be empty.");

                WhenAsync(CheckIsAuthAsync, () =>
                {
                    RuleFor(v => v.Username).NotEmpty().MaximumLength(100);
                    RuleFor(v => v.Password).NotEmpty().MaximumLength(100);
                });
            });
        }

        private async Task<bool> CheckIsAuthAsync(MailSetting setting, CancellationToken cancellation)
        {
            return await Task.FromResult(setting.IsAuthentication);
        }

        private async Task<bool> CheckEnabledAsync(MailSetting setting, CancellationToken cancellation)
        {
            return await Task.FromResult(setting.Enabled);
        }
    }
}