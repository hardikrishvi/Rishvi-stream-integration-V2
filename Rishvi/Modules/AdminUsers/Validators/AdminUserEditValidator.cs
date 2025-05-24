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
    public class AdminUserEditValidator : RishviAbstractValidator<AdminUserEditDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminUserEditValidator() { }

        public AdminUserEditValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Name).NotEmpty().MaximumLength(100);

            RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(100);
            RuleFor(v => v).MustAsync(UniqueEmailAsync).WithMessage("{PropertyName} already used with other user.");

            WhenAsync(CheckPasswordAsync, () =>
            {
                RuleFor(v => v.Password).NotEmpty().Length(6, 20);
            });

            //RuleFor(v => v.Roles).NotNull().Must(ValidRole).WithMessage("Invalid role selected.");
        }

        private async Task<bool> CheckPasswordAsync(AdminUserEditDto dto, CancellationToken arg2)
        {
            return await Task.FromResult(!string.IsNullOrEmpty(dto.Password));
        }

        private async Task<bool> UniqueEmailAsync(AdminUserEditDto dto, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<AdminUser>().AnyAsync(w => w.Email == dto.Email && w.Id != dto.Id);
            return !result;
        }
    }
}
