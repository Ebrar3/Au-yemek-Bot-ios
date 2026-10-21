using AuBot.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenulerController : ControllerBase
{
    [HttpGet("bugun")]
    public IActionResult GetBugununMenusu([FromQuery] OgunTuru ogun = OgunTuru.Ogle)
    {
        return Ok(new
        {
            Tarih = DateOnly.FromDateTime(DateTime.Today),
            Ogun = ogun.ToString(),
            ToplamKalori = 1120,
            TatilMi = false,
            Yemekler = new[]
            {
                new { Sira = 1, Kategori = "Çorba", Ad = "Mercimek Çorbası", Kalori = 180, OrtalamaPuan = 4.6 },
                new { Sira = 2, Kategori = "Ana Yemek", Ad = "Orman Kebabı", Kalori = 420, OrtalamaPuan = 4.4 },
                new { Sira = 3, Kategori = "Yardımcı Yemek", Ad = "Pirinç Pilavı", Kalori = 310, OrtalamaPuan = 4.2 },
                new { Sira = 4, Kategori = "Tatlı / Salata", Ad = "Ayran & Mevsim Salata", Kalori = 210, OrtalamaPuan = 4.5 }
            }
        });
    }
}
