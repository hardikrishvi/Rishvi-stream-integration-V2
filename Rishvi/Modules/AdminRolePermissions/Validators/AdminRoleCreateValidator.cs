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
    public class AdminRoleCreateValidator : AbstractValidator<AdminRoleCreateDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminRoleCreateValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
            RuleFor(v => v.Permissions).NotNull()
                .MustAsync(ValidPermissionsAsync).WithMessage("Invalid permission selected.");
        }

        private async Task<bool> ValidPermissionsAsync(List<Guid> permissionIds, CancellationToken cancellation)
        {
            var data = await _unitOfWork.Context.Set<AdminPermission>().CountAsync(w =>
                        permissionIds.Contains(w.Id)) == permissionIds.Count;
            return data;
        }
    }
}
