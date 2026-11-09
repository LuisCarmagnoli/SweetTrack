using Microsoft.EntityFrameworkCore;
using SweetTrack.Catalog.Domain.Entities;
using SweetTrack.Catalog.Domain.Enums;

namespace SweetTrack.Catalog.Data.Seed;

public static class CatalogDbSeeder
{
    public static async Task SeedAsync(CatalogDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Brands.AnyAsync(cancellationToken))
        {
            return; // Ya fue sembrado
        }

        var brands = new List<Brand>
        {
            new() { Name = "Arcor", LogoUrl = "https://images.sweettrack.local/brands/arcor.png", IsActive = true },
            new() { Name = "Mondelez", LogoUrl = "https://images.sweettrack.local/brands/mondelez.png", IsActive = true },
            new() { Name = "Felfort", LogoUrl = "https://images.sweettrack.local/brands/felfort.png", IsActive = true },
            new() { Name = "Bagley", LogoUrl = "https://images.sweettrack.local/brands/bagley.png", IsActive = true },
            new() { Name = "Terrabusi", LogoUrl = "https://images.sweettrack.local/brands/terrabusi.png", IsActive = true },
            new() { Name = "Georgalos", LogoUrl = "https://images.sweettrack.local/brands/georgalos.png", IsActive = true },
            new() { Name = "Billiken", LogoUrl = "https://images.sweettrack.local/brands/billiken.png", IsActive = true },
            new() { Name = "Guaymallén", LogoUrl = "https://images.sweettrack.local/brands/guaymallen.png", IsActive = true }
        };

        var categories = new List<Category>
        {
            new() { Name = "Chocolates", Slug = "chocolates", Description = "Barras, chocolatines, bombones y tabletas", IsActive = true },
            new() { Name = "Alfajores", Slug = "alfajores", Description = "Alfajores simples, triples y de mousse", IsActive = true },
            new() { Name = "Caramelos y Gomitas", Slug = "caramelos-y-gomitas", Description = "Gomitas masticables, caramelos ácidos y rellenos", IsActive = true },
            new() { Name = "Galletitas", Slug = "galletitas", Description = "Galletitas dulces, rellenas y secas mayoristas", IsActive = true },
            new() { Name = "Chupetines y Chicles", Slug = "chupetines-y-chicles", Description = "Chicles en tiras, chupetines con chicle y pastillas", IsActive = true },
            new() { Name = "Turrones y Obleas", Slug = "turrones-y-obleas", Description = "Turrones de maní, obleas bañadas y snacks dulces", IsActive = true }
        };

        await context.Brands.AddRangeAsync(brands, cancellationToken);
        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Mapeo por nombre para asignación segura de IDs
        var brandMap = brands.ToDictionary(b => b.Name, b => b.Id);
        var categoryMap = categories.ToDictionary(c => c.Name, c => c.Id);

