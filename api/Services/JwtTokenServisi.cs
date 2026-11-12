using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuBot.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuBot.Api.Services;

/// <summary>
/// ASP.NET Core Identity kullanıcıları için rol bilgilerini içeren JWT Bearer token üretir.
/// </summary>
public class JwtTokenServisi
{
    private readonly IConfiguration _config;

    public JwtTokenServisi(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime SonGecerlilik) TokenOlustur(Kullanici kullanici, IList<string> roller)
    {
        var jwtAyar = _config.GetSection("Jwt");
        var gizliAnahtar = jwtAyar["Key"]
            ?? throw new InvalidOperationException("appsettings.json içerisinde 'Jwt:Key' yapılandırması bulunamadı.");

        var issuer = jwtAyar["Issuer"] ?? "AuBot.Api";
        var audience = jwtAyar["Audience"] ?? "AuBot.Mobile";
        var gecerlilikDakika = int.TryParse(jwtAyar["ExpireMinutes"], out var dk) ? dk : 1440;

        var ep = kullanici.Email ?? kullanici.UserName ?? string.Empty;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, kullanici.Id),
            new(JwtRegisteredClaimNames.Email, ep),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, kullanici.Id),
            new(ClaimTypes.Name, kullanici.AdSoyad),
            new(ClaimTypes.Email, ep),
            new("adSoyad", kullanici.AdSoyad)
        };

        foreach (var rol in roller)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(gizliAnahtar));
        var imza = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256);
        var sonGecerlilik = DateTime.UtcNow.AddMinutes(gecerlilikDakika);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: sonGecerlilik,
            signingCredentials: imza);

        var tokenMetni = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (tokenMetni, sonGecerlilik);
    }
}
