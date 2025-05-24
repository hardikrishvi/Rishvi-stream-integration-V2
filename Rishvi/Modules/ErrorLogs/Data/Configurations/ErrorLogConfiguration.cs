using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.ErrorLogs.Models;
namespace Rishvi.Modules.ErrorLogs.Data.Configurations
{
    public class ErrorLogMap : Core.Data.IEntityTypeConfiguration<ErrorLog>
    {
        public void Map(EntityTypeBuilder<ErrorLog> builder)
        {
            builder.HasKey(x => x.ErrorLogID);
        }
    }
}
