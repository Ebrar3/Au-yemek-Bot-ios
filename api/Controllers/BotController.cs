using Microsoft.AspNetCore.Mvc;

namespace AuBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BotController : ControllerBase
{
    [HttpGet("adim")]
    public IActionResult GetBotAdim([FromQuery] string secenek = "bugun_ogle")
    {
        return Ok(new
        {
            Secenek = secenek,
            Mesaj = "Bugünün Öğle Menüsü (Mercimek Çorbası, Orman Kebabı, Pirinç Pilavı, Ayran & Salata - 1120 kcal)",
            SecenekButonlari = new[]
            {
                new { Kod = "puanlama", Etiket = "⭐ Puanlama" },
                new { Kod = "aksam", Etiket = "🌙 Akşam Yemeği" },
                new { Kod = "ertesi_gun_ogle", Etiket = "⏭️ Ertesi Günün Öğle Yemeği" },
                new { Kod = "yardim", Etiket = "🆘 Yardım" }
            }
        });
    }
}
