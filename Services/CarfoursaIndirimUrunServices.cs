using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using AkilliFiyatWeb.Services;

namespace AkilliFiyatWeb.Services
{
    public class CarfoursaIndirimUrunServices
    {
        private readonly DataContext _context;
        private readonly KelimeKontrol _kelimeKontrol;


        public CarfoursaIndirimUrunServices(DataContext context, KelimeKontrol kelimeKontrol)
        {
            _context = context;
            _kelimeKontrol = kelimeKontrol;
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
                                itemPriceDouble = Math.Round(itemPriceDouble, 2);


                                var dataSrc = imgElement.GetAttributeValue("data-src", "");

                                // Virgülü noktaya çevir



                                var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");
                                ayrintLinkString = RemovePartFromUrl(ayrintLinkString, "/quickView");

                                if (itemEskiFiyat != null)
                                {
                                    var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];
                                    Double EskiFiyatDouble = Convert.ToDouble(eskiFiyatString);

                                    Double indirimOran = (EskiFiyatDouble - itemPriceDouble) / EskiFiyatDouble * 100;
                                    indirimOran = Math.Round(indirimOran, 0);

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

        private string CarArama(string query)
        {
            string query2 = _kelimeKontrol.ConvertTurkishToEnglish2(query);

            try
            {
                query2 = WebUtility.UrlEncode(_kelimeKontrol.ConvertTurkishToEnglish2(query2));
                return "https://www.carrefoursa.com/search/?text=" + query2;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

        public async Task<List<Urunler>> CarfoursaKayit(String query)
        {

            string searchUrl = CarArama(query);

            var urunlerList = new List<Urunler>();

            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(searchUrl);
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

                                double benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(itemName));
                                int katSayi = _kelimeKontrol.IkinciKelime2(query, itemName);
                                if (katSayi > 0)
                                {
                                    benzerlikOrani += katSayi;
                                }


                                double itemPriceDouble = Convert.ToDouble(itemPrice);
                                itemPriceDouble = Math.Round(itemPriceDouble, 2);


                                var dataSrc = imgElement.GetAttributeValue("data-src", "");

                                // Virgülü noktaya çevir



                                var ayrintLinkString = "https://www.carrefoursa.com" + ayrintiLink.GetAttributeValue("href", "");
                                ayrintLinkString = RemovePartFromUrl(ayrintLinkString, "/quickView");

                                if (itemEskiFiyat != null)
                                {
                                    var eskiFiyatString = itemEskiFiyat.InnerText.Trim().Split(" TL")[0];
                                    Double EskiFiyatDouble = Convert.ToDouble(eskiFiyatString);

                                    Double indirimOran = (EskiFiyatDouble - itemPriceDouble) / EskiFiyatDouble * 100;
                                    indirimOran = Math.Round(indirimOran, 0);

                                    var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", benzerlikOrani, ayrintLinkString, 0, eskiFiyatString + " \u20BA", indirimOran);
                                    urunlerList.Add(urun);
                                }
                                else
                                {
                                    var urun = new Urunler(itemName, itemPriceDouble + " \u20BA", dataSrc, "Carrefour-SA", "/img/Carrefour-SA.png", benzerlikOrani, ayrintLinkString);
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

}