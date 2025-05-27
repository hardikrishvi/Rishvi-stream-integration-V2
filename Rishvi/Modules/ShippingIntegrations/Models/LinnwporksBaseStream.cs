using LinnworksMacroHelpers.Classes;
using LinnworksMacroHelpers.Interfaces;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class LinnworksBaseStream
    {
        public IRuntimeHelper RunTime { get; set; }
        public LinnworksAPI.ApiObjectManager Api { get; set; }
        public IProxyFactory ProxyFactory { get; set; }
        public MacroConfigurationProxy Configuration { get; set; }

        public ISettingsHelper SettingsHelper { get; set; }

        public LinnworksAPI.AuthController controller = new LinnworksAPI.AuthController(new LinnworksAPI.ApiContext("https://api.linnworks.net"));

        public LinnworksAPI.BaseSession authorized = new LinnworksAPI.BaseSession();
        public LinnworksAPI.ApiContext context = null;

        public LinnworksBaseStream(string token)
        {
            //Fetch the application settings from the config file
            authorized = controller.AuthorizeByApplication(new LinnworksAPI.AuthorizeByApplicationRequest
            {
                ApplicationId = ApplicationSettings.ApplicationId,
                ApplicationSecret = ApplicationSettings.ApplicationSecret,
                Token = Guid.Parse(token)
            });

            context = new LinnworksAPI.ApiContext(authorized.Token, authorized.Server);
            Api = new LinnworksAPI.ApiObjectManager(context);
        }
    }
}
