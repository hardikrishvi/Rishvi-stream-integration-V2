using System;

namespace Rishvi.Modules.Configurations.Models
{
    public class Configuration
    {
        public Guid Id { get; set; }
        public ConfigurationType ConfigurationType { get; set; }
        public string Value { get; set; }
    }

    public enum ConfigurationType
    {
        MailSetting = 100,
        SiteSetting = 101
    }
}