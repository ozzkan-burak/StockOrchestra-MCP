namespace StockOrchestra.Data.Models;

public enum MovementType { In, Out }

public class StockMovement
{
  public int Id { get; set; }
  // Hangi urune ait oldugu
  public int ProductId { get; set; }
  public Product Product { get; set; } = null!;
  // Degisim miktari
  public int Quantity { get; set; }
  // Hareket tipi: Giris veya Cikis
  public MovementType Type { get; set; }
  // Hareketin gerceklestigi zaman (Trend analizi icin kritik)
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}