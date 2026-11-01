using System.Globalization;
using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Controllers;

/// <summary>
/// Yemek kayıtları için CRUD, kategori filtreleme ve Türkçe karakter duyarsız arama uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class YemeklerController : ControllerBase
{
    private static readonly CultureInfo TrKultur = new("tr-TR");
    private readonly YemekBotDbContext _db;

    public YemeklerController(YemekBotDbContext db)
    {
        _db = db;
    }

    // GET: api/yemekler?kategoriId=2&ara=pilav
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<YemekDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<YemekDto>>> GetAll([FromQuery] int? kategoriId, [FromQuery] string? ara)
    {
        var sorgu = _db.Yemekler.AsNoTracking().AsQueryable();

        if (kategoriId.HasValue)
            sorgu = sorgu.Where(y => y.KategoriId == kategoriId.Value);

        if (!string.IsNullOrWhiteSpace(ara))
        {
            var aranan = AramaAdiOlustur(ara);
            sorgu = sorgu.Where(y => y.AramaAdi.Contains(aranan));
        }

        var liste = await sorgu
            .OrderBy(y => y.Kategori!.Sira)
            .ThenBy(y => y.Ad)
            .Select(y => new YemekDto(
                y.Id,
                y.Ad,
                y.KategoriId,
                y.Kategori!.Ad,
                y.Kalori,
                y.Degerlendirmeler.Any()
                    ? Math.Round(y.Degerlendirmeler.Average(d => (double)d.Puan), 2)
                    : null))
            .ToListAsync();

        return Ok(liste);
    }

    // GET: api/yemekler/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(YemekDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<YemekDto>> GetById(int id)
    {
        var yemek = await _db.Yemekler
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new YemekDto(
                x.Id,
                x.Ad,
                x.KategoriId,
                x.Kategori!.Ad,
                x.Kalori,
                x.Degerlendirmeler.Any()
                    ? Math.Round(x.Degerlendirmeler.Average(d => (double)d.Puan), 2)
                    : null))
            .FirstOrDefaultAsync();

        return yemek is null
            ? NotFound(new { Mesaj = "Yemek bulunamadı." })
            : Ok(yemek);
    }

    // POST: api/yemekler
    [HttpPost]
    [ProducesResponseType(typeof(YemekDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<YemekDto>> Create(YemekKaydetDto dto)
    {
        var kategori = await _db.Kategoriler.FindAsync(dto.KategoriId);
        if (kategori is null)
            return BadRequest(new { Mesaj = "Seçilen kategori bulunamadı." });

        var aramaAdi = AramaAdiOlustur(dto.Ad);
        if (await _db.Yemekler.AnyAsync(y => y.AramaAdi == aramaAdi))
            return Conflict(new { Mesaj = "Bu isimde bir yemek zaten kayıtlı." });

        var yemek = new Yemek
        {
            Ad = dto.Ad.Trim(),
            AramaAdi = aramaAdi,
            KategoriId = dto.KategoriId,
            Kalori = dto.Kalori
        };

        _db.Yemekler.Add(yemek);
        await _db.SaveChangesAsync();

        var sonuc = new YemekDto(yemek.Id, yemek.Ad, yemek.KategoriId, kategori.Ad, yemek.Kalori, null);
        return CreatedAtAction(nameof(GetById), new { id = yemek.Id }, sonuc);
    }

    // PUT: api/yemekler/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, YemekKaydetDto dto)
    {
        var yemek = await _db.Yemekler.FindAsync(id);
        if (yemek is null)
            return NotFound(new { Mesaj = "Yemek bulunamadı." });

        if (!await _db.Kategoriler.AnyAsync(k => k.Id == dto.KategoriId))
            return BadRequest(new { Mesaj = "Seçilen kategori bulunamadı." });

        var yeniAramaAdi = AramaAdiOlustur(dto.Ad);
        if (await _db.Yemekler.AnyAsync(y => y.Id != id && y.AramaAdi == yeniAramaAdi))
            return Conflict(new { Mesaj = "Bu isimde başka bir yemek zaten kayıtlı." });

        yemek.Ad = dto.Ad.Trim();
        yemek.AramaAdi = yeniAramaAdi;
        yemek.KategoriId = dto.KategoriId;
        yemek.Kalori = dto.Kalori;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/yemekler/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var yemek = await _db.Yemekler.FindAsync(id);
        if (yemek is null)
            return NotFound(new { Mesaj = "Yemek bulunamadı." });

        if (await _db.MenuYemekleri.AnyAsync(my => my.YemekId == id))
            return Conflict(new { Mesaj = "Bu yemek bir menüde kullanılıyor; önce menüden çıkarın." });

        if (await _db.Degerlendirmeler.AnyAsync(d => d.YemekId == id))
            return Conflict(new { Mesaj = "Bu yemeğe ait öğrenci değerlendirmeleri var; silinemez." });

        _db.Yemekler.Remove(yemek);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Excel'deki "MERCİMEK ÇORBASI" ile "Mercimek Çorbası" gibi yazımları
    /// aynı kabul edebilmek için Türkçe karakterleri sadeleştirip küçük harfe çevirir.
    /// </summary>
    private static string AramaAdiOlustur(string ad)
    {
        var s = ad.Trim().ToLower(TrKultur);
        return s.Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ş', 's')
                .Replace('ü', 'u');
    }
}
