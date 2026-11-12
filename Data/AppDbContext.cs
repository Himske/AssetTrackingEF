using AssetTrackingEF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetTrackingEF.Data {
    internal class AppDbContext: DbContext {
        private readonly string _connectionString =
            @"Server=localhost\SQLEXPRESS;Database=asset_tracking_ef;Trusted_Connection=True;TrustServerCertificate=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            //optionsBuilder.UseSqlServer(_connectionString).LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], Microsoft.Extensions.Logging.LogLevel.Information);
            optionsBuilder.UseSqlServer(_connectionString);
        }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<ComputerAsset> ComputerAssets { get; set; }
        public DbSet<MobileAsset> MobileAssets { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }
    }
}
