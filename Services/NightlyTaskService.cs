using System;
using System.Threading;
using System.Threading.Tasks;
using AkilliFiyatWeb.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AkilliFiyatWeb.Services
{
    public class NightlyTaskService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _calismaZamani;

        public NightlyTaskService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _calismaZamani = new TimeSpan(11, 2, 0); // Saat 12:14
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var simdikiZaman = DateTime.Now;
                var beklemeSuresi = simdikiZaman.Date + _calismaZamani - simdikiZaman;

                if (beklemeSuresi < TimeSpan.Zero)
                {
                    beklemeSuresi = beklemeSuresi.Add(new TimeSpan(1, 0, 0, 0));
                }

                await Task.Delay(beklemeSuresi, stoppingToken);

                using (var kapsam = _serviceProvider.CreateScope())
                {
                    var veritabaniBaglantisi = kapsam.ServiceProvider.GetRequiredService<DataContext>();
                    var migrosServisi = kapsam.ServiceProvider.GetRequiredService<MigrosIndirimUrunServices>();
                    var carfoursaServisi = kapsam.ServiceProvider.GetRequiredService<CarfoursaIndirimUrunServices>();
                    var bimServisi = kapsam.ServiceProvider.GetRequiredService<BimIndirimUrunServices>();
                    var a101Servisi = kapsam.ServiceProvider.GetRequiredService<A101IndirimUrunServices>();

                    try
                    {
                        veritabaniBaglantisi.Urunler.RemoveRange(veritabaniBaglantisi.Urunler);
                        await veritabaniBaglantisi.SaveChangesAsync();

                        await migrosServisi.IndirimMigrosKayit();

                        await carfoursaServisi.IndirimCarfoursaKayit();

                        await bimServisi.IndirimBimKayit();

                        await a101Servisi.IndirimA101Kayit();

                        Console.WriteLine("GeceLikGorevServisi: Görevler başarıyla tamamlandı.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("GeceLikGorevServisi: Hata oluştu: " + ex.Message);
                    }
                }
            }
        }
    }
}