using Rishvi.Modules.AdminRolePermissions.Data.Permissions;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Web.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Data.Seed
{
    public class AdminPermissionSeed : BaseSeed
    {
        public AdminPermissionSeed(SqlContext context) : base(context)
        {
            OrderId = 2;
        }

        public override void Seed()
        {
            if (!Context.Set<AdminPermission>().Any(w => w.DisplayName == "Admin Permissions"))
            {
                CreateAdminPermissions();
            }
        }

        private void CreateAdminPermissions()
        {
            var listPermission = AdminPermission.Create("Admin Permissions", AdminPermissionPermission.List);

            Context.Set<AdminPermission>().Add(listPermission);
            Context.SaveChanges();

            var _adminPermissionRepository = Context.Set<AdminPermission>().AsQueryable();

            NestedSet.SeedNode(_adminPermissionRepository, listPermission);
            Context.Set<AdminPermission>().Update(listPermission);
            Context.SaveChanges();

            var insertUpdateDeletePermissions = AdminPermission.CreateInsertUpdateDelete("Admin Permissions",
                AdminPermissionPermission.List, listPermission.Id);

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