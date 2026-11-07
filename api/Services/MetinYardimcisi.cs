using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AuBot.Api.Services;

/// <summary>
/// Yemek adları için ortak metin işlemleri (Excel aktarımı ve Yemek CRUD aynı kuralları kullanır).
/// </summary>
public static partial class MetinYardimcisi
{
    private static readonly CultureInfo TrKultur = new("tr-TR");

    [GeneratedRegex(@"\s+")]
    private static partial Regex BoslukDeseni();

    /// <summary>
    /// "MERCİMEK  ÇORBASI" ile "Mercimek Çorbası" aynı yemek sayılsın diye:
    /// fazla boşlukları siler, küçük harfe çevirir ve Türkçe karakterleri sadeleştirir.
    /// </summary>
    public static string AramaAdiOlustur(string ad)
    {
        var s = BoslukDeseni().Replace(ad.Trim(), " ").ToLower(TrKultur);
        return s.Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ş', 's')
                .Replace('ü', 'u');
    }

    /// <summary>
    /// Excel'deki büyük harfli adları okunaklı hale getirir:
    /// "IZGARA TAVUK BUT/PATATES GARNİTÜR" → "Izgara Tavuk But/Patates Garnitür".
    /// </summary>
    public static string BaslikYap(string metin)
    {
        var kucuk = BoslukDeseni().Replace(metin.Trim(), " ").ToLower(TrKultur);
        var sonuc = new StringBuilder(kucuk.Length);
        var kelimeBasi = true;

        foreach (var c in kucuk)
        {
            sonuc.Append(kelimeBasi ? char.ToUpper(c, TrKultur) : c);
            kelimeBasi = c is ' ' or '/' or '-' or '(';
        }

        return sonuc.ToString();
    }
}
