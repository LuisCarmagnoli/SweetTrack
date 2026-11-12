using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Quartz;
using SweetTrack.Ordering.API.Hubs;
using SweetTrack.Ordering.Application.Interfaces;
using SweetTrack.Ordering.Application.Jobs;
using SweetTrack.Ordering.Application.Options;
using SweetTrack.Ordering.Application.Services;
using SweetTrack.Ordering.Data.Context;
using SweetTrack.Ordering.Data.Seed;

namespace SweetTrack.Ordering.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Opciones de reglas de negocio
        builder.Services.Configure<BusinessRulesOptions>(
            builder.Configuration.GetSection(BusinessRulesOptions.SectionName));

        // 2. Caché en memoria para tickets efímeros de WebSockets
        builder.Services.AddMemoryCache();

        // 3. SignalR para notificaciones en tiempo real
        builder.Services.AddSignalR();

        // 4. Configurar CORS para el cliente SPA Frontend
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

        // 5. Configuración de Swagger / OpenAPI con soporte para JWT Bearer
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SweetTrack Ordering & Logistics API",
                Version = "v1",
                Description = "Microservicio de Pedidos Mayoristas, Despacho, Tiempo Real y Remitos para SweetTrack"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa el token JWT en formato: Bearer {tu_token}"
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

        // 6. Configuración de Persistencia con SQL Server
        var connectionString = builder.Configuration.GetConnectionString("OrderingDatabase");
        builder.Services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 7. Inyección de Dependencias de Servicios de Dominio y Aplicación
        builder.Services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();
        builder.Services.AddScoped<IPdfGeneratorService, PdfWaybillGenerator>();
        builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        // 8. Tareas Programadas en Background con Quartz.NET
        builder.Services.AddQuartz(q =>
        {
            // Consolidación diaria de rutas a las 06:00 AM
            var consolidationKey = new JobKey("DailyOrderConsolidationJob");
            q.AddJob<DailyOrderConsolidationJob>(opts => opts.WithIdentity(consolidationKey));
            q.AddTrigger(opts => opts
                .ForJob(consolidationKey)
                .WithIdentity("DailyOrderConsolidationTrigger")
                .WithCronSchedule("0 0 6 * * ?")); // 06:00 AM diario

            // Limpieza de carritos abandonados cada 1 hora
            var cleanupKey = new JobKey("CleanupAbandonedCartsJob");
            q.AddJob<CleanupAbandonedCartsJob>(opts => opts.WithIdentity(cleanupKey));
            q.AddTrigger(opts => opts
                .ForJob(cleanupKey)
                .WithIdentity("CleanupAbandonedCartsTrigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()));
        });
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // 9. Configuración de Autenticación con JWT Bearer emitido por SweetTrack.AuthAPI
        var secret = builder.Configuration.GetValue<string>("JwtOptions:Secret")
                     ?? "UnaClaveSuperSecretaYLargaParaProtegerLosTokensDeSweetTrack123!@#";
        var issuer = builder.Configuration.GetValue<string>("JwtOptions:Issuer") ?? "sweettrack-auth-api";
        var audience = builder.Configuration.GetValue<string>("JwtOptions:Audience") ?? "sweettrack-frontend";

        var key = Encoding.ASCII.GetBytes(secret);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
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

            // Soporte para autenticación en WebSockets SignalR si se pasa el token en query
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/orders"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SweetTrack Ordering API v1");
            });
            app.MapOpenApi();

            // Migración automática y sembrado inicial en entorno de desarrollo si la BD está accesible
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                if (dbContext.Database.CanConnect())
                {
                    await dbContext.Database.MigrateAsync();
                    await OrderingDbSeeder.SeedAsync(dbContext);
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Aviso: No se pudo conectar a la base de datos de pedidos al iniciar.");
            }
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontendApp");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Mapeo del Hub de SignalR
        app.MapHub<OrdersHub>("/hubs/orders");

        await app.RunAsync();
    }
}
