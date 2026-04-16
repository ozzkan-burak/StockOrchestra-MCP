using AnalyticalStore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using AnalyticalStore.Infrastructure.Data;

/// <summary>
/// Analytical Store Service Konfigürasyonu
/// </summary>
var builder = Host.CreateApplicationBuilder(args);

// Redis bağlantısı
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// Repository
var postgresConnectionString = builder.Configuration.GetValue<string>("Postgres:ConnectionString") 
    ?? "Host=localhost;Database=stockorchestra_ledger;Username=stockorchestra;Password=stockorchestra_secure_pass_2024";
builder.Services.AddSingleton(sp => new PriceRepository(postgresConnectionString));

// Worker yapılandırması
builder.Services.Configure<WorkerConfig>(builder.Configuration.GetSection("Worker"));

// Worker servisi
builder.Services.AddHostedService<Worker>();

// Logging
builder.Logging.AddConsole();

var host = builder.Build();

host.Run();