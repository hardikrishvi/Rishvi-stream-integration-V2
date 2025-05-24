using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Data.Permissions;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Web.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Data.Seed
{
    public class AdminRoleSeed : BaseSeed
    {
        public AdminRoleSeed(SqlContext context) : base(context) { }

        public override void Seed()
        {
            if (!Context.Set<AdminRole>().Any())
            {
                CreateRoles();
            }

            if (!Context.Set<AdminPermission>().Any(w => w.DisplayName == "Admin Roles"))
            {
                CreateAdminPermissions();
            }
        }

        public void CreateRoles()
        {

            var adminRole = new AdminRole()
            {
                Name = "Administrator",
                SystemName = "administrator"
            };

            Context.Set<AdminRole>().Add(adminRole);

            Context.SaveChanges();
        }

        private void CreateAdminPermissions()
        {
            var listPermission = AdminPermission.Create("Admin Roles", AdminRolePermission.List);

            Context.Set<AdminPermission>().Add(listPermission);
            Context.SaveChanges();

            var _adminPermissionRepository = Context.Set<AdminPermission>().AsQueryable();

            NestedSet.SeedNode(_adminPermissionRepository, listPermission);
            Context.Set<AdminPermission>().Update(listPermission);
            Context.SaveChanges();

            var insertUpdateDeletePermissions = AdminPermission.CreateInsertUpdateDelete("Admin Roles",
                AdminRolePermission.List, listPermission.Id);

            Context.Set<AdminPermission>().AddRange(insertUpdateDeletePermissions);
            Context.SaveChanges();

            foreach (var item in insertUpdateDeletePermissions)
            {
                var _adminPermissionRepository1 = Context.Set<AdminPermission>().AsQueryable();
                NestedSet.SeedNode(_adminPermissionRepository1, item);
                Context.Set<AdminPermission>().Update(listPermission);
                Context.SaveChanges();
            }

            UpdateAdministratorRoleWithPermissions(listPermission);
            UpdateAdministratorRoleWithPermissions(insertUpdateDeletePermissions);
        }
    }
}