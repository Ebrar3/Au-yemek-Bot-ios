using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

/// <summary>
/// Yemekhanede sunulan tekil bir yemeği, kategorisini ve kalori bilgisini temsil eder.
/// </summary>
public class Yemek
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Yemek adı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Yemek adı 2 ile 150 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    /// <summary>
    /// Türkçe karakter duyarsız arama ve mükerrer kayıt kontrolü için normalize edilmiş yemek adı.
    /// </summary>
    [StringLength(150)]
    public string AramaAdi { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir kategori seçilmelidir.")]
    public int KategoriId { get; set; }

    public Kategori? Kategori { get; set; }

    [Range(0, 3000, ErrorMessage = "Kalori değeri 0 ile 3000 kcal arasında olmalıdır.")]
    public int? Kalori { get; set; }

    public ICollection<MenuYemek> MenuYemekleri { get; set; } = new List<MenuYemek>();
    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
