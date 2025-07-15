using DP.Base.Extensions;
using Ingress.Lib.Base;
using System.Collections.Generic;

namespace Acme.Contracts
{
    public class AcmeAcquireGenerationContext
    {
        public AcquireGenerationInputModel Project { get; set; }

        public AcmeJobInfo JobInfo { get; set; } = new AcmeJobInfo();

        public BlobConfigInfo BlobDestinationConfigInfo { get; set; }

        public string Diagnostics { get; set; }

        public AcmeAcquireGenerationContext()
        {
        }
    }
}