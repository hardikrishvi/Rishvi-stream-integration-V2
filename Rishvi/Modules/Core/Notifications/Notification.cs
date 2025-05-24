using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Hangfire;
using Rishvi.Modules.Configurations.Models;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.EmailTemplates.Data.Emails;
using Rishvi.Modules.EmailTemplates.Models;
using Rishvi.Modules.EmailTemplates.Services;

namespace Rishvi.Modules.Core.Notifications
{
    public class Notification
    {
        private readonly INotificationService _notificationService;
        protected MailHelper MailHelper;
        protected Dictionary<string, object> Variables = new Dictionary<string, object>();
        protected EmailTemplate EmailTemplate;
        private readonly SiteSetting _siteSetting;
        private readonly MailSetting _mailSetting;

        public Notification(
            INotificationService notificationService)
        {
            _notificationService = notificationService;

            var mailSettings = _notificationService.ByConfiguration<MailSetting>(ConfigurationType.MailSetting);
            MailHelper = new MailHelper(mailSettings);

            _siteSetting = _notificationService.ByConfiguration<SiteSetting>(ConfigurationType.SiteSetting);
            _mailSetting = _notificationService.ByConfiguration<MailSetting>(ConfigurationType.MailSetting);
        }

        public Result Send(bool isFront = true)
        {
            PrepareMailHelperForSend(isFront);

            var mailModel = _mailMessageModel;
            BackgroundJob.Enqueue(() => SendMail(mailModel));
            return new Result();
        }

        public async Task SendAsync(bool isFront = true)
        {
            await PrepareMailHelperForSendAsync(isFront);

            var mailModel = _mailMessageModel;
            SendMail(mailModel);
        }

        public void SendAndForgetResponse(bool isFront = true)
        {
            PrepareMailHelperForSend(isFront);
            Task.Run(() => MailHelper.Send()); // Do not call async method in Task.Run
        }

        public async Task SendAndForgetResponseAsync(bool isFront = true)
        {
            await PrepareMailHelperForSendAsync(isFront);

            var mailModel = _mailMessageModel;
            SendMail(mailModel);
        }

        public void SendMail(MailMessageModel model)
        {
            try
            {
                MailMessage message = new MailMessage();

                var fromAddress = string.IsNullOrEmpty(model.FromEmail.Name) ? new MailAddress(model.FromEmail.Email) : new MailAddress(model.FromEmail.Email, model.FromEmail.Name);
                message.From = fromAddress;
                model.ToEmails.ForEach(a => message.To.Add(new MailAddress(a.Email, a.Name)));
                model.CcEmails.ForEach(a => message.CC.Add(new MailAddress(a.Email, a.Name)));
                model.BccEmails.ForEach(a => message.Bcc.Add(new MailAddress(a.Email, a.Name)));
                foreach (var item in model.Attachments)
                {
                    var att = new Attachment(new MemoryStream(item.File), item.FileName);
                    message.Attachments.Add(att);
                }

                message.Subject = model.Subject;
                message.Body = model.Body;
                message.IsBodyHtml = true;

                var smtpClient = new SmtpClient(_mailSetting.Host, _mailSetting.Port)
                {
                    EnableSsl = _mailSetting.EnableSsl
                };

                if (_mailSetting.IsAuthentication)
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_mailSetting.Username, _mailSetting.Password);
                }

                smtpClient.Send(message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private MailMessageModel _mailMessageModel
        {
            get
            {
                MailMessageModel model = new MailMessageModel
                {
                    Subject = MailHelper.PrepareSubjectWithVariables(),
                    Body = MailHelper.PrepareBodyWithVariables(),
                    ToEmails = MailHelper.ToAddresses,
                    FromEmail = MailHelper.GetFromAddress(),
                    CcEmails = MailHelper.CcAddresses,
                    BccEmails = MailHelper.BccAddresses,
                    Attachments = MailHelper.GetAttachments(),
                    Host = _mailSetting.Host,
                    Port = _mailSetting.Port,
                    Username = _mailSetting.Username,
                    Password = _mailSetting.Password
                };

                return model;
            }
        }

        private void PrepareMailHelperForSend(bool isFront = true)
        {
            var layout = isFront
                ? _notificationService.ByEmailTemplateType(EmailTemplateEmail.Layout).Content
                : _notificationService.ByEmailTemplateType(EmailTemplateEmail.AdminLayout).Content;

            MailHelper.Subject(EmailTemplate.Subject);
            MailHelper.Body(layout.Replace("{%Body%}", EmailTemplate.Content));

            AddVariables();
            MailHelper.Variables(Variables);
        }

        private async Task PrepareMailHelperForSendAsync(bool isFront = true)
        {
            var template = isFront
                ? await _notificationService.ByEmailTemplateTypeAsync(EmailTemplateEmail.Layout)
                : await _notificationService.ByEmailTemplateTypeAsync(EmailTemplateEmail.AdminLayout);

            string layout = template.Content;

            MailHelper.Subject(EmailTemplate.Subject);
            MailHelper.Body(layout.Replace("{%Body%}", EmailTemplate.Content));

            AddVariables();
            MailHelper.Variables(Variables);
        }

        private void AddVariables()
        {
            Variables.AddOrUpdate("WebsiteUrl", _siteSetting.WebsiteUrl);
            Variables.AddOrUpdate("WebsiteName", _siteSetting.SiteTitle);
            Variables.AddOrUpdate("ContactEmail", _mailSetting.ContactEmail);

            Variables.AddOrUpdate("Year", DateTime.Today.Year);
        }

        private string GenerateSocialLink(string url, string name)
        {
            return !string.IsNullOrEmpty(url)
                ? $@"<td height='80' width='35'  valign='middle' align='center'>
                        <a style='display:inline-block;text-align:center;' target='_blank' href='{url}'>
                            <img src='{_siteSetting.WebsiteUrl}dist/client/img/email/icn_{name.ToLower()}.png' style='width:16px;' alt='{name}' />
                        </a> </td>"
                : "";
        }

    }
}
