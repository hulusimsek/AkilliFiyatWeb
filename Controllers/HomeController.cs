using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using AkilliFiyatWeb.Data;
using Microsoft.EntityFrameworkCore;
using AkilliFiyatWeb.Entity;
using AngleSharp;
using AngleSharp.Dom;
using HtmlAgilityPack;
using System.Globalization;
using System.Net;
namespace AkilliFiyatWeb.Controllers;

public class HomeController : Controller
{
    private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
    private readonly DataContext _context;
    private readonly CarfoursaIndirimUrunServices _carfoursaIndirimUrunServices;
    private readonly SokUrunServices _sokUrunServices;
    private readonly KelimeKontrol _kelimeKontrol;

    [ActivatorUtilitiesConstructor]
    public HomeController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext context, CarfoursaIndirimUrunServices carfoursaIndirimUrunServices,
                            SokUrunServices sokUrunServices, KelimeKontrol KelimeKontrol)
    {
        _migrosIndirimUrunServices = migrosIndirimUrunServices;
        _context = context;
        _carfoursaIndirimUrunServices = carfoursaIndirimUrunServices;
        _sokUrunServices = sokUrunServices;
        _kelimeKontrol = KelimeKontrol;
    }

    public async Task<IActionResult> Index()
    {
        var urunler = await _context.Urunler.ToListAsync();
        ViewBag.SiralanmisUrunler = urunler.Where(u => u.IndirimOran != null && u.IndirimOran != 0)
                                   .OrderByDescending(u => u.IndirimOran)
                                   .ToList();
        return View(urunler);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            // Arama sonuçlarını al ve ViewData üzerinden görünüme gönder
            ViewData["query"] = query;
            List<Urunler> _sonucUrunler = new List<Urunler>();
            _sonucUrunler.AddRange(await _migrosIndirimUrunServices.MigrosKayit(query, "1711827415305000067"));
            _sonucUrunler.AddRange(await _carfoursaIndirimUrunServices.CarfoursaKayit(query));
            _sonucUrunler.AddRange(await _sokUrunServices.SokKayit(query));

            var siraliUrunler = _sonucUrunler
                                        .Where(u => u.Benzerlik != null && u.Benzerlik != 0)
                                        .GroupBy(u => u.Benzerlik > 40 ? 1 : u.Benzerlik > 30 ? 2 : u.Benzerlik > 20 ? 3 : u.Benzerlik > 10 ? 4 : 5) // Benzerlik değerine göre grupla
                                        .OrderBy(g => g.Key)  // Grupları büyükten küçüğe göre sırala
                                        .SelectMany(g => g.OrderByDescending(u => u.Fiyat))  // Her grubu içindeki ürünleri fiyata göre sırala ve birleştir
                                        .ToList();
            foreach (var urun in siraliUrunler)
            {
                Console.WriteLine("Ürün Adı: " + urun.UrunAdi); // Varsayılan olarak ürün adını yazdırabilirsiniz
                Console.WriteLine("Benzerlik: " + urun.Benzerlik); // Benzerlik değerini yazdır
            }
            return View(siraliUrunler);
        }

        // Arama sayfasını göster
        return View();
    }
}
