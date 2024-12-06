using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Core.Models;

namespace DataAccess
{
    public static class CsvImporter
    {
        public static List<(TempHumidityRecord record, double moldRisk)> Import(string filePath)
        {
            var records = new List<(TempHumidityRecord, double)>();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Hoppa över header-raden om det finns en
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        // Förväntade kolumner: Date, Temperature, Humidity, IsIndoor
                        if (values.Length < 4) continue;

                        DateTime date;
                        double temperature, humidity;
                        bool isIndoor;

                        // Försök att konvertera och fånga eventuella fel
                        if (DateTime.TryParse(values[0], out date) &&
                            double.TryParse(values[1], out temperature) &&
                            double.TryParse(values[2], out humidity) &&
                            bool.TryParse(values[3], out isIndoor))
                        {
                            var record = new TempHumidityRecord
                            {
                                Date = date,
                                Temperature = temperature,
                                Humidity = humidity,
                                IsIndoor = isIndoor
                            };

                            // Beräkna mögelrisken baserat på temperatur och luftfuktighet (exempel)
                            double moldRisk = CalculateMoldRisk(temperature, humidity);

                            records.Add((record, moldRisk));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid CSV-import: {ex.Message}");
            }

            return records;
        }

        private static double CalculateMoldRisk(double temperature, double humidity)
        {
            // Enkel logik för att beräkna mögelrisk: hög luftfuktighet och låg temperatur ökar risken
            return (humidity - 60) * (20 - temperature) * 0.1;
        }
    }
}
