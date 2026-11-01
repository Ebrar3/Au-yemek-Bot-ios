using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Dtos;

/// <summary>
/// Kategori listeleme ve detay sorgularında döndürülen veri transfer nesnesi.
/// </summary>
public record KategoriDto(
    int Id,
    string Ad,
    int Sira,
    int YemekSayisi
);

/// <summary>
/// Yeni kategori oluşturma (POST) ve mevcut kategoriyi güncelleme (PUT) isteği modeli.
/// </summary>
public class KategoriKaydetDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Kategori adı 2–60 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Sıra değeri 1 ile 10 arasında olmalıdır.")]
    public int Sira { get; set; }
}

/// <summary>
/// Yemek listeleme ve detay sorgularında kategori adı ve ortalama puanla birlikte döndürülen DTO.
/// </summary>
public record YemekDto(
    int Id,
    string Ad,
    int KategoriId,
    string KategoriAdi,
    int? Kalori,
    double? OrtalamaPuan
);

/// <summary>
/// Yeni yemek ekleme (POST) ve mevcut yemeği güncelleme (PUT) isteği modeli.
/// </summary>
public class YemekKaydetDto
{
    [Required(ErrorMessage = "Yemek adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Yemek adı 2–150 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçiniz.")]
    public int KategoriId { get; set; }

    [Range(0, 3000, ErrorMessage = "Kalori değeri 0 ile 3000 kcal arasında olmalıdır.")]
    public int? Kalori { get; set; }
}
