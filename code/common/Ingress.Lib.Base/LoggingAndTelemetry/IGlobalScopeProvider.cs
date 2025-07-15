using System;
using System.Collections.Generic;

namespace Ingress.Lib.Base
{
    public interface IGlobalScopeProvider
    {
        IDisposable BeginScope(object state);
        IReadOnlyCollection<object> GetCurrentScopeData();
    }
}
