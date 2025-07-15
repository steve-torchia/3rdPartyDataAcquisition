using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DP.Base.Contracts;
using Microsoft.Extensions.Configuration;

namespace DP.Base.Utilities
{
    public interface IApplicationEx
    {
        string ApplicationFullName { get; }
        string ApplicationFullNameWithHost { get; }
        string ApplicationInstance { get; }
        string ApplicationLogDirectory { get; set; }
        string ApplicationLogFileName { get; set; }
        string ApplicationName { get; }
        string ApplicationSubName { get; }
        void ClearConfigurationValue(string name, string value);
        string DefaultHostName { get; }
        string EnvironmentValuePrefix { get; set; }
        string GetConfigurationValue(string name, string defaultValue);

        System.Collections.Generic.List<KeyValuePair<string, string>> GetConfigurationValues(string sectionName);

        string[] HostNames { get; }
        string LogFileTimestampFormat { get; set; }
        void OverrideConfigurationValue(string name, string value);
        void SetApplicationName(string applicationName);
        void SetApplicationSubName(string applicationSubName);
        void SetEnvironmentVariable(string name, string value, EnvironmentVariableTarget environmentVariableTarget);

        Task<string> LookupSecretAsync(IConfigurationSection section, string sectionFieldName);

        Task<string> LookupSecretAsync(string secretUri);

        event AssemblyResolveEventHandler AssemblyResolve;

        //IServiceLocatorScopeProvider GetGlobalContextServiceLocatorScopeProvider();

        //ILoggerFactory GetGlobalContextLoggerFactory();

        IGlobalContext GlobalContext { get; }

        void SetGlobalContext(IGlobalContext globalContext, bool forceReset = false);

        //void CreateGlobalContext(IContextContainer container);

        IContextFinder ContextFinder { get; }

        IConfigurationRoot Config { get; }

        IDataAffinityProvider DataAffinityProvider { get; set; }
    }
}
