namespace Acme.ProcessGeneration
{
    public class AcmeGenProcessed : AcmeGenOriginal
    {
        public string Source { get; set; }
        public string WeatherYear { get; set; }
        public string ProjectId { get; set; }
        public string LocalDt { get; set; }
    }
}