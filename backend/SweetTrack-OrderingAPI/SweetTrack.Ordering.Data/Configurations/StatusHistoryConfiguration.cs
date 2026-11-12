using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Ordering.Domain.Entities;

namespace SweetTrack.Ordering.Data.Configurations;

public class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> builder)
    {
        builder.ToTable("StatusHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.EstadoAnterior)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.EstadoNuevo)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.UsuarioModificadorId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.NotasMotivo)
            .HasMaxLength(500);

        builder.Property(h => h.TimestampUtc)
            .IsRequired();
    }
}

