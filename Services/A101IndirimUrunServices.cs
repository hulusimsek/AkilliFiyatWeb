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
using Newtonsoft.Json.Linq;
using AkilliFiyatWeb.Migrations;
using Urunler = AkilliFiyatWeb.Entity.Urunler;

namespace AkilliFiyatWeb.Services
{
	public class A101IndirimUrunServices
	{
		private readonly DataContext _context;
		private readonly ApiService _apiService;
		private readonly KelimeKontrol _kelimeKontrol;
		public A101IndirimUrunServices(DataContext context, ApiService apiService, KelimeKontrol kelimeKontrol)
		{
			_context = context;
			_apiService = apiService;
			_kelimeKontrol = kelimeKontrol;
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

		public async Task<List<Urunler>> A101Kayit(string query)
		{
			List<Urunler> a101Urunler = new List<Urunler>();

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
								"/img/A-101.png",
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
}