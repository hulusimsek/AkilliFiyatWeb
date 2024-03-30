using System;
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

namespace AkilliFiyatWeb.Services
{
    public class A101IndirimUrunServices
    {
        private readonly DataContext _context;
        public A101IndirimUrunServices(DataContext context)
    {
        _context = context;
    }
        public async Task<List<Urunler>> IndirimA101Kayit()
    {
        var urunlerList = new List<Urunler>();

        try
        {
            var url = "https://www.a101.com.tr/ekstra/haftanin-yildizlari";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var articles = doc.DocumentNode.SelectNodes("//article");

            foreach (var article in articles)
            {
                try
                {
                    var itemNameElement = article.SelectSingleNode(".//header/hgroup/h3");
                    var itemPriceElement = article.SelectSingleNode(".//section/span");
                    var itemEskiFiyat = article.SelectSingleNode(".//section/s");
                    var imgElement = article.SelectSingleNode(".//figure/div/div/img");
                    var ayrintiLink = article.SelectSingleNode(".//a");

                    if (itemNameElement != null && itemPriceElement != null && imgElement != null && ayrintiLink != null)
                    {
                        var itemName = itemNameElement.InnerText.Trim();
                        var itemPrice = itemPriceElement.InnerText;
                        var ayrintLinkString = ayrintiLink.GetAttributeValue("href", "");

                        string eskiFiyat = "";
                        if (itemEskiFiyat != null)
                        {
                            eskiFiyat = itemEskiFiyat.InnerText.Replace("\u20BA", "");
                        }

                        var dataSrc = imgElement.GetAttributeValue("src", "");

                        double indirimOran = 0.0;

                        if (!string.IsNullOrEmpty(eskiFiyat) && double.TryParse(itemPrice, out double yeniFiyat) && double.TryParse(eskiFiyat.Replace(" \u20BA", ""), out double eskiFiyatDouble))
                        {
                            indirimOran = ((eskiFiyatDouble - yeniFiyat) / eskiFiyatDouble) * 100;
                        }

                        var urun = new Urunler(itemName, itemPrice + " \u20BA", dataSrc, "A-101", "/img/A-101.png", 0.0, ayrintLinkString, 0, eskiFiyat + " \u20BA", indirimOran);
                        urunlerList.Add(urun);
                        await _context.Urunler.AddAsync(urun);
                    }


                    else
                    {
                        Console.WriteLine("Bir veya daha fazla özellik null");
                        if (itemNameElement == null)
                        {
                            Console.WriteLine("itemNameElement null");
                        }
                        if (itemPriceElement == null)
                        {
                            Console.WriteLine("itemPriceElement null");
                        }
                        if (imgElement == null)
                        {
                            Console.WriteLine("imgElement null");
                        }
                        if (ayrintiLink == null)
                        {
                            Console.WriteLine("ayrintiLink null");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        await _context.SaveChangesAsync();
        return urunlerList;
    }
    }
}