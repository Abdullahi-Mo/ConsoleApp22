using CsvHelper;
using CsvHelper.Configuration;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public static class CsvImporter
{
    /// <summary>
    /// Importerar data från en CSV-fil och beräknar mögelrisk för varje post.
    /// </summary>
    /// <param name="filePath">Sökvägen till CSV-filen.</param>
    /// <returns>Lista med poster och deras mögelrisk.</returns>
    public static List<(TempHumidityRecord Record, double MoldRisk)> Import(string filePath)
    {
        var records = new List<TempHumidityRecord>();

        // Ange svensk kultur för att hantera decimaler korrekt
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("sv-SE"))
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<TempHumidityRecordMap>();  // Registrera mappningen för CSV

                // Läs alla rader från CSV-filen och lagra dem i en lista
                var tempHumidityRecords = csv.GetRecords<TempHumidityRecord>().ToList();

                // Lägg till lästa poster till records
                records.AddRange(tempHumidityRecords);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ett CSV-relaterat fel inträffade: {ex.Message}");
            throw;
        }

        // Beräkna mögelrisk för varje post
        return records.Select(r => (Record: r, MoldRisk: r.Humidity * r.Temperature / 100.0)).ToList();
    }
}
