using Rishvi.Modules.Core.DTOs;
using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminRolePermissions.Admin.Models.DTOs
{
    public class AdminRoleListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<IdNameDto> Permissions { get; set; }
    }
}
