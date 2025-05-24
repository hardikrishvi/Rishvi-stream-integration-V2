using System;
using System.Linq;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.AdminUsers.Models;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Core.Data.Seed;
using Rishvi.Modules.Core.Encryption;

namespace Rishvi.Modules.AdminUsers.Data.Seed
{
    public class AdminUserSeed : BaseSeed
    {
        public AdminUserSeed(SqlContext context) : base(context)
        {
            OrderId = 3;
        }

        public override void Seed()
        {
            if (Context.Set<AdminUser>().Any())
            {
                return;
            }

            var salt = SecurityHelper.GenerateSalt();
            var adminUserSpinx = new AdminUser()
            {
                Name = "Rishvi Admin",
                Email = "admin@rishvi.co.uk",
                Password = SecurityHelper.GenerateHash("Rishvi@123", salt),
                Salt = salt,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            Context.Set<AdminUser>().Add(adminUserSpinx);
            Context.SaveChanges();

            var administratorRole = Context.Set<AdminRole>().FirstOrDefault(w => w.SystemName == "administrator");
            if (administratorRole == null)
            {
                return;
            }

            Context.Set<AdminUsersAdminRoles>().Add(new AdminUsersAdminRoles
            {
                AdminRoleId = administratorRole.Id,
                AdminUserId = adminUserSpinx.Id
            });

            Context.SaveChanges();
        }
    }
}
