using LinnworksMacroHelpers.Classes;
using LinnworksMacroHelpers.Interfaces;
using System;

namespace LinnworksMacroHelpers
{
    public class LinnworksMacroBase
    {
        public IRuntimeHelper RunTime { get; set; }
        public LinnworksAPI.ApiObjectManager Api { get; set; }
        public IProxyFactory ProxyFactory { get; set; }
        public ILogger Logger { get; set; }
        public MacroConfigurationProxy Configuration { get; set; }

        public ISettingsHelper SettingsHelper { get; set; }

        public LinnworksAPI.AuthController controller = new LinnworksAPI.AuthController(new LinnworksAPI.ApiContext("https://api.linnworks.net"));

        public LinnworksAPI.BaseSession authorized = new LinnworksAPI.BaseSession();
        public LinnworksAPI.ApiContext context = null;
        public LinnworksMacroBase()
        {
            authorized = controller.AuthorizeByApplication(new LinnworksAPI.AuthorizeByApplicationRequest
            {
                ApplicationId = Guid.Parse("818ff96c-d52e-4ec4-8a59-9bb048fb1da4"),
                ApplicationSecret = Guid.Parse("86560302-ecbc-4e6e-b9b8-305eb3b974b4"),
                Token = Guid.Parse("53f82994-142b-4863-b86d-faa499207b0d")
            });

            context = new LinnworksAPI.ApiContext(authorized.Token, authorized.Server);
            Api = new LinnworksAPI.ApiObjectManager(context);
        }
    }

    public class LinnworksMacroBaseStatic
    {
        public static IRuntimeHelper RunTime { get; set; }
        public static LinnworksAPI.ApiObjectManager Api { get; set; }
        public static IProxyFactory ProxyFactory { get; set; }
        public static ILogger Logger { get; set; }
        public static MacroConfigurationProxy Configuration { get; set; }

        public static ISettingsHelper SettingsHelper { get; set; }

        public static LinnworksAPI.AuthController controller = new LinnworksAPI.AuthController(new LinnworksAPI.ApiContext("https://api.linnworks.net"));

        public static LinnworksAPI.BaseSession authorized = new LinnworksAPI.BaseSession();
        public static LinnworksAPI.ApiContext context = null;
        public static void AuthorizeByApplication(Guid? token, Guid applicationId, Guid ApplicationSecret)
        {
            authorized = controller.AuthorizeByApplication(new LinnworksAPI.AuthorizeByApplicationRequest
            {
                ApplicationId = applicationId,
                ApplicationSecret = ApplicationSecret,
                Token = token.Value
            });

            context = new LinnworksAPI.ApiContext(authorized.Token, authorized.Server);
            Api = new LinnworksAPI.ApiObjectManager(context);
        }
    }
}