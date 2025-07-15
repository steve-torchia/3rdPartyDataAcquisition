using System;
using System.Collections.Generic;
using System.Text;

namespace EEDurableFunctionsCommon
{
    [Serializable]
    public class OrchestrationResults
    {
        public List<string> Success { get; set; } = new List<string>();
        public List<string> Failure { get; set; } = new List<string>();

        public OrchestrationDetails Details { get; set; } = new OrchestrationDetails();

        public List<string> Notes { get; set; } = new List<string>();

        public class OrchestrationDetails
        {
            public List<string> Success { get; set; } = new List<string>();
            public List<OrchestrationFailureDetails> Failure { get; set; } = new List<OrchestrationFailureDetails>();
            public List<string> Notes { get; set; } = new List<string>();
        }

        public class OrchestrationFailureDetails
        {
            public string Id { get; set; }
            public string Error { get; set; }
        }

        public void AddFailure(string id, string diagnostics)
        {
            Failure.Add($"{id}");
            this.Details.Failure.Add(new OrchestrationFailureDetails { Id = id, Error = diagnostics });
        }

        public void AddSuccess(string id, string diagnostics)
        {
            Success.Add($"{id}");
            this.Details.Success.Add($"{id} => {diagnostics}");
        }
    }
}
