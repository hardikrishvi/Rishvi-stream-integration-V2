using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Admin.Validators;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core;
using Rishvi.Modules.Core.Content;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Encryption;
using Rishvi.Modules.Core.Validators;

namespace Rishvi.Modules.AdminUsers.Admin.Services.Auth
{
    public interface IAdminResetPasswordService
    {
        Task<bool> IsValidTokenAsync(string token);
        Task<Result> ResetPasswordAsync(string token, AdminResetPasswordDto dto);
    }

    public class AdminResetPasswordService : IAdminResetPasswordService
    {
        private readonly IRepository<AdminUser> _adminUserRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminResetPasswordService(
            IRepository<AdminUser> adminUserRepository,
            IUnitOfWork unitOfWork)
        {
            _adminUserRepository = adminUserRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsValidTokenAsync(string token)
        {
            return !string.IsNullOrEmpty(token) &&
                   await _adminUserRepository.AsNoTracking().AnyAsync(w => w.ForgotPasswordToken == token && w.IsActive);
        }

        public async Task<Result> ResetPasswordAsync(string token, AdminResetPasswordDto dto)
        {
            var validator = new AdminResetPasswordValidator();
            var result = await validator.ValidateResultAsync(dto);
            if (!result.Success)
            {
                return result;
            }

            var adminUser = await _adminUserRepository.AsNoTracking().FirstOrDefaultAsync(w =>
                                        w.Email == dto.Email && w.ForgotPasswordToken == token && w.IsActive);

            if (adminUser == null)
            {
                await result.SetErrorAsync(Messages.InvalidForgotPasswordToken);
                await result.SetBlankRedirectAsync();
                return result;
            }

            await SetNewPasswordAsync(adminUser, dto.Password);

            await result.SetBlankRedirectAsync();
            await result.SetSuccessAsync(Messages.SuccessResetPassword);
            return result;
        }

        private async Task SetNewPasswordAsync(AdminUser adminUser, string newPassword)
        {
            adminUser.Salt = SecurityHelper.GenerateSalt();
            adminUser.Password = SecurityHelper.GenerateHash(newPassword, adminUser.Salt);

            adminUser.ForgotPasswordToken = null;
            adminUser.WrongPasswordAttempt = 0;
            adminUser.AccountLockedOn = null;
            await _adminUserRepository.UpdateAsync(adminUser);

            await _unitOfWork.CommitAsync();
        }
    }
}
