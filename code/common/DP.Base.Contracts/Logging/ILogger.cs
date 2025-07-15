using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DP.Base.Contracts.Logging
{
    /// <summary>
    ///   Common contract for trace instrumentation. You 
    ///   can implement this contrat with several frameworks.
    ///   .NET Diagnostics API, EntLib, Log4Net,NLog etc.
    ///   <remarks>
    ///     The use of this abstraction depends on the real needs you have and the specific features  
    ///     you want to use of a particular existing implementation. 
    ///     You could also eliminate this abstraction and directly use "any" implementation in your code, 
    ///     Logger.Write(new LogEntry()) in EntLib, or LogManager.GetLog("logger-name") with log4net... etc.
    ///   </remarks>
    /// </summary>
    public interface ILogger
    {
        Dictionary<string, string> CustomData { get; }
        string CustomDataDelimiter { get; }

        void Debug(string message);
        void Debug(string message, params object[] args);
        void Debug(string message, Exception exception);

        void Error(string message);
        void Error(string message, params object[] args);
        void Error(string message, Exception exception);

        ILoggerFactory Factory { get; }
        void Fatal(string message);
        void Fatal(string message, params object[] args);
        void Fatal(string message, Exception exception);

        void Info(string message);
        void Info(string message, Exception exception);
        void Info(string message, params object[] args);

        bool IsDebugEnabled { get; }
        bool IsEnabled(LogLevel level);
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsVerboseEnabled { get; }
        bool IsWarnEnabled { get; }
        void Log(LogLevel level, string message);
        void Log(LogLevel level, string message, Exception exception);
        void Log(LogLevel level, string message, params object[] args);

        string Name { get; }
        void Verbose(string message);
        void Verbose(string message, Exception exception);
        void Verbose(string message, params object[] args);

        void Warn(string message);
        void Warn(string message, Exception exception);
        void Warn(string message, params object[] args);

        void LogStart(Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Debug, [CallerMemberName] string methodName = "");
        void LogStart(Guid? requestId, Guid? parentRequestId, object[] parameterList, LogLevel level = LogLevel.Debug, [CallerMemberName] string methodName = "");
        void LogEnd(Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Debug, [CallerMemberName] string methodName = "");
        void LogException(Exception ex, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogException(Exception ex, string description, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogCallResultFailure(ICallResult callResult, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogCallResultFailure(ICallResult callResult, string description, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogCallResultFailure<T>(ICallResult<T> callResult, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogCallResultFailure<T>(ICallResult<T> callResult, string description, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogArgumentNullError(Guid? requestId, Guid? parentRequestId, string argumentName, LogLevel level = LogLevel.Error, [CallerMemberName] string methodName = "");
        void LogMessage(string customMessage, Guid? requestId, Guid? parentRequestId, LogLevel level = LogLevel.Debug, [CallerMemberName] string methodName = "");
    }
}