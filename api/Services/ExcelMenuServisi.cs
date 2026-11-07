using System.Globalization;
using System.Text.RegularExpressions;
using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Services;

/// <summary>Excel dosyası beklenen formatta değilse fırlatılır (Controller 400 döndürür).</summary>
public class ExcelFormatHatasi(string mesaj) : Exception(mesaj);

/// <summary>
/// Ankara Üniversitesi'nin aylık öğle/akşam Excel menülerini ClosedXML ile okuyup veritabanına aktarır.
///
/// Beklenen dosya düzeni (OrnekMenuler klasöründeki Ekim 2026 dosyaları):
///   A sütunu : Tarih
///   B sütunu : Çorba            (öğle dosyasında "MAHLUTA ÇORBA (204 kkal)" biçiminde)
///   C sütunu : Ana Yemek
///   D sütunu : Yardımcı Yemek
///   E sütunu : Tatlı / Salata / İçecek
///   F sütunu : Toplam enerji    ("1383 KKAL", yalnızca öğle dosyasında)
///   Tatil günleri: B sütununda açıklama ("29 EKİM CUMHURİYET BAYRAMI"), C–E boş (birleştirilmiş hücre).
/// </summary>
public partial class ExcelMenuServisi
{
    private static readonly CultureInfo TrKultur = new("tr-TR");
    private const int YemekSutunSayisi = 4; // B, C, D, E

