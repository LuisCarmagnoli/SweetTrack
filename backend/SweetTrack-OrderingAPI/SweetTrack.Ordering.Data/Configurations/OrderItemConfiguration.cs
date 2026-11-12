using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Ordering.Domain.Entities;

namespace SweetTrack.Ordering.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CodigoBarrasSnapshot)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.NombreProductoSnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.PrecioUnitarioSnapshot)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.SubtotalLinea)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
