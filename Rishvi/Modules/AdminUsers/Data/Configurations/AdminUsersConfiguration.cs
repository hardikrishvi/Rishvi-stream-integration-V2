using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.AdminUsers.Models;

namespace Rishvi.Modules.AdminUsers.Data.Configurations
{
    public class AdminUsersConfiguration : Core.Data.IEntityTypeConfiguration<AdminUser>
    {
        public void Map(EntityTypeBuilder<AdminUser> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(250).IsUnicode(false);

            builder.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(256).IsUnicode(false);

            builder.Property(t => t.Password)
                .IsRequired()
                .HasMaxLength(256).IsUnicode(false);

            builder.Property(t => t.Salt)
              .IsRequired()
              .HasMaxLength(128).IsUnicode(false);

            builder.Property(t => t.ForgotPasswordToken)
              .HasMaxLength(256).IsUnicode(false);
            ;
        }
    }
}
