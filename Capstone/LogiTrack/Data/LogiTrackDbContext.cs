using LogiTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiTrack.Data;

public sealed class LogiTrackDbContext(DbContextOptions<LogiTrackDbContext> options) : DbContext(options)
{
    public DbSet<FulfillmentCenter> FulfillmentCenters => Set<FulfillmentCenter>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FulfillmentCenter>()
            .HasIndex(center => center.Code)
            .IsUnique();
        modelBuilder.Entity<InventoryItem>()
            .HasIndex(item => new { item.Sku, item.FulfillmentCenterId })
            .IsUnique();
        modelBuilder.Entity<InventoryItem>().Property(item => item.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<OrderLine>().Property(line => line.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<OrderLine>()
            .HasOne(line => line.InventoryItem)
            .WithMany()
            .HasForeignKey(line => line.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}