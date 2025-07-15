using CsvHelper.Configuration;

namespace Acme.ProcessGeneration
{
    public class AcmeGenProcessedMap : ClassMap<AcmeGenProcessed>
    {
        public AcmeGenProcessedMap()
        {
            Map(m => m.Template);
            Map(m => m.Dt);
            Map(m => m.Value);
        }
    }
}