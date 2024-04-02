using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class SokUrunServices
    {
        private readonly KelimeKontrol _kelimeKontrol;
        public SokUrunServices(KelimeKontrol kelimeKontrol) {
            _kelimeKontrol = kelimeKontrol;
        }
        private string SokArama(string query)
    {
        string query2 = _kelimeKontrol.ConvertTurkishToEnglish2(query);

        try
        {
            query2 = WebUtility.UrlEncode(_kelimeKontrol.ConvertTurkishToEnglish2(query2));
            return "https://www.sokmarket.com.tr/arama?q=" + query2;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }

    public async Task<List<Urunler>> SokKayit(String query)
    {

        string searchUrl = SokArama(query);

        var urunlerList = new List<Urunler>();

        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(searchUrl);
            var content = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var elements = htmlDocument.DocumentNode.SelectNodes("//div[@class='PLPProductListing_PLPCardParent__GC2qb']");

            if (elements != null)
            {
                foreach (var element in elements)
                {
                    try
                    {
                        string html = element.InnerHtml;
                        HtmlDocument test = new HtmlDocument();
                        test.LoadHtml(html);

                        var itemNameElement = test.DocumentNode.SelectSingleNode(".//h2[@class='CProductCard-module_title__u8bMW']");
                        var itemPriceElement = test.DocumentNode.SelectSingleNode(".//span[@class='CPriceBox-module_price__bYk-c']");
                        var imgElement = test.DocumentNode.SelectSingleNode(".//div[@class='CProductCard-module_imageContainer__aTMdz']//img");
                        var ayrintiLink = test.DocumentNode.SelectSingleNode(".//a");

                        if (itemNameElement != null && itemPriceElement != null && imgElement != null && ayrintiLink != null)
                        {
                            string urunAdi = itemNameElement.InnerText;
                            string fiyat = itemPriceElement.InnerText.Replace("\u20BA", "");
                            string dataSrc = imgElement.GetAttributeValue("src", "");
                            string ayrintLinkString = "https://www.sokmarket.com.tr/" + ayrintiLink.GetAttributeValue("href", "");

                            double benzerlikOrani = _kelimeKontrol.BenzerlikHesapla(_kelimeKontrol.ConvertTurkishToEnglish(query), _kelimeKontrol.ConvertTurkishToEnglish(urunAdi));
                            int katSayi = _kelimeKontrol.IkinciKelime2(query, urunAdi);
                            if (katSayi > 0)
                            {
                                benzerlikOrani += katSayi;
                            }

                            var itemEskiFiyat = test.DocumentNode.SelectSingleNode(".//span[@class='priceLineThrough js-variant-price']");

                            if (benzerlikOrani > 0.10)
                            {
                                urunlerList.Add(new Urunler(urunAdi, $"{fiyat} \u20BA", dataSrc, "Şok", "/img/Sok.png", benzerlikOrani, ayrintLinkString, 0, null, 0));
                            }
                            else
                            {
                                // Log or handle low similarity
                            }
                        }

                        else
                        {
                            if (itemNameElement == null)
                            {
                                Console.WriteLine("Item name element is null.");
                            }
                            if (itemPriceElement == null)
                            {
                                Console.WriteLine("Item price element is null.");
                            }
                            if (imgElement == null)
                            {
                                Console.WriteLine("Image element is null.");
                            }
                            if (ayrintiLink == null)
                            {
                                Console.WriteLine("Detail link element is null.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle inner loop exception
                    }
                }
            }
            else {
                System.Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return urunlerList;
    }

    private string RemovePartFromUrl(string url, string partToRemove)
    {
        return url.Replace(partToRemove, "");
    }
    }
}