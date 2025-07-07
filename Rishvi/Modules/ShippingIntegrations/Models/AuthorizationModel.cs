using Newtonsoft.Json;

namespace Rishvi.Modules.ShippingIntegrations.Models
{
    public class AuthorizationModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; } = 3600; // Default to 1 hour if not specified
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }

        public DateTime ExpireTime => DateTime.UtcNow.AddSeconds(ExpiresIn);
    }
}
