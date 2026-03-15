// src/StockOrchestra.Server/Models/JsonRpcModels.cs

namespace StockOrchestra.Server.Models;

// JSON-RPC 2.0 standart istek yapisi.
public class JsonRpcRequest
{
  // Protokol versiyonu, genellikle "2.0".
  public string Jsonrpc { get; set; } = "2.0";
  // Istemci tarafindan atanan benzersiz istek kimligi.
  public object? Id { get; set; }
  // Cagrilacak metodun adi (ornegin: "tools/call").
  public string Method { get; set; } = string.Empty;
  // Metoda gonderilen parametreler.
  public Dictionary<string, object>? Params { get; set; }
}

// Sunucunun istemciye dondugu cevap yapisi.
public class JsonRpcResponse
{
  public string Jsonrpc { get; set; } = "2.0";
  // Istekle ayni kimlik (Id) geri donulmelidir.
  public object? Id { get; set; }
  // Basarili sonuc verisi.
  public object? Result { get; set; }
  // Hata durumunda donulecek hata nesnesi.
  public JsonRpcError? Error { get; set; }
}

public class JsonRpcError
{
  public int Code { get; set; }
  public string Message { get; set; } = string.Empty;
  public object? Data { get; set; }
}