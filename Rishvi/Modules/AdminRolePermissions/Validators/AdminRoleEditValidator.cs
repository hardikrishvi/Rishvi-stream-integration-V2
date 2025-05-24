using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Validators
{
    public class AdminRoleEditValidator : AbstractValidator<AdminRoleEditDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminRoleEditValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Name).NotEmpty().MaximumLength(100);

            RuleFor(v => v.PermissionIds).NotNull()
                    .MustAsync(ValidPermissionsAsync).WithMessage("Invalid permission selected.");
        }

        private async Task<bool> ValidPermissionsAsync(List<Guid> permissionIds, CancellationToken cancellation)
        {
            return await _unitOfWork.Context.Set<AdminPermission>().CountAsync(w =>
                       permissionIds.Contains(w.Id)) == permissionIds.Count;
        }
    }
}
