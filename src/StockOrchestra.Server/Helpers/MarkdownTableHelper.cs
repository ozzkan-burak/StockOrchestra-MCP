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
}
