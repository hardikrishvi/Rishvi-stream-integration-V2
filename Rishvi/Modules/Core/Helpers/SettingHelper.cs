using System.Globalization;

namespace Rishvi.Modules.Core.Helpers
{
    public class AppSettings
    {
        public static IConfiguration _configuration;

        public static void AppSettingsConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string DataStoragePath
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:DataStoragePath");
            }
        }
        public static string ConfigStoragePath
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:ConfigStoragePath");
            }
        }

        public static string WizardStagesConfigPath
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:WizardStagesConfigPath");
            }
        }

        public static string ApplicationId
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:ApplicationId");
            }
        }


        public static string ApplicationSecret
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:ApplicationSecret");
            }
        }

        public static string LinnworksAPIUrl
        {
            get
            {
                return Setting<string>("LinnworkAppCredential:LinnworksAPIUrl");
            }
        }

        public static string LogFile
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:LogFile");
            }
        }
        public static string ExceptionFile
        {
            get
            {
                return Setting<string>("LinnworkAppSetting:ExceptionFile");
            }
        }
        public static string StreamApiBasePath
        {
            
            get
            {
                return Setting<string>("StreamSetting:ApiBasePath");
            }
        }
        public static string StreamOAuthUrl
        {
            get
            {
                return Setting<string>("StreamSetting:OAuthUrl");
            }
        }
        public static string GrantType
        {
            get
            {
                return Setting<string>("StreamSetting:GrantType");
            }
        }
        //public static string ClientId
        //{
        //    get
        //    {
        //        return Setting<string>("StreamSetting:ClientId");
        //    }
        //}
        public static string CreateOrderUrl
        {
            get
            {
                return Setting<string>("StreamSetting:CreateOrderUrl");
            }
        }
        public static string DeleteOrderUrl
        {
            get
            {
                return Setting<string>("StreamSetting:DeleteOrderUrl");
            }
        }
        //public static string ClientSecret
        //{
        //    get
        //    {
        //        return Setting<string>("StreamSetting:ClientSecret");
        //    }
        //}
        public static string AwsBuketName
        {
            get
            {
                return Setting<string>("AwsSetting:AwsBuketName");
            }
        }
        public static string AwsAccessKey
        {
            get
            {
                return Setting<string>("AwsSetting:AwsAccessKey");
            }
        }
        public static string AwsSecretKey
        {
            get
            {
                return Setting<string>("AwsSetting:AwsSecretKey");
            }
        }


        public static void LogException(string error, string step)
        {
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(AppSettings.ExceptionFile, true);
                sw.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " : " + step + " : " + error);
                sw.Close();
            }
            catch (Exception ex)
            {

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
