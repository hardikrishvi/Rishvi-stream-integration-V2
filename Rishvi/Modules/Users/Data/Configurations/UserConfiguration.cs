using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.Users.Models;

namespace Rishvi.Modules.ErrorLogs.Data.Configurations
{
    public class UserMap : Core.Data.IEntityTypeConfiguration<User>
    {
        public void Map(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.Firstname).HasMaxLength(500);
            builder.Property(x => x.Lastname).HasMaxLength(500);
            builder.Property(x => x.EmailAddress).HasMaxLength(256);
            builder.Property(x => x.Username).HasMaxLength(256);
            builder.Property(x => x.Company).HasMaxLength(1000);
            builder.Property(x => x.LinnworksServerUrl).HasMaxLength(500);
            //builder.Property(x => x.Password).HasMaxLength(255);
            //builder.Property(x => x.Salt).HasMaxLength(128);
        }
    }
}