    // "MAHLUTA ÇORBA (204 kkal)" → ad = "MAHLUTA ÇORBA", kalori = 204
    [GeneratedRegex(@"^(?<ad>.*?)\s*\(\s*(?<kalori>\d+)\s*kk?al\s*\)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex KaloriDeseni();

    [GeneratedRegex(@"\d+")]
    private static partial Regex SayiDeseni();

    private readonly YemekBotDbContext _db;
    private readonly ILogger<ExcelMenuServisi> _logger;

    public ExcelMenuServisi(YemekBotDbContext db, ILogger<ExcelMenuServisi> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Excel'den okunan ham satır.</summary>
    private sealed record ExcelSatiri(int SatirNo, DateOnly Tarih, string[] YemekHucreleri, string ToplamHucresi);

    public async Task<ExcelYuklemeSonucDto> YukleAsync(Stream akis, string dosyaAdi, OgunTuru? ogun, CancellationToken ct = default)
    {
        var uyarilar = new List<string>();

        // 1) Dosyayı aç ve ham satırları oku
        XLWorkbook kitap;
        try
        {
            kitap = new XLWorkbook(akis);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Excel dosyası açılamadı: {Dosya}", dosyaAdi);
            throw new ExcelFormatHatasi("Dosya okunamadı. Lütfen geçerli bir .xlsx Excel dosyası seçin.");
        }

        using (kitap)
        {
            var sayfa = kitap.Worksheet(1);
            var gercekOgun = OgunuBelirle(sayfa, ogun, uyarilar);
            var satirlar = SatirlariOku(sayfa);

            if (satirlar.Count == 0)
                throw new ExcelFormatHatasi("Dosyada A sütununda tarih bulunan hiçbir menü satırı bulunamadı.");

            // 2) Dosyanın ayını bul ve hatalı tarihleri düzelt
            var (yil, ay) = BaskinAyiBul(satirlar);
            satirlar = TarihleriDuzelt(satirlar, yil, ay, uyarilar);

            // 3) Veritabanına aktar
            return await VeritabaninaAktarAsync(satirlar, dosyaAdi, gercekOgun, yil, ay, uyarilar, ct);
        }
    }

    // ------------------------------------------------------------------ Okuma

    private static OgunTuru OgunuBelirle(IXLWorksheet sayfa, OgunTuru? istenen, List<string> uyarilar)
    {
        if (istenen.HasValue)
            return istenen.Value;

        // Başlık satırlarında "AKŞAM" geçiyorsa akşam menüsüdür ("2026 AKŞAM YEMEK MENÜSÜ")
        var baslik = string.Join(" ", Enumerable.Range(1, 3).Select(r => sayfa.Cell(r, 1).GetString()))
                           .ToUpper(TrKultur);
        var ogun = baslik.Contains("AKŞAM") ? OgunTuru.Aksam : OgunTuru.Ogle;
        uyarilar.Add($"Öğün belirtilmedi; dosya başlığına göre '{MenuIslemleri.OgunAdi(ogun)}' olarak algılandı.");
        return ogun;
    }

    private static List<ExcelSatiri> SatirlariOku(IXLWorksheet sayfa)
    {
        var sonuc = new List<ExcelSatiri>();

        foreach (var satir in sayfa.RowsUsed())
        {
            // A sütunu tarih değilse (başlık, dipnot, imza satırları) atla
            if (!TarihOku(satir.Cell(1), out var tarih))
                continue;

            var yemekler = Enumerable.Range(2, YemekSutunSayisi)
                .Select(sutun => satir.Cell(sutun).GetString().Trim())
                .ToArray();
            var toplam = satir.Cell(2 + YemekSutunSayisi).GetString().Trim();

            sonuc.Add(new ExcelSatiri(satir.RowNumber(), tarih, yemekler, toplam));
        }

        return sonuc;
    }

    private static bool TarihOku(IXLCell hucre, out DateOnly tarih)
    {
        if (hucre.DataType == XLDataType.DateTime)
        {
            tarih = DateOnly.FromDateTime(hucre.GetDateTime());
            return true;
        }

        // Tarih metin olarak yazılmışsa ("01.10.2026") onu da kabul et
        return DateOnly.TryParseExact(hucre.GetString().Trim(), new[] { "dd.MM.yyyy", "d.M.yyyy" },
            TrKultur, DateTimeStyles.None, out tarih);
    }

    // ------------------------------------------------------------------ Tarih kontrolü

    /// <summary>Dosyadaki satırların çoğunluğunun ait olduğu ay (dosyanın gerçek ayı).</summary>
    private static (int Yil, int Ay) BaskinAyiBul(List<ExcelSatiri> satirlar)
    {
        var enCok = satirlar
            .GroupBy(s => (s.Tarih.Year, s.Tarih.Month))
            .OrderByDescending(g => g.Count())
            .First();
        return enCok.Key;
    }

    /// <summary>
    /// Ayı dosyanın geri kalanından farklı olan satırları (ör. Ekim dosyasında "12.04.2026")
    /// aynı gün numarasıyla dosyanın ayına taşır ve yöneticiye uyarı olarak raporlar.
    /// Aynı tarih iki kez geçerse ilk satır kullanılır.
    /// </summary>
    private static List<ExcelSatiri> TarihleriDuzelt(List<ExcelSatiri> satirlar, int yil, int ay, List<string> uyarilar)
    {
        var ayAdi = new DateTime(yil, ay, 1).ToString("MMMM yyyy", TrKultur);
        var sonuc = new List<ExcelSatiri>();
        var gorulenTarihler = new HashSet<DateOnly>();

        foreach (var s in satirlar)
        {
            var satir = s;
            if (s.Tarih.Year != yil || s.Tarih.Month != ay)
            {
                if (s.Tarih.Day > DateTime.DaysInMonth(yil, ay))
                {
                    uyarilar.Add($"Satır {s.SatirNo}: {s.Tarih:dd.MM.yyyy} tarihi {ayAdi} ayına ait değil; satır atlandı.");
                    continue;
                }

                var duzeltilmis = new DateOnly(yil, ay, s.Tarih.Day);
                uyarilar.Add($"Satır {s.SatirNo}: {s.Tarih:dd.MM.yyyy} tarihi dosyanın ayıyla ({ayAdi}) uyuşmuyor; {duzeltilmis:dd.MM.yyyy} olarak düzeltildi.");
                satir = s with { Tarih = duzeltilmis };
            }

            if (!gorulenTarihler.Add(satir.Tarih))
            {
                uyarilar.Add($"Satır {s.SatirNo}: {satir.Tarih:dd.MM.yyyy} tarihi dosyada birden fazla kez geçiyor; ilk satır kullanıldı.");
                continue;
            }

            sonuc.Add(satir);
        }

        return sonuc;
    }

    // ------------------------------------------------------------------ Veritabanı

    private async Task<ExcelYuklemeSonucDto> VeritabaninaAktarAsync(
        List<ExcelSatiri> satirlar, string dosyaAdi, OgunTuru ogun, int yil, int ay,
        List<string> uyarilar, CancellationToken ct)
    {
        // Kategoriler Excel sütun sırasına göre: B=1 Çorba, C=2 Ana Yemek, D=3 Yardımcı, E=4 Tatlı/Salata
        var kategoriler = await _db.Kategoriler.ToDictionaryAsync(k => k.Sira, ct);
        for (var sira = 1; sira <= YemekSutunSayisi; sira++)
        {
            if (!kategoriler.ContainsKey(sira))
                throw new ExcelFormatHatasi($"Veritabanında sırası {sira} olan kategori yok. Önce kategorileri tanımlayın.");
        }

        // Var olan yemekleri arama adına göre önbelleğe al (aynı yemek ikinci kez eklenmesin)
        var yemekler = await _db.Yemekler.ToDictionaryAsync(y => y.AramaAdi, ct);

        // Bu ayın bu öğüne ait mevcut menüleri (yeniden yüklemede güncellenir)
        var ilkGun = new DateOnly(yil, ay, 1);
        var sonGun = ilkGun.AddMonths(1).AddDays(-1);
        var mevcutMenuler = await _db.Menuler
            .Include(m => m.MenuYemekleri)
            .Where(m => m.Ogun == ogun && m.Tarih >= ilkGun && m.Tarih <= sonGun)
            .ToDictionaryAsync(m => m.Tarih, ct);

        int eklenen = 0, guncellenen = 0, tatil = 0, yeniYemek = 0;

        foreach (var satir in satirlar)
        {
            var doluHucreler = satir.YemekHucreleri.Count(h => h.Length > 0);
            if (doluHucreler == 0)
            {
                uyarilar.Add($"Satır {satir.SatirNo} ({satir.Tarih:dd.MM.yyyy}): yemek bilgisi yok; satır atlandı.");
                continue;
            }

            var menuYemekleri = new List<Yemek>();
            string? aciklama = null;
            var tatilMi = doluHucreler == 1 && satir.YemekHucreleri[0].Length > 0 && !KaloriDeseni().IsMatch(satir.YemekHucreleri[0]);

            if (tatilMi)
            {
                aciklama = MetinYardimcisi.BaslikYap(satir.YemekHucreleri[0]);
                tatil++;
            }
            else
            {
                for (var i = 0; i < YemekSutunSayisi; i++)
                {
                    var hucre = satir.YemekHucreleri[i];
                    if (hucre.Length == 0)
                    {
                        uyarilar.Add($"Satır {satir.SatirNo} ({satir.Tarih:dd.MM.yyyy}): '{kategoriler[i + 1].Ad}' sütunu boş.");
                        continue;
                    }

                    var (ad, kalori) = YemekHucresiniAyristir(hucre);
                    var yemek = YemegiBulVeyaOlustur(ad, kalori, kategoriler[i + 1], yemekler, ref yeniYemek);
                    if (!menuYemekleri.Contains(yemek))
                        menuYemekleri.Add(yemek);
                }
            }

            var toplamKalori = tatilMi ? null : ToplamKaloriyiKontrolEt(satir, menuYemekleri, uyarilar);

            // Menü varsa güncelle, yoksa ekle (aynı gün + öğün için tek menü kuralı)
            if (mevcutMenuler.TryGetValue(satir.Tarih, out var menu))
            {
                guncellenen++;
            }
            else
            {
                menu = new Menu { Tarih = satir.Tarih, Ogun = ogun };
                _db.Menuler.Add(menu);
                mevcutMenuler[satir.Tarih] = menu;
                eklenen++;
            }

            menu.TatilMi = tatilMi;
            menu.Aciklama = aciklama;
            menu.ToplamKalori = toplamKalori;
            MenuIslemleri.YemekleriEsitle(menu, menuYemekleri);
        }

        // Tüm değişiklikler tek seferde (tek transaction) kaydedilir: ya hepsi ya hiçbiri
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Excel aktarıldı: {Dosya} – {Eklenen} eklendi, {Guncellenen} güncellendi, {Uyari} uyarı",
            dosyaAdi, eklenen, guncellenen, uyarilar.Count);

        return new ExcelYuklemeSonucDto(dosyaAdi, ogun, yil, ay, eklenen, guncellenen, tatil, yeniYemek, uyarilar);
    }

    private static (string Ad, int? Kalori) YemekHucresiniAyristir(string hucre)
    {
        var eslesme = KaloriDeseni().Match(hucre);
        return eslesme.Success
            ? (MetinYardimcisi.BaslikYap(eslesme.Groups["ad"].Value), int.Parse(eslesme.Groups["kalori"].Value))
            : (MetinYardimcisi.BaslikYap(hucre), null);
    }

    private Yemek YemegiBulVeyaOlustur(string ad, int? kalori, Kategori kategori,
        Dictionary<string, Yemek> yemekler, ref int yeniYemek)
    {
        var aramaAdi = MetinYardimcisi.AramaAdiOlustur(ad);

        if (yemekler.TryGetValue(aramaAdi, out var mevcut))
        {
            // Akşam dosyasında kalori yok; öğle dosyasından gelen kalori korunur.
            // Excel'de kalori varsa en güncel değer olarak kaydedilir.
            if (kalori.HasValue)
                mevcut.Kalori = kalori;
            return mevcut;
        }

        var yeni = new Yemek { Ad = ad, AramaAdi = aramaAdi, KategoriId = kategori.Id, Kalori = kalori };
        _db.Yemekler.Add(yeni);
        yemekler[aramaAdi] = yeni;
        yeniYemek++;
        return yeni;
    }

    /// <summary>
    /// Yemeklerin kalorilerinden toplamı hesaplar ve F sütunundaki toplamla karşılaştırır.
    /// Excel'deki toplam yanlışsa (ör. 1572 yerine 2720) hesaplanan değer kullanılır.
    /// </summary>
    private static int? ToplamKaloriyiKontrolEt(ExcelSatiri satir, List<Yemek> yemekler, List<string> uyarilar)
    {
        var hesaplanan = MenuIslemleri.ToplamKaloriHesapla(yemekler);

        var sayi = SayiDeseni().Match(satir.ToplamHucresi);
        int? exceldeki = sayi.Success ? int.Parse(sayi.Value) : null;

        if (hesaplanan.HasValue && exceldeki.HasValue && hesaplanan != exceldeki)
        {
            uyarilar.Add($"Satır {satir.SatirNo} ({satir.Tarih:dd.MM.yyyy}): Excel'deki toplam {exceldeki} kkal, " +
                         $"yemeklerin toplamı {hesaplanan} kkal; hesaplanan değer kullanıldı.");
        }

        return hesaplanan ?? exceldeki;
    }
}
