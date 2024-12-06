using Microsoft.EntityFrameworkCore;
using Core.Models;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {
        // Tabell för temperatur- och luftfuktighetsdata
        public DbSet<TempHumidityRecord> TempHumidityRecords { get; set; }

        // Konfigurerar databaskopplingen
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Använder SQLite som databas
                optionsBuilder.UseSqlite("Data Source=TempHumidityApp.db");
            }
        }

        // Konfigurerar databasens struktur
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TempHumidityRecord>(entity =>
            {
                entity.HasKey(e => e.Id); // Primärnyckel

                entity.Property(e => e.Date)
                    .IsRequired(); // Datum är obligatoriskt

                entity.Property(e => e.Temperature)
                    .IsRequired(); // Temperatur är obligatoriskt

                entity.Property(e => e.Humidity)
                    .IsRequired(); // Luftfuktighet är obligatoriskt

                entity.Property(e => e.IsIndoor)
                    .IsRequired(); // Anger om det är inomhusdata
            });
        }
    }
}