using Rishvi.Modules.Core.Filters;
using System;

namespace Rishvi.Modules.Users.Models.DTOs
{
    public class UserAdminFilterDto : BaseFilterDto
    {
        public UserAdminFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "DESC";
        }
        public Guid? UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Company { get; set; }
        public bool? IsActive { get; set; }
        //public bool? IsDeleted { get; set; }

    }
}
