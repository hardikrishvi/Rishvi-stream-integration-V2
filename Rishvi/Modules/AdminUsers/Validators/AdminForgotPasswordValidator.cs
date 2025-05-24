using FluentValidation;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminForgotPasswordValidator : AbstractValidator<AdminForgotPasswordDto>
    {
        public AdminForgotPasswordValidator()
        {
            RuleFor(v => v.Email).NotEmpty().EmailAddress();
        }
    }
}