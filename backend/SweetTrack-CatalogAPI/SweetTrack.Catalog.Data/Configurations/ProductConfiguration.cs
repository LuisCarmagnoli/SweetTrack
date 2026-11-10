using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SweetTrack.Catalog.Domain.Entities;

namespace SweetTrack.Catalog.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Barcode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Barcode)
            .IsUnique();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.WholesalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.BoxPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.UnitsPerBox)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(p => p.StockUnits)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.SafetyStock)
            .HasDefaultValue(10)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        // Concurrencia optimista mediante fila de versión
        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasMany(p => p.StockMovements)
            .WithOne(sm => sm.Product)
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

