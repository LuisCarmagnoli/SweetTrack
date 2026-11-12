using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Ordering.Domain.Entities;

namespace SweetTrack.Ordering.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.NumeroPedido)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.NumeroPedido)
            .IsUnique();

        builder.Property(o => o.KioskUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.RazonSocialKiosco)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.CuitKiosco)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.DireccionEntrega)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.MontoIva)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.MontoTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.EstadoActual)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.MetodoPago)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.NotasEntrega)
            .HasMaxLength(500);

        builder.Property(o => o.RowVersion)
            .IsRowVersion();

        // Relación 1 a N con OrderItem
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación 1 a N con StatusHistory (auditoría inmutable)
        builder.HasMany(o => o.StatusHistories)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación opcional con DeliveryRoute
        builder.HasOne(o => o.DeliveryRoute)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.DeliveryRouteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

