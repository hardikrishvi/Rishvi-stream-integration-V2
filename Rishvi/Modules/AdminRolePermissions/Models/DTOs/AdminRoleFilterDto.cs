using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminRoleFilterDto : BaseFilterDto
    {
        public string Name { get; set; }

        public AdminRoleFilterDto()
        {
            SortColumn = "Name";
            SortType = "ASC";
        }
    }
}
