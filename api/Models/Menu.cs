using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

public enum OgunTuru
{
    Ogle = 1,
    Aksam = 2
}

public class Menu
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    public DateOnly Tarih { get; set; }

    public OgunTuru Ogun { get; set; } = OgunTuru.Ogle;

    public int? ToplamKalori { get; set; }

    public bool TatilMi { get; set; }

    [StringLength(200)]
    public string? Aciklama { get; set; }

    public ICollection<MenuYemek> MenuYemekleri { get; set; } = new List<MenuYemek>();
    public ICollection<Degerlendirme> Degerlendirmeler { get; set; } = new List<Degerlendirme>();
}
