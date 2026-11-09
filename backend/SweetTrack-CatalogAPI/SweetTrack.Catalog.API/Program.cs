using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SweetTrack.Catalog.Application.Channels;
using SweetTrack.Catalog.Application.Interfaces;
using SweetTrack.Catalog.Application.Mappings;
using SweetTrack.Catalog.Application.Services;
using SweetTrack.Catalog.Application.Workers;
using SweetTrack.Catalog.Data;
using SweetTrack.Catalog.Data.Seed;

namespace SweetTrack.Catalog.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Registro de Mapeos con Mapster
        MappingConfig.RegisterMappings();

        // 2. Configurar CORS para el cliente SPA Frontend
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontendApp", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 3. Configuración de Swagger / OpenAPI con soporte para JWT Bearer
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SweetTrack Catalog & Inventory API",
                Version = "v1",
                Description = "Microservicio de Catálogo Mayorista e Inventario Físico para SweetTrack"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa el token JWT en este formato: Bearer {tu_token}"
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", doc, null),
                    new List<string>()
                }
            });
        });

        builder.Services.AddOpenApi();

        // 4. Configuración de Persistencia (Entity Framework Core con SQL Server)
        var connectionString = builder.Configuration.GetConnectionString("CatalogDatabase");
        builder.Services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 5. Inyección de Dependencias de Casos de Uso y Servicios
        builder.Services.AddScoped<ICatalogService, CatalogService>();
        builder.Services.AddScoped<IStockService, StockService>();

        // 6. Canal Asíncrono e Importador Masivo en Segundo Plano (Worker)
        builder.Services.AddSingleton<IBulkProductImportChannel, BulkProductImportChannel>();
        builder.Services.AddHostedService<BulkImportWorker>();

        // 7. Configuración de Autenticación con JWT Bearer emitido por SweetTrack.AuthAPI
        var secret = builder.Configuration.GetValue<string>("JwtOptions:Secret");
        var issuer = builder.Configuration.GetValue<string>("JwtOptions:Issuer");
        var audience = builder.Configuration.GetValue<string>("JwtOptions:Audience");

        var key = Encoding.ASCII.GetBytes(secret ?? throw new InvalidOperationException("JwtOptions:Secret no configurado."));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Solo para entorno de desarrollo local
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Pipeline HTTP
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SweetTrack Catalog API v1");
            });
            app.MapOpenApi();

            // Migración automática y sembrado de catálogo en entorno de desarrollo si la BD está accesible
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
                if (dbContext.Database.CanConnect())
                {
                    await dbContext.Database.MigrateAsync();
                    await CatalogDbSeeder.SeedAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Aviso: No se pudo conectar a la base de datos de catálogo al iniciar. Las migraciones automáticas se omitieron.");
            }
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowFrontendApp");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}

