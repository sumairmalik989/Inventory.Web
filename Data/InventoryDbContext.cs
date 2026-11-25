using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>()
            .Property(p => p.StockVersion).IsConcurrencyToken();
        modelBuilder.Entity<Product>()
            .Property(p => p.Sku).HasMaxLength(40);
        modelBuilder.Entity<StockMovement>()
            .HasOne(m => m.Product)
            .WithMany(p => p.Movements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StockMovement>()
            .HasIndex(m => new { m.ProductId, m.CreatedUtc });
    }
}
