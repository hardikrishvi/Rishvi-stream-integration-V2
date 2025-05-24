using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.AdminRolePermissions.Models;
using Rishvi.Modules.Core.Data;

namespace Rishvi.Modules.AdminRolePermissions.Data.Configurations
{
    public class AdminUsersAdminRolesConfiguration : IEntityTypeConfiguration<AdminUsersAdminRoles>
    {
        public void Map(EntityTypeBuilder<AdminUsersAdminRoles> builder)
        {
            builder.HasKey(t => new { t.AdminUserId, t.AdminRoleId });
        }
    }
}
