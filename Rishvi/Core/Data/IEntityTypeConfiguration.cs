using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rishvi.Core.Data
{
    public interface IEntityTypeConfiguration<TEntityType> where TEntityType : class
    {
        void Map(EntityTypeBuilder<TEntityType> builder);
    }
}
