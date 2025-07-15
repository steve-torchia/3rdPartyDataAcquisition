using Ingress.Lib.Base;
using Ingress.Lib.Base.Contracts;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsCommon
{
    public abstract class ActivityFunctionBase<T>
    {
        protected ILogger<T> Log { get; private set; }

        // Used for DI in Testing. During normal operation, this wrapper is created on the fly via passed-in context info
        protected IBlobContainerWrapper BlobContainerWrapper { get; }

        public ActivityFunctionBase(IBlobContainerWrapper blobContainerWrapper = null, ILogger<T> log = null)
        {
            this.Log = log;
            this.BlobContainerWrapper = blobContainerWrapper;
        }

        protected IBlobContainerWrapper GetBlobContainerWrapper(IBlobConfigInfo blobConfigInfo)
        {
            if (this.BlobContainerWrapper == null)
            {
                lock (this)
                {
                    if (this.BlobContainerWrapper == null)
                    {
                        return new BlobContainerWrapper(blobConfigInfo);
                    }
                }
            }

            return this.BlobContainerWrapper;
        }
    }
}