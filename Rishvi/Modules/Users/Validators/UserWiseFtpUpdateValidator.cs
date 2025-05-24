using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Validators;
using Rishvi.Modules.Users.Models.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Rishvi.Modules.Users.Validators
{
    public class UserWiseFtpUpdateValidator : RishviAbstractValidator<UserWiseFtpUpdateDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserWiseFtpUpdateValidator() { }

        public UserWiseFtpUpdateValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            string pattern = @"(?<protocol>s?ftp):\/\/(?:(?<user>[^@\s]+)@)?(?<host>[^\?\s\:]+)(?:\:(?<port>[0-9]+))?(?:\?(?<password>.+))?";
            RuleFor(v => v.Host).NotEmpty().NotNull().Matches(pattern);
            RuleFor(v => v.Port).NotEmpty().NotNull();
            RuleFor(v => v.UserName).NotEmpty().NotNull();
            RuleFor(v => v.Password).NotEmpty().NotNull();

        }

    }
}
