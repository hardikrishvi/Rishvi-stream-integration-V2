using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminUserCreateValidator : RishviAbstractValidator<AdminUserCreateDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminUserCreateValidator() { }

        public AdminUserCreateValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Name).NotEmpty().MaximumLength(250);
            RuleFor(v => v.Email).NotEmpty().MaximumLength(250)
                   .MustAsync(UniqueEmailAsync).WithMessage("{PropertyName} already used with other resource.");

            RuleFor(v => v.Password).NotEmpty().Length(6, 20);

            //RuleFor(v => v.Roles).NotNull()
            //  .Must(ValidRole).WithMessage("Invalid role selected.");
        }

        private async Task<bool> UniqueEmailAsync(string email, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<AdminUser>().AnyAsync(w => w.Email == email);
            return !result;
        }
    }
}
