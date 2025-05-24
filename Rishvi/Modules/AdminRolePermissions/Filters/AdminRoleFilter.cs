using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminRolePermissions.Filters
{
    public class AdminRoleFilter : BaseFilter<AdminRole, AdminRoleFilterDto>
    {
        public AdminRoleFilter(IQueryable<AdminRole> query, AdminRoleFilterDto dto) : base(query, dto)
        {
        }

        internal void Name()
        {
            Query = Query.Where(w => w.Name.Contains(Dto.Name));
        }
    }
}
