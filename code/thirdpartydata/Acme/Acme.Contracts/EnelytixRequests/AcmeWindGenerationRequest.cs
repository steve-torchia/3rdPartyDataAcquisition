using System.Collections.Generic;

namespace Acme.Contracts
{
    public class AcmeWindGenerationRequest : AcmeRequestBase
    {
        public List<TurbineType> TurbineTypes { get; set; }
    }
}
