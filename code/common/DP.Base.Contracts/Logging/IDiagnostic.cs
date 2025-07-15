using System;
using System.Collections.Generic;

namespace DP.Base.Contracts.Logging
{
    public interface IDiagnostic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">method name. </param>
        /// <param name="levelForLog">what level is the message for the logger. Diag will pass message to logger for default logging.</param>
        /// <param name="diagLevel">how important is the diag message (could be used for sorting) 0 is least important and default.</param>
        /// <param name="message">message to log.</param>
        /// <param name="exception">exception type.</param>
        void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, Exception exception);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">method name. </param>
        /// <param name="levelForLog">what level is the message for the logger. Diag will pass message to logger for default logging.</param>
        /// <param name="diagLevel">how important is the diag message (could be used for sorting) 0 is least important and default.</param>
        /// <param name="message">message to log.</param>
        /// <param name="args">arguments.</param>
        void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args);

        IEnumerable<Tuple<string, DiagnosticLevel, string>> GetDiagnostics(DiagnosticsSortOrder sortOrder);

        bool Contains(string name);

        void LogWithReplace(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args);
    }

    public interface IDiagnosticWrapper : IDiagnostic
    {
        void Initialize(IDiagnostic diagnostic, IContext context);
    }

    public enum DiagnosticsSortOrder
    {
        /// <summary>
        /// DiagnosticsSortOrder Chronological
        /// </summary>
        Chronological,

        /// <summary>
        /// DiagnosticsSortOrder ByLevel
        /// </summary>
        ByLevel,
    }

    public enum DiagnosticLevel
    {
        /// <summary>
        /// just of note,  lowest importance
        /// </summary>
        Note,

        /// <summary>
        /// Info, should not impact results, but something interesting
        /// </summary>
        Informative,

        /// <summary>
        /// A potential problem that may impact results
        /// </summary>
        Warning,

        /// <summary>
        /// A problem that will impact results
        /// </summary>
        Error,
    }
}
