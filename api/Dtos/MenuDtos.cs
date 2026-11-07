using System.ComponentModel.DataAnnotations;
using AuBot.Api.Models;
using Microsoft.AspNetCore.Http;

namespace AuBot.Api.Dtos;

/// <summary>
/// Menüdeki tek bir yemek (menüdeki sırası, kategorisi, kalorisi ve ortalama puanıyla).
/// </summary>
public record MenuYemekDto(
    int Id,
    string Ad,
    int KategoriId,
    string KategoriAdi,
    int Sira,
    int? Kalori,
    double? OrtalamaPuan
);

/// <summary>
/// Bir günün öğle veya akşam menüsü. Mobil uygulama bu nesneyi "Günlük Menü Kartı" olarak gösterir.
/// </summary>
public record MenuDto(
    int Id,
    DateOnly Tarih,
    OgunTuru Ogun,
    string OgunAdi,
    int? ToplamKalori,
    bool TatilMi,
    string? Aciklama,
    IReadOnlyList<MenuYemekDto> Yemekler
);

/// <summary>
/// Yeni menü ekleme (POST) ve menü güncelleme (PUT) isteği.
/// Toplam kalori istemciden alınmaz; seçilen yemeklerden sunucuda hesaplanır.
/// </summary>
public class MenuKaydetDto : IValidatableObject
{
    [Required(ErrorMessage = "Tarih zorunludur (ör. 2026-10-15).")]
    public DateOnly? Tarih { get; set; }

    [Required(ErrorMessage = "Öğün zorunludur.")]
    [EnumDataType(typeof(OgunTuru), ErrorMessage = "Öğün 1 (Öğle) veya 2 (Akşam) olmalıdır.")]
    public OgunTuru? Ogun { get; set; }

    public bool TatilMi { get; set; }

    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    public string? Aciklama { get; set; }

    [MaxLength(6, ErrorMessage = "Bir menüde en fazla 6 yemek olabilir.")]
    public List<int> YemekIdleri { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TatilMi)
        {
            if (YemekIdleri.Count > 0)
                yield return new ValidationResult("Tatil günü menüsüne yemek eklenemez.", new[] { nameof(YemekIdleri) });
            if (string.IsNullOrWhiteSpace(Aciklama))
                yield return new ValidationResult("Tatil günleri için açıklama girilmelidir (ör. '29 Ekim Cumhuriyet Bayramı').", new[] { nameof(Aciklama) });
        }
        else if (YemekIdleri.Count == 0)
        {
            yield return new ValidationResult("Tatil olmayan bir menüde en az bir yemek bulunmalıdır.", new[] { nameof(YemekIdleri) });
        }

        if (YemekIdleri.Count != YemekIdleri.Distinct().Count())
            yield return new ValidationResult("Aynı yemek bir menüye birden fazla kez eklenemez.", new[] { nameof(YemekIdleri) });
    }
}

/// <summary>
/// Excel menü yükleme isteği (multipart/form-data).
/// </summary>
public class ExcelYuklemeIstegiDto
{
    [Required(ErrorMessage = "Excel dosyası seçilmelidir.")]
    public IFormFile? Dosya { get; set; }

    /// <summary>Boş bırakılırsa dosya başlığından ("AKŞAM YEMEK MENÜSÜ") otomatik algılanır.</summary>
    [EnumDataType(typeof(OgunTuru), ErrorMessage = "Öğün 1 (Öğle) veya 2 (Akşam) olmalıdır.")]
    public OgunTuru? Ogun { get; set; }
}

/// <summary>
/// Excel aktarımının özeti: kaç menü eklendi/güncellendi ve dosyada bulunan sorunlar.
/// </summary>
public record ExcelYuklemeSonucDto(
    string DosyaAdi,
    OgunTuru Ogun,
    int Yil,
    int Ay,
    int EklenenMenu,
    int GuncellenenMenu,
    int TatilGunu,
    int YeniYemek,
    IReadOnlyList<string> Uyarilar
);
