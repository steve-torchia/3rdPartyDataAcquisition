using Microsoft.Extensions.Logging;
using System;

namespace Ingress.Lib.Base
{
    /// <summary>
    /// Custom logger factory which returns a ScopeTrackingLogger, which tracks scope data and includes it in log messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScopeTrackingLoggerFactory<T> : ILogger<T>
    {
        private readonly ScopeTrackingLogger<T> _logger;

        public ScopeTrackingLoggerFactory(ILoggerFactory loggerFactory, IGlobalScopeProvider scopeProvider)
        {
            var baseLogger = loggerFactory.CreateLogger<T>();
            _logger = new ScopeTrackingLogger<T>(baseLogger, scopeProvider);
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}