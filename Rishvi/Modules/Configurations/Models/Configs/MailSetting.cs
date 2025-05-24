namespace Rishvi.Modules.Configurations.Models.Configs
{
    public class MailSetting
    {
        public bool Enabled { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ContactEmail { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool IsAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}