using Rishvi.Modules.Configurations.Models.Configs;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Rishvi.Modules.Core.Helpers
{
    public class SendEmailHelper
    {
        public static bool SendEmail(string toName, string toEmail, string title, string[] ccEmailId,string body, MailSetting mailSetting, string fileText, string fileName, bool isCSVAttachment)
        {
            MailMessage message = new MailMessage(mailSetting.FromEmail, toEmail);
            message.Subject = title;
            message.Body = body;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            foreach (string item in ccEmailId)
            {
                message.CC.Add(new MailAddress(item));
            }
            //MailAddress copy1 = new MailAddress("dhaval.pop@gmail.com");
            //MailAddress copy2 = new MailAddress("dhavalpop.spinx@gmail.com");
            //message.CC.Add(copy1);
            //message.CC.Add(copy2);

            if (!string.IsNullOrEmpty(fileText))
            {
                byte[] filebytes = Encoding.UTF8.GetBytes(fileText);
                MemoryStream stream = new MemoryStream(filebytes);

                //Add a new attachment to the E-mail message, using the correct MIME type
                if (isCSVAttachment)
                {
                    Attachment attachment = new Attachment(stream, new ContentType("text/csv"));
                    attachment.Name = fileName;
                    message.Attachments.Add(attachment);
                }
                else
                {
                    Attachment attachment = new Attachment(stream, new ContentType("text/plain"));
                    attachment.Name = fileName;
                    message.Attachments.Add(attachment);
                }
            }

            SmtpClient client = new SmtpClient(mailSetting.Host, mailSetting.Port);
            System.Net.NetworkCredential basicCredential1 = new System.Net.NetworkCredential(mailSetting.Username, mailSetting.Password);
            client.EnableSsl = mailSetting.EnableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;

            client.Send(message);

            return true;
        }
    }
}
