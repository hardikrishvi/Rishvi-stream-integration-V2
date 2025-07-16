using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class OrderRootConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<OrderRoot>
{
    public void Map(EntityTypeBuilder<OrderRoot> builder)
    {
        // Primary Key
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.OrderId).HasDefaultValueSql("NEWID()");

        // Scalar properties
        builder.Property(x => x.NumOrderId);
        builder.Property(x => x.IsPostFilteredOut);
        builder.Property(x => x.CanFulfil);
        builder.Property(x => x.HasItems);
        builder.Property(x => x.TotalItemsSum);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // FolderName is a list of strings – EF Core doesn't support primitive collections directly
        // You can either ignore it or store it as a serialized string (e.g. JSON)
        builder.Ignore(x => x.FolderName); // Or map as a backing field if needed



        builder.HasOne(x => x.CustomerInfo)
            .WithMany()
            .HasForeignKey("CustomerInfoId");





        builder.HasOne(x => x.Fulfillment)
            .WithMany()
            .HasForeignKey("FulfillmentId");

        //builder.HasMany(x => x.Items)
        //    .WithOne()
        //    .HasForeignKey(x => x.OrderId);
    }
}
