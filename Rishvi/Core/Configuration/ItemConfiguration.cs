using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Models;

namespace Rishvi.Configuration;


public class ItemConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<Item>
{
    public void Map(EntityTypeBuilder<Item> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        builder.Property(x => x.ItemId).IsRequired();

        // Scalar properties
        builder.Property(x => x.ItemNumber).HasMaxLength(100);
        builder.Property(x => x.SKU).HasMaxLength(100);
        builder.Property(x => x.Title).HasMaxLength(255);
        builder.Property(x => x.Quantity);
        builder.Property(x => x.CategoryName).HasMaxLength(100);
        builder.Property(x => x.StockLevelsSpecified);
        builder.Property(x => x.OnOrder);
        builder.Property(x => x.InOrderBook);
        builder.Property(x => x.Level);
        builder.Property(x => x.MinimumLevel);
        builder.Property(x => x.AvailableStock);
        builder.Property(x => x.PricePerUnit);
        builder.Property(x => x.UnitCost);
        builder.Property(x => x.Cost);
        builder.Property(x => x.CostIncTax);
        builder.Property(x => x.Weight);
        builder.Property(x => x.BarcodeNumber).HasMaxLength(100);
        builder.Property(x => x.ChannelSKU).HasMaxLength(100);
        builder.Property(x => x.ChannelTitle).HasMaxLength(255);
        builder.Property(x => x.BinRack).HasMaxLength(100);
        builder.Property(x => x.ImageId).HasMaxLength(100);
        builder.Property(x => x.RowId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.StockItemId);
        builder.Property(x => x.StockItemIntId);

        // Timestamp fields
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Ignore self-referencing navigation
        builder.Ignore(x => x.CompositeSubItems);
    }
}
