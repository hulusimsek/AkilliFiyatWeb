using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AkilliFiyatWeb.Entity
{
    public class Urunler
    {
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