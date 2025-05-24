using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Rishvi.Modules.Configurations.Models.Configs;

namespace Rishvi.Modules.Configurations.Admin.Validators
{
    public class ConfigurationSiteSettingValidator : AbstractValidator<SiteSetting>
    {
        public ConfigurationSiteSettingValidator()
        {
            RuleFor(v => v.SiteTitle).NotEmpty().MaximumLength(100);
            RuleFor(v => v.WebsiteUrl).NotEmpty().MaximumLength(256)
                .MustAsync(BeAValidUrlAsync).WithMessage("Invalid 'Website Url'.");
        }

        private async Task<bool> BeAValidUrlAsync(string url, CancellationToken cancellation)
        {
            return await Task.FromResult(Uri.TryCreate(url, UriKind.Absolute, out _));
        }
    }
}