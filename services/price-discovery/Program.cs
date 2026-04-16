using PriceDiscovery;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Price Discovery Service Konfigürasyonu ve DI Container kurulumu.
/// </summary>
/// <remarks>
/// Mimari Mantık:
/// - Host.CreateApplicationBuilder: Worker service için hazır hosting
/// - HttpClient Factory: HttpClient yaşam döngüsünü yönetir
/// - Logging: Yapılandırılabilir loglama
/// - Configuration: appsettings.json'dan yükleme
/// </remarks>
var builder = Host.CreateApplicationBuilder(args);

// HttpClient Factory ekle - her fetcher için ayrı instance
builder.Services.AddHttpClient();

// Worker servisini kaydet
builder.Services.AddHostedService<Worker>();

// Logging yapılandırması
builder.Logging.AddConsole();

var host = builder.Build();

host.Run();