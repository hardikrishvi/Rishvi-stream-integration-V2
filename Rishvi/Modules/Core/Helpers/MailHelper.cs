using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Rishvi.Modules.Configurations.Models.Configs;
using Rishvi.Modules.EmailTemplates.Models;

namespace Rishvi.Modules.Core.Helpers
{
    public class MailHelper
    {
        private readonly MailSetting _mailSetting;
        private readonly IList<MailAddress> _toAddresses;
        private readonly IList<MailAddress> _ccAddresses;
        private readonly IList<MailAddress> _bccAddresses;
        private string _subject;
        private string _body;

        private IDictionary<string, object> _variables;
        private MailAddress _from;

        public IList<Attachment> Attachments { get; set; }
        public IList<AttachmentFile> AttachmentFiles { get; set; }

        public MailHelper(MailSetting mailSetting)
        {
            _mailSetting = mailSetting;
            _toAddresses = new List<MailAddress>();
            _ccAddresses = new List<MailAddress>();
            _bccAddresses = new List<MailAddress>();
            _variables = new Dictionary<string, object>();
            AttachmentFiles = new List<AttachmentFile>();
        }

        public Result Send()
        {
            try
            {
                var message = PrepareMailMessage();

                GetSmtpClient().Send(message);

                return new Result().SetSuccess();
            }
            catch (Exception ex)
            {
                return new Result().SetError(ex.Message);
            }
        }

        public MailHelper To(string email)
        {
            _toAddresses.Add(new MailAddress(email));

            return this;
        }

        public MailHelper To(string name, string email)
        {
            _toAddresses.Add(new MailAddress(email, name));

            return this;
        }

        public MailHelper Cc(string email)
        {
            _ccAddresses.Add(new MailAddress(email));

            return this;
        }

        public MailHelper Cc(string name, string email)
        {
            _ccAddresses.Add(new MailAddress(email, name));

            return this;
        }

        public MailHelper Bcc(string email)
        {
            _bccAddresses.Add(new MailAddress(email));

            return this;
        }

        public MailHelper Bcc(string name, string email)
        {
            _bccAddresses.Add(new MailAddress(email, name));

            return this;
        }

        public MailHelper Subject(string subject)
        {
            _subject = subject;

            return this;
        }

        public MailHelper Body(string body)
        {
            _body = body;

            return this;
        }

        public MailHelper Variables(IDictionary<string, object> bodyValues)
        {
            _variables = bodyValues;

            return this;
        }

        public MailHelper AddVariables(string key, object value)
        {
            _variables.Add(key, value);

            return this;
        }

        public MailHelper From(string email)
        {
            _from = new MailAddress(email);

            return this;
        }

        public MailHelper From(string name, string email)
        {
            _from = new MailAddress(email, name);

            return this;
        }

        public string ConcatenateEmails(List<EmailClass> emails)
        {
            return emails != null && emails.Count > 0 ? emails.Select(s => s.Email).ToList().StringJoin(',') : string.Empty;
        }

        public async Task<Result> SendAsync()
        {
            try
            {
                var message = PrepareMailMessage();

                await GetSmtpClient()
                    .SendMailAsync(message);

                return new Result().SetSuccess();
            }
            catch (Exception ex)
            {
                return new Result().SetError(ex.Message);
            }
        }

        private MailMessage PrepareMailMessage()
        {
            var message = new MailMessage();

            foreach (var toAddress in _toAddresses)
            {
                message.To.Add(toAddress);
            }

            foreach (var ccAddress in _ccAddresses)
            {
                message.CC.Add(ccAddress);
            }

            foreach (var bccAddress in _bccAddresses)
            {
                message.Bcc.Add(bccAddress);
            }

            message.Subject = PrepareSubjectWithVariables();
            message.From = PrepareFrom();
            message.Body = PrepareBodyWithVariables();
            message.IsBodyHtml = true;

            if (Attachments != null)
            {
                if (Attachments.Any())
                {
                    foreach (var attachment in Attachments)
                    {
                        message.Attachments.Add(attachment);
                    }
                }
            }

            return message;
        }

        private SmtpClient GetSmtpClient()
        {
            var smtpClient = new SmtpClient(_mailSetting.Host, _mailSetting.Port)
            {
                EnableSsl = _mailSetting.EnableSsl
            };

            if (_mailSetting.IsAuthentication)
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_mailSetting.Username, _mailSetting.Password);
            }

            return smtpClient;
        }

        public MailAddress PrepareFrom()
        {
            if (_from != null)
            {
                return _from;
            }
            else
            {
                return !string.IsNullOrEmpty(_mailSetting.FromName)
                    ? new MailAddress(_mailSetting.FromEmail, _mailSetting.FromName)
                    : new MailAddress(_mailSetting.FromEmail);
            }
        }

        public string PrepareSubjectWithVariables()
        {
            return !_variables.Any() ? _subject : ReplaceWithVariable(_subject);
        }

        public string PrepareBodyWithVariables()
        {
            return !_variables.Any() ? _body : ReplaceWithVariable(_body);
        }

        private string ReplaceWithVariable(string str)
        {
            return _variables.Aggregate(str,
                (current, value) => current.Replace("{%" + value.Key + "%}", Convert.ToString(value.Value)));
        }

        public IList<ToAddress> ToAddresses => _toAddresses.Select(a => new ToAddress { Email = a.Address, Name = a.DisplayName }).ToList();
        public IList<ToAddress> CcAddresses => _ccAddresses.Select(a => new ToAddress { Email = a.Address, Name = a.DisplayName }).ToList();
        public IList<ToAddress> BccAddresses => _bccAddresses.Select(a => new ToAddress { Email = a.Address, Name = a.DisplayName }).ToList();
        public FromAddress GetFromAddress()
        {
            var from = new FromAddress();
            if (_from != null)
            {
                from.Email = _from.Address;
                from.Name = _from.DisplayName;
            }
            else
            {
                from.Email = _mailSetting.FromEmail;
                from.Name = _mailSetting.FromName;
            }

            return from;
        }
        public IList<AttachmentFile> GetAttachments() => AttachmentFiles;
    }

    public class ToAddress
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class FromAddress
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class AttachmentFile
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
    }

    public class MailMessageModel
    {
        public MailMessageModel()
        {
            Attachments = new List<AttachmentFile>();
        }
        public FromAddress FromEmail { get; set; }
        public IList<ToAddress> ToEmails { get; set; }
        public IList<ToAddress> CcEmails { get; set; }
        public IList<ToAddress> BccEmails { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IList<AttachmentFile> Attachments { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
