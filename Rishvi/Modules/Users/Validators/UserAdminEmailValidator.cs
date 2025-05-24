using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;
using Rishvi.Modules.Users.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Rishvi.Modules.Users.Validators
{
    public class UserAdminEmailValidator : RishviAbstractValidator<UserAdminEmailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserAdminEmailValidator() { }

        public UserAdminEmailValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            RuleFor(v => v.EmailAddress).EmailAddress().NotEmpty().MaximumLength(250)
                   .MustAsync(CheckEmailAddressAsync).WithMessage("Invalid Email Address");
            RuleFor(v => v.KlaviyoEmail).EmailAddress().NotEmpty().NotNull();
            RuleFor(v => v.KlaviyoPublicKey).NotEmpty().NotNull();
        }

        private async Task<bool> CheckEmailAddressAsync(UserAdminEmailDto dto, string emailAddress, CancellationToken cancellation)
        {
            var result = await _unitOfWork.Context.Set<Models.User>().AnyAsync(w => w.EmailAddress == emailAddress);
            return result;
        }
    }
}
