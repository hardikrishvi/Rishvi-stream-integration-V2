using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;

namespace Rishvi.Modules.AdminRolePermissions.ListOrders
{
    public class AdminRoleListOrder : BaseListOrder<AdminRole>
    {
        public AdminRoleListOrder(IQueryable<AdminRole> query, BaseFilterDto dto) : base(query, dto)
        {
        }

        internal void Name()
        {
            Query = OrderBy(t => t.Name);
        }
    }
}
