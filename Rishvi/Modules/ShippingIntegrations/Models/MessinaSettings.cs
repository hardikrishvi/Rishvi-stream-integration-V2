using System.Globalization;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class MessinaSettings
    {
        public static IConfiguration _configuration;
        public static void MessinaSettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static string HostName 
        {
            get
            {
                return Setting<string>("MessinaSettings:HostName");
            }
        } 
        public static string TradingAPI_ServerURL
        {
            get
            {
                return Setting<string>("MessinaSettings:TradingAPI_ServerURL");
            }
        } 
        public static string DeveloperId
        {
            get
            {
                return Setting<string>("MessinaSettings:DeveloperId");
            }
        } 
        public static string ProdClientId
        {
            get
            {
                return Setting<string>("MessinaSettings:ProdClientId");
            }
        }
        public static string ProdRedirectURL
        {
            get
            {
                return Setting<string>("MessinaSettings:ProdRedirectURL");
            }
        }
        public static string ProdClientSecret
        {
            get
            {
                return Setting<string>("MessinaSettings:ProdClientSecret");
            }
        }
        public static string TradingAPI_Version 
        {
            get
            {
                return Setting<string>("MessinaSettings:TradingAPI_Version");
            }
        }
        public static string WebApiURL 
        {
            get
            {
                return Setting<string>("MessinaSettings:WebApiURL");
            }
        } 

        public static string FtpHost 
        {
            get
            {
                return Setting<string>("MessinaSettings:FtpHost");
            }
        } 
        public static string FtpUsername 
        {
            get
            {
                return Setting<string>("MessinaSettings:FtpUsername");
            }
        }
        public static string FtpPassword 
        {
            get
            {
                return Setting<string>("MessinaSettings:FtpPassword");
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
