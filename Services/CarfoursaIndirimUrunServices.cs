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
        var urunlerList = new List<Urunler>();

        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://www.carrefoursa.com/");
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var elements = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'product_click')]");

            if (elements != null)
            {
                foreach (var element in elements)
                {
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



                            int index = itemPrice.IndexOf('.') + 3; // Noktadan sonra 2 basamak
                            itemPrice = itemPrice.Replace('.', ',');
                            if (index < itemPrice.Length)
                            {
                                itemPrice = itemPrice.Substring(0, index);
                            }

                            double itemPriceDouble = Convert.ToDouble(itemPrice);
                            System.Console.WriteLine(itemPriceDouble);
                            itemPriceDouble = Math.Round(itemPriceDouble, 2);


                            System.Console.WriteLine(itemPriceDouble);

                            var dataSrc = imgElement.GetAttributeValue("data-src", "");

                            // Virgülü noktaya çevir



                            var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");
                            ayrintLinkString = RemovePartFromUrl(ayrintLinkString, "/quickView");

                            if (itemEskiFiyat != null)
                            {
                                var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];
                                Double EskiFiyatDouble = Convert.ToDouble(eskiFiyatString);

                                Double indirimOran =  ( EskiFiyatDouble - itemPriceDouble ) / EskiFiyatDouble * 100;
                                indirimOran = Math.Round(indirimOran,0);

                                var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", 0.0, ayrintLinkString, 0, eskiFiyatString + " \u20BA", indirimOran);
                                urunlerList.Add(urun);
                                _context.Urunler.Add(urun);
                            }
                            else
                            {
                                var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "https://sdgmapturkey.com/wp-content/uploads/carrefoursa.png", 0.0, ayrintLinkString);
                                urunlerList.Add(urun);
                                _context.Urunler.Add(urun);
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

        return urunlerList;
    }

    private string RemovePartFromUrl(string url, string partToRemove)
    {
        return url.Replace(partToRemove, "");
    }

    }
}