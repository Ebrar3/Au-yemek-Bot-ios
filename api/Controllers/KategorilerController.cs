using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KategorilerController : ControllerBase
{
    private readonly YemekBotDbContext _db;

    public KategorilerController(YemekBotDbContext db)
    {
        _db = db;
    }

    // GET: api/kategoriler
    [HttpGet]
    public async Task<ActionResult<IEnumerable<KategoriDto>>> GetAll()
    {
        var liste = await _db.Kategoriler
            .OrderBy(k => k.Sira)
            .Select(k => new KategoriDto(k.Id, k.Ad, k.Sira, k.Yemekler.Count))
            .ToListAsync();
        return Ok(liste);
    }

    // GET: api/kategoriler/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<KategoriDto>> GetById(int id)
    {
        var k = await _db.Kategoriler
            .Where(x => x.Id == id)
            .Select(x => new KategoriDto(x.Id, x.Ad, x.Sira, x.Yemekler.Count))
            .FirstOrDefaultAsync();
        return k is null ? NotFound(new { Mesaj = "Kategori bulunamadı." }) : Ok(k);
    }

    // POST: api/kategoriler
    [HttpPost]
    public async Task<ActionResult<KategoriDto>> Create(KategoriKaydetDto dto)
    {
        var kategori = new Kategori { Ad = dto.Ad.Trim(), Sira = dto.Sira };
        _db.Kategoriler.Add(kategori);
        await _db.SaveChangesAsync();

        var sonuc = new KategoriDto(kategori.Id, kategori.Ad, kategori.Sira, 0);
        return CreatedAtAction(nameof(GetById), new { id = kategori.Id }, sonuc);
    }

    // PUT: api/kategoriler/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, KategoriKaydetDto dto)
    {
        var kategori = await _db.Kategoriler.FindAsync(id);
        if (kategori is null) return NotFound(new { Mesaj = "Kategori bulunamadı." });

        kategori.Ad = dto.Ad.Trim();
        kategori.Sira = dto.Sira;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/kategoriler/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var kategori = await _db.Kategoriler.Include(k => k.Yemekler).FirstOrDefaultAsync(k => k.Id == id);
        if (kategori is null) return NotFound(new { Mesaj = "Kategori bulunamadı." });

        if (kategori.Yemekler.Count > 0)
            return Conflict(new { Mesaj = "Bu kategoriye bağlı yemekler var; önce yemekleri silin veya taşıyın." });

        _db.Kategoriler.Remove(kategori);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
