namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class MessinaSettings
    {
        public string HostName { get; set; } = null!;
        public string TradingAPI_ServerURL { get; set; } = null!;
        public string DeveloperId { get; set; } = null!;
        public string ProdClientId { get; set; } = null!;
        public string ProdRedirectURL { get; set; } = null!;
        public string ProdClientSecret { get; set; } = null!;
        public string TradingAPI_Version { get; set; } = null!;
        public string WebApiURL { get; set; } = null!;

        public string FtpHost { get; set; } = null!;
        public string FtpUsername { get; set; } = null!;
        public string FtpPassword { get; set; } = null!;
    }
}
