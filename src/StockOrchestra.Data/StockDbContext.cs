using Microsoft.EntityFrameworkCore;
using StockOrchestra.Data.Models;

namespace StockOrchestra.Data;

public class StockDbContext : DbContext
{
  public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

  public DbSet<Product> Products => Set<Product>();
  public DbSet<StockMovement> StockMovements => Set<StockMovement>();
  public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Fluent API ile baslangic verileri (Seed Data) ekleyebiliriz
    modelBuilder.Entity<Product>().HasData(
        new Product { Id = 1, Name = "Laptop", CurrentStock = 15, CriticalThreshold = 5 },
        new Product { Id = 2, Name = "Mouse", CurrentStock = 3, CriticalThreshold = 10 }
    );
  }
}