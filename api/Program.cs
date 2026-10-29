using AuBot.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı: SQL Server LocalDB + Entity Framework Core
builder.Services.AddDbContext<YemekBotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controller ve JSON ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger (OpenAPI) dokümantasyonu
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Aü Bot Web API", Version = "v1", Description = "Yemekhane menü, yemek ve değerlendirme servisleri" });
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
