using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Ordering.Domain.Entities;

namespace SweetTrack.Ordering.Data.Configurations;

public class DeliveryRouteConfiguration : IEntityTypeConfiguration<DeliveryRoute>
{
    public void Configure(EntityTypeBuilder<DeliveryRoute> builder)
    {
        builder.ToTable("DeliveryRoutes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.NombreChofer)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.PatenteVehiculo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.TransportistaId)
            .HasMaxLength(100);

        builder.Property(r => r.EstadoRuta)
            .HasConversion<int>()
            .IsRequired();
    }
}
