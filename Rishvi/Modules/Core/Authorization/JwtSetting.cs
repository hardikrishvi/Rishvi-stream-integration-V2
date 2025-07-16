namespace Rishvi.Modules.Core.Authorization
{
    public class JwtSetting
    {
        public string SecretKey { get; set; }
        public TimeSpan TokenLifeTime { get; set; }
        public string Issuer { get; set; }
    }
}