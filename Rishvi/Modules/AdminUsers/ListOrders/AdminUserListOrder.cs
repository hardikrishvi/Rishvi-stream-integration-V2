using System.Linq;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;

namespace Rishvi.Modules.AdminUsers.ListOrders
{
    public class AdminUserListOrder : BaseListOrder<AdminUser>
    {
        public AdminUserListOrder(IQueryable<AdminUser> query, BaseFilterDto dto) : base(query, dto)
        {
        }

        internal void Id()
        {
            Query = OrderBy(t => t.Id);
        }

        internal void Name()
        {
            Query = OrderBy(t => t.Name);
        }

        internal void Email()
        {
            Query = OrderBy(t => t.Email);
        }

        internal void IsActive()
        {
            Query = OrderBy(t => t.IsActive);
        }

        internal void CreatedAt()
        {
            Query = OrderBy(t => t.CreatedAt);
        }

        internal void UpdatedAt()
        {
            Query = OrderBy(t => t.UpdatedAt);
        }

        internal void LastLoginAt()
        {
            Query = OrderBy(t => t.LastLoginAt);
        }
    }
}
