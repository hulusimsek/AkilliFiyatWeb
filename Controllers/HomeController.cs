using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AkilliFiyatWeb.Models;
using AkilliFiyatWeb.Services;
using AkilliFiyatWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AkilliFiyatWeb.Controllers;

public class HomeController : Controller
{
    private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
    private readonly DataContext _dataContext;

    [ActivatorUtilitiesConstructor]
        public HomeController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext dataContext)
        {
            _migrosIndirimUrunServices = migrosIndirimUrunServices;
            _dataContext = dataContext;
        }

    public async Task<IActionResult> Index()
    {
        var urunler = await _dataContext.Urunler.ToListAsync();
        return View(urunler);
    }
}
