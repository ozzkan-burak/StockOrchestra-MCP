using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockOrchestra.Data;
using StockOrchestra.Server.Models;
using StockOrchestra.Server.Helpers;

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                       ?? "Host=localhost;Database=StockDb;Username=postgres;Password=password";

var serviceCollection = new ServiceCollection();
serviceCollection.AddDbContext<StockDbContext>(options => options.UseNpgsql(connectionString));
var serviceProvider = serviceCollection.BuildServiceProvider();

Console.Error.WriteLine("StockOrchestra MCP Sunucusu Hazir.");

while (true)
{
  var line = Console.ReadLine();
  if (string.IsNullOrEmpty(line)) break;

  try
  {
    var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, jsonOptions);
    if (request == null) continue;

    JsonRpcResponse response = request.Method switch
    {
      "initialize" => new JsonRpcResponse { Id = request.Id, Result = new { protocolVersion = "2024-11-05", serverInfo = new { name = "stock-orchestra", version = "1.0.0" } } },
      "tools/list" => HandleListTools(request.Id),
      "tools/call" => await HandleCallTool(request, serviceProvider, jsonOptions),
      _ => new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Message = "Metod Yok" } }
    };

    Console.WriteLine(JsonSerializer.Serialize(response, jsonOptions));
  }
  catch (Exception ex) { Console.Error.WriteLine($"Hata: {ex.Message}"); }
}

JsonRpcResponse HandleListTools(object? id)
{
  var tools = new object[] {
        new { name = "list_products", description = "Stok listesini getirir.", inputSchema = (object?)null },
        new { name = "create_purchase_request", description = "Satin alma talebi acar.", inputSchema = new { type = "object", properties = new { productId = new { type = "integer" }, quantity = new { type = "integer" } } } }
    };
  return new JsonRpcResponse { Id = id, Result = new { tools } };
}

async Task<JsonRpcResponse> HandleCallTool(JsonRpcRequest request, IServiceProvider sp, JsonSerializerOptions options)
{
  using var scope = sp.CreateScope();
  var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
  var toolName = request.Params?["name"]?.ToString();

  if (toolName == "list_products")
  {
    var products = await db.Products.ToListAsync();
    return new JsonRpcResponse { Id = request.Id, Result = new { content = new[] { new { type = "text", text = MarkdownTableHelper.ToMarkdownTable(products) } } } };
  }
  return new JsonRpcResponse { Id = request.Id, Error = new JsonRpcError { Message = "Bulunamadi" } };
}