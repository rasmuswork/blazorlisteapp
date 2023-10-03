using System;
using Microsoft.EntityFrameworkCore;
using TempApp;
using System.Collections.Concurrent;


namespace blazorlisteapp.Data
{



    public class AppDbContext : DbContext
    {
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=holidays.db");
    }
}

