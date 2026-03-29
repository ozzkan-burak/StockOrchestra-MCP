using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockOrchestra.Data;

public class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
{
  public StockDbContext CreateDbContext(string[] args)
  {
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                           ?? "Host=localhost;Database=StockDb;Username=postgres;Password=password";

    var optionsBuilder = new DbContextOptionsBuilder<StockDbContext>();
    optionsBuilder.UseNpgsql(connectionString);

    return new StockDbContext(optionsBuilder.Options);
  }
}
