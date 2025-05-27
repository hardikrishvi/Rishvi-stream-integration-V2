using System.Globalization;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class ApplicationSettings
    {
        public static IConfiguration _configuration;

        public static void ApplicationSettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static Guid ApplicationId
        {
            get
            {
                return Setting<Guid>("ApplicationSettings:ApplicationId");
            }
        }

        public static Guid ApplicationSecret 
        {
            get
            {
                return Setting<Guid>("ApplicationSettings:ApplicationSecret");
            }
        }
        public static Guid Token
        {
            get
            {
                return Setting<Guid>("ApplicationSettings:Token");
            }
        }

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
