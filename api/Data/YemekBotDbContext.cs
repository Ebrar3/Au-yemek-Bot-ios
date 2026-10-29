using AuBot.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AuBot.Api.Data;

public class YemekBotDbContext : DbContext
{
    public YemekBotDbContext(DbContextOptions<YemekBotDbContext> options) : base(options)
    {
    }

    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Yemek> Yemekler => Set<Yemek>();
    public DbSet<Menu> Menuler => Set<Menu>();
    public DbSet<MenuYemek> MenuYemekleri => Set<MenuYemek>();
    public DbSet<Degerlendirme> Degerlendirmeler => Set<Degerlendirme>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Kategori – Yemek (1–N)
        modelBuilder.Entity<Yemek>()
            .HasOne(y => y.Kategori)
            .WithMany(k => k.Yemekler)
            .HasForeignKey(y => y.KategoriId)
            .OnDelete(DeleteBehavior.Restrict);

        // Menü – Yemek (N–N) ara tablosu: birleşik birincil anahtar
        modelBuilder.Entity<MenuYemek>()
            .HasKey(my => new { my.MenuId, my.YemekId });

        modelBuilder.Entity<MenuYemek>()
            .HasOne(my => my.Menu)
            .WithMany(m => m.MenuYemekleri)
            .HasForeignKey(my => my.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuYemek>()
            .HasOne(my => my.Yemek)
            .WithMany(y => y.MenuYemekleri)
            .HasForeignKey(my => my.YemekId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aynı tarihte aynı öğünden yalnızca bir menü olabilir
        modelBuilder.Entity<Menu>()
            .HasIndex(m => new { m.Tarih, m.Ogun })
            .IsUnique();

        // Değerlendirme ilişkileri
        modelBuilder.Entity<Degerlendirme>()
            .HasOne(d => d.Menu)
            .WithMany(m => m.Degerlendirmeler)
            .HasForeignKey(d => d.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Degerlendirme>()
            .HasOne(d => d.Yemek)
            .WithMany(y => y.Degerlendirmeler)
            .HasForeignKey(d => d.YemekId)
            .OnDelete(DeleteBehavior.Restrict);

        // Başlangıç (seed) verileri
        modelBuilder.Entity<Kategori>().HasData(
            new Kategori { Id = 1, Ad = "Çorba", Sira = 1 },
            new Kategori { Id = 2, Ad = "Ana Yemek", Sira = 2 },
            new Kategori { Id = 3, Ad = "Yardımcı Yemek", Sira = 3 },
            new Kategori { Id = 4, Ad = "Tatlı / Salata / İçecek", Sira = 4 }
        );

        modelBuilder.Entity<Yemek>().HasData(
            new Yemek { Id = 1, Ad = "Mercimek Çorbası", AramaAdi = "mercimek corbasi", KategoriId = 1, Kalori = 180 },
            new Yemek { Id = 2, Ad = "Ezogelin Çorbası", AramaAdi = "ezogelin corbasi", KategoriId = 1, Kalori = 190 },
            new Yemek { Id = 3, Ad = "Orman Kebabı", AramaAdi = "orman kebabi", KategoriId = 2, Kalori = 420 },
            new Yemek { Id = 4, Ad = "Tavuk Sote", AramaAdi = "tavuk sote", KategoriId = 2, Kalori = 380 },
            new Yemek { Id = 5, Ad = "Pirinç Pilavı", AramaAdi = "pirinc pilavi", KategoriId = 3, Kalori = 310 },
            new Yemek { Id = 6, Ad = "Bulgur Pilavı", AramaAdi = "bulgur pilavi", KategoriId = 3, Kalori = 290 },
            new Yemek { Id = 7, Ad = "Ayran", AramaAdi = "ayran", KategoriId = 4, Kalori = 110 },
            new Yemek { Id = 8, Ad = "Kemalpaşa Tatlısı", AramaAdi = "kemalpasa tatlisi", KategoriId = 4, Kalori = 310 }
        );
    }
}
