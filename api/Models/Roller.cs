namespace AuBot.Api.Models;

/// <summary>
/// Sistemde tanımlı kullanıcı rolleri (4. Hafta: ASP.NET Core Identity Rolleri).
/// </summary>
public static class Roller
{
    public const string Yonetici = "Yönetici";
    public const string Ogrenci = "Öğrenci";

    public static readonly string[] TumRoller = [Yonetici, Ogrenci];
}
