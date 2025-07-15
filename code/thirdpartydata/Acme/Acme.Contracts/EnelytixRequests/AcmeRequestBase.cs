using System.Collections.Generic;

namespace Acme.Contracts
{
    public class AcmeRequestBase
    {
        public List<string> WeatherYears { get; set; } = new List<string>();
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string NameplateCapacity { get; set; }
    }
}
