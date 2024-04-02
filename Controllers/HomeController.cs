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
using Newtonsoft.Json.Linq;
namespace AkilliFiyatWeb.Controllers;

public class HomeController : Controller
{
    private readonly MigrosIndirimUrunServices _migrosIndirimUrunServices;
    private readonly DataContext _context;
    private readonly CarfoursaIndirimUrunServices _carfoursaIndirimUrunServices;
    private readonly SokUrunServices _sokUrunServices;
    private readonly A101IndirimUrunServices _a101IndirimServices;
    private readonly KelimeKontrol _kelimeKontrol;
	private readonly ApiService _apiService;

    [ActivatorUtilitiesConstructor]
	public HomeController(MigrosIndirimUrunServices migrosIndirimUrunServices, DataContext context, CarfoursaIndirimUrunServices carfoursaIndirimUrunServices,
							SokUrunServices sokUrunServices, KelimeKontrol KelimeKontrol, A101IndirimUrunServices a101IndirimServices, ApiService apiService)
	{
		_migrosIndirimUrunServices = migrosIndirimUrunServices;
		_context = context;
		_carfoursaIndirimUrunServices = carfoursaIndirimUrunServices;
		_sokUrunServices = sokUrunServices;
		_kelimeKontrol = KelimeKontrol;
		_a101IndirimServices = a101IndirimServices;
		_apiService = apiService;
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
            _sonucUrunler.AddRange(await _a101IndirimServices.A101Kayit(query));

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

	public async Task<List<Urunler>> A101Kayit(string query)
	{
		List<Urunler> a101Urunler = new List<Urunler>();

		query = "kola";
		var a101 = await _apiService.A101ApiAsync(query, "100", "1");

		if (a101 != null)
		{
			dynamic jsonResponse = JObject.Parse(a101);
			Console.WriteLine(jsonResponse.ToString());
			if (jsonResponse != null && jsonResponse.res != null && jsonResponse.res.Count > 0 && jsonResponse.res[0].page_content != null)
			{
				foreach (var eleman in jsonResponse.res[0].page_content)
				{
					string formatliDeger = $"{eleman.price:#.##} \u20BA";

					var urunAdi = eleman.title.ToString();
					if (urunAdi != null)
					{
                        await Console.Out.WriteLineAsync(urunAdi);
                    }
					else
					{
                        await Console.Out.WriteLineAsync("name null");
                    }
					var benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(urunAdi));
					var katSayi = _kelimeKontrol.IkinciKelime2(query, urunAdi);
					if (katSayi > 0)
					{
						benzerlikOrani += katSayi;
					}
					string imageUrl = "";
					foreach (var item in eleman.image)
					{
						if (item.imageType == "product")
						{
							imageUrl = item.url;
						}
					}
					if (benzerlikOrani > 0.1)
					{
						var eklenecekUrun = new Urunler(urunAdi, formatliDeger,
							imageUrl.ToString(), "A-101",
							"img/A-101.png",
							benzerlikOrani, eleman.seoUrl.ToString());
						eklenecekUrun.EskiFiyat = ($"{eleman.Price} \u20BA");
						eklenecekUrun.UrunlerId = eleman.id;

						a101Urunler.Add(eklenecekUrun);
					}
				}
			}
			else
			{
				if (jsonResponse == null)
				{
					Console.WriteLine("--------------- jsonResponse null döndü");
				}
				else if (jsonResponse.res == null)
				{
					Console.WriteLine("--------------- jsonResponse.res null döndü");
				}
				else if (jsonResponse.res.Count == 0)
				{
					Console.WriteLine("--------------- jsonResponse.res boş döndü");
				}
				else if (jsonResponse.res[0] == null)
				{
					Console.WriteLine("--------------- jsonResponse.res[0] null döndü");
				}
				else if (jsonResponse.res[0].page_content == null)
				{
					Console.WriteLine("--------------- jsonResponse.res[0].page_content null döndü");
				}

			}
		}
		else
		{
			Console.WriteLine("başarsız");
		}


		return a101Urunler;
	}
}
