using Core.Models;
using System.Linq;

namespace DataAccess
{
    public static class DataSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            if (!context.TempHumidityRecords.Any())
            {
                context.TempHumidityRecords.AddRange(new[]
                {
                    new TempHumidityRecord { Date = DateTime.Now.AddDays(-6), Temperature = 12.5, Humidity = 65, IsIndoor = true },
                    new TempHumidityRecord { Date = DateTime.Now.AddDays(-5), Temperature = 8.0, Humidity = 70, IsIndoor = false },
                    new TempHumidityRecord { Date = DateTime.Now.AddDays(-4), Temperature = 10.0, Humidity = 75, IsIndoor = true }
                });

                context.SaveChanges();
            }
        }
    }
}