using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Data.Permissions;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.AdminUsers.Data.Permissions;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Web.Modules.Core.Data;

namespace Rishvi.Modules.AdminUsers.Data.Seed
{
    public class UserPermissionsSeed : BaseSeed
    {
        public UserPermissionsSeed(SqlContext context) : base(context)
        {
            OrderId = 520;
        }

        public override void Seed()
        {
            if (!Context.Set<AdminPermission>().Any(w => w.DisplayName == "Users"))
            {
                CreateAdminPermissions();
            }
        }

        private void CreateAdminPermissions()
        {
            var listPermission = AdminPermission.Create("Users", UsersPermission.List);

            Context.Set<AdminPermission>().Add(listPermission);
            Context.SaveChanges();

            var _adminPermissionRepository = Context.Set<AdminPermission>().AsQueryable();

            NestedSet.SeedNode(_adminPermissionRepository, listPermission);
            Context.Set<AdminPermission>().Update(listPermission);
            Context.SaveChanges();

            var insertUpdateDeletePermissions = AdminPermission.CreateInsertUpdateDelete("Users",
                UsersPermission.List, listPermission.Id);

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