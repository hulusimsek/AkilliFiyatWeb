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
namespace AkilliFiyatWeb.Controllers;

public class HomeController : Controller
{
    private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
    private readonly DataContext _context;

    [ActivatorUtilitiesConstructor]
    public HomeController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext context)
    {
        _migrosIndirimUrunServices = migrosIndirimUrunServices;
        _context = context;
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
            return View(_sonucUrunler);
        }

        // Arama sayfasını göster
        return View();
    }
}
