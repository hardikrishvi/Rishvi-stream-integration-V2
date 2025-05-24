using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.ErrorLogs.Models;
namespace Rishvi.Modules.ErrorLogs.Data.Configurations
{
    public class SystemLogMap : Core.Data.IEntityTypeConfiguration<SystemLog>
    {
        public void Map(EntityTypeBuilder<SystemLog> builder)
        {
            builder.HasKey(x => x.SystemLogID);
            builder.Property(x => x.ModuleName).HasMaxLength(500);
            builder.Property(x => x.Status).HasMaxLength(1000);
        }
    }
}
