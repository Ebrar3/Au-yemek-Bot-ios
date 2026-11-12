using System.Text;
using AuBot.Api.Data;
using AuBot.Api.Models;
using AuBot.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı: SQL Server LocalDB + Entity Framework Core
builder.Services.AddDbContext<YemekBotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ASP.NET Core Identity (Kullanıcı ve Rol Yönetimi: Yönetici, Öğrenci)
builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<YemekBotDbContext>()
.AddDefaultTokenProviders();

// 3. JWT Bearer Kimlik Doğrulama Yapılandırması
var jwtAyar = builder.Configuration.GetSection("Jwt");
var jwtAnahtar = jwtAyar["Key"] ?? "AuBot_AnkaraUniversitesi_2026_CokGizliJwtAnahtari_EnAz32Karakter!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtAyar["Issuer"] ?? "AuBot.Api",
        ValidAudience = jwtAyar["Audience"] ?? "AuBot.Mobile",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAnahtar)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// 4. Uygulama Servisleri
builder.Services.AddScoped<ExcelMenuServisi>();
builder.Services.AddScoped<JwtTokenServisi>();

// 5. Controller ve JSON ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 6. Swagger (OpenAPI) + JWT Bearer Authorize Desteği
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aü Bot Web API",
        Version = "v1",
        Description = "Ankara Üniversitesi Yemekhane menü, yemek, kategori, değerlendirme ve JWT kimlik doğrulama Web API servisleri"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Bearer token giriniz. Örnek: /api/auth/login uç noktasından aldığınız token değerini buraya yapıştırın."
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = []
    });
});

// 7. Mobil istemci (Flutter) erişimi için CORS yapılandırması
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

// 8. Uygulama başlarken bekleyen EF Core migration'larını uygula, rolleri/örnek kullanıcıları ve Excel menülerini yükle
using (var scope = app.Services.CreateScope())
{
    var servisler = scope.ServiceProvider;
    var db = servisler.GetRequiredService<YemekBotDbContext>();
    await db.Database.MigrateAsync();

    await IdentitySeedVerisi.RolleriVeOrnekKullanicilariOlusturAsync(servisler);

    if (!await db.Menuler.AnyAsync())
    {
        var excelServisi = servisler.GetRequiredService<ExcelMenuServisi>();
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
