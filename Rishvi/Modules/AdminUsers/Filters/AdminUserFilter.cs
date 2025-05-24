using System.Linq;
using Rishvi.Modules.AdminUsers.Admin.Models.DTOs;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminUsers.Filters
{
    public class AdminUserFilter : BaseFilter<AdminUser, AdminUserFilterDto>
    {
        public AdminUserFilter(IQueryable<AdminUser> query, AdminUserFilterDto dto) : base(query, dto)
        {
        }

        internal void Id()
        {
            Query = Query.Where(w => w.Id == Dto.Id);
        }

        internal void Name()
        {
            Query = Query.Where(w => w.Name.Contains(Dto.Name));
        }

        internal void Email()
        {
            Query = Query.Where(w => w.Email.Contains(Dto.Email));
        }

        internal void RoleName()
        {
            Query = Query.Where(w => w.AdminUsersAdminRoles.Select(s => s.AdminRole.Name).Contains(Dto.RoleName));
        }

        internal void IsActive()
        {
            Query = Query.Where(w => w.IsActive == Dto.IsActive);
        }

        internal void FromCreatedAt()
        {
            Query = Query.Where(w => w.CreatedAt >= Dto.FromCreatedAt);
        }

        internal void ToCreatedAt()
        {
            Query = Query.Where(w => w.CreatedAt <= Dto.ToCreatedAt);
        }

        internal void FromUpdatedAt()
        {
            Query = Query.Where(w => w.UpdatedAt >= Dto.FromUpdatedAt);
        }

        internal void ToUpdatedAt()
        {
            Query = Query.Where(w => w.UpdatedAt <= Dto.ToUpdatedAt);
        }

        internal void FromLastLoginAt()
        {
            Query = Query.Where(w => w.LastLoginAt >= Dto.FromLastLoginAt);
        }

        internal void ToLastLoginAt()
        {
            Query = Query.Where(w => w.LastLoginAt <= Dto.ToLastLoginAt);
        }
    }
}
