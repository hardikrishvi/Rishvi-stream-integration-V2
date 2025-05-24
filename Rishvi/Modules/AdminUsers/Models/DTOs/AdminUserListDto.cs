using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.DTOs;
using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminUserListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<IdNameDto> Roles { get; set; }
    }
}
