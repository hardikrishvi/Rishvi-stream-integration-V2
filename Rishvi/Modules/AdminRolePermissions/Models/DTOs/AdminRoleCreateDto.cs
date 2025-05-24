using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminRoleCreateDto
    {
        public string Name { get; set; }
        public List<Guid> Permissions { get; set; }
    }
}
