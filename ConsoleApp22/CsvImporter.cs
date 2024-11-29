using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Core.Models;
using Core.Services;
using CsvHelper;
using CsvHelper.Configuration;

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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
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
                records = csv.GetRecords<TempHumidityRecord>().ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ett fel inträffade vid inläsning av CSV-filen: {ex.Message}");
            throw;
        }

        // Beräkna mögelrisk för varje post
        var recordsWithMoldRisk = records.Select(r => (Record: r, MoldRisk: CalculationService.CalculateMoldRisk(r))).ToList();
        return recordsWithMoldRisk;
    }
}
