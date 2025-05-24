using FluentValidation;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.Core.Content;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminResetPasswordValidator : AbstractValidator<AdminResetPasswordDto>
    {
        public AdminResetPasswordValidator()
        {
            RuleFor(v => v.Email).NotEmpty().EmailAddress();

            RuleFor(v => v.Password).NotEmpty().Length(6, 20)
                .Equal(v => v.ConfirmPassword).WithMessage(Messages.BothPasswordMatch);

            RuleFor(v => v.ConfirmPassword).NotEmpty().Length(6, 20);
        }
    }
}