using Rishvi.Modules.AdminUsers.Models;
using System;

namespace Rishvi.Modules.AdminRolePermissions.Models
{
    public class AdminUsersAdminRoles
    {
        public Guid AdminRoleId { get; set; }
        public AdminRole AdminRole { get; set; }

        public Guid AdminUserId { get; set; }
        public AdminUser AdminUser { get; set; }
    }
}
