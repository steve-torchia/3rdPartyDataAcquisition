using DP.Base.Contracts.Logging;

namespace DP.Base.Context
{
    public class ObjectContextAction : ObjectContext
    {
        public ObjectContextAction()
            : base()
        {
        }

        protected override IDiagnostic CreateDiagnostic()
        {
            return new Diagnostic();
        }
    }
}
