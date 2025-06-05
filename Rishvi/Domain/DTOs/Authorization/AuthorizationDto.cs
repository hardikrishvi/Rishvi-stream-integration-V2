namespace Rishvi.Domain.DTOs.Authorization
{
    public class AuthorizationDto
    {
        public int Id { get; set; }
        public DateTime IntegratedDateTime { get; set; }
        public bool IsConfigActive { get; set; }
        public string ConfigStatus { get; set; }
        public string AddressLine1 { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string ContactName { get; set; }
        public string ContactPhoneNo { get; set; }
        public string CountryCode { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string SessionID { get; set; }
        public string LabelReference { get; set; }
        public string Email { get; set; }
        public string LinnworksUniqueIdentifier { get; set; }
        public string AuthorizationToken { get; set; }
        public string AccountName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string access_token { get; set; }
        public DateTime? ExpirationTime { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
        public string FtpHost { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public int FtpPort { get; set; }
        public string LinnworksToken { get; set; }
        public string LinnworksServer { get; set; }
        public string LinnRefreshToken { get; set; }
        public string fulfiilmentLocation { get; set; }
        public bool PartyFileCreated { get; set; }
    }
}
