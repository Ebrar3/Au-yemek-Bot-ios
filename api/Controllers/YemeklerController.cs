using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class YemeklerController : ControllerBase
{
    private readonly YemekBotDbContext _db;

    public YemeklerController(YemekBotDbContext db)
    {
        _db = db;
    }

    // GET: api/yemekler?kategoriId=2&ara=pilav
    [HttpGet]
    public async Task<ActionResult<IEnumerable<YemekDto>>> GetAll([FromQuery] int? kategoriId, [FromQuery] string? ara)
    {
        var sorgu = _db.Yemekler.AsQueryable();

        if (kategoriId.HasValue)
            sorgu = sorgu.Where(y => y.KategoriId == kategoriId.Value);

        if (!string.IsNullOrWhiteSpace(ara))
        {
            var aranan = AramaAdiOlustur(ara);
            sorgu = sorgu.Where(y => y.AramaAdi.Contains(aranan));
        }

        var liste = await sorgu
            .OrderBy(y => y.Kategori!.Sira).ThenBy(y => y.Ad)
            .Select(y => new YemekDto(
                y.Id, y.Ad, y.KategoriId, y.Kategori!.Ad, y.Kalori,
                y.Degerlendirmeler.Any() ? y.Degerlendirmeler.Average(d => (double)d.Puan) : null))
            .ToListAsync();

        return Ok(liste);
    }

    // GET: api/yemekler/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<YemekDto>> GetById(int id)
    {
        var y = await _db.Yemekler
            .Where(x => x.Id == id)
            .Select(x => new YemekDto(
                x.Id, x.Ad, x.KategoriId, x.Kategori!.Ad, x.Kalori,
                x.Degerlendirmeler.Any() ? x.Degerlendirmeler.Average(d => (double)d.Puan) : null))
            .FirstOrDefaultAsync();
        return y is null ? NotFound(new { Mesaj = "Yemek bulunamadı." }) : Ok(y);
    }

    // POST: api/yemekler
    [HttpPost]
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
    public async Task<IActionResult> Update(int id, YemekKaydetDto dto)
    {
        var yemek = await _db.Yemekler.FindAsync(id);
        if (yemek is null) return NotFound(new { Mesaj = "Yemek bulunamadı." });

        if (!await _db.Kategoriler.AnyAsync(k => k.Id == dto.KategoriId))
            return BadRequest(new { Mesaj = "Seçilen kategori bulunamadı." });

        yemek.Ad = dto.Ad.Trim();
        yemek.AramaAdi = AramaAdiOlustur(dto.Ad);
        yemek.KategoriId = dto.KategoriId;
        yemek.Kalori = dto.Kalori;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/yemekler/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var yemek = await _db.Yemekler.FindAsync(id);
        if (yemek is null) return NotFound(new { Mesaj = "Yemek bulunamadı." });

        if (await _db.MenuYemekleri.AnyAsync(my => my.YemekId == id))
            return Conflict(new { Mesaj = "Bu yemek bir menüde kullanılıyor; önce menüden çıkarın." });

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
        var s = ad.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
        return s.Replace('ç', 'c').Replace('ğ', 'g').Replace('ı', 'i')
                .Replace('ö', 'o').Replace('ş', 's').Replace('ü', 'u');
    }
}
