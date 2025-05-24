namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class ServiceHelperSettings
    {
        public string StoragePath { get; set; } = null!;
        public string OrderStoreLocation { get; set; } = null!;
        public string ProductStoreLocation { get; set; } = null!;
        public string StockStoreLocation { get; set; } = null!;
        public string PriceStoreLocation { get; set; } = null!;
        public string ProcessStoreLocation { get; set; } = null!;
        public string ApiBasePath { get; set; } = null!;
        public string OAuthUrl { get; set; } = null!;
        public string SyncPath { get; set; } = null!;
    }
}
