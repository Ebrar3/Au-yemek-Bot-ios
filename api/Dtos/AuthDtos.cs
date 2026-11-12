using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Dtos;

/// <summary>
/// Yeni kullanıcı kaydı (POST /api/auth/register) isteği modeli.
/// </summary>
public class KayitIstegiDto
{
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad 2 ile 100 karakter arasında olmalıdır.")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(150, ErrorMessage = "E-posta adresi en fazla 150 karakter olabilir.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Sifre { get; set; } = string.Empty;
}

/// <summary>
/// Kullanıcı girişi (POST /api/auth/login) isteği modeli.
/// </summary>
public class GirisIstegiDto
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Sifre { get; set; } = string.Empty;
}

/// <summary>
/// Başarılı giriş veya kayıt sonrasında istemciye (Flutter / Swagger) döndürülen JWT kimlik doğrulama yanıtı.
/// </summary>
public record AuthYanitDto(
    string Token,
    DateTime SonGecerlilik,
    string KullaniciId,
    string AdSoyad,
    string Eposta,
    string Rol,
    IReadOnlyList<string> Roller,
    bool YoneticiMi,
    string Mesaj
);

/// <summary>
/// Oturum açmış kullanıcının profil ve rol bilgileri (GET /api/auth/me).
/// </summary>
public record KullaniciBilgiDto(
    string KullaniciId,
    string AdSoyad,
    string Eposta,
    string Rol,
    IReadOnlyList<string> Roller,
    bool YoneticiMi,
    DateTime KayitTarihi
);
