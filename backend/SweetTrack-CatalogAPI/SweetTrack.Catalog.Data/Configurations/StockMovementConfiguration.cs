using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Catalog.Domain.Entities;

namespace SweetTrack.Catalog.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.MovementType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .IsRequired();

        builder.Property(sm => sm.ResultingStock)
            .IsRequired();

        builder.Property(sm => sm.Reason)
            .HasMaxLength(250);

        builder.Property(sm => sm.ResponsibleUserId)
            .HasMaxLength(100);

        builder.Property(sm => sm.TimestampUtc)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
    }
}
