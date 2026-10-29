# Aü Bot – Etkileşimli Bot Destekli Yemekhane Menü ve Değerlendirme Mobil Uygulaması

**BLM4587 IOS İle Mobil Uygulama Geliştirme I (Flutter) – Dönem Projesi (2026–2027 Güz)**

Ankara Üniversitesi öğrencilerinin ve personelinin aylık yemekhane menülerine mobil cihazlarından (Android ve iOS) hızlıca ulaşmasını, yemekleri puanlamasını ve seçenek/buton tabanlı etkileşimli sohbet botu üzerinden menü sorgulamasını sağlayan **Flutter** mobil uygulaması ve **ASP.NET Core Web API** projesi.

## Grup Bilgileri

- **Grup Adı:** Aü Bot
- **Google Drive Klasörü:** [Haftalık Proje Videoları (Drive)](https://drive.google.com/drive/folders/1ztHWc15BiB4d6YKkUulbMzqFPwshkLb2?usp=sharing)

| Ad Soyad | Öğrenci No | GitHub | Lider Olduğu Haftalar |
|---|---|---|---|
| **Arife Ebrar Üstüner** | 23291207 | [@Ebrar3](https://github.com/Ebrar3) | 1., 2., 3. ve 4. haftalar |
| **Efe Can Seymen** | 23291006 | [@efecanseymen](https://github.com/efecanseymen) | 5., 6., 7., 8. ve 9. haftalar |

## Klasör Yapısı

Depoda Web API (backend) ve Flutter mobil uygulama ayrı klasörlerde tutulmaktadır:

- `/api` : ASP.NET Core Web API (.NET 9), Entity Framework Core, MS SQL Server, JWT Kimlik Doğrulama ve Swagger (OpenAPI)
- `/mobile` : Flutter (Dart, Material 3) mobil uygulaması
- `/OrnekMenuler` : Ankara Üniversitesi aylık öğle ve akşam örnek Excel menü dosyaları

## Kullanılan Teknolojiler

- **Mobil Uygulama:** Flutter (Dart, Material 3), `http`, `provider`, `shared_preferences`, `intl`
- **Web API (Backend):** ASP.NET Core Web API (.NET 9), Swashbuckle (Swagger UI), `ClosedXML` (Excel menü ayrıştırma)
- **Veritabanı & ORM:** Microsoft SQL Server (LocalDB) & Entity Framework Core (Code-First, Migration)
- **Kimlik Doğrulama & Roller:** ASP.NET Core Identity + JWT Bearer (`Yönetici`, `Öğrenci`)
- **Harici API:** Telegram Bot API (`Telegram.Bot`)

## Sohbet Botu İşleyişi (Seçenek / Buton Tabanlı)

Sohbet ekranı açıldığında kullanıcı metin yazmak zorunda kalmadan tıklanabilir seçenek butonlarıyla ilerler:
- **Varsayılan Açılış:** Bugünün **Öğle Menüsü** (yemekler, kaloriler ve ortalama puanlar) gösterilir.
- **Seçenekler:**
  - `⭐ Puanlama`: Seçili öğündeki yemeklere 1–5 arası puan verme
  - `🌙 Akşam Yemeği` / `☀️ Öğle Yemeği`: Aynı günün diğer öğününe geçiş
  - `⏭️ Ertesi Günün Öğle Yemeği`: Sonraki günün öğle menüsüne geçiş
  - `⏮️ Önceki Günün Öğle Yemeği`: İleri tarihe geçildikten sonra önceki güne dönüş
  - `🆘 Yardım`: Hata/öneri bildirimi için ekip iletişim e-posta adresinin gösterimi

---

## Kurulum ve Çalıştırma Adımları

### 1. Depoyu Klonlayın
```bash
git clone https://github.com/Ebrar3/Au-yemek-Bot-ios.git
cd Au-yemek-Bot-ios
```

### 2. Web API (Backend) Kurulumu ve Veritabanı Migration Adımları

**Gereksinimler:** .NET 10 SDK ve SQL Server LocalDB (Visual Studio ile birlikte gelir; kontrol için `sqllocaldb info` komutu `MSSQLLocalDB` döndürmelidir).

```bash
# Depo kök klasöründe: migration aracını (dotnet-ef) yükle
dotnet tool restore

# Veritabanını (AuBotDb) oluştur, tabloları ve başlangıç verilerini ekle
cd api
dotnet tool run dotnet-ef database update

# Web API sunucusunu başlat
dotnet run --launch-profile http
```
- API adresi: `http://localhost:5000`
- **Swagger arayüzü:** `http://localhost:5000/swagger` (tüm uç noktalar buradan test edilebilir)

Bağlantı dizesi `api/appsettings.json` içindeki `DefaultConnection` alanındadır:
`Server=(localdb)\mssqllocaldb;Database=AuBotDb;Trusted_Connection=True;`

Model sınıflarında değişiklik yapıldığında yeni migration oluşturmak için:
```bash
dotnet tool run dotnet-ef migrations add DegisiklikAdi
dotnet tool run dotnet-ef database update
```

### Web API Uç Noktaları (2. hafta itibarıyla)

| Metot | Uç Nokta | Açıklama |
|---|---|---|
| GET | `/api/kategoriler` | Tüm kategoriler (yemek sayısıyla) |
| GET | `/api/kategoriler/{id}` | Tek kategori |
| POST | `/api/kategoriler` | Kategori ekle |
| PUT | `/api/kategoriler/{id}` | Kategori güncelle |
| DELETE | `/api/kategoriler/{id}` | Kategori sil (bağlı yemek varsa 409) |
| GET | `/api/yemekler?kategoriId=&ara=` | Yemekleri listele / filtrele / ara |
| GET | `/api/yemekler/{id}` | Tek yemek |
| POST | `/api/yemekler` | Yemek ekle |
| PUT | `/api/yemekler/{id}` | Yemek güncelle |
| DELETE | `/api/yemekler/{id}` | Yemek sil (menüde kullanılıyorsa 409) |
| GET | `/api/menuler/bugun?ogun=1` | Bugünün menüsü (3. haftada veritabanına bağlanacak) |
| GET | `/api/bot/adim?secenek=` | Seçenek tabanlı bot adımı |

### 3. Mobil Uygulama (Flutter) Kurulumu ve Çalıştırma
- **Önerilen Flutter Sürümü:** Flutter 3.24+ (Dart SDK `^3.5.0`)

API adresini çalıştırdığınız ortama göre `mobile/lib/config/api_config.dart` dosyasından ayarlayabilirsiniz:
- **iOS Simülatör:** `http://localhost:5000/api`
- **Android Emülatör:** `http://10.0.2.2:5000/api`

Paketleri yükleyin ve uygulamayı başlatın:
```bash
cd ../mobile
flutter pub get
flutter run
```
