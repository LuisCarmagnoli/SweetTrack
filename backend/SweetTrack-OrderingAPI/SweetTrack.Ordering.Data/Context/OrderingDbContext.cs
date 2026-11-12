using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SweetTrack.Ordering.Domain.Entities;

namespace SweetTrack.Ordering.Data.Context;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<DeliveryRoute> DeliveryRoutes => Set<DeliveryRoute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
