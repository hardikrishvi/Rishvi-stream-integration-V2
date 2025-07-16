using System.Globalization;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class StreamApiSettings
    {
        public static IConfiguration _configuration;
        public static void StreamApiSettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string DemoUrl
        {
            get
            {
                return Setting<string>("StreamApiSettings:DemoUrl");
            }
        }
        public static string ProductionUrl { get; set; } = null!;

        private static T Setting<T>(string name)
        {
            var value = _configuration.GetValue<T>(name);

            if (value == null)
            {
                throw new Exception(String.Format("Could not find setting '{0}',", name));
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
