using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AkilliFiyatWeb.Entity;
using HtmlAgilityPack;
using AkilliFiyatWeb.Data;

namespace AkilliFiyatWeb.Services
{
    public class BimIndirimUrunServices
    {
        private readonly DataContext _context;

        public BimIndirimUrunServices(DataContext context)
        {
            _context = context;
        }
        public async Task<List<Urunler>> IndirimBimKayit()
        {
            var urunlerList = new List<Urunler>();

            try
            {
                var baseUrl = "https://www.bim.com.tr/";

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36");

                var response = await httpClient.GetAsync(baseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var html = await response.Content.ReadAsStringAsync();
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    var innerTriangleElements = htmlDocument.DocumentNode.SelectNodes(".//div[contains(@class, 'inner triangle')]");
                    if (innerTriangleElements != null && innerTriangleElements.Count >= 2)
                    {
                        var secondInnerTriangle = innerTriangleElements[1];
                        var subButtonElement = secondInnerTriangle.SelectSingleNode(".//a[contains(@class, 'subButton')]");
                        if (subButtonElement != null)
                        {
                            var hrefLink = baseUrl + subButtonElement.GetAttributeValue("href", "");
                            var productResponse = await httpClient.GetAsync(hrefLink);

                            if (productResponse.IsSuccessStatusCode)
                            {
                                var productContent = await productResponse.Content.ReadAsStringAsync();
                                var productHtmlDocument = new HtmlDocument();
                                productHtmlDocument.LoadHtml(productContent);

                                var urunler1 = await ProcessElements1(productHtmlDocument.DocumentNode.SelectNodes(".//div[contains(@class, 'product col-xl-3 col-lg-3 col-md-4 col-sm-6 col-12')]"));
                                var urunler2 = await ProcessElements2(productHtmlDocument.DocumentNode.SelectNodes(".//div[contains(@class, 'product col-xl-3 col-lg-3 col-md-4 col-sm-6 col-12 LoadGroup0')]"));

                                urunlerList.AddRange(urunler1);
                                urunlerList.AddRange(urunler2);

                                // AddAsync ve SaveChangesAsync'i aynı transaction içinde kullanın
                                using (var transaction = await _context.Database.BeginTransactionAsync())
                                {
                                    try
                                    {
                                        foreach (var urun in urunlerList)
                                        {
                                            await _context.Urunler.AddAsync(urun);
                                        }
                                        var result = await _context.SaveChangesAsync();

                                        // Geri dönüş değerini kontrol et
                                        if (result > 0)
                                        {
                                            Console.WriteLine("Değişiklikler başarıyla kaydedildi. " + result);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Değişiklikler kaydedilemedi veya herhangi bir değişiklik yapılmadı.");
                                        }


                                        // Tüm işlemler başarılı olduysa, işlemi commit edin
                                        await transaction.CommitAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        // Hata durumunda işlemi geri al
                                        await transaction.RollbackAsync();
                                        Console.WriteLine("Hata: " + ex.Message);
                                    }
                                }
                            }
                            else
                            {
                                var errorContent = await productResponse.Content.ReadAsStringAsync();
                                Console.WriteLine($"Error: {productResponse.ReasonPhrase}");
                                Console.WriteLine($"Error Content: {errorContent}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                // Hata oluştuğunda loglama veya hata yönetimi yapılabilir
            }

            return urunlerList;
        }


        private async Task<List<Urunler>> ProcessElements1(HtmlNodeCollection elements)
        {
            var urunler = new List<Urunler>();

            foreach (var element in elements)
            {
                try
                {
                    var urun = new Urunler();
                    var itemNameElement = element.SelectSingleNode(".//h2[contains(@class, 'subTitle')]");
                    var itemNameElement2 = element.SelectSingleNode(".//h2[contains(@class, 'title')]");
                    var textQuantifyElements = element.SelectNodes(".//div[contains(@class, 'text quantify')]");
                    var ayrintiLinkElement = element.SelectSingleNode(".//a");

                    if (itemNameElement != null && itemNameElement2 != null && textQuantifyElements != null && ayrintiLinkElement != null)
                    {
                        var ayrintLinkString = "https://www.bim.com.tr/" + ayrintiLinkElement.GetAttributeValue("href", "");
                        var itemPrice = textQuantifyElements.Count >= 2 ? textQuantifyElements[1].InnerText : "";
                        var itemEskiFiyat = textQuantifyElements != null ? textQuantifyElements[0].InnerText : "";
                        var itemPriceElement2 = element.SelectSingleNode(".//span[contains(@class, 'number')]");

                        var itemName = itemNameElement.InnerText;
                        var itemName2 = itemNameElement2.InnerText;
                        var itemPrice2 = itemPriceElement2 != null ? itemPriceElement2.InnerText : "";
                        var dataSrc = element.SelectSingleNode(".//img").GetAttributeValue("xsrc", "");
                        var doubleEskiFiyat = Convert.ToDouble(itemEskiFiyat);

                        Double itemFiyat = Convert.ToDouble(itemPrice + itemPrice2);

                        double indirimOran = (doubleEskiFiyat - itemFiyat) / doubleEskiFiyat * 100;
                        indirimOran = Math.Round(indirimOran, 0);

                        urunler.Add(new Urunler(itemName + " " + itemName2, itemPrice + itemPrice2 + " ₺", "https://www.bim.com.tr" + dataSrc, "Bim", "~/img/Bim.png", 0.0, ayrintLinkString, 0, itemEskiFiyat, indirimOran));
                    }
                    else
                    {
                        if (itemNameElement == null)
                        {
                            Console.WriteLine("subTitle öğesi bulunamadı.");
                        }
                        if (itemNameElement2 == null)
                        {
                            Console.WriteLine("title öğesi bulunamadı.");
                        }
                        if (textQuantifyElements == null)
                        {
                            Console.WriteLine("text quantify öğeleri bulunamadı.");
                        }
                        if (ayrintiLinkElement == null)
                        {
                            Console.WriteLine("a öğesi bulunamadı.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                }
            }


            return urunler;
        }

        private async Task<List<Urunler>> ProcessElements2(HtmlNodeCollection elements)
        {
            var urunler = new List<Urunler>();

            foreach (var element in elements)
            {
                try
                {
                    var urun = new Urunler();
                    var itemNameElement = element.SelectSingleNode(".//h2[contains(@class, 'subTitle')]");
                    var itemNameElement2 = element.SelectSingleNode(".//h2[contains(@class, 'title')]");
                    var textQuantifyElements = element.SelectNodes(".//div[contains(@class, 'text quantify')]");
                    var ayrintiLinkElement = element.SelectSingleNode(".//a");

                    var ayrintLinkString = "https://www.bim.com.tr" + ayrintiLinkElement.GetAttributeValue("href", "");

                    var itemPrice = textQuantifyElements != null && textQuantifyElements.Count >= 2 ? textQuantifyElements[1].InnerText : "";
                    var itemPriceElement2 = element.SelectSingleNode(".//span[contains(@class, 'number')]");
                    var itemEskiFiyat = textQuantifyElements != null ? textQuantifyElements[0].InnerText : "";
                    var doubleEskiFiyat = Convert.ToDouble(itemEskiFiyat);

                    var itemName = itemNameElement.InnerText;
                    var itemName2 = itemNameElement2.InnerText;
                    var itemPrice2 = itemPriceElement2.InnerText;
                    var dataSrc = element.SelectSingleNode(".//img").GetAttributeValue("xsrc", "");
                    Double itemFiyat = Convert.ToDouble(itemPrice + itemPrice2);

                    double indirimOran = (doubleEskiFiyat - itemFiyat) / doubleEskiFiyat * 100;
                    indirimOran = Math.Round(indirimOran, 0);

                    urunler.Add(new Urunler(itemName + " " + itemName2, itemPrice + itemPrice2 + " ₺", "https://www.bim.com.tr" + dataSrc, "Bim", "/img/Bim.png", 0.0, ayrintLinkString, 0, itemEskiFiyat, indirimOran));
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    Console.WriteLine("Hata: " + ex.Message);
                }
            }

            return urunler;
        }



    }
}