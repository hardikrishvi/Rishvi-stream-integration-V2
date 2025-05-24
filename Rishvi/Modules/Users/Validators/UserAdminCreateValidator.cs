using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;
using Rishvi.Modules.Users.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Rishvi.Modules.Users.Validators
{
    public class UserAdminCreateValidator : RishviAbstractValidator<UserAdminCreateDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserAdminCreateValidator() { }

        public UserAdminCreateValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Firstname).NotEmpty().MaximumLength(250);
            RuleFor(v => v.Lastname).NotEmpty().MaximumLength(250);
            RuleFor(v => v.Company).NotEmpty().MaximumLength(250);
            RuleFor(v => v.EmailAddress).NotEmpty().MaximumLength(250)
                   .MustAsync(UniqueEmailAsync).WithMessage("{PropertyName} already used with other resource.");
            RuleFor(v => v.Username).NotEmpty().MaximumLength(250)
                   .MustAsync(UniqueUserNameAsync).WithMessage("{PropertyName} already used with other resource.");
        }

        private async Task<bool> UniqueEmailAsync(string emailAddress, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<Models.User>().AnyAsync(w => w.EmailAddress == emailAddress);
            return !result;
        }

        private async Task<bool> UniqueUserNameAsync(string userName, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<Models.User>().AnyAsync(w => w.Username == userName);
            return !result;
        }
    }
}
