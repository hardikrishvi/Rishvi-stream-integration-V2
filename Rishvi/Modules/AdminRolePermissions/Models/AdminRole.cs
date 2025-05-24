using System;
using System.Collections.Generic;
using System.Linq;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Models
{
    public class AdminRole : IModificationHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<AdminUsersAdminRoles> AdminUsersAdminRoles { get; set; }

        public List<AdminRolesAdminPermissions> AdminRolesAdminPermissionses { get; set; }

        public static AdminRolesAdminPermissions[] AddPermissionsToRole(AdminRole adminRole, AdminPermission[] adminPermissions)
        {
            return adminPermissions.Select(adminPermission => AdminRolesAdminPermissions.Create(adminRole, adminPermission)).ToArray();
        }

        public static AdminRolesAdminPermissions[] AddPermissionsToRole(AdminRole adminRole, Guid[] adminPermissions)
        {
            return adminPermissions.Select(adminPermission => AdminRolesAdminPermissions.Create(adminRole, adminPermission)).ToArray();
        }
    }
}