        var products = new List<Product>
        {
            new()
            {
                Barcode = "7790580123456",
                Name = "Bon o Bon Leche x 18 u",
                Description = "Bombón de chocolate con leche relleno con crema de maní y oblea crujiente.",
                BrandId = brandMap["Arcor"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 420.00m,
                BoxPrice = 7560.00m,
                UnitsPerBox = 18,
                StockUnits = 150,
                SafetyStock = 25,
                ImageUrl = "https://images.sweettrack.local/products/bon-o-bon.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790580123457",
                Name = "Alfajor Águila Minirrelleno Clásico",
                Description = "Alfajor bañado en chocolate semiamargo con mousse y dulce de leche.",
                BrandId = brandMap["Arcor"],
                CategoryId = categoryMap["Alfajores"],
                WholesalePrice = 850.00m,
                BoxPrice = 10200.00m,
                UnitsPerBox = 12,
                StockUnits = 80,
                SafetyStock = 15,
                ImageUrl = "https://images.sweettrack.local/products/alfajor-aguila.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790580123458",
                Name = "Turrón Arcor de Maní 25g",
                Description = "Clásico turrón crocante con relleno de crema de maní entre obleas.",
                BrandId = brandMap["Arcor"],
                CategoryId = categoryMap["Turrones y Obleas"],
                WholesalePrice = 180.00m,
                BoxPrice = 9000.00m,
                UnitsPerBox = 50,
                StockUnits = 300,
                SafetyStock = 50,
                ImageUrl = "https://images.sweettrack.local/products/turron-arcor.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790580123459",
                Name = "Gomitas Mogul Frutales 30g",
                Description = "Gomitas masticables con formas frutales elaboradas con jugo natural.",
                BrandId = brandMap["Arcor"],
                CategoryId = categoryMap["Caramelos y Gomitas"],
                WholesalePrice = 320.00m,
                BoxPrice = 3840.00m,
                UnitsPerBox = 12,
                StockUnits = 120,
                SafetyStock = 20,
                ImageUrl = "https://images.sweettrack.local/products/mogul-frutal.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790580123460",
                Name = "Chocolate Cofler Block 38g",
                Description = "Tableta de chocolate con leche extra con maní tostado seleccionado.",
                BrandId = brandMap["Arcor"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 950.00m,
                BoxPrice = 15200.00m,
                UnitsPerBox = 16,
                StockUnits = 95,
                SafetyStock = 20,
                ImageUrl = "https://images.sweettrack.local/products/cofler-block.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7622300123461",
                Name = "Alfajor Milka Mousse Blanco",
                Description = "Alfajor triple con masa suave, relleno de mousse de chocolate y bañado en chocolate blanco Milka.",
                BrandId = brandMap["Mondelez"],
                CategoryId = categoryMap["Alfajores"],
                WholesalePrice = 980.00m,
                BoxPrice = 11760.00m,
                UnitsPerBox = 12,
                StockUnits = 60,
                SafetyStock = 15,
                ImageUrl = "https://images.sweettrack.local/products/milka-mousse.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7622300123462",
                Name = "Galletitas Oreo Original 118g",
                Description = "Galletitas sándwich de cacao oscuro rellenas de crema sabor a vainilla.",
                BrandId = brandMap["Mondelez"],
                CategoryId = categoryMap["Galletitas"],
                WholesalePrice = 750.00m,
                BoxPrice = 18000.00m,
                UnitsPerBox = 24,
                StockUnits = 200,
                SafetyStock = 30,
                ImageUrl = "https://images.sweettrack.local/products/oreo-118g.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7622300123463",
                Name = "Chocolate Cadbury Frutilla 27g",
                Description = "Tableta de chocolate con leche suave rellena con pulpa de frutilla.",
                BrandId = brandMap["Mondelez"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 890.00m,
                BoxPrice = 16020.00m,
                UnitsPerBox = 18,
                StockUnits = 75,
                SafetyStock = 15,
                ImageUrl = "https://images.sweettrack.local/products/cadbury-frutilla.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7622300123464",
                Name = "Chicles Beldent Menta 10s",
                Description = "Goma de mascar sin azúcar con microgránulos de frescura sabor menta.",
                BrandId = brandMap["Mondelez"],
                CategoryId = categoryMap["Chupetines y Chicles"],
                WholesalePrice = 380.00m,
                BoxPrice = 7600.00m,
                UnitsPerBox = 20,
                StockUnits = 180,
                SafetyStock = 25,
                ImageUrl = "https://images.sweettrack.local/products/beldent-menta.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790040123465",
                Name = "Chocolatín Jack con Muñeco 18g",
                Description = "Clásico chocolatín con leche que incluye sorpresa coleccionable en su interior.",
                BrandId = brandMap["Felfort"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 1100.00m,
                BoxPrice = 13200.00m,
                UnitsPerBox = 12,
                StockUnits = 45,
                SafetyStock = 10,
                ImageUrl = "https://images.sweettrack.local/products/jack-felfort.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790040123466",
                Name = "Barra Marroc 14g",
                Description = "Bocado de praliné elaborado artesanalmente con pasta de maní y chocolate con leche.",
                BrandId = brandMap["Felfort"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 490.00m,
                BoxPrice = 29400.00m,
                UnitsPerBox = 60,
                StockUnits = 240,
                SafetyStock = 40,
                ImageUrl = "https://images.sweettrack.local/products/marroc-felfort.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790040123467",
                Name = "Dos Corazones Felfort 28g",
                Description = "Golosina de chocolate con leche con relleno de dulce de leche y poema sorpresa.",
                BrandId = brandMap["Felfort"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 620.00m,
                BoxPrice = 14880.00m,
                UnitsPerBox = 24,
                StockUnits = 90,
                SafetyStock = 15,
                ImageUrl = "https://images.sweettrack.local/products/dos-corazones.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790580987651",
                Name = "Galletitas Chocolinas 170g",
                Description = "Galletitas secas crocantes a base de chocolate puro, base ideal para postres y kiosco.",
                BrandId = brandMap["Bagley"],
                CategoryId = categoryMap["Galletitas"],
                WholesalePrice = 820.00m,
                BoxPrice = 19680.00m,
                UnitsPerBox = 24,
                StockUnits = 160,
                SafetyStock = 30,
                ImageUrl = "https://images.sweettrack.local/products/chocolinas.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7622300987652",
                Name = "Alfajor Terrabusi Triple Torta",
                Description = "Alfajor triple bañado en chocolate semiamargo con abundante dulce de leche repostero.",
                BrandId = brandMap["Terrabusi"],
                CategoryId = categoryMap["Alfajores"],
                WholesalePrice = 790.00m,
                BoxPrice = 9480.00m,
                UnitsPerBox = 12,
                StockUnits = 110,
                SafetyStock = 20,
                ImageUrl = "https://images.sweettrack.local/products/terrabusi-torta.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790250987653",
                Name = "Mantecol Clásico 250g",
                Description = "Postre de pasta de maní aireada tradicional de confitería argentina.",
                BrandId = brandMap["Georgalos"],
                CategoryId = categoryMap["Chocolates"],
                WholesalePrice = 1650.00m,
                BoxPrice = 19800.00m,
                UnitsPerBox = 12,
                StockUnits = 50,
                SafetyStock = 10,
                ImageUrl = "https://images.sweettrack.local/products/mantecol.png",
                IsActive = true
            },
            new()
            {
                Barcode = "7790150987654",
                Name = "Alfajor Guaymallén Triple Chocolate",
                Description = "Alfajor triple de gran volumen bañado con cobertura de chocolate y dulce de leche.",
                BrandId = brandMap["Guaymallén"],
                CategoryId = categoryMap["Alfajores"],
                WholesalePrice = 420.00m,
                BoxPrice = 10080.00m,
                UnitsPerBox = 24,
                StockUnits = 350,
                SafetyStock = 50,
                ImageUrl = "https://images.sweettrack.local/products/guaymallen-triple.png",
                IsActive = true
            }
        };

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Generar movimientos de stock iniciales para auditoría
        var movements = products.Select(p => new StockMovement
        {
            ProductId = p.Id,
            MovementType = StockMovementType.IngresoManual,
            Quantity = p.StockUnits,
            ResultingStock = p.StockUnits,
            Reason = "Carga inicial de inventario del catálogo base",
            ResponsibleUserId = "system-seeder",
            TimestampUtc = DateTime.UtcNow
        }).ToList();

        await context.StockMovements.AddRangeAsync(movements, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
