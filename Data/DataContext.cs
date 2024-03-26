using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AkilliFiyatWeb.Entity;
using Microsoft.EntityFrameworkCore;

namespace AkilliFiyatWeb.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {
            
        }
        public DbSet<Urunler> Urunler =>  Set<Urunler>();
    }
}