using System;
using System.Threading.Tasks;
using static DurableFunctionsCommon.DurableFunctionHelpers;
using Microsoft.DurableTask.Client;

namespace DurableFunctionsCommon
{
    public abstract class OrchestrationClientTestingSupport
    {
        /// <summary>
        /// Need this to be able to mock the static/extension method DurableTaskClient.GetInstanceAsync().  
        /// Moq does not natively support extension methods so we get around it by wrapping the extension methods we want to override.
        /// </summary>
        public Func<DurableTaskClient, string, Task<OrchestrationMetadata>> OrchestrationClientGetStatusAsyncMethod
        {
            get { return _getStatusAsynchMethod ??= (obj, val) => { return obj.GetInstanceAsync(val); }; }
            set { _getStatusAsynchMethod = value; }
        }
        private Func<DurableTaskClient, string, Task<OrchestrationMetadata>> _getStatusAsynchMethod;
    }
}