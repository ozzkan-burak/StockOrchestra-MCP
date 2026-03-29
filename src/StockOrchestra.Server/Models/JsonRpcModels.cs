using System.Text.Json.Nodes;

namespace StockOrchestra.Server.Models;

public sealed class JsonRpcRequest
{
  public string Jsonrpc { get; set; } = "2.0";
  public string Method { get; set; } = string.Empty;
  public object? Id { get; set; }
  public JsonObject? Params { get; set; }
}

public sealed class JsonRpcResponse
{
  public string Jsonrpc { get; set; } = "2.0";
  public object? Id { get; set; }
  public object? Result { get; set; }
  public JsonRpcError? Error { get; set; }
}

public sealed class JsonRpcError
{
  public int Code { get; set; } = -32601;
  public string Message { get; set; } = string.Empty;
  public object? Data { get; set; }
}
