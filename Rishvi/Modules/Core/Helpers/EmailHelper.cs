using System.Net.Mail;
using System.Text;

namespace Rishvi.Modules.Core.Helpers
{
    public static class EmailHelper
    {
        public static string ServerName = "smtp-relay.brevo.com";
        public static bool useSSL = true;
        public static int port = 587;
        public static string username = "info@rishvi.co.uk";
        public static string fromEmail = "info@rishvi.co.uk";
        public static string fromName = "Rishvi";
        public static string password = "QqELbRU7aPvK5Osd";
        public static string toEmail = "hardik@rishvi.co.uk";
        public static string toName = "Rishvi";

        public static bool SendEmail(string title, string body)
        {
            MailMessage message = new MailMessage(fromEmail, toEmail);
            message.Subject = title + " Stream shipping integration";
            message.Body = body;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient(ServerName, port);
            System.Net.NetworkCredential basicCredential1 = new System.Net.NetworkCredential(username, password);
            client.EnableSsl = useSSL;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            client.Send(message);

            return true;
        }
    }
}
