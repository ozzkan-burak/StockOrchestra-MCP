using System.Text;
using StockOrchestra.Data.Models;

namespace StockOrchestra.Server.Helpers;

public static class MarkdownTableHelper
{
  public static string ToMarkdownTable(IEnumerable<Product> products)
  {
    var builder = new StringBuilder();
    builder.AppendLine("| Id | Urun | Stok | Kritik Esik |");
    builder.AppendLine("|---:|---|---:|---:|");

    foreach (var product in products)
    {
      builder.AppendLine($"| {product.Id} | {product.Name} | {product.CurrentStock} | {product.CriticalThreshold} |");
    }

    return builder.ToString();
  }

  public static string ToMarkdownTable(IEnumerable<StockMovement> movements)
  {
    var builder = new StringBuilder();
    builder.AppendLine("| Id | ProductId | Miktar Degisimi | Sebep | Zaman (UTC) |");
    builder.AppendLine("|---:|---:|---:|---|---|");

    foreach (var movement in movements)
    {
      builder.AppendLine($"| {movement.Id} | {movement.ProductId} | {movement.QuantityChange} | {movement.Reason} | {movement.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} |");
    }

    return builder.ToString();
  }
}
