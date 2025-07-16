using Rishvi.Modules.Core.Aws;

namespace Rishvi.Modules.ShippingIntegrations.Models.Classes
{
    public class UserConfig
    {
        public List<UserStageConfig> StageConfigs = new List<UserStageConfig>();
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        //public static UserConfig Load(string AuthorizationToken)
        //{
        //    string folderName = AppSettings.WizardStagesConfigPath + "\\" + AuthorizationToken;
        //    if (!System.IO.Directory.Exists(folderName))
        //    {
        //        System.IO.Directory.CreateDirectory(folderName);
        //    }
        //    string fileName = AppSettings.WizardStagesConfigPath + "\\" + AuthorizationToken + "\\stageConfigs.json";
        //    if (System.IO.File.Exists(fileName))
        //    {
        //        string json = System.IO.File.ReadAllText(fileName);
        //        return Newtonsoft.Json.JsonConvert.DeserializeObject<UserConfig>(json);
        //    }
        //    else
        //    {
        //        return new UserConfig();
        //    }
        //}

        public static UserConfig Load(string authorizationToken)
        {
            if (string.IsNullOrWhiteSpace(authorizationToken))
                throw new ArgumentNullException("authorizationToken");

            if (AwsS3.S3FileIsExists("Authorization", "Files/" + authorizationToken + ".json").Result)
            {
                string json = AwsS3.GetS3File("Authorization", "Files/" + authorizationToken + ".json");
                UserConfig output = Newtonsoft.Json.JsonConvert.DeserializeObject<UserConfig>(json);
                return output;
            }
            else
            {
                return null;
            }

        }

    }
    public class UserStageConfig
    {
        public string ConfigStage;
        public List<UserStageConfigItem> Items = new List<UserStageConfigItem>();
    }


    public class UserStageConfigItem
    {
        public string ConfigItemId;
        public string SelectedValue;
    }
}
