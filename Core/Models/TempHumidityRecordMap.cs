using CsvHelper.Configuration;
using Core.Models;
using System.Globalization;

public sealed class TempHumidityRecordMap : ClassMap<TempHumidityRecord>
{
    public TempHumidityRecordMap()
    {
        // Mappa datum och använd rätt format för datumfältet
        Map(m => m.Date)
            .Name("Datum")
            .TypeConverterOption.Format("yyyy-MM-dd HH:mm"); // Korrekt format för datum

        // Mappa temperatur och luftfuktighet
        Map(m => m.Temperature)
            .Name("Temp")
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture); // Hantera decimaltecken korrekt

        Map(m => m.Humidity)
            .Name("Luftfuktighet")
            .TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture); // Hantera decimaltecken korrekt

        // Mappa inomhus/utomhus med en logik baserat på textinnehåll
        Map(m => m.IsIndoor)
            .Convert(args => args.Row.GetField("Plats").ToLower() == "inne");
    }
}
