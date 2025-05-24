using System;
using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminUserFilterDto : BaseFilterDto
    {
        public AdminUserFilterDto()
        {
            SortColumn = "CreatedAt";
            SortType = "DESC";
        }

        public Guid? Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public string RoleName { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? FromLastLoginAt { get; set; }
        public DateTime? ToLastLoginAt { get; set; }
    }
}
