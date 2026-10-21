using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

public class Kategori
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(60)]
    public string Ad { get; set; } = string.Empty;

    /// <summary>
    /// Excel menüsündeki sütun sırası (1: Çorba, 2: Ana Yemek, 3: Yardımcı Yemek, 4: Tatlı/Salata/İçecek).
    /// </summary>
    public int Sira { get; set; }

    public ICollection<Yemek> Yemekler { get; set; } = new List<Yemek>();
}
