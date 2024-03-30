using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AkilliFiyatWeb.Entity
{
    public class Urunler
    {
        public Urunler()
    {
        // Boş constructor
    }
        public Urunler(string urunAdi, string fiyat, string urunResmi, string marketAdi, string marketResmi, double benzerlik, string ayrintiLink, int miktar , string eskiFiyat, double indirimOran)
    {
        UrunAdi = urunAdi;
        Fiyat = fiyat;
        UrunResmi = urunResmi;
        MarketAdi = marketAdi;
        MarketResmi = marketResmi;
        Benzerlik = benzerlik;
        AyrintiLink = ayrintiLink;
        Miktar = miktar;
        EskiFiyat = eskiFiyat;
        IndirimOran = indirimOran;
    }

    public Urunler(string urunAdi, string fiyat, string urunResmi, string marketAdi, string marketResmi, double benzerlik, string ayrintiLink)
    {
        UrunAdi = urunAdi;
        Fiyat = fiyat;
        UrunResmi = urunResmi;
        MarketAdi = marketAdi;
        MarketResmi = marketResmi;
        Benzerlik = benzerlik;
        AyrintiLink = ayrintiLink;
        Miktar = 0;
        EskiFiyat = null;
        IndirimOran = null;
    }

        [Key]
        public int UrunlerId { get; set; }
        public string? UrunAdi { get; set; }
        public string? Fiyat { get; set; }
        public string? UrunResmi { get; set; }
        public string? MarketAdi { get; set; }
        public string? MarketResmi { get; set; }
        public double? Benzerlik { get; set; }
        public string? AyrintiLink { get; set; }
        public int Miktar { get; set; } = 0;
        public string? EskiFiyat { get; set; }
        public double? IndirimOran { get; set; }

        

    }

    
}