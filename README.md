Proje Başlığı: StockOrchestra-MCP: Otonom Stok Yönetim Köprüsü
Kısa Tanım: LLM (Large Language Model) dünyası ile kurumsal veri kaynakları arasındaki izolasyonu yıkan, .NET tabanlı bir MCP (Model Context Protocol) sunucusudur. Bu proje, yapay zekanın standart bir protokol üzerinden PostgreSQL veritabanına güvenli erişimini sağlayarak otonom stok takibi ve satın alma talebi oluşturma yetenekleri kazanmasını hedefler.

Senaryo: Şirket personeli Claude Desktop veya benzeri bir MCP destekli arayüz üzerinden "Kritik seviyenin altına düşen ürünleri listele ve eksik miktar kadar satın alma talebi oluştur" talimatını verir. Ajan, MCP sunucusu üzerinden PostgreSQL'e bağlanır, stokları analiz eder, eksik miktarları hesaplar ve satın alma tablosuna gerekli kayıtları atarak işlemi onay için kullanıcıya raporlar.

Teknik Stack:

Runtime: .NET 8.0 / 9.0

Protocol: Model Context Protocol (MCP)

Database: PostgreSQL (Docker)

Communication: JSON-RPC over stdio

ORM: Entity Framework Core

Kurulum ve Başlatma
1. Veritabanı (Docker)
Projenin kök dizininde yer alan Docker Compose dosyası ile PostgreSQL'i ayağa kaldırın:

Bash
docker-compose up -d
2. Sunucu Yapılandırması
MCP sunucusu Claude Desktop gibi istemciler tarafından şu komutla tetiklenir:

JSON
{
  "mcpServers": {
    "stock-orchestra": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/StockOrchestra.Server.csproj"],
      "env": {
        "CONNECTION_STRING": "Host=localhost;Database=StockDb;Username=postgres;Password=password"
      }
    }
  }
}
Proje Yetenekleri (Tools)
list_products: Tüm ürün listesini ve mevcut stok durumlarını çeker.

get_low_stock: Belirlenen kritik eşiğin altındaki ürünleri filtreler.

create_purchase_request: Eksik ürünler için otomatik satın alma kaydı oluşturur.
