namespace Core.Models
{
    public class TempHumidityRecord
    {
        public int Id { get; set; } // Unikt ID för varje post
        public DateTime Date { get; set; } // Datum för mätningen
        public double Temperature { get; set; } // Temperaturvärdet
        public double Humidity { get; set; } // Luftfuktighetsvärdet
        public bool IsIndoor { get; set; } // Om mätningen är inomhus eller utomhus
    }
}
