using System;

namespace Rishvi.Modules.AdminRolePermissions.Models
{
    public class AdminRolesAdminPermissions
    {
        public Guid AdminRoleId { get; set; }
        public AdminRole AdminRole { get; set; }

        public Guid AdminPermissionId { get; set; }
        public AdminPermission AdminPermission { get; set; }

        public static AdminRolesAdminPermissions Create(AdminRole adminRole, AdminPermission adminPermission)
        {
            return new AdminRolesAdminPermissions
            {
                AdminRoleId = adminRole.Id,
                AdminPermissionId = adminPermission.Id
            };
        }

        public static AdminRolesAdminPermissions Create(AdminRole adminRole, Guid adminPermissionId)
        {
            return new AdminRolesAdminPermissions
            {
                AdminRoleId = adminRole.Id,
                AdminPermissionId = adminPermissionId
            };
        }
    }
}
