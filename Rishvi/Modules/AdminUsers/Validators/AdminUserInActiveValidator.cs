using System;
using System.Collections.Generic;
using Rishvi.Modules.Core.Validators;

namespace Rishvi.Modules.AdminUsers.Admin.Validators
{
    public class AdminUserInActiveValidator : RishviAbstractValidator<List<Guid>>
    {
        public AdminUserInActiveValidator()
        {
        }
    }
}
