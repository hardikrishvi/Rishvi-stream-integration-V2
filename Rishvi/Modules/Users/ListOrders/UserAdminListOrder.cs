using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Core.ListOrders;
using Rishvi.Modules.Users.Models.DTOs;
using System.Linq;

namespace Rishvi.Modules.Users.ListOrders
{
    public class UserAdminListOrder : BaseListOrder<UserAdminListDto>
    {
        public UserAdminListOrder(IQueryable<UserAdminListDto> query, BaseFilterDto dto) : base(query, dto)
        {
        }

        internal void UserId()
        {
            Query = OrderBy(t => t.UserId);
        }

        internal void Firstname()
        {
            Query = OrderBy(t => t.Firstname);
        }
        internal void Lastname()
        {
            Query = OrderBy(t => t.Lastname);
        }

        internal void EmailAddress()
        {
            Query = OrderBy(t => t.EmailAddress);
        }
        internal void Username()
        {
            Query = OrderBy(t => t.Username);
        }
        internal void Company()
        {
            Query = OrderBy(t => t.Company);
        }

        internal void IsActive()
        {
            Query = OrderBy(t => t.IsActive);
        }
        //internal void IsDeleted()
        //{
        //    Query = OrderBy(t => t.IsDeleted);
        //}

        internal void CreatedAt()
        {
            Query = OrderBy(t => t.CreatedAt);
        }

        internal void UpdatedAt()
        {
            Query = OrderBy(t => t.UpdatedAt);
        }

    }
}
