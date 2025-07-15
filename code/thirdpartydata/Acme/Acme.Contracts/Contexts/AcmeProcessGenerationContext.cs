using Ingress.Lib.Base;

namespace Acme.Contracts
{
    public class AcmeProcessGenerationContext
    {
        public BlobConfigInfo BlobConfigInfo { get; set; }

        public string ProcessRequestJson { get; set; }

        public string ZipFile { get; set; }

        public AcmeProcessGenerationContext()
        {
        }
    }
}