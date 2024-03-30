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
        ViewBag.SiralanmisUrunler = urunler.Where(u => u.IndirimOran != null && u.IndirimOran !=0)
                                   .OrderByDescending(u => u.IndirimOran)
                                   .ToList();
        return View(urunler);
    }

    public async Task<List<Urunler>> IndirimA101Kayit()
    {
        var urunlerList = new List<Urunler>();

        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36");
            var response = await httpClient.GetAsync("https://www.a101.com.tr/haftanin-yildizlari");
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var elements = htmlDocument.DocumentNode.SelectNodes("//article[contains(@class, 'flex flex-col relative w-full px-3 bg-white border border-brand-gray-skeleton rounded-2xl')]");

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    System.Console.WriteLine(element.OuterHtml);
                    try
                    {
                        var html = element.InnerHtml;
                        var test = new HtmlDocument();
                        test.LoadHtml(html);

                        var itemNameElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-name')]");
                        var itemPriceElement = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'item-price')]");
                        var imgElement = test.DocumentNode.SelectSingleNode(".//img");
                        var ayrintiLink = test.DocumentNode.SelectSingleNode(".//a");
                        var itemEskiFiyat = test.DocumentNode.SelectSingleNode(".//span[contains(@class, 'priceLineThrough') and contains(@class, 'js-variant-price')]");

                        if (itemNameElement != null && itemPriceElement != null && imgElement != null)
                        {
                            var itemName = itemNameElement.InnerText.Trim();
                            var itemPrice = itemPriceElement.GetAttributeValue("content", "");

                            double parsedPrice;
                            if (double.TryParse(itemPrice, out parsedPrice))
                            {
                                parsedPrice = parsedPrice / 10.0;
                                itemPrice = parsedPrice.ToString("#.##");
                            }
                            else
                            {
                                // Fiyatı çıkartamadık
                                continue;
                            }

                            var dataSrc = imgElement.GetAttributeValue("data-src", "");

                            var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");

                            if (itemEskiFiyat != null)
                            {
                                var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];

                                var urun = new Urunler(itemName, itemPrice + " \u20BA", dataSrc, "carfoursa", "https://sdgmapturkey.com/wp-content/uploads/carrefoursa.png", 0.0, ayrintLinkString, 0, eskiFiyatString + " \u20BA", 0);
                                urunlerList.Add(urun);
                            }
                            else
                            {
                                var urun = new Urunler(itemName, itemPrice + " \u20BA", dataSrc, "carfoursa", "https://sdgmapturkey.com/wp-content/uploads/carrefoursa.png", 0.0, ayrintLinkString);
                                urunlerList.Add(urun);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return urunlerList;
    }
}
