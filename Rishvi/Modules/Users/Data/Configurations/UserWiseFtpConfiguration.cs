using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.Core.Data;
using Rishvi.Modules.Users.Models;

namespace Rishvi.Modules.Users.Data.Configurations
{
    public class UserWiseFtpConfiguration : IEntityTypeConfiguration<UserWiseFtp>
    {
        public void Map(EntityTypeBuilder<UserWiseFtp> builder)
        {
            //builder.HasKey(t => new { t.UserId});
            builder.Property(x => x.Host).HasMaxLength(250);
            builder.Property(x => x.UserName).HasMaxLength(250);
            builder.Property(x => x.Password).HasMaxLength(250);
        }
    }
}
