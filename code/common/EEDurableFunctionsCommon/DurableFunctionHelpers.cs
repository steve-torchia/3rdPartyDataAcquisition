namespace DurableFunctionsCommon
{
    public class DurableFunctionHelpers
    {
        public enum DurableFunctionType
        {
            HttpTrigger,
            Orchestrator,
            SubOrchestrator,
            Activity
        }

        public class Orchestration
        {
            public class CustomStatus
            {
                public const string Success = "SUCCESS";
                public const string ErrorCondition = "ERROR";
                public const string Running = "RUNNING";
            }
        }
    }
}