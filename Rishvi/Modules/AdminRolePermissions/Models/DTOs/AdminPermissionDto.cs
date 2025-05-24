using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminPermissionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }

        public int? Left { get; set; }
        public int? Right { get; set; }
        public int Depth { get; set; }

        public bool? IsParentSelected { get; set; }
        public Guid? ParentId { get; set; }
    }
}
