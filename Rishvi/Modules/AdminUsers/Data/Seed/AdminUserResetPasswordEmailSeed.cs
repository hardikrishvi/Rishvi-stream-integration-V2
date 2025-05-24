using System;
using System.Collections.Generic;
using System.Linq;
using Rishvi.Modules.AdminUsers.Data.Emails;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Modules.EmailTemplates.Models;

namespace Rishvi.Modules.AdminUsers.Data.Seed
{
    public class AdminUserResetPasswordEmailSeed : BaseSeed
    {
        public AdminUserResetPasswordEmailSeed(SqlContext context) : base(context) { }

        public override void Seed()
        {
            if (Context.Set<EmailTemplate>().Any(s => s.EmailTemplateType == AdminUserEmail.AdminUserResetPassword))
            {
                return;
            }

            var content = ReadFile("AdminUsers", "AdminUserResetPassword.html");

            Context.Set<EmailTemplate>().Add(new EmailTemplate
            {
                Name = "Admin : Reset Password",
                EmailTemplateType = AdminUserEmail.AdminUserResetPassword,
                TemplateType = TemplateType.Admin,
                ToEmails = new List<EmailClass> { new EmailClass() { Email = "contact@rishvi.co.uk" } },
                Subject = "Password Reset Request on {%WebsiteName%}",
                Content = content,
                IsActive = true,
                HideEmailSection = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });

            Context.SaveChanges();
        }
    }
}
