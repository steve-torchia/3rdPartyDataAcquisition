using System;
using NLog.Config;

namespace DP.Base.Contracts.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(object owner);
        ILogger GetLogger(string name);
        ILogger GetLogger(Type ownerType);
        void SetConfiguration();

        LoggingConfiguration GetConfiguration();
    }
}
