namespace StockOrchestra.Data.Models;

public enum RequestStatus { Pending, Approved, Completed }

public class PurchaseRequest
{
  public int Id { get; set; }
  public int ProductId { get; set; }
  public Product Product { get; set; } = null!;
  // Talep edilen miktar
  public int RequestedQuantity { get; set; }
  // Talebin durumu
  public RequestStatus Status { get; set; } = RequestStatus.Pending;
  public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}