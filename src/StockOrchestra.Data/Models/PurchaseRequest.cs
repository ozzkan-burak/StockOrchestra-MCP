namespace StockOrchestra.Data.Models;

public class PurchaseRequest
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public int Quantity { get; set; }
  public string Status { get; set; } = "Pending";
  public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
