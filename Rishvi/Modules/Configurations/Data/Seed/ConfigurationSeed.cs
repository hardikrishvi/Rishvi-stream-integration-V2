using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Rishvi.Modules.Configurations.Models;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using YamlDotNet.RepresentationModel;

namespace Rishvi.Modules.Configurations.Data.Seed
{
    public class ConfigurationSeed : BaseSeed
    {
        public ConfigurationSeed(SqlContext context) : base(context) { }

        public override void Seed()
        {

            if (Context.Set<Configuration>().Any())
            {
                return;
            }

            MailSetting();
            SiteSetting();

            Context.SaveChanges();
        }

        private YamlNode BySetting(string name)
        {

            var input = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(),
                $@"App_Data\settings.yaml"));

            var stringReader = new StringReader(input);

            var yaml = new YamlStream();
            yaml.Load(stringReader);

            return yaml.Documents[0].RootNode[name];
        }

        private T GetValue<T>(YamlNode node)
        {
            var value = ((YamlScalarNode)node).Value;
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        private void Insert(SqlContext context, ConfigurationType configurationType, string value)
        {
            context.Set<Configuration>().Add(new Configuration
            {
                ConfigurationType = configurationType,
                Value = value
            });
        }

        private void MailSetting()
        {
            var node = BySetting("MailSettings");

            var mailSetting = new MailSetting()
            {
                Enabled = GetValue<bool>(node["Enabled"]),
                FromName = GetValue<string>(node["FromName"]),
                FromEmail = GetValue<string>(node["FromEmail"]),
                ContactEmail = GetValue<string>(node["ContactEmail"]),
                Host = GetValue<string>(node["Host"]),
                Port = GetValue<int>(node["Port"]),
                EnableSsl = GetValue<bool>(node["EnableSsl"]),
                IsAuthentication = GetValue<bool>(node["IsAuthentication"]),
                Username = GetValue<string>(node["Username"]),
                Password = GetValue<string>(node["Password"])
            };

            Insert(Context, ConfigurationType.MailSetting, JsonConvert.SerializeObject(mailSetting));
        }

        private void SiteSetting()
        {
            var node = BySetting("SiteSetting");

            var socialLinks = new SiteSetting()
            {
                SiteTitle = GetValue<string>(node["SiteTitle"]),
                WebsiteUrl = GetValue<string>(node["WebsiteUrl"])
            };

            Insert(Context, ConfigurationType.SiteSetting, JsonConvert.SerializeObject(socialLinks));
        }
    }
}
