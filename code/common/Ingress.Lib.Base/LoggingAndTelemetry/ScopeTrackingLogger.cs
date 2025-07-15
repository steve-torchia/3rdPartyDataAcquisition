using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ingress.Lib.Base
{
    /// <summary>
    /// Custom logger that tracks scope data and includes it in log messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScopeTrackingLogger<T> : ILogger<T>
    {
        private readonly ILogger<T> _innerLogger;
        private readonly IGlobalScopeProvider _globalScopeProvider;
        private readonly AsyncLocal<List<object>> _localScopeData = new();
        
        public ScopeTrackingLogger(ILogger<T> innerLogger, IGlobalScopeProvider globalScopeProvider)
        {
            _innerLogger = innerLogger;
            _globalScopeProvider = globalScopeProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Create inner logger scope
            var innerScope = _innerLogger.BeginScope(state);
            
            // Store locally for GetCurrentScopeData method backward compatibility
            _localScopeData.Value ??= new List<object>();
            _localScopeData.Value.Add(state);
            
            // Also add to global scope provider
            var globalScope = _globalScopeProvider.BeginScope(state);
            
            return new MultiDisposable(innerScope, globalScope, this, state);
        }

        public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // For enhanced logging, you have two options:
            
            // Option 1: Include scope data in the log entry (works but may duplicate if inner logger already does this)
            var scopeData = _globalScopeProvider.GetCurrentScopeData();
            string scopeInfo = FormatScopeInfo(scopeData);
            
            // Create a wrapper for the state and formatter that includes scope data
            var wrappedState = new ScopeAwareState<TState>(state, scopeInfo);
            
            // Custom formatter that includes scope data
            string ScopeAwareFormatter(ScopeAwareState<TState> s, Exception e)
            {
                var originalMessage = formatter(s.OriginalState, e);
                if (!string.IsNullOrEmpty(s.ScopeInfo))
                {
                    return $"{originalMessage} {s.ScopeInfo}";
                }
                return originalMessage;
            }
            
            // Log with our custom wrapper
            _innerLogger.Log(logLevel, eventId, wrappedState, exception, ScopeAwareFormatter);
            
            // Option 2: Just pass through to inner logger (if your provider already handles scopes)
            // _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        // Format scope info into a readable string
        private string FormatScopeInfo(IReadOnlyCollection<object> scopes)
        {
            if (scopes == null || !scopes.Any())
                return string.Empty;
                
            var builder = new StringBuilder();
            builder.Append("Scope data: [");
            
            bool first = true;
            foreach (var scope in scopes)
            {
                if (!first)
                    builder.Append(", ");
                    
                first = false;
                
                // Handle dictionary scopes (most common)
                if (scope is IDictionary<string, object> dictScope)
                {
                    builder.Append('{');
                    bool firstKey = true;
                    
                    foreach (var kvp in dictScope)
                    {
                        if (!firstKey)
                            builder.Append(", ");
                            
                        firstKey = false;
                        builder.Append($"\"{kvp.Key}\":");
                        
                        if (kvp.Value is string)
                            builder.Append($"\"{kvp.Value}\"");
                        else
                            builder.Append(kvp.Value);
                    }
                    
                    builder.Append('}');
                }
                else
                {
                    // For non-dictionary scopes, just use ToString()
                    builder.Append(scope);
                }
            }
            
            builder.Append(']');
            return builder.ToString();
        }

        // For backward compatibility
        public IReadOnlyCollection<object> GetCurrentScopeData()
        {
            return _globalScopeProvider.GetCurrentScopeData();
        }

        // For including scope information in log messages
        private class ScopeAwareState<TOriginalState>
        {
            public TOriginalState OriginalState { get; }
            public string ScopeInfo { get; }

            public ScopeAwareState(TOriginalState originalState, string scopeInfo)
            {
                OriginalState = originalState;
                ScopeInfo = scopeInfo;
            }
        }

        private class MultiDisposable : IDisposable
        {
            private readonly IDisposable _innerScope;
            private readonly IDisposable _globalScope;
            private readonly ScopeTrackingLogger<T> _logger;
            private readonly object _state;

            public MultiDisposable(IDisposable innerScope, IDisposable globalScope, ScopeTrackingLogger<T> logger, object state)
            {
                _innerScope = innerScope;
                _globalScope = globalScope;
                _logger = logger;
                _state = state;
            }

            public void Dispose()
            {
                // Clean up local scope tracking
                if (_logger._localScopeData.Value != null)
                {
                    _logger._localScopeData.Value.Remove(_state);
                }
                
                // Dispose both scopes
                _innerScope.Dispose();
                _globalScope.Dispose();
            }
        }
    }
}
