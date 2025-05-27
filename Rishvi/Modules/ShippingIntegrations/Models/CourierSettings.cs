using System.Globalization;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class CourierSettings
    {
        public static IConfiguration _configuration;

        public static void CourierSettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static Guid SelectedServiceId
        {
            get
            {
                return Setting<Guid>("CourierSettings:SelectedServiceId");
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
