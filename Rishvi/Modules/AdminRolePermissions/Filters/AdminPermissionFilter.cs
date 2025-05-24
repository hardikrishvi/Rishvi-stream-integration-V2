using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Filters
{
    public class AdminPermissionFilter : BaseFilter<AdminPermission, AdminPermissionFilterDto>
    {
        public AdminPermissionFilter(IQueryable<AdminPermission> query, AdminPermissionFilterDto dto) : base(query, dto)
        {
        }

        internal void Name()
        {
            Query = Query.Where(w => w.Name.Contains(Dto.Name));
        }

        internal void DisplayName()
        {
            Query = Query.Where(w => w.DisplayName.Contains(Dto.DisplayName));
        }
    }
}
