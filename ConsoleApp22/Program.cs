using System;
using System.Linq;
using Core.Models;
using Core.Services;
using DataAccess;

namespace UI
{
    class Program
    {
        static void Main(string[] args)
        {
            using var context = new AppDbContext();
            context.Database.EnsureCreated();
            DataSeeder.SeedData(context);

            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("Välkommen till Abdullahi/Abdikadirs dataväder analys!");
                Console.WriteLine("Välj ett alternativ:");
                Console.WriteLine("1. Visa alla data");
                Console.WriteLine("2. Visa medeltemperatur");
                Console.WriteLine("3. Visa sortering efter temperatur");
                Console.WriteLine("4. Visa sortering efter luftfuktighet");
                Console.WriteLine("5. Visa sortering efter mögelrisk");
                Console.WriteLine("6. Visa meteorologisk höst");
                Console.WriteLine("7. Visa meteorologisk vinter");
                Console.WriteLine("8. Läs in data från CSV-fil");
                Console.WriteLine("9. Visa skillnader mellan inomhus- och utomhustemperatur");
                Console.WriteLine("10. Visa balkongdörrens öppettider");
                Console.WriteLine("11. Visa medeltemperatur för valt datum utomhus");
                Console.WriteLine("12. Sortera utomhus medeltemperatur från varmast till kallast");
                Console.WriteLine("13. Sortera utomhus medelluftfuktighet från torrast till fuktigast");
                Console.WriteLine("0. Avsluta");

                Console.Write("\nDitt val: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowAllData(context);
                        break;
                    case "2":
                        ShowAverageTemperature(context);
                        break;
                    case "3":
                        ShowSortedByTemperature(context);
                        break;
                    case "4":
                        ShowSortedByHumidity(context);
                        break;
                    case "5":
                        ShowSortedByMoldRisk(context);
                        break;
                    case "6":
                        ShowMeteorologicalAutumn(context);
                        break;
                    case "7":
                        ShowMeteorologicalWinter(context);
                        break;
                    case "8":
                        ImportFromCsv(context);
                        break;
                    case "9":
                        ShowTemperatureDifferences(context);
                        break;
                    case "10":
                        ShowBalconyDoorOpenings(context);
                        break;
                    case "11":
                        ShowOutdoorAverageTemperatureForDate(context);
                        break;
                    case "12":
                        ShowOutdoorTemperatureSorted(context);
                        break;
                    case "13":
                        ShowOutdoorHumiditySorted(context);
                        break;
                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val, försök igen.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void ShowAllData(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            Console.WriteLine("\nAlla data:");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}, {record.Temperature}°C, {record.Humidity}%, {(record.IsIndoor ? "Inomhus" : "Utomhus")}");
            }
            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowAverageTemperature(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            var avgTemp = CalculationService.CalculateAverageTemperature(records);
            Console.WriteLine($"\nMedeltemperatur: {avgTemp:F2}°C");
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowSortedByTemperature(AppDbContext context)
        {
            var records = CalculationService.SortByTemperature(context.TempHumidityRecords.ToList());
            Console.WriteLine("\nSorterat efter temperatur (högst till lägst):");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}, {record.Temperature}°C");
            }
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowSortedByHumidity(AppDbContext context)
        {
            var records = CalculationService.SortByHumidity(context.TempHumidityRecords.ToList());
            Console.WriteLine("\nSorterat efter luftfuktighet (högst till lägst):");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}, {record.Humidity}%");
            }
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowSortedByMoldRisk(AppDbContext context)
        {
            var records = CalculationService.SortByMoldRisk(context.TempHumidityRecords.ToList());
            Console.WriteLine("\nSorterat efter mögelrisk (högst till lägst):");
            foreach (var (record, moldRisk) in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}, {record.Temperature}°C, {record.Humidity}%, Mögelrisk: {moldRisk:F2}");
            }
            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowMeteorologicalAutumn(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            var autumnDate = CalculationService.GetMeteorologicalAutumn(records);

            if (autumnDate.HasValue)
            {
                Console.WriteLine($"Meteorologisk höst börjar {autumnDate.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk höst identifierades.");
            }

            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowMeteorologicalWinter(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            var winterDate = CalculationService.GetMeteorologicalWinter(records);

            if (winterDate.HasValue)
            {
                Console.WriteLine($"Meteorologisk vinter börjar {winterDate.Value:yyyy-MM-dd}");
            }
            else
            {
                Console.WriteLine("Ingen meteorologisk vinter identifierades.");
            }

            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ImportFromCsv(AppDbContext context)
        {
            Console.Write("\nAnge sökvägen till CSV-filen: ");
            var filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Filen hittades inte.");
                Console.ReadKey();
                return;
            }

            try
            {
                var recordsWithMoldRisk = CsvImporter.Import(filePath);
                foreach (var (record, moldRisk) in recordsWithMoldRisk)
                {
                    if (!context.TempHumidityRecords.Any(r =>
                        r.Date == record.Date &&
                        r.Temperature == record.Temperature &&
                        r.Humidity == record.Humidity))
                    {
                        context.TempHumidityRecords.Add(record);
                        Console.WriteLine($"Lagt till: {record.Date:yyyy-MM-dd}, {record.Temperature}°C, {record.Humidity}%, Mögelrisk: {moldRisk:F2}");
                    }
                }
                context.SaveChanges();
                Console.WriteLine("Import från CSV lyckades.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade vid inläsning: {ex.Message}");
            }

            Console.ReadKey();
        }

        static void ShowTemperatureDifferences(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            if (!records.Any())
            {
                Console.WriteLine("Inga poster hittades i databasen.");
                Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
                Console.ReadKey();
                return;
            }

            var (maxDiff, minDiff, maxIndoor, maxOutdoor, minIndoor, minOutdoor) =
                CalculationService.SortIndoorOutdoorDifferences(records);

            Console.WriteLine("\nStörsta temperaturskillnad:");
            Console.WriteLine($"Inomhus: {maxIndoor.Temperature}°C, Utomhus: {maxOutdoor.Temperature}°C, Skillnad: {maxDiff:F2}°C");
            Console.WriteLine("\nMinsta temperaturskillnad:");
            Console.WriteLine($"Inomhus: {minIndoor.Temperature}°C, Utomhus: {minOutdoor.Temperature}°C, Skillnad: {minDiff:F2}°C");

            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowBalconyDoorOpenings(AppDbContext context)
        {
            var records = context.TempHumidityRecords.ToList();
            var balconyOpenings = CalculationService.CalculateDailyBalconyOpenTimes(records);

            Console.WriteLine("\nBalkongdörrens öppettider per dag:");
            foreach (var (date, totalOpenTime) in balconyOpenings)
            {
                Console.WriteLine($"{date:yyyy-MM-dd}: {totalOpenTime.TotalMinutes} minuter");
            }
            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowOutdoorAverageTemperatureForDate(AppDbContext context)
        {
            Console.Write("Ange datum (yyyy-MM-dd): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime selectedDate))
            {
                var records = context.TempHumidityRecords
                    .Where(r => r.Date.Date == selectedDate && !r.IsIndoor)
                    .ToList();

                if (records.Any())
                {
                    var avgTemp = records.Average(r => r.Temperature);
                    Console.WriteLine($"Medeltemperatur utomhus för {selectedDate:yyyy-MM-dd}: {avgTemp:F2}°C");
                }
                else
                {
                    Console.WriteLine("Inga utomhusposter hittades för detta datum.");
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt datumformat.");
            }

            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowOutdoorTemperatureSorted(AppDbContext context)
        {
            var records = context.TempHumidityRecords
                .Where(r => !r.IsIndoor)
                .GroupBy(r => r.Date.Date)
                .Select(g => new { Date = g.Key, AvgTemperature = g.Average(r => r.Temperature) })
                .OrderByDescending(g => g.AvgTemperature)
                .ToList();

            Console.WriteLine("\nSortering av utomhus medeltemperatur (varmast till kallast):");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}: {record.AvgTemperature:F2}°C");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        static void ShowOutdoorHumiditySorted(AppDbContext context)
        {
            var records = context.TempHumidityRecords
                .Where(r => !r.IsIndoor)
                .GroupBy(r => r.Date.Date)
                .Select(g => new { Date = g.Key, AvgHumidity = g.Average(r => r.Humidity) })
                .OrderBy(g => g.AvgHumidity)
                .ToList();

            Console.WriteLine("\nSortering av utomhus medelluftfuktighet (torrast till fuktigast):");
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Date:yyyy-MM-dd}: {record.AvgHumidity:F2}%");
            }

            Console.WriteLine("\nTryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }
    }
}