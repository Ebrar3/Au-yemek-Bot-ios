using System.Security.Claims;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using AuBot.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuBot.Api.Controllers;

/// <summary>
/// ASP.NET Core Identity ve JWT Bearer tabanlı kullanıcı kayıt, giriş ve oturum doğrulama uç noktaları (4. Hafta Görevi).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly JwtTokenServisi _jwtServisi;

    public AuthController(UserManager<Kullanici> userManager, JwtTokenServisi jwtServisi)
    {
        _userManager = userManager;
        _jwtServisi = jwtServisi;
    }

    // POST: api/auth/register
    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur, varsayılan olarak "Öğrenci" rolünü atar ve JWT token döndürür.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthYanitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthYanitDto>> Register([FromBody] KayitIstegiDto dto)
    {
        var temizEposta = dto.Eposta.Trim().ToLowerInvariant();
        var mevcut = await _userManager.FindByEmailAsync(temizEposta);
        if (mevcut is not null)
        {
            return Conflict(new { Mesaj = "Bu e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut." });
        }

        var yeniKullanici = new Kullanici
        {
            UserName = temizEposta,
            Email = temizEposta,
            AdSoyad = dto.AdSoyad.Trim(),
            EmailConfirmed = true
        };

        var sonuc = await _userManager.CreateAsync(yeniKullanici, dto.Sifre);
        if (!sonuc.Succeeded)
        {
            var hatalar = sonuc.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new
            {
                Mesaj = "Kullanıcı kaydı oluşturulamadı. Şifre kurallarını kontrol ediniz.",
                Hatalar = hatalar
            });
        }

        // Yeni kayıt olan kullanıcılara varsayılan olarak "Öğrenci" rolü atanır
        await _userManager.AddToRoleAsync(yeniKullanici, Roller.Ogrenci);
        var roller = await _userManager.GetRolesAsync(yeniKullanici);

        var (token, sonGecerlilik) = _jwtServisi.TokenOlustur(yeniKullanici, roller);
        var yanit = YanitOlustur(yeniKullanici, roller, token, sonGecerlilik, "Kayıt işlemi başarıyla tamamlandı.");

        return StatusCode(StatusCodes.Status201Created, yanit);
    }

    // POST: api/auth/login
    /// <summary>
    /// E-posta ve şifre doğrulaması yaparak kullanıcıya JWT Bearer token ve rol bilgilerini döndürür.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthYanitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthYanitDto>> Login([FromBody] GirisIstegiDto dto)
    {
        var temizEposta = dto.Eposta.Trim().ToLowerInvariant();
        var kullanici = await _userManager.FindByEmailAsync(temizEposta);

        if (kullanici is null || !await _userManager.CheckPasswordAsync(kullanici, dto.Sifre))
        {
            return Unauthorized(new { Mesaj = "E-posta adresi veya şifre hatalı." });
        }

        var roller = await _userManager.GetRolesAsync(kullanici);
        if (roller.Count == 0)
        {
            await _userManager.AddToRoleAsync(kullanici, Roller.Ogrenci);
            roller = await _userManager.GetRolesAsync(kullanici);
        }

        var (token, sonGecerlilik) = _jwtServisi.TokenOlustur(kullanici, roller);
        var yanit = YanitOlustur(kullanici, roller, token, sonGecerlilik, "Giriş başarılı.");

        return Ok(yanit);
    }

    // GET: api/auth/me
    /// <summary>
    /// Geçerli JWT Bearer token'a sahip oturum açmış kullanıcının profil ve rol bilgilerini döndürür.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(KullaniciBilgiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<KullaniciBilgiDto>> GetMe()
    {
        var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(kullaniciId))
            return Unauthorized(new { Mesaj = "Geçersiz oturum." });

        var kullanici = await _userManager.FindByIdAsync(kullaniciId);
        if (kullanici is null)
            return Unauthorized(new { Mesaj = "Kullanıcı bulunamadı." });

        var roller = await _userManager.GetRolesAsync(kullanici);
        var yoneticiMi = roller.Contains(Roller.Yonetici);
        var anaRol = yoneticiMi ? Roller.Yonetici : (roller.FirstOrDefault() ?? Roller.Ogrenci);

        return Ok(new KullaniciBilgiDto(
            kullanici.Id,
            kullanici.AdSoyad,
            kullanici.Email ?? string.Empty,
            anaRol,
            roller.ToList(),
            yoneticiMi,
            kullanici.KayitTarihi
        ));
    }

    // GET: api/auth/roller
    /// <summary>
    /// Sistemde tanımlı kullanıcı rollerini (Yönetici, Öğrenci) listeler.
    /// </summary>
    [HttpGet("roller")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetRoller()
    {
        return Ok(Roller.TumRoller);
    }

    private static AuthYanitDto YanitOlustur(
        Kullanici kullanici,
        IList<string> roller,
        string token,
        DateTime sonGecerlilik,
        string mesaj)
    {
        var yoneticiMi = roller.Contains(Roller.Yonetici);
        var anaRol = yoneticiMi ? Roller.Yonetici : (roller.FirstOrDefault() ?? Roller.Ogrenci);

        return new AuthYanitDto(
            token,
            sonGecerlilik,
            kullanici.Id,
            kullanici.AdSoyad,
            kullanici.Email ?? string.Empty,
            anaRol,
            roller.ToList(),
            yoneticiMi,
            mesaj
        );
    }
}
