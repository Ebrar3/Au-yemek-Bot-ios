using System.ComponentModel.DataAnnotations;

namespace AuBot.Api.Models;

/// <summary>
/// Öğrencilerin belirli bir menüdeki yemeğe verdiği puan (1–5) ve yorum kaydını temsil eder.
/// </summary>
public class Degerlendirme
{
    public int Id { get; set; }

    public string? KullaniciId { get; set; }

    public int MenuId { get; set; }
    public Menu? Menu { get; set; }

    public int YemekId { get; set; }
    public Yemek? Yemek { get; set; }

    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
    public int Puan { get; set; }

    [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir.")]
    public string? Yorum { get; set; }

    public DateTime Tarih { get; set; } = DateTime.UtcNow;

    public bool Onaylandi { get; set; } = true;
}
