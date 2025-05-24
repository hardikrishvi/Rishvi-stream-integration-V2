using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Notifications;
using Rishvi.Modules.AdminUsers.Admin.Validators;
using Rishvi.Modules.AdminUsers.Data.Emails;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;
using Rishvi.Modules.EmailTemplates.Admin.Services;

namespace Rishvi.Modules.AdminUsers.Admin.Services.Auth
{
    public interface IAdminForgotPasswordService
    {
        Task<Result> ForgotPasswordAsync(AdminForgotPasswordDto dto);
    }

    public class AdminForgotPasswordService : IAdminForgotPasswordService
    {
        private readonly IRepository<AdminUser> _adminUserRepository;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly AdminUserForgotPasswordNotification _adminUserForgotPasswordNotification;
        private readonly IUnitOfWork _unitOfWork;

        public AdminForgotPasswordService(
            IRepository<AdminUser> adminUserRepository,
            IEmailTemplateService emailTemplateService,
            AdminUserForgotPasswordNotification adminUserForgotPasswordNotification,
            IUnitOfWork unitOfWork)
        {
            _adminUserRepository = adminUserRepository;
            _emailTemplateService = emailTemplateService;
            _adminUserForgotPasswordNotification = adminUserForgotPasswordNotification;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ForgotPasswordAsync(AdminForgotPasswordDto dto)
        {
            var validator = new AdminForgotPasswordValidator();
            var result = await validator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var adminUser = await _adminUserRepository.AsNoTracking()
                .FirstOrDefaultAsync(w => w.IsActive && w.Email == dto.Email);

            if (adminUser != null)
            {
                if (await _emailTemplateService.IsEmailTemplateActiveAsync(AdminUserEmail.AdminUserResetPassword))
                {
                    await GenerateAndSaveForgotPasswordTokenAsync(adminUser);
                    await _adminUserForgotPasswordNotification.SendWithPrepareAsync(adminUser, false);
                    //BackgroundJob.Enqueue<AdminUserForgotPasswordNotification>(job => job.SendWithPrepareAsync(adminUser, false));
                }
            }

            //if (!result.Success) return result;
            result = await ForgotPasswordResponseAsync(dto.Email);

            return result;
        }

        private async Task<string> GenerateAndSaveForgotPasswordTokenAsync(AdminUser adminUser)
        {
            var passwordResetToken = Core.Helpers.StringHelper.RandomString(12);

            adminUser.ForgotPasswordToken = passwordResetToken;
            await _adminUserRepository.UpdateAsync(adminUser);
            await _unitOfWork.CommitAsync();

            return passwordResetToken;
        }

        private async Task<Result> ForgotPasswordResponseAsync(string email)
        {
            return await new Result().SetSuccessAsync(
                $"If there is an account associated with {email} you will receive an email with a link to reset your password.");
        }
    }
}