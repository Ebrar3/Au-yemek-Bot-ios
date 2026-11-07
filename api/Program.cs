using AuBot.Api.Data;
using AuBot.Api.Models;
using AuBot.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı: SQL Server LocalDB + Entity Framework Core
builder.Services.AddDbContext<YemekBotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servisler: ClosedXML Excel menü ayrıştırma servisi
builder.Services.AddScoped<ExcelMenuServisi>();

// Controller ve JSON ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger (OpenAPI) dokümantasyonu
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Aü Bot Web API",
        Version = "v1",
        Description = "Ankara Üniversitesi Yemekhane menü, yemek, kategori ve değerlendirme Web API servisleri"
    });
});

// Mobil istemci (Flutter) erişimi için CORS yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileAppPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Uygulama başlarken bekleyen EF Core migration'larını uygula ve OrnekMenuler Excel dosyalarını içe aktar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<YemekBotDbContext>();
    db.Database.Migrate();

    if (!db.Menuler.Any())
    {
        var excelServisi = scope.ServiceProvider.GetRequiredService<ExcelMenuServisi>();
        var ornekKlasor = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "OrnekMenuler"));
        var ogleDosya = Path.Combine(ornekKlasor, "Ekim_2026_Ogle_Menusu.xlsx");
        var aksamDosya = Path.Combine(ornekKlasor, "Ekim_2026_Aksam_Menusu.xlsx");

        if (File.Exists(ogleDosya))
        {
            using var akis = File.OpenRead(ogleDosya);
            await excelServisi.YukleAsync(akis, Path.GetFileName(ogleDosya), OgunTuru.Ogle);
        }

        if (File.Exists(aksamDosya))
        {
            using var akis = File.OpenRead(aksamDosya);
            await excelServisi.YukleAsync(akis, Path.GetFileName(aksamDosya), OgunTuru.Aksam);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aü Bot API v1"));
}

app.UseCors("MobileAppPolicy");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
