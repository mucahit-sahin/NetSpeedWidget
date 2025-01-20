using Microsoft.EntityFrameworkCore;
using NetSpeedWidget.Models;
using System;
using System.IO;

namespace NetSpeedWidget.Data
{
    public class NetworkDbContext : DbContext
    {
        public DbSet<NetworkUsageData> NetworkUsages { get; set; }
        public DbSet<ApplicationUsageData> ApplicationUsages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NetSpeedWidget");

            Directory.CreateDirectory(appDataPath);
            var dbPath = Path.Combine(appDataPath, "networkstats.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkUsageData>()
                .HasMany(n => n.ApplicationUsages)
                .WithOne(a => a.NetworkUsageData)
                .HasForeignKey(a => a.NetworkUsageDataId);
        }
    }
}