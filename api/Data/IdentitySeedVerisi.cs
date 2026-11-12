using AuBot.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace AuBot.Api.Data;

/// <summary>
/// Uygulama ilk kez çalıştığında "Yönetici" ve "Öğrenci" rollerini
/// ve 1. sınıf içi gösterim / testler için varsayılan hesapları oluşturur.
/// </summary>
public static class IdentitySeedVerisi
{
    public static async Task RolleriVeOrnekKullanicilariOlusturAsync(IServiceProvider servisler)
    {
        var roleManager = servisler.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = servisler.GetRequiredService<UserManager<Kullanici>>();

        // 1. Rolleri oluştur ("Yönetici" ve "Öğrenci")
        foreach (var rolAdi in Roller.TumRoller)
        {
            if (!await roleManager.RoleExistsAsync(rolAdi))
            {
                await roleManager.CreateAsync(new IdentityRole(rolAdi));
            }
        }

        // 2. Varsayılan Yönetici hesabı
        await KullaniciOlusturAsync(
            userManager,
            eposta: "admin@ankara.edu.tr",
            sifre: "Admin123!",
            adSoyad: "Yemekhane Yöneticisi",
            rol: Roller.Yonetici);

        // 3. Varsayılan Öğrenci hesabı
        await KullaniciOlusturAsync(
            userManager,
            eposta: "ogrenci@ankara.edu.tr",
            sifre: "Ogrenci123!",
            adSoyad: "Örnek Öğrenci",
            rol: Roller.Ogrenci);
    }

    private static async Task KullaniciOlusturAsync(
        UserManager<Kullanici> userManager,
        string eposta,
        string sifre,
        string adSoyad,
        string rol)
    {
        var mevcut = await userManager.FindByEmailAsync(eposta);
        if (mevcut is null)
        {
            mevcut = new Kullanici
            {
                UserName = eposta,
                Email = eposta,
                AdSoyad = adSoyad,
                EmailConfirmed = true
            };

            var sonuc = await userManager.CreateAsync(mevcut, sifre);
            if (!sonuc.Succeeded)
                return;
        }

        if (!await userManager.IsInRoleAsync(mevcut, rol))
        {
            await userManager.AddToRoleAsync(mevcut, rol);
        }
    }
}
