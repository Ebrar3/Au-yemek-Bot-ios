var builder = WebApplication.CreateBuilder(args);

// Controller ve JSON ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

app.UseHttpsRedirection();
app.UseCors("MobileAppPolicy");
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    Proje = "Aü Bot – Yemekhane Menü ve Değerlendirme Web API",
    Durum = "Çalışıyor",
    UcNoktalar = new[] { "/api/menuler", "/api/yemekler", "/api/bot/adim" }
}));

app.Run();
