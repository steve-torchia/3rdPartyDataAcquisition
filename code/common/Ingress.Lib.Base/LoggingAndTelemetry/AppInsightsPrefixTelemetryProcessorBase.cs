using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Ingress.Lib.Base
{
    /// <summary>
    /// Make it easier to identify telemetry from this application in the Application Insights
    /// </summary>
    public abstract class AppInsightsPrefixTelemetryProcessorBase : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

        // All logging from this application will be prefixed with this string in Application Insights
        // Override this class to define the prefix for your application
        protected virtual string Prefix => "AppInsightsPrefixGoesHere";

        public AppInsightsPrefixTelemetryProcessorBase(ITelemetryProcessor next)
        {
            _next = next;
        }

        public void Process(ITelemetry item)
        {
            // Handle trace telemetry (typical logging)
            if (item is TraceTelemetry trace)
            {
                trace.Message = $"{Prefix}{trace.Message}";
            }
            // Handle exception telemetry (errors)
            else if (item is ExceptionTelemetry exception)
            {
                exception.Message = $"{Prefix}{exception.Message}";
            }
            // Handle custom events (can include warnings and info)
            else if (item is EventTelemetry evt)
            {
                evt.Name = $"{Prefix}{evt.Name}";
            }
            // Handle requests
            else if (item is RequestTelemetry request)
            {
                request.Name = $"{Prefix}{request.Name}";
            }

            _next.Process(item);
        }
    }
}
