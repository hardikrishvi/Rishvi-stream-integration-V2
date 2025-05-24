namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class TokenDetails
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }
    }

    public class ReportModelReq
    {
        public string email { get; set; }
    }
}
