using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Dtos;

/// <summary>Kategori listeleme/detay yanıtı.</summary>
public record KategoriDto(int Id, string Ad, int Sira, int YemekSayisi);

/// <summary>Kategori ekleme/güncelleme isteği.</summary>
public class KategoriKaydetDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Kategori adı 2–60 karakter olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Sıra 1 ile 10 arasında olmalıdır.")]
    public int Sira { get; set; }
}

/// <summary>Yemek listeleme/detay yanıtı.</summary>
public record YemekDto(int Id, string Ad, int KategoriId, string KategoriAdi, int? Kalori, double? OrtalamaPuan);

/// <summary>Yemek ekleme/güncelleme isteği.</summary>
public class YemekKaydetDto
{
    [Required(ErrorMessage = "Yemek adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Yemek adı 2–150 karakter olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçiniz.")]
    public int KategoriId { get; set; }

    [Range(0, 3000, ErrorMessage = "Kalori değeri 0 ile 3000 arasında olmalıdır.")]
    public int? Kalori { get; set; }
}
