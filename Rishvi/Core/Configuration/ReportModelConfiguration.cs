using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rishvi.Modules.ShippingIntegrations.Models;

namespace Rishvi.Core.Configuration;

public class ReportModelConfiguration : Rishvi.Core.Data.IEntityTypeConfiguration<ReportModel>
{
    public void Map(EntityTypeBuilder<ReportModel> builder)
    {
        // Primary Key
        builder.HasKey(x => x._id);

        builder.Property(x => x._id)
               .HasMaxLength(50)
               .HasDefaultValueSql("NEWID()");

        builder.Property(x => x.AuthorizationToken).HasMaxLength(255).IsRequired(false);
        builder.Property(x => x.email).HasMaxLength(255).IsRequired(false);
        builder.Property(x => x.LinnNumOrderId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.EbayChannelOrderRef).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.OrderLineItemId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.StreamOrderId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.StreamConsignmentId).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.StreamTrackingNumber).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.StreamTrackingURL).HasMaxLength(500).IsRequired(false);

        // JSON fields
        builder.Property(x => x.EbayOrderDetaailJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.LinnOrderDetailsJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.StreamOrderCreateJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.DispatchOrderInEbayJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.DispatchOrderInLinnJson).HasColumnType("nvarchar(max)").IsRequired(false);

        // Date fields
        builder.Property(x => x.createdDate).IsRequired(false);
        builder.Property(x => x.updatedDate).IsRequired(false);
        builder.Property(x => x.DownloadLinnOrderInSystem).IsRequired(false);
        builder.Property(x => x.DownloadEbayOrderInSystem).IsRequired(false);
        builder.Property(x => x.DispatchEbayOrderInStream).IsRequired(false);
        builder.Property(x => x.DispatchEbayOrderFromStream).IsRequired(false);
        builder.Property(x => x.CreateLinnOrderInStream).IsRequired(false);
        builder.Property(x => x.LastUpdateLinnOrderForStream).IsRequired(false);
        builder.Property(x => x.DispatchLinnOrderFromStream).IsRequired(false);
        builder.Property(x => x.DispatchLinnOrderInStream).IsRequired(false);
        builder.Property(x => x.CreateEbayOrderInStream).IsRequired(false);

        // Boolean fields
        builder.Property(x => x.IsLinnOrderCreatedInStream).IsRequired(false);
        builder.Property(x => x.IsEbayOrderCreatedInStream).IsRequired(false);
        builder.Property(x => x.IsLinnOrderDispatchInStream).IsRequired(false);
        builder.Property(x => x.IsEbayOrderDispatchInStream).IsRequired(false);
        builder.Property(x => x.IsLinnOrderDispatchFromStream).IsRequired(false);
        builder.Property(x => x.IsEbayOrderDispatchFromStream).IsRequired(false);

        // Modification history fields
        builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()")
               .IsRequired(false);

        builder.Property(x => x.UpdatedAt).IsRequired(false);
    }
}