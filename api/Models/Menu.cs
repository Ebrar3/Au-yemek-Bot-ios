using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

/// <summary>
/// Yemekhane öğün türleri (1: Öğle, 2: Akşam).
/// </summary>
public enum OgunTuru
{
    Ogle = 1,
    Aksam = 2
}

/// <summary>
/// Belirli bir tarih ve öğüne ait günlük yemekhane menüsünü temsil eder.
/// </summary>
public class Menu
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Tarih { get; set; }

    public OgunTuru Ogun { get; set; } = OgunTuru.Ogle;

    [Range(0, 10000, ErrorMessage = "Toplam kalori değeri geçerli aralıkta olmalıdır.")]
    public int? ToplamKalori { get; set; }

    public bool TatilMi { get; set; }

    [StringLength(200, ErrorMessage = "Menü açıklaması en fazla 200 karakter olabilir.")]
    public string? Aciklama { get; set; }

    public ICollection<MenuYemek> MenuYemekleri { get; set; } = new List<MenuYemek>();
    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
