using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

/// <summary>
/// Yemekhane menüsündeki yemek kategorilerini (Çorba, Ana Yemek, Yardımcı Yemek, Tatlı/Salata) temsil eder.
/// </summary>
public class Kategori
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 60 karakter arasında olmalıdır.")]
    public string Ad { get; set; } = string.Empty;

    /// <summary>
    /// Excel menüsündeki sütun sırası (1: Çorba, 2: Ana Yemek, 3: Yardımcı Yemek, 4: Tatlı/Salata/İçecek).
    /// </summary>
    [Range(1, 10, ErrorMessage = "Kategori sırası 1 ile 10 arasında olmalıdır.")]
    public int Sira { get; set; }

    public ICollection<Yemek> Yemekler { get; set; } = new List<Yemek>();
}
