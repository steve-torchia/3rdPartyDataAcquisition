using Microsoft.Extensions.Logging;
using System;

namespace Ingress.Lib.Base
{
    public static class AppInsightsLoggerExtensions
    {
        public static void LogErrorEx(this ILogger logger, string message, Exception ex = null)
        {
            var errMsg = $"!ERROR: {message}";

            // Log the message as both information and error. This is useful when searching through Application Insights since there are
            // two tables, trace and exceptions. Sending Errors to Trace let us see them inline with the other messages
            logger.LogInformation(errMsg);   // Goes to Application Insights as a Trace
            logger.LogError($"{ex}, {errMsg}"); // Goes to Application Insights as an Exception
        }
    }
}
