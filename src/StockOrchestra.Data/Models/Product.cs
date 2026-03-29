namespace StockOrchestra.Data.Models;

public class Product
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int CurrentStock { get; set; }
  public int CriticalThreshold { get; set; }
}
