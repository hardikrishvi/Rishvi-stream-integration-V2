using System.ComponentModel;
using System.Threading.Tasks;
using Rishvi.Modules.AdminUsers.Data.Emails;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Helpers;
using Rishvi.Modules.Core.Notifications;
using Rishvi.Modules.EmailTemplates.Services;

namespace Rishvi.Modules.AdminUsers.Admin.Notifications
{
    public class AdminUserForgotPasswordNotification : Notification
    {
        private readonly INotificationService _notificationService;

        public AdminUserForgotPasswordNotification(
            INotificationService notificationService) : base(notificationService)
        {
            _notificationService = notificationService;
        }

        #region Non-Async
        public AdminUserForgotPasswordNotification Prepare(AdminUser adminUser)
        {
            EmailTemplate = _notificationService.ByEmailTemplateType(AdminUserEmail.AdminUserResetPassword);

            MailHelper.To(adminUser.Name, adminUser.Email);

            Variables.Add("ResetPasswordLink", "reset-password/" + adminUser.ForgotPasswordToken);

            return this;
        }

        [DisplayName("Forgot Password Notification")]
        public Result SendWithPrepare(AdminUser adminUser, bool isFront = true)
        {
            Prepare(adminUser).SendAndForgetResponse(isFront);
            return new Result();
        }
        #endregion

        public async Task<AdminUserForgotPasswordNotification> PrepareAsync(AdminUser adminUser)
        {
            EmailTemplate = await _notificationService.ByEmailTemplateTypeAsync(AdminUserEmail.AdminUserResetPassword);

            MailHelper.To(adminUser.Name, adminUser.Email);

            Variables.Add("ResetPasswordLink", $"reset-password/{adminUser.ForgotPasswordToken}");

            //Required to convert file to byte array for File
            //MailHelper._attachmentFiles = new List<AttachmentFile>
            //{
            //    new AttachmentFile(){ File = file, FileName="attachment.pdf"}
            //};

            return this;
        }

        [DisplayName("Forgot Password Notification")]
        public async Task<Result> SendWithPrepareAsync(AdminUser adminUser, bool isFront = true)
        {
            var result = await PrepareAsync(adminUser);
            await result.SendAndForgetResponseAsync(isFront);
            return new Result();
        }
    }
}
