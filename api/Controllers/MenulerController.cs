using AuBot.Api.Data;
using AuBot.Api.Dtos;
using AuBot.Api.Models;
using AuBot.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Controllers;

/// <summary>
/// Günlük ve aylık yemekhane menüleri için tam CRUD (GET, POST, PUT, DELETE)
/// ve ClosedXML ile aylık Excel menü içe aktarma uç noktaları.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MenulerController : ControllerBase
{
    private readonly YemekBotDbContext _db;
    private readonly ExcelMenuServisi _excelServisi;

    public MenulerController(YemekBotDbContext db, ExcelMenuServisi excelServisi)
    {
        _db = db;
        _excelServisi = excelServisi;
    }

    // GET: api/menuler?yil=2026&ay=10&ogun=1
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MenuDto>>> GetAll(
        [FromQuery] int? yil,
        [FromQuery] int? ay,
        [FromQuery] OgunTuru? ogun)
    {
        var sorgu = _db.Menuler
            .AsNoTracking()
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Kategori)
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Degerlendirmeler)
            .AsQueryable();

        if (yil.HasValue)
            sorgu = sorgu.Where(m => m.Tarih.Year == yil.Value);

        if (ay.HasValue)
            sorgu = sorgu.Where(m => m.Tarih.Month == ay.Value);

        if (ogun.HasValue)
            sorgu = sorgu.Where(m => m.Ogun == ogun.Value);

        var menuler = await sorgu
            .OrderBy(m => m.Tarih)
            .ThenBy(m => m.Ogun)
            .ToListAsync();

        return Ok(menuler.Select(MenuDtoyaCevir));
    }

    // GET: api/menuler/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenuDto>> GetById(int id)
    {
        var menu = await _db.Menuler
            .AsNoTracking()
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Kategori)
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Degerlendirmeler)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu is null)
            return NotFound(new { Mesaj = $"Id={id} olan menü bulunamadı." });

        return Ok(MenuDtoyaCevir(menu));
    }

    // GET: api/menuler/bugun?ogun=1&tarih=2026-10-15
    [HttpGet("bugun")]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MenuDto>> GetBugununMenusu(
        [FromQuery] OgunTuru ogun = OgunTuru.Ogle,
        [FromQuery] DateOnly? tarih = null)
    {
        var hedefTarih = tarih ?? DateOnly.FromDateTime(DateTime.Today);

        var menu = await _db.Menuler
            .AsNoTracking()
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Kategori)
            .Include(m => m.MenuYemekleri)
                .ThenInclude(my => my.Yemek)
                    .ThenInclude(y => y!.Degerlendirmeler)
            .FirstOrDefaultAsync(m => m.Tarih == hedefTarih && m.Ogun == ogun);

        // Hedef tarihte menü yoksa en yakın tarihteki menüyü getir
        if (menu is null)
        {
            menu = await _db.Menuler
                .AsNoTracking()
                .Include(m => m.MenuYemekleri)
                    .ThenInclude(my => my.Yemek)
                        .ThenInclude(y => y!.Kategori)
                .Include(m => m.MenuYemekleri)
                    .ThenInclude(my => my.Yemek)
                        .ThenInclude(y => y!.Degerlendirmeler)
                .Where(m => m.Ogun == ogun)
                .OrderByDescending(m => m.Tarih)
                .FirstOrDefaultAsync();
        }

        if (menu is not null)
            return Ok(MenuDtoyaCevir(menu));

        // Veritabanında henüz hiç menü yoksa varsayılan örnek menü döndür
        return Ok(new MenuDto(
            0,
            hedefTarih,
            ogun,
            MenuIslemleri.OgunAdi(ogun),
            1120,
            false,
            null,
            new List<MenuYemekDto>
            {
                new(1, "Mercimek Çorbası", 1, "Çorba", 1, 180, 4.6),
                new(3, "Orman Kebabı", 2, "Ana Yemek", 2, 420, 4.4),
                new(5, "Pirinç Pilavı", 3, "Yardımcı Yemek", 3, 310, 4.2),
                new(7, "Ayran", 4, "Tatlı / Salata / İçecek", 4, 110, 4.5)
            }
        ));
    }

    // POST: api/menuler
    [HttpPost]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MenuDto>> Create([FromBody] MenuKaydetDto dto)
    {
        var tarih = dto.Tarih!.Value;
        var ogun = dto.Ogun!.Value;

        if (await _db.Menuler.AnyAsync(m => m.Tarih == tarih && m.Ogun == ogun))
            return Conflict(new { Mesaj = $"{tarih:dd.MM.yyyy} tarihli {MenuIslemleri.OgunAdi(ogun)} menüsü zaten kayıtlı." });

        var yemekler = await YemekleriGetirAsync(dto.YemekIdleri);
        if (yemekler is null)
            return BadRequest(new { Mesaj = "Gönderilen yemek ID'lerinden bazıları veritabanında bulunamadı." });

        var menu = new Menu
        {
            Tarih = tarih,
            Ogun = ogun,
            TatilMi = dto.TatilMi,
            Aciklama = dto.Aciklama?.Trim(),
            ToplamKalori = dto.TatilMi ? null : MenuIslemleri.ToplamKaloriHesapla(yemekler)
        };

        MenuIslemleri.YemekleriEsitle(menu, yemekler);
        _db.Menuler.Add(menu);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = menu.Id }, MenuDtoyaCevir(menu));
    }

    // PUT: api/menuler/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] MenuKaydetDto dto)
    {
        var menu = await _db.Menuler
            .Include(m => m.MenuYemekleri)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menu is null)
            return NotFound(new { Mesaj = $"Id={id} olan menü bulunamadı." });

        var tarih = dto.Tarih!.Value;
        var ogun = dto.Ogun!.Value;

        if (await _db.Menuler.AnyAsync(m => m.Id != id && m.Tarih == tarih && m.Ogun == ogun))
            return Conflict(new { Mesaj = $"{tarih:dd.MM.yyyy} tarihli {MenuIslemleri.OgunAdi(ogun)} menüsü zaten kayıtlı." });

        var yemekler = await YemekleriGetirAsync(dto.YemekIdleri);
        if (yemekler is null)
            return BadRequest(new { Mesaj = "Gönderilen yemek ID'lerinden bazıları veritabanında bulunamadı." });

        menu.Tarih = tarih;
        menu.Ogun = ogun;
        menu.TatilMi = dto.TatilMi;
        menu.Aciklama = dto.Aciklama?.Trim();
        menu.ToplamKalori = dto.TatilMi ? null : MenuIslemleri.ToplamKaloriHesapla(yemekler);

        MenuIslemleri.YemekleriEsitle(menu, yemekler);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/menuler/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _db.Menuler.FindAsync(id);
        if (menu is null)
            return NotFound(new { Mesaj = $"Id={id} olan menü bulunamadı." });

        _db.Menuler.Remove(menu);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/menuler/excel-yukle
    [HttpPost("excel-yukle")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExcelYuklemeSonucDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExcelYuklemeSonucDto>> ExcelYukle([FromForm] ExcelYuklemeIstegiDto istek, CancellationToken ct)
    {
        if (istek.Dosya is null || istek.Dosya.Length == 0)
            return BadRequest(new { Mesaj = "Yüklenecek Excel dosyası boş olamaz." });

        try
        {
            await using var akis = istek.Dosya.OpenReadStream();
            var sonuc = await _excelServisi.YukleAsync(akis, istek.Dosya.FileName, istek.Ogun, ct);
            return Ok(sonuc);
        }
        catch (ExcelFormatHatasi ex)
        {
            return BadRequest(new { Mesaj = ex.Message });
        }
    }

    private async Task<List<Yemek>?> YemekleriGetirAsync(List<int> yemekIdleri)
    {
        if (yemekIdleri.Count == 0)
            return new List<Yemek>();

        var yemekSozlugu = await _db.Yemekler
            .Include(y => y.Kategori)
            .Include(y => y.Degerlendirmeler)
            .Where(y => yemekIdleri.Contains(y.Id))
            .ToDictionaryAsync(y => y.Id);

        if (yemekSozlugu.Count != yemekIdleri.Count)
            return null;

        return yemekIdleri.Select(id => yemekSozlugu[id]).ToList();
    }

    private static MenuDto MenuDtoyaCevir(Menu m)
    {
        var yemekler = m.MenuYemekleri
            .OrderBy(my => my.Sira)
            .Select(my => new MenuYemekDto(
                my.YemekId,
                my.Yemek?.Ad ?? string.Empty,
                my.Yemek?.KategoriId ?? 0,
                my.Yemek?.Kategori?.Ad ?? string.Empty,
                my.Sira,
                my.Yemek?.Kalori,
                my.Yemek != null && my.Yemek.Degerlendirmeler.Any()
                    ? Math.Round(my.Yemek.Degerlendirmeler.Average(d => (double)d.Puan), 2)
                    : null
            ))
            .ToList();

        return new MenuDto(
            m.Id,
            m.Tarih,
            m.Ogun,
            MenuIslemleri.OgunAdi(m.Ogun),
            m.ToplamKalori,
            m.TatilMi,
            m.Aciklama,
            yemekler
        );
    }
}
