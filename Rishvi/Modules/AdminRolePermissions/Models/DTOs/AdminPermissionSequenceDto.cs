using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminPermissionSequenceDto
    {
        public AdminPermissionSequenceItemDto Item { get; set; }
        public IList<AdminPermissionSequenceDto> Children { get; set; }
    }

    public class AdminPermissionSequenceItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
