using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Encryption;
using Rishvi.Modules.Core.Validators;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminChangePasswordValidator : RishviAbstractValidator<AdminChangePasswordDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminChangePasswordValidator() { }

        public AdminChangePasswordValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.CurrentPassword).NotEmpty().MaximumLength(20);
            RuleFor(v => v).MustAsync(ValidateCurrentPasswordAsync).WithMessage("Invalid 'Current Password'.");
            RuleFor(v => v.NewPassword).NotEmpty().Length(6, 20)
                .Equal(v => v.ConfirmPassword).WithMessage("'New Password' should match to 'Confirm Password'.");
            RuleFor(v => v.ConfirmPassword).NotEmpty();
        }
        private async Task<bool> ValidateCurrentPasswordAsync(AdminChangePasswordDto dto, CancellationToken cancellation)
        {
            var adminUser = await _unitOfWork.Context.Set<AdminUser>().FindAsync(dto.Id);
            return SecurityHelper.VerifyHash(dto.CurrentPassword, adminUser.Password, adminUser.Salt);
        }
    }
}
