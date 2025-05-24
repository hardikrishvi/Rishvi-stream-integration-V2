using Rishvi.Modules.Core.DTOs;
using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminRoleEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<AdminPermissionDropDownDto> AllPermissions { get; set; }
        public List<Guid> PermissionIds { get; set; }
    }
}
