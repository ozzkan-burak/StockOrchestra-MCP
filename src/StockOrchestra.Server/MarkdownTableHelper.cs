// src/StockOrchestra.Server/Helpers/MarkdownTableHelper.cs

using System.Text;
using System.Reflection;

namespace StockOrchestra.Server.Helpers;

public static class MarkdownTableHelper
{
  // Verilen nesne listesini otomatik olarak Markdown tablosuna donusturur.
  public static string ToMarkdownTable<T>(IEnumerable<T> items)
  {
    if (items == null || !items.Any()) return "Veri bulunamadi.";

    var sb = new StringBuilder();
    // Tipin tum public ozelliklerini (Properties) aliyoruz.
    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    // Tablo basliklarini olusturuyoruz.
    sb.Append("| ");
    foreach (var prop in properties)
    {
      sb.Append(prop.Name).Append(" | ");
    }
    sb.AppendLine();

    // Baslik ile veri arasindaki ayirici cizgiyi olusturuyoruz.
    sb.Append("| ");
    foreach (var prop in properties)
    {
      sb.Append("--- | ");
    }
    sb.AppendLine();

    // Veri satirlarini dolduruyoruz.
    foreach (var item in items)
    {
      sb.Append("| ");
      foreach (var prop in properties)
      {
        var value = prop.GetValue(item)?.ToString() ?? "-";
        sb.Append(value).Append(" | ");
      }
      sb.AppendLine();
    }

    return sb.ToString();
  }
}