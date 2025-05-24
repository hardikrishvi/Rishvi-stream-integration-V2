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
        private readonly ApplicationSettings _appSettings;
        public LinnworksBaseStream(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public LinnworksBaseStream(string token)
        {
            //Fetch the application settings from the config file
            authorized = controller.AuthorizeByApplication(new LinnworksAPI.AuthorizeByApplicationRequest
            {
                ApplicationId = Guid.Parse("060f6bc8-cb09-4682-b280-de37f8892f4c"),
                ApplicationSecret = Guid.Parse("7166a843-a1e3-450b-88a9-626feb3229a6"),
                Token = Guid.Parse(token)
            });

            context = new LinnworksAPI.ApiContext(authorized.Token, authorized.Server);
            Api = new LinnworksAPI.ApiObjectManager(context);
        }
    }
}
