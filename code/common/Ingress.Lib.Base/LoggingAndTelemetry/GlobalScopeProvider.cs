using System;
using System.Collections.Generic;
using System.Threading;

namespace Ingress.Lib.Base
{
    /// <summary>
    /// Provides a global scope for logging. Safe to use in async methods.
    /// </summary>
    public class GlobalScopeProvider : IGlobalScopeProvider
    {
        // AsyncLocal is used to store data that is unique to the current async control flow
        private static readonly AsyncLocal<List<object>> _globalScopeData = new();

        public IDisposable BeginScope(object state)
        {
            _globalScopeData.Value ??= new List<object>();
            _globalScopeData.Value.Add(state);
            
            return new ScopeDisposable(state);
        }

        public IReadOnlyCollection<object> GetCurrentScopeData() 
        {
            return _globalScopeData.Value ?? new List<object>();
        }

        private class ScopeDisposable : IDisposable
        {
            private readonly object _state;

            public ScopeDisposable(object state)
            {
                _state = state;
            }

            public void Dispose()
            {
                if (_globalScopeData.Value != null)
                {
                    _globalScopeData.Value.Remove(_state);
                }
            }
        }
    }
}
