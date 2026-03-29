// src/StockOrchestra.Server/Program.cs

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockOrchestra.Data;
using StockOrchestra.Data.Models;
using StockOrchestra.Server.Helpers;
using StockOrchestra.Server.Models;

// JSON serileştirme ayarları: CamelCase zorunluluğu ve null değerlerin yoksayılması.
var jsonOptions = new JsonSerializerOptions
{
  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// Veritabanı bağlantı dizesi: Senin belirttiğin Port=5434 üzerinden yapılandırıldı.
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? "Host=localhost;Port=5434;Database=StockDb;Username=postgres;Password=password";

var serviceCollection = new ServiceCollection();
serviceCollection.AddDbContext<StockDbContext>(options => options.UseNpgsql(connectionString));
var serviceProvider = serviceCollection.BuildServiceProvider();

// Log mesajları sadece stderr kanalına basılmalıdır.
Console.Error.WriteLine("StockOrchestra MCP Sunucusu Hazir. Tum araclar yuklendi.");

var stdin = Console.OpenStandardInput();

// Ana mesaj dinleme döngüsü.
while (true)
{
  var line = await ReadIncomingMessage(stdin);
  if (line == null) break;

  try
  {
    var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, jsonOptions);
    if (request == null) continue;

    JsonRpcResponse response = request.Method switch
    {
      "initialize" => new JsonRpcResponse
      {
        Id = request.Id,
        Result = new
        {
          protocolVersion = "2024-11-05",
          serverInfo = new { name = "stock-orchestra", version = "1.0.0" },
          capabilities = new { tools = new { listChanged = false } }
        }
      },
      "tools/list" => HandleListTools(request.Id),
      "tools/call" => await HandleCallTool(request, serviceProvider),
      _ => new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Message = "Metod Yok" } }
    };

    // Yanıtı standart çıktıya JSON olarak yazıyoruz.
    Console.WriteLine(JsonSerializer.Serialize(response, jsonOptions));
    // Linux ve asenkron akışlar için tamponu boşaltmak kritiktir.
    Console.Out.Flush();
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine($"Kritik Hata: {ex.Message}");
  }
}

// Content-Length başlığını okuyan ve paketi tam bayt sayısında alan güvenli okuma metodu.
async Task<string?> ReadIncomingMessage(Stream input)
{
  while (true)
  {
    var line = await ReadAsciiLine(input);
    if (line == null) return null;

    if (string.IsNullOrWhiteSpace(line)) continue;

    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
    {
      var lengthText = line.Split(':', 2)[1].Trim();
      if (!int.TryParse(lengthText, out var contentLength) || contentLength <= 0) continue;

      // Header ve body arasındaki boş satırı atla.
      while (true)
      {
        var headerLine = await ReadAsciiLine(input);
        if (headerLine == null) return null;
        if (headerLine.Length == 0) break;
      }

      var buffer = new byte[contentLength];
      var totalRead = 0;
      while (totalRead < contentLength)
      {
        var read = await input.ReadAsync(buffer.AsMemory(totalRead, contentLength - totalRead));
        if (read == 0) return null;
        totalRead += read;
      }

      return Encoding.UTF8.GetString(buffer);
    }
    return line;
  }
}

// Satır sonu karakterine kadar ASCII karakterleri okuyan yardımcı metod.
async Task<string?> ReadAsciiLine(Stream input)
{
  var bytes = new List<byte>(128);
  var buffer = new byte[1];

  while (true)
  {
    var read = await input.ReadAsync(buffer.AsMemory(0, 1));
    if (read == 0) return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
    if (buffer[0] == (byte)'\n') return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
    bytes.Add(buffer[0]);
  }
}

// AI modeline sunulacak araçların listesi ve şemaları.
JsonRpcResponse HandleListTools(object? id)
{
  var tools = new object[] {
        new {
            name = "list_products",
            description = "Tum stok durumunu listeler.",
            inputSchema = new { type = "object", properties = new { } }
        },
        new {
            name = "get_stock_trend",
            description = "Bir urunun son hareketlerini analiz eder.",
            inputSchema = new {
                type = "object",
                properties = new { productId = new { type = "integer" } },
                required = new[] { "productId" }
            }
        },
        new {
            name = "execute_query",
            description = "Sadece SELECT iceren SQL sorgularini calistirir.",
            inputSchema = new {
                type = "object",
                properties = new { sql = new { type = "string" } },
                required = new[] { "sql" }
            }
        },
        new {
            name = "create_purchase_request",
            description = "Eksik urunler icin yeni bir satin alma talebi acar. Mukerrer kayit kontrolu yapar.",
            inputSchema = new {
                type = "object",
                properties = new {
                    productId = new { type = "integer" },
                    quantity = new { type = "integer" }
                },
                required = new[] { "productId", "quantity" }
            }
        }
    };

  return new JsonRpcResponse { Id = id, Result = new { tools } };
}

// Gelen araç çağrılarını işleyen ana mantık.
async Task<JsonRpcResponse> HandleCallTool(JsonRpcRequest request, IServiceProvider sp)
{
  using var scope = sp.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
  var toolName = request.Params?["name"]?.ToString();
  var args = request.Params?["arguments"] as JsonObject;

  try
  {
    if (toolName == "list_products")
    {
      var products = await db.Products.ToListAsync();
      return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = MarkdownTableHelper.ToMarkdownTable(products) } } } };
    }

    if (toolName == "get_stock_trend")
    {
      var pId = args?["productId"]?.GetValue<int>() ?? 0;
      var movements = await db.StockMovements.Where(m => m.ProductId == pId).OrderByDescending(m => m.CreatedAtUtc).Take(10).ToListAsync();
      return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = MarkdownTableHelper.ToMarkdownTable(movements) } } } };
    }

    if (toolName == "execute_query")
    {
      var sql = args?["sql"]?.GetValue<string>();
      if (string.IsNullOrEmpty(sql) || !sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = "HATA: Sadece SELECT sorgulari." } } } };

      var result = await db.Products.FromSqlRaw(sql).ToListAsync();
      return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = MarkdownTableHelper.ToMarkdownTable(result) } } } };
    }

    if (toolName == "create_purchase_request")
    {
      var pId = args?["productId"]?.GetValue<int>() ?? 0;
      var qty = args?["quantity"]?.GetValue<int>() ?? 0;

      if (pId <= 0 || qty <= 0)
        return new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Message = "Gecersiz productId/quantity" } };

      var existingRequest = await db.PurchaseRequests.AnyAsync(r => r.ProductId == pId && r.Status == "Pending");
      if (existingRequest)
        return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = $"UYARI: Urun ID {pId} icin zaten beklemede talep var." } } } };

      var newRequest = new PurchaseRequest { ProductId = pId, Quantity = qty, Status = "Pending" };
      db.PurchaseRequests.Add(newRequest);
      await db.SaveChangesAsync();

      return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = $"BASARILI: Talep olusturuldu (ID: {newRequest.Id})." } } } };
    }
  }
  catch (Exception ex) { return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = $"Hata: {ex.Message}" } } } }; }

  return new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Message = "Tool Yok" } };
}