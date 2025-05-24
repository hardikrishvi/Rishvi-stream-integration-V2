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
    public class AdminEditProfileValidator : RishviAbstractValidator<AdminEditProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminEditProfileValidator() { }

        public AdminEditProfileValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Name).NotEmpty().MaximumLength(250);

            RuleFor(v => v.Email).NotEmpty()
              .EmailAddress().MaximumLength(256);
            RuleFor(v => v).MustAsync(UniqueEmailAsync).WithMessage("{PropertyName} already used with other user.");
        }

        private async Task<bool> UniqueEmailAsync(AdminEditProfileDto dto, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<AdminUser>().AnyAsync(w => w.Email == dto.Email && w.Id != dto.Id);
            return !result;
        }

    }
}
