using FluentValidation;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminLoginValidator : AbstractValidator<AdminLoginDto>
    {
        public AdminLoginValidator()
        {
            RuleFor(v => v.Email).NotEmpty().EmailAddress();
            RuleFor(v => v.Password).NotEmpty();
        }
    }
}