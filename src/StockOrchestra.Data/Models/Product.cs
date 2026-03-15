namespace StockOrchestra.Data.Models;

public class Product
{
  // Urunun benzersiz kimligi
  public int Id { get; set; }
  // Urun adi
  public string Name { get; set; } = string.Empty;
  // Anlik stok miktari
  public int CurrentStock { get; set; }
  // Siparis verilmesi gereken alt sinir
  public int CriticalThreshold { get; set; }
  // Stok hareketleri ile iliski
  public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}