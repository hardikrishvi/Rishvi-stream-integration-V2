using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Validators
{
    public class AdminPermissionValidator : AbstractValidator<AdminPermissionDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminPermissionValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(v => v.IsParentSelected).NotEmpty().WithName("Parent Selection");

            WhenAsync(CheckParentAsync, () =>
               {
                   RuleFor(v => v.ParentId).NotEmpty().WithName("Parent Permission").NotEqual(v => v.Id);
                   RuleFor(v => v).MustAsync(ValidParentAsync).WithMessage("Invalid parent permission selected.");
               });

            RuleFor(v => v.Name).NotEmpty().MaximumLength(100);

            RuleFor(v => v).MustAsync(UniqueNameAsync)
                .WithMessage("System name already used with other permission.");

            RuleFor(v => v.DisplayName).NotEmpty().MaximumLength(100);
        }

        private async Task<bool> CheckParentAsync(AdminPermissionDto dto, CancellationToken cancellation)
        {
            return await Task.FromResult(dto.IsParentSelected == true);
        }
        private async Task<bool> UniqueNameAsync(AdminPermissionDto dto, CancellationToken cancellation)
        {
            var query = _unitOfWork.Context.Set<AdminPermission>().Where(w => w.Name == dto.Name);
            var result = dto.Id == Guid.Parse("00000000-0000-0000-0000-000000000000") ? await query.AnyAsync() : await query.AnyAsync(w => w.Id != dto.Id);

            return !result;
        }
        private async Task<bool> ValidParentAsync(AdminPermissionDto dto, CancellationToken cancellation)
        {
            if (dto.Id == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                return true;
            }

            var childPermissionIds = await _unitOfWork.Context.Set<AdminPermission>()
                .Where(w => w.Left >= dto.Left && w.Right <= dto.Right).Select(s => s.Id).ToListAsync();

            return childPermissionIds.IndexOf(dto.ParentId ?? Guid.Parse("00000000-0000-0000-0000-000000000000")) == -1;
        }

    }
}
