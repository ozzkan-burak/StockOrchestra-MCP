namespace StockOrchestra.Data.Models;

public class StockMovement
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public int QuantityChange { get; set; }
  public string Reason { get; set; } = string.Empty;
  public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
