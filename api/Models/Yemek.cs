using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

public class Yemek
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Yemek adı zorunludur.")]
    [StringLength(150)]
    public string Ad { get; set; } = string.Empty;

    [StringLength(150)]
    public string AramaAdi { get; set; } = string.Empty;

    public int KategoriId { get; set; }
    public Kategori? Kategori { get; set; }

    [Range(0, 3000, ErrorMessage = "Kalori değeri 0 ile 3000 arasında olmalıdır.")]
    public int? Kalori { get; set; }

    public ICollection<MenuYemek> MenuYemekleri { get; set; } = new List<MenuYemek>();
    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
