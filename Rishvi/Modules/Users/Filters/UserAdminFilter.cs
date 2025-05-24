using Rishvi.Modules.Core.Filters;
using Rishvi.Modules.Users.Models;
using Rishvi.Modules.Users.Models.DTOs;
using System.Linq;

namespace Rishvi.Modules.Users.Filters
{
    public class UserAdminFilter : BaseFilter<UserAdminListDto, UserAdminFilterDto>
    {
        public UserAdminFilter(IQueryable<UserAdminListDto> query, UserAdminFilterDto dto) : base(query, dto)
        {
        }
        internal void UserId()
        {
            Query = Query.Where(w => w.UserId == Dto.UserId);
        }

        internal void Firstname()
        {
            Query = Query.Where(w => w.Firstname.Contains(Dto.Firstname));
        }

        internal void Lastname()
        {
            Query = Query.Where(w => w.Lastname.Contains(Dto.Lastname));
        }
        internal void EmailAddress()
        {
            Query = Query.Where(w => w.EmailAddress.Contains(Dto.EmailAddress));
        }
        internal void Username()
        {
            Query = Query.Where(w => w.Username.Contains(Dto.Username));
        }
        internal void Company()
        {
            Query = Query.Where(w => w.Company.Contains(Dto.Company));
        }
        //internal void IsDeleted()
        //{
        //    Query = Query.Where(w => w.IsDeleted == Dto.IsDeleted);
        //}

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
    }
}
