using Rishvi.Modules.Core.Filters;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminPermissionFilterDto : BaseFilterDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
