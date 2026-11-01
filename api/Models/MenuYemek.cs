namespace AuBot.Api.Models;

/// <summary>
/// Menü ve Yemek arasındaki çoka-çok (N–N) ilişkiyi kuran ara tablo modeli.
/// </summary>
public class MenuYemek
{
    public int MenuId { get; set; }
    public Menu? Menu { get; set; }

    public int YemekId { get; set; }
    public Yemek? Yemek { get; set; }

    /// <summary>
    /// Yemeğin o günkü menü içerisindeki gösterim sırası (1..4).
    /// </summary>
    public int Sira { get; set; }
}
