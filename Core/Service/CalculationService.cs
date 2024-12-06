using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Services
{
    public static class CalculationService
    {
        /// <summary>
        /// Beräknar medeltemperaturen för alla poster.
        /// </summary>
        public static double CalculateAverageTemperature(IEnumerable<TempHumidityRecord> records)
        {
            return records.Average(r => r.Temperature);
        }

        /// <summary>
        /// Sorterar poster efter temperatur (högst till lägst).
        /// </summary>
        public static IEnumerable<TempHumidityRecord> SortByTemperature(IEnumerable<TempHumidityRecord> records)
        {
            return records.OrderByDescending(r => r.Temperature);
        }

        /// <summary>
        /// Sorterar poster efter luftfuktighet (högst till lägst).
        /// </summary>
        public static IEnumerable<TempHumidityRecord> SortByHumidity(IEnumerable<TempHumidityRecord> records)
        {
            return records.OrderByDescending(r => r.Humidity);
        }

        /// <summary>
        /// Sorterar poster efter mögelrisk (högst till lägst).
        /// </summary>
        public static IEnumerable<(TempHumidityRecord Record, double MoldRisk)> SortByMoldRisk(IEnumerable<TempHumidityRecord> records)
        {
            return records.Select(r => (r, MoldRisk: CalculateMoldRisk(r)))
                          .OrderByDescending(x => x.MoldRisk);
        }

        /// <summary>
        /// Beräknar mögelrisk baserat på luftfuktighet och temperatur.
        /// </summary>
        public static double CalculateMoldRisk(TempHumidityRecord record)
        {
            return record.Humidity * record.Temperature / 100.0;
        }

        /// <summary>
        /// Identifierar datum för meteorologisk höst (utomhus).
        /// </summary>
        public static DateTime? GetMeteorologicalAutumn(IEnumerable<TempHumidityRecord> records)
        {
            var outdoorRecords = records.Where(r => !r.IsIndoor).OrderBy(r => r.Date).ToList();

            for (int i = 0; i <= outdoorRecords.Count - 5; i++)
            {
                if (outdoorRecords.Skip(i).Take(5).All(r => r.Temperature < 10))
                {
                    return outdoorRecords[i].Date.Date;
                }
            }

            return null; // Om ingen höst hittas
        }

        /// <summary>
        /// Identifierar datum för meteorologisk vinter (utomhus).
        /// </summary>
        public static DateTime? GetMeteorologicalWinter(IEnumerable<TempHumidityRecord> records)
        {
            var outdoorRecords = records.Where(r => !r.IsIndoor).OrderBy(r => r.Date).ToList();

            for (int i = 0; i <= outdoorRecords.Count - 5; i++)
            {
                if (outdoorRecords.Skip(i).Take(5).All(r => r.Temperature < 0))
                {
                    return outdoorRecords[i].Date.Date;
                }
            }

            return null; // Om ingen vinter hittas
        }

        /// <summary>
        /// Sorterar poster efter största och minsta temperaturskillnad mellan inne och ute.
        /// </summary>
        public static (double MaxDifference, double MinDifference, TempHumidityRecord MaxDiffIndoor, TempHumidityRecord MaxDiffOutdoor, TempHumidityRecord MinDiffIndoor, TempHumidityRecord MinDiffOutdoor)
        SortIndoorOutdoorDifferences(IEnumerable<TempHumidityRecord> records)
        {
            var indoorRecords = records.Where(r => r.IsIndoor).ToList();
            var outdoorRecords = records.Where(r => !r.IsIndoor).ToList();

            double maxDifference = double.MinValue;
            double minDifference = double.MaxValue;
            TempHumidityRecord maxDiffIndoor = null;
            TempHumidityRecord maxDiffOutdoor = null;
            TempHumidityRecord minDiffIndoor = null;
            TempHumidityRecord minDiffOutdoor = null;

            foreach (var indoor in indoorRecords)
            {
                foreach (var outdoor in outdoorRecords)
                {
                    double difference = Math.Abs(indoor.Temperature - outdoor.Temperature);

                    if (difference > maxDifference)
                    {
                        maxDifference = difference;
                        maxDiffIndoor = indoor;
                        maxDiffOutdoor = outdoor;
                    }

                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        minDiffIndoor = indoor;
                        minDiffOutdoor = outdoor;
                    }
                }
            }

            return (maxDifference, minDifference, maxDiffIndoor, maxDiffOutdoor, minDiffIndoor, minDiffOutdoor);
        }

        /// <summary>
        /// Beräknar balkongdörrens öppettider per dag baserat på temperatursänkningar.
        /// </summary>
        public static List<(DateTime Date, TimeSpan TotalOpenTime)> CalculateDailyBalconyOpenTimes(IEnumerable<TempHumidityRecord> records)
        {
            // Filtrera inomhusposter och sortera efter datum
            var indoorRecords = records.Where(r => r.IsIndoor).OrderBy(r => r.Date).ToList();
            var balconyOpenings = new List<(DateTime Date, TimeSpan TotalOpenTime)>();

            // Iterera genom inomhusposter för att identifiera temperatursänkningar
            for (int i = 1; i < indoorRecords.Count; i++)
            {
                var prevRecord = indoorRecords[i - 1];
                var currentRecord = indoorRecords[i];

                // Kontrollera om temperaturen har sjunkit med minst 2°C
                if (prevRecord.Temperature - currentRecord.Temperature >= 2)
                {
                    var date = currentRecord.Date.Date;
                    var openDuration = TimeSpan.FromMinutes(15);

                    // Kolla om balkongdörren redan är öppen för det datumet
                    var existingOpening = balconyOpenings.FirstOrDefault(x => x.Date == date);

                    if (existingOpening != default)
                    {
                        // Uppdatera total öppettid
                        balconyOpenings.Remove(existingOpening);
                        balconyOpenings.Add((date, existingOpening.TotalOpenTime + openDuration));
                    }
                    else
                    {
                        // Lägg till nytt öppningstillfälle
                        balconyOpenings.Add((date, openDuration));
                    }
                }
            }

            // Returnera sorterade öppningstider (från längst till kortast)
            return balconyOpenings.OrderByDescending(x => x.TotalOpenTime).ToList();
        }
    }
}