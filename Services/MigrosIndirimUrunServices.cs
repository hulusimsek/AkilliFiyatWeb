using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using AkilliFiyatWeb.Entity;
using Newtonsoft.Json.Linq; // JSON.NET kütüphanesini ekleyin


namespace AkilliFiyatWeb.Services
{
    public class MigrosIndirimUrunServices
    {
        private readonly ApiService _apiService;
        private readonly DataContext _dataContext; // ApplicationDbContext'e bağlanacak bir context

        public MigrosIndirimUrunServices(ApiService apiService, DataContext dataContext)
        {
            _apiService = apiService;
            _dataContext = dataContext;
        }

        public async Task<List<Urunler>> IndirimMigrosKayit()
        {
            List<Urunler> indirimliMigrosUrunler = new List<Urunler>();

            for (int i = 5; i > 0; i = i - 2)
            {
                var indirimlerMigros = await _apiService.IndirimMigrosApiAsync(i);

                if (indirimlerMigros != null)
                {
                    dynamic jsonResponse = JObject.Parse(indirimlerMigros);
                    if (jsonResponse != null && jsonResponse.data != null && jsonResponse.data.searchInfo != null && jsonResponse.data.searchInfo.storeProductInfos != null)
                    {
                        foreach (var urun in jsonResponse.data.searchInfo.storeProductInfos)
                        {
                            decimal fiyat;
                            if (decimal.TryParse(urun.shownPrice.ToString(), out fiyat))
                            {
                                decimal result = fiyat / 100;
                                string resultString = result.ToString();
                            }
                            else
                            {
                                fiyat = 0;
                            }

                            double fiyat2 = (double)fiyat / 100.0;
                            string fiyat3 = fiyat2.ToString("0.00");

                            Urunler eklenecekUrun = new Urunler
                            {
                                UrunAdi = urun.name,
                                Fiyat = fiyat3 + " ₺",
                                UrunResmi = urun.images[0].urls.PRODUCT_HD,
                                MarketAdi = "migros",
                                MarketResmi = "https://seeklogo.com/images/M/Migros-logo-09BB1C8FEF-seeklogo.com.png",
                                Benzerlik = 1.0,
                                AyrintiLink = urun.information
                            };

                            if (urun.badges != null && urun.badges.Count > 0 && urun.badges[0].value != null)
                            {
                                eklenecekUrun.EskiFiyat = urun.badges[0].value;
                            }

                            indirimliMigrosUrunler.Add(eklenecekUrun);
                            await _dataContext.Urunler.AddAsync(eklenecekUrun);
                        }
                    }
                }
                else
                {
                    // API çağrısı başarısız oldu.
                }
            }

            await _dataContext.SaveChangesAsync();
            return indirimliMigrosUrunler;
        }
    }

}