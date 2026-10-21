namespace AuBot.Api.Models;

public class MenuYemek
{
    public int MenuId { get; set; }
    public Menu? Menu { get; set; }

    public int YemekId { get; set; }
    public Yemek? Yemek { get; set; }

    public int Sira { get; set; }
}
