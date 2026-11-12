using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuBot.Api.Models;

/// <summary>
/// Aü Bot uygulama kullanıcısı (ASP.NET Core Identity AspNetUsers tablosu).
/// Öğrenci, akademik/idari personel ve yemekhane yöneticilerini temsil eder.
/// </summary>
public class Kullanici : IdentityUser
{
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad 2 ile 100 karakter arasında olmalıdır.")]
    public string AdSoyad { get; set; } = string.Empty;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
