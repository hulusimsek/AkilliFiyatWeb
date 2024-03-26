using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;


namespace AkilliFiyatWeb.Services
{
    public class CarfoursaIndirimUrunServices
    {
        private readonly DataContext _context;

        public CarfoursaIndirimUrunServices(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Urunler>> IndirimCarfoursaKayit()
        {
            var urunlerArrayList = new List<Urunler>();

            try
            {
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync("https://www.carrefoursa.com/");
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var productClicks = htmlDocument.DocumentNode.SelectNodes("//div[@class='product_click']");
                if (productClicks != null)
                {
                    foreach (var productClick in productClicks)
                    {
                        try
                        {
                            var itemNameElement = productClick.SelectSingleNode(".//h2[@class='item-name']");
                            var itemPriceElement = productClick.SelectSingleNode(".//span[@class='item-price']");
                            var imgElement = productClick.SelectSingleNode(".//img");

                            if (itemNameElement != null && itemPriceElement != null && imgElement != null)
                            {
                                var itemName = itemNameElement.InnerText.Trim();
                                var itemPrice = itemPriceElement.GetAttributeValue("content", "");
                                var dataSrc = imgElement.GetAttributeValue("data-src", "");

                                var ayrintiLink = productClick.SelectSingleNode(".//a");
                                var ayrintLinkString = ayrintiLink != null ? "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "") : "";

                                var itemEskiFiyat = productClick.SelectSingleNode(".//span[@class='priceLineThrough js-variant-price']");
                                if (itemEskiFiyat != null)
                                {
                                    var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(new string[] { " TL" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                    Urunler eklenecekUrun = new Urunler
                                    {
                                        UrunAdi = itemName,
                                        Fiyat = itemPrice + " ₺",
                                        UrunResmi = dataSrc,
                                        MarketAdi = "carfoursa",
                                        MarketResmi = "https://seeklogo.com/images/M/Migros-logo-09BB1C8FEF-seeklogo.com.png",
                                        Benzerlik = 1.0,
                                        AyrintiLink = ayrintLinkString,
                                        EskiFiyat = eskiFiyatString + " ₺"
                                    
                                    };
                                    await _context.Urunler.AddAsync(eklenecekUrun);

                                }
                                else
                                {
                                    Urunler eklenecekUrun = new Urunler
                                    {
                                        UrunAdi = itemName,
                                        Fiyat = itemPrice + " ₺",
                                        UrunResmi = dataSrc,
                                        MarketAdi = "carfoursa",
                                        MarketResmi = "https://seeklogo.com/images/M/Migros-logo-09BB1C8FEF-seeklogo.com.png",
                                        Benzerlik = 1.0,
                                        AyrintiLink = ayrintLinkString,
                                        EskiFiyat = null
                                    
                                    };

                                    await _context.Urunler.AddAsync(eklenecekUrun);
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

            await _context.SaveChangesAsync();

            return urunlerArrayList;
        }

    }
}