using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.Configurations.Models;

namespace Rishvi.Modules.Configurations.Data.Configurations
{
    public class ConfigurationsConfiguration : Core.Data.IEntityTypeConfiguration<Configuration>
    {
        public void Map(EntityTypeBuilder<Configuration> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            //Set unique value
            builder.HasIndex(x => x.ConfigurationType, "IX_ConfigurationsUniqueConfigurationType").IsUnique();
        }
    }
}