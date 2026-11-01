using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Controllers;

/// <summary>
/// Yemek kategorileri için CRUD (Listeleme, Detay, Ekleme, Güncelleme, Silme) uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class KategorilerController : ControllerBase
{
    private readonly YemekBotDbContext _db;

    public KategorilerController(YemekBotDbContext db)
    {
        _db = db;
    }

    // GET: api/kategoriler
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KategoriDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KategoriDto>>> GetAll()
    {
        var liste = await _db.Kategoriler
            .AsNoTracking()
            .OrderBy(k => k.Sira)
            .ThenBy(k => k.Id)
            .Select(k => new KategoriDto(k.Id, k.Ad, k.Sira, k.Yemekler.Count))
            .ToListAsync();

        return Ok(liste);
    }

    // GET: api/kategoriler/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(KategoriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KategoriDto>> GetById(int id)
    {
        var kategori = await _db.Kategoriler
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new KategoriDto(x.Id, x.Ad, x.Sira, x.Yemekler.Count))
            .FirstOrDefaultAsync();

        return kategori is null
            ? NotFound(new { Mesaj = "Kategori bulunamadı." })
            : Ok(kategori);
    }

    // POST: api/kategoriler
    [HttpPost]
    [ProducesResponseType(typeof(KategoriDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<KategoriDto>> Create(KategoriKaydetDto dto)
    {
        var temizAd = dto.Ad.Trim();
        if (await _db.Kategoriler.AnyAsync(k => k.Ad == temizAd))
            return Conflict(new { Mesaj = "Bu isimde bir kategori zaten mevcut." });

        var kategori = new Kategori
        {
            Ad = temizAd,
            Sira = dto.Sira
        };

        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        var sonuc = new KategoriDto(kategori.Id, kategori.Ad, kategori.Sira, 0);
        return CreatedAtAction(nameof(GetById), new { id = kategori.Id }, sonuc);
    }

    // PUT: api/kategoriler/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, KategoriKaydetDto dto)
    {
        var kategori = await _db.Kategoriler.FindAsync(id);
        if (kategori is null)
            return NotFound(new { Mesaj = "Kategori bulunamadı." });

        var temizAd = dto.Ad.Trim();
        if (await _db.Kategoriler.AnyAsync(k => k.Id != id && k.Ad == temizAd))
            return Conflict(new { Mesaj = "Bu isimde başka bir kategori zaten mevcut." });

        kategori.Ad = temizAd;
        kategori.Sira = dto.Sira;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/kategoriler/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var kategori = await _db.Kategoriler
            .Include(k => k.Yemekler)
            .FirstOrDefaultAsync(k => k.Id == id);

        if (kategori is null)
            return NotFound(new { Mesaj = "Kategori bulunamadı." });

        if (kategori.Yemekler.Count > 0)
            return Conflict(new { Mesaj = "Bu kategoriye bağlı yemekler var; önce yemekleri silin veya taşıyın." });

        _db.Kategoriler.Remove(kategori);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
