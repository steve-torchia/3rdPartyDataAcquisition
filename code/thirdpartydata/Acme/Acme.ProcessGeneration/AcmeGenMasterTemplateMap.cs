using CsvHelper.Configuration;

namespace Acme.ProcessGeneration
{
    public class AcmeGenMasterTemplateMap : ClassMap<AcmeGenMasterTemplate>
    {
        public AcmeGenMasterTemplateMap()
        {
            Map(m => m.Template);
            Map(m => m.TemplateDescrip);
            Map(m => m.Periodicity);
            Map(m => m.TimeZone);
            //Map(m => m.IsBinary);
            //Map(m => m.inValidationReport);
        }
    }
}