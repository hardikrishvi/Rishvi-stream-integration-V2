using System.Runtime.CompilerServices;
using System.Text;
using LinnworksAPI;
using Microsoft.Extensions.Options;
using Rishvi.Modules.ShippingIntegrations.Models;
using Rishvi.Modules.ShippingIntegrations.Models.Classes;

namespace Rishvi.Modules.ShippingIntegrations.Core.Helper
{
    public class ServiceHelper : IServiceHelper
    {
        private readonly TradingApiOAuthHelper _tradingApiOAuthHelper;
        private readonly string _StoragePath;
        private readonly string _OrderStoreLocation;
        private readonly string _ProductStoreLocation;
        private readonly string _StockStoreLocation;
        private readonly string _PriceStoreLocation;
        private readonly string _ProcessStoreLocation;

        private readonly string _ApiBasePath;
        private readonly string _OAuthUrl;
        private readonly string _SyncPath;
        private readonly string _ImageURL;

        public ServiceHelper(TradingApiOAuthHelper tradingApiOAuthHelper, IConfiguration config)
        {
            _tradingApiOAuthHelper = tradingApiOAuthHelper;
            _StoragePath = config["ServiceHelperSettings:StoragePath"] ?? GetSetting("StoragePath");
            _OrderStoreLocation = config["ServiceHelperSettings:OrderStoreLocation"];
            _ProductStoreLocation = config["ServiceHelperSettings:ProductStoreLocation"];
            _StockStoreLocation = config["ServiceHelperSettings:StockStoreLocation"];
            _PriceStoreLocation = config["ServiceHelperSettings:PriceStoreLocation"];
            _ProcessStoreLocation = config["ServiceHelperSettings:ProcessStoreLocation"] ?? GetSetting("ProcessStoreLocation");
            _ApiBasePath = config["ServiceHelperSettings:ApiBasePath"] ?? GetSetting("ApiBasePath");
            _OAuthUrl = config["ServiceHelperSettings:OAuthUrl"] ?? GetSetting("OAuthUrl");
            _SyncPath = config["ServiceHelperSettings:SyncPath"] ?? GetSetting("SyncPath");
            _ImageURL = config["ServiceHelperSettings:ImageURL"] ?? GetSetting("ImageURL");
        }
        
        private static string GetSetting([CallerMemberName] string name = null)
        {
            return System.Configuration.ConfigurationManager.AppSettings.Get(name) ?? string.Empty;
        }

        public string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashed);
        }

        public string TransformEmail(string email)
        {
            return email.ToLower().Replace("@", "_").Replace(".", "_");
        }

        public FieldsFilter CreateFilters(int linnhour)
        {
            return new FieldsFilter
            {
                TextFields = new List<TextFieldFilter>
                {
                    new TextFieldFilter
                    {
                        FieldCode = FieldCode.SHIPPING_INFORMATION_TRACKING_NUMBER,
                        Type = TextFieldFilterType.Equal,
                        Text = ""
                    },
                    new TextFieldFilter
                    {
                        FieldCode = FieldCode.SHIPPING_INFORMATION_VENDOR,
                        Type = TextFieldFilterType.Contains,
                        Text = "Stream"
                    }
                },
                DateFields = new List<DateFieldFilter>
                {
                    new DateFieldFilter
                    {
                        //DateFrom = DateTime.Now.AddHours(-linnhour),
                        //DateTo = DateTime.Now,
                        //FieldCode = FieldCode.GENERAL_INFO_DATE,
                        //Type = DateTimeFieldFilterType.Range,
                        //Value = 0
                    }
                }
            };
        }

        public async Task ManageIdentifier(string linntoken, string identifier)
        {
            var obj = new LinnworksBaseStream(linntoken);
            var identifierTag = identifier.ToUpper();

            // Check if the identifier exists
            var list = obj.Api.OpenOrders.GetIdentifiers();
            if (list.All(d => d.Tag != identifierTag))
            {
                // Save the new identifier
                await SaveNewIdentifier(obj, identifierTag);
            }
            else
            {
                // Identifier already exists, delete and recreate
                obj.Api.OpenOrders.DeleteIdentifier(new DeleteIdentifiersRequest
                {
                    Tag = identifierTag
                });

                await SaveNewIdentifier(obj, identifierTag);
            }
        }

        private async Task SaveNewIdentifier(LinnworksBaseStream obj, string identifierTag)
        {
            await Task.Run(() =>
            {
                obj.Api.OpenOrders.SaveIdentifier(new SaveIdentifiersRequest
                {
                    Identifier = new Identifier
                    {
                        Name = identifierTag,
                        Tag = identifierTag,
                        IsCustom = true,
                        ImageId = Guid.NewGuid(),
                        ImageUrl = _ImageURL + identifierTag + ".png"
                    }
                });
            });
        }

        public class WebhookSubscription
        {
            public string Event { get; }
            public string Identifier { get; }
            public string Url { get; }

            public WebhookSubscription(string @event, string identifier, string url)
            {
                Event = @event;
                Identifier = identifier;
                Url = url;
            }
        }

        public async Task CreateWebhook(Rishvi.Models.Authorization user, WebhookSubscription webhook, string token)
        {
             // Consider injecting via DI
            await _tradingApiOAuthHelper.CreateStreamWebhook(
                user,
                webhook.Event,
                webhook.Identifier,
                webhook.Url,
                "POST",
                "application/json",
                token
            );
        }

    }
}
