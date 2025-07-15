//#define DEBUG_KEY_VAULT

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using DP.Base.Collections;
//using DP.Base.Contracts;
//using DP.Base.Contracts.ComponentModel;
//using DP.Base.Contracts.Logging;
//using DP.Base.Contracts.Security;
//using Microsoft.Azure.KeyVault;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;

//namespace DP.Base.Utilities
//{
//    public class ApplicationEx : DP.Base.Utilities.IApplicationEx
//    {
//        private readonly bool checkKeyVault = true;

//        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool SetDllDirectory(string lpPathName);

//        public class ConfigurationNames
//        {
//            public const string GlobalContext = "GlobalContext";
//            public const string LoggerFactory = "GContext_LoggerFactory";

//            public const string DefaultServiceLocatorProvider = "DefaultServiceLocatorProvider";
//            public const string DefaultComponentRegistrationInfoProvider = "DefaultComponentRegistrationInfoProvider";
//        }

//        private Collections.DataExpirationCache<string, string, string> configValueCache;
//        private Lazy<ILogger> logLazy = null;

//        private static volatile IApplicationEx instance;
//        public static IApplicationEx Instance
//        {
//            get
//            {
//                if (instance == null)
//                {
//                    lock (typeof(ApplicationEx))
//                    {
//                        if (instance == null)
//                        {
//                            instance = new ApplicationEx();
//                        }
//                    }
//                }

//                return instance;
//            }
//        }

//        protected ApplicationEx()
//        {
//#if DEBUG
//            this.checkKeyVault = false;
//#endif

//#if DEBUG_KEY_VAULT
//            this.checkKeyVault = true;
//#endif

//            AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
//            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
//            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
//            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);

//            this.configLazy = new Lazy<IConfigurationRoot>(
//                () =>
//                {
//                    string environment = Environment.GetEnvironmentVariable("Environment");
//                    string machineName = Environment.MachineName;

//                    return new ConfigurationBuilder()
//                            //.SetBasePath(System.IO.Directory.GetCurrentDirectory())
//                            .AddJsonFile("appsettings.json")
//                            .Build();                    
//                });

//            this.logLazy = new Lazy<ILogger>(() => this.GlobalContext.LoggerFactory.GetLogger(typeof(ApplicationEx)));

//            //load the config
//            var conf = this.Config;

//            this.configValueCache = new Collections.DataExpirationCache<string, string, string>(this.GetConfigurationValueCore, null, null, 0);

//            this.ContextFinder = new DP.Base.Context.CurrentContextFinder();

//            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\RegisteredDataProviders\");
//            bool res = SetDllDirectory(path);
//            if (!res)
//            {
//                this.Log.Error("Failed to set additional dll search path!");
//            }
//        }
        
//        public async Task<string> LookupSecretAsync(IConfigurationSection section, string sectionFieldName)
//        {                  
//            string secret = section[sectionFieldName];

//            if (this.checkKeyVault && secret.Contains("vault.azure.net"))
//            {
//                try
//                {
//                    using (var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(this.GetKeyVaultAccessToken)))
//                    {
//                        var result = await keyVaultClient.GetSecretAsync(secret);
//                        secret = result.Value;
//                    }
//                }
//                catch (Exception exp)
//                {
//                    this.Log.Error("Failed to retrieve secret from vault", exp);
//                }
//            }

//            return secret;
//        }

//        public async Task<string> LookupSecretAsync(string secretUri)
//        {
//            string secret = secretUri;

//            if (this.checkKeyVault && secret.Contains("vault.azure.net"))
//            {
//                try
//                {
//                    using (var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(this.GetKeyVaultAccessToken)))
//                    {
//                        var result = await keyVaultClient.GetSecretAsync(secret);
//                        secret = result.Value;
//                    }
//                }
//                catch (Exception exp)
//                {
//                    this.Log.Error("Failed to retrieve secret from vault", exp);
//                }
//            }

//            return secret;
//        }        

//        private async Task<string> GetKeyVaultAccessToken(string authority, string resource, string scope)
//        {
//            try
//            {
//                var sectionVault = ApplicationEx.Instance.Config.GetSection("AzureKeyVault");
//                var clientId = sectionVault["ClientId"];
//                var clientSecret = sectionVault["ClientSecret"];

//                var clientCredential = new ClientCredential(clientId, clientSecret);
//                var authenticationContex = new AuthenticationContext(authority);
//                var result = await authenticationContex.AcquireTokenAsync(resource, clientCredential);

//                if (result == null)
//                {
//                    throw new InvalidOperationException("Failed to obtain the JWT token");
//                }

//                return result.AccessToken;
//            }
//            catch (Exception exp)
//            {
//                this.Log.Error("GetKeyVaultAccessToken:Failed to get access token", exp);
//                throw exp;
//            }
//        }

//        private volatile string applicationName;
//        private volatile string applicationSubName;
//        private volatile string applicationFullName;
//        private volatile string applicationInstance;
//        private volatile string applicationFullNameWithHost;
//        private string[] hostNameAr = null;

//        public string ApplicationName
//        {
//            get
//            {
//                if (this.applicationName == null)
//                {
//                    lock (typeof(EnvironmentEx))
//                    {
//                        this.BuildApplicationName();
//                    }
//                }

//                return this.applicationName;
//            }
//        }

//        private Lazy<IConfigurationRoot> configLazy;
//        public IConfigurationRoot Config
//        {
//            get
//            {
//                return this.configLazy.Value;
//            }
//        }

//        private string GetApplicationSetting(string name)
//        {
//            return this.Config.GetSection("Application")[name];
//        }

//        public List<KeyValuePair<string, string>> GetConfigurationValues(string sectionName)
//        {
//            var section = this.Config.GetSection(sectionName);
//            return section.GetChildren().Select(a => new KeyValuePair<string, string>(a.Key, a.Value)).ToList();
//        }

//        private string BuildApplicationName()
//        {
//            if (this.applicationName == null)
//            {
//                string tmpLogFileName = null;
//                try
//                {
//                    tmpLogFileName = this.GetApplicationSetting("ApplicationName");
//                }
//                catch
//                {
//                }

//                if (string.IsNullOrEmpty(tmpLogFileName))
//                {
//                    tmpLogFileName = System.IO.Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
//                }

//                this.applicationName = tmpLogFileName;
//            }

//            return this.applicationName;
//        }

//        public string ApplicationSubName
//        {
//            get
//            {
//                if (this.applicationSubName == null)
//                {
//                    lock (typeof(EnvironmentEx))
//                    {
//                        this.GetApplicationSubName();
//                    }
//                }

//                return this.applicationSubName;
//            }
//        }

//        private string GetApplicationSubName()
//        {
//            if (this.applicationSubName == null)
//            {
//                string applicationSubNameLocal = null;
//                try
//                {
//                    applicationSubNameLocal = this.GetApplicationSetting("ApplicationSubName");
//                }
//                catch
//                {
//                }

//                if (applicationSubNameLocal == null)
//                {
//                    applicationSubNameLocal = string.Empty;
//                }

//                this.applicationSubName = applicationSubNameLocal;
//            }

//            return this.applicationSubName;
//        }

//        public void SetApplicationName(string applicationName)
//        {
//            lock (typeof(EnvironmentEx))
//            {
//                this.applicationName = applicationName;
//                this.applicationInstance = null;
//                this.applicationFullName = null;
//                this.applicationFullNameWithHost = null;
//            }
//        }

//        public void SetApplicationSubName(string applicationSubName)
//        {
//            lock (typeof(EnvironmentEx))
//            {
//                this.applicationSubName = applicationSubName;
//                this.applicationInstance = null;
//                this.applicationFullName = null;
//                this.applicationFullNameWithHost = null;
//            }
//        }

//        public IContextFinder ContextFinder { get; set; }

//        public IUserGroupProvider UserGroupProvider { get; set; }

//        public string ApplicationFullName
//        {
//            get
//            {
//                var localAppFullName = this.applicationFullName;
//                if (localAppFullName == null)
//                {
//                    lock (typeof(EnvironmentEx))
//                    {
//                        if (this.applicationFullName == null)
//                        {
//                            var subName = this.GetApplicationSubName();
//                            if (string.IsNullOrWhiteSpace(subName) == false)
//                            {
//                                this.applicationFullName = localAppFullName = string.Join("_", this.BuildApplicationName(), subName);
//                            }
//                            else
//                            {
//                                this.applicationFullName = localAppFullName = this.BuildApplicationName();
//                            }
//                        }
//                    }
//                }

//                return localAppFullName;
//            }
//        }

//        private object applicationInstanceSync = new object();
//        public string ApplicationInstance
//        {
//            get
//            {
//                if (this.applicationInstance == null)
//                {
//                    lock (this.applicationInstanceSync)
//                    {
//                        if (this.applicationInstance == null)
//                        {
//                            this.applicationInstance = string.Format("{0}.{1}.{2}", this.ApplicationFullName, this.DefaultHostName, System.Diagnostics.Process.GetCurrentProcess().Id);
//                        }
//                    }
//                }

//                return this.applicationInstance;
//            }
//        }

//        public string ApplicationFullNameWithHost
//        {
//            get
//            {
//                if (this.applicationFullNameWithHost == null)
//                {
//                    lock (this.applicationInstanceSync)
//                    {
//                        if (this.applicationFullNameWithHost == null)
//                        {
//                            this.applicationFullNameWithHost = string.Format("{0}.{1}", this.ApplicationFullName, this.DefaultHostName);
//                        }
//                    }
//                }

//                return this.applicationFullNameWithHost;
//            }
//        }

//        public string DefaultHostName
//        {
//            get
//            {
//                return this.HostNames[0];
//            }
//        }

//        public string[] HostNames
//        {
//            get
//            {
//                if (this.hostNameAr != null)
//                {
//                    return this.hostNameAr;
//                }

//                lock (this)
//                {
//                    if (this.hostNameAr != null)
//                    {
//                        return this.hostNameAr;
//                    }

//                    List<string> hostnameList = new List<string>();
//                    string tmpHostNamelist = null;
//                    try
//                    {
//                        tmpHostNamelist = this.GetConfigurationValue("hostnames", string.Empty);
//                    }
//                    catch
//                    {
//                    }

//                    string upperHostName = null;
//                    if (string.IsNullOrEmpty(tmpHostNamelist) == false)
//                    {
//                        var tmpHostNameAr = tmpHostNamelist.Split(',');
//                        foreach (var tmpHostName in tmpHostNameAr)
//                        {
//                            if (string.IsNullOrWhiteSpace(tmpHostName))
//                            {
//                                continue;
//                            }

//                            upperHostName = tmpHostName.ToUpperInvariant();
//                            if (hostnameList.Contains(upperHostName) == false)
//                            {
//                                hostnameList.Add(upperHostName);
//                            }
//                        }
//                    }

//                    upperHostName = System.Net.Dns.GetHostName().ToUpperInvariant();
//                    if (!string.IsNullOrWhiteSpace(upperHostName) && hostnameList.Contains(upperHostName) == false)
//                    {
//                        hostnameList.Add(upperHostName);
//                    }

//                    upperHostName = Environment.MachineName.ToUpperInvariant();
//                    if (!string.IsNullOrWhiteSpace(upperHostName) && hostnameList.Contains(upperHostName) == false)
//                    {
//                        hostnameList.Add(upperHostName);
//                    }

//                    if (hostnameList.Contains("LOCALHOST") == false)
//                    {
//                        hostnameList.Add("LOCALHOST");
//                    }

//                    if (hostnameList.Contains("127.0.0.1") == false)
//                    {
//                        hostnameList.Add("127.0.0.1");
//                    }

//                    System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
//                    // IPAddress class contains the address of a computer on an IP network. 
//                    foreach (var alias in hostEntry.Aliases)
//                    {
//                        upperHostName = alias.ToUpperInvariant();
//                        if (!string.IsNullOrWhiteSpace(upperHostName) && hostnameList.Contains(upperHostName) == false)
//                        {
//                            hostnameList.Add(upperHostName);
//                        }
//                    }

//                    foreach (var ipAddress in hostEntry.AddressList)
//                    {
//                        var ipAddressString = ipAddress.ToString();
//                        if (hostnameList.Contains(ipAddressString) == false)
//                        {
//                            hostnameList.Add(ipAddressString);
//                        }
//                    }

//                    this.hostNameAr = hostnameList.ToArray();
//                }

//                return this.hostNameAr;
//            }
//        }

//        /// <summary>
//        /// checks in order
//        /// 1) App.Config key = name
//        /// 2) EnvironmentVar TS_name
//        /// 3) uses default
//        /// </summary>
//        /// <returns></returns>
//        public string GetConfigurationValue(string name, string defaultValue)
//        {
//            return this.configValueCache.GetValue(name, defaultValue);
//        }

//        public void OverrideConfigurationValue(string name, string value)
//        {
//            this.configValueCache.SetValue(name, value);
//        }

//        public void ClearConfigurationValue(string name, string value)
//        {
//            this.configValueCache.RemoveValue(name);
//        }

//        private string environmentValuePrefix = null;
//        public string EnvironmentValuePrefix
//        {
//            get
//            {
//                if (string.IsNullOrWhiteSpace(this.environmentValuePrefix))
//                {
//                    var tmp = this.GetConfigurationValueCore("EnvironmentValuePrefix", null, false);
//                    if (string.IsNullOrWhiteSpace(tmp))
//                    {
//                        tmp = this.GetType().FullName.Split('.')[0];
//                    }

//                    this.EnvironmentValuePrefix = string.Concat(tmp, "_");
//                }

//                return this.environmentValuePrefix;
//            }
//            set
//            {
//                this.environmentValuePrefix = value;
//                if (this.environmentValuePrefix != null)
//                {
//                    this.environmentValuePrefix = this.environmentValuePrefix.ToUpperInvariant();
//                }
//            }
//        }

//        protected string GetConfigurationValueCore(string name, string defaultValue)
//        {
//            return this.GetConfigurationValueCore(name, defaultValue, true);
//        }

//        protected string GetConfigurationValueCore(string name, string defaultValue, bool checkEnvironment)
//        {
//            string retVal = null;
//            try
//            {
//                retVal = this.GetApplicationSetting(name);
//                if (string.IsNullOrWhiteSpace(retVal) == false)
//                {
//                    return retVal;
//                }
//            }
//            catch
//            {
//            }

//            try
//            {
//                if (checkEnvironment == true)
//                {
//                    retVal = Environment.GetEnvironmentVariable(this.EnvironmentValuePrefix + name, System.EnvironmentVariableTarget.Machine);
//                    if (string.IsNullOrWhiteSpace(retVal) == false)
//                    {
//                        return retVal;
//                    }
//                }
//            }
//            catch
//            {
//            }

//            return defaultValue;
//        }

//        public void SetEnvironmentVariable(string name, string value, EnvironmentVariableTarget environmentVariableTarget)
//        {
//            #region validation
//            if (string.IsNullOrWhiteSpace(name))
//            {
//                throw new ArgumentNullException("name");
//            }

//            if (name.ToUpperInvariant().StartsWith(this.EnvironmentValuePrefix))
//            {
//                throw new ArgumentException("Do not use the" + this.EnvironmentValuePrefix + " prefix for the name parameter", name);
//            }
//            #endregion

//            var prefixedName = this.EnvironmentValuePrefix + name;
//            Environment.SetEnvironmentVariable(prefixedName, value, environmentVariableTarget);
//        }

//        public const string DefaultLogFileTimestampFormat = "yyyyMMddHHmmss";
//        private string logFileTimestampFormat = null;
//        public string LogFileTimestampFormat
//        {
//            get
//            {
//                if (this.logFileTimestampFormat == null)
//                {
//                    string tmpLogFileTimeStampFormat = null;
//                    try
//                    {
//                        tmpLogFileTimeStampFormat = this.GetApplicationSetting("LogFileTimestampFormat");
//                    }
//                    catch
//                    {
//                    }

//                    if (string.IsNullOrWhiteSpace(tmpLogFileTimeStampFormat) == false)
//                    {
//                        this.logFileTimestampFormat = tmpLogFileTimeStampFormat;
//                    }
//                    else
//                    {
//                        this.logFileTimestampFormat = DefaultLogFileTimestampFormat;
//                    }
//                }

//                return this.logFileTimestampFormat;
//            }
//            set
//            {
//                this.logFileTimestampFormat = value;
//            }
//        }

//        private string applicationLogDirectory;
//        public string ApplicationLogDirectory
//        {
//            get
//            {
//                if (this.applicationLogDirectory == null)
//                {
//                    var tmp = this.GetConfigurationValueCore("ApplicationLogDirectory", null);

//                    if (string.IsNullOrWhiteSpace(tmp))
//                    {
//                        tmp = System.IO.Path.GetTempPath();
//                    }

//                    this.ApplicationLogDirectory = tmp;
//                }

//                return this.applicationLogDirectory;
//            }
//            set
//            {
//                this.applicationLogDirectory = value;
//            }
//        }

//        private string applicationLogFileName;
//        public string ApplicationLogFileName
//        {
//            get
//            {
//                if (this.applicationLogDirectory == null)
//                {
//                    var tmp = this.GetConfigurationValueCore("ApplicationLogFileName", null);

//                    if (string.IsNullOrWhiteSpace(tmp) == true)
//                    {
//                        tmp = this.ApplicationFullName;
//                    }

//                    this.ApplicationLogFileName = tmp;
//                }

//                return this.applicationLogFileName;
//            }
//            set
//            {
//                this.applicationLogFileName = value;
//            }
//        }

//        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ueea)
//        {
//            Exception ex = ueea.ExceptionObject as Exception;
//            try
//            {
//                if (this.Log.IsErrorEnabled)
//                {
//                    if (ex != null)
//                    {
//                        this.Log.Error("CurrentDomain_UnhandledException", ex);
//                    }
//                    else
//                    {
//                        this.Log.Error("CurrentDomain_UnhandledException", ueea.ExceptionObject);
//                    }
//                }
//            }
//            catch (Exception logEx)
//            {
//                try
//                {
//                    var directory = this.ApplicationLogDirectory;
//                    using (var sw = new System.IO.StreamWriter(Path.Combine(directory, string.Format("AppError{0}.log", DateTime.UtcNow.Ticks))))
//                    {
//                        sw.WriteLine(logEx.ToString());
//                        sw.WriteLine(ueea.ExceptionObject);
//                    }
//                }
//                catch (Exception innerLog)
//                {
//                    System.Diagnostics.Trace.WriteLine(innerLog);
//                }
//            }
//        }

//        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
//        {
//            AssemblyResolveEventArgs area = new AssemblyResolveEventArgs() { ResolveEventArgs = args };
//            this.assemblyResolverEventList.FireEvents(new object[] { sender, area });

//            var upperName = args.Name.ToUpperInvariant();
//            if (area.Assembly == null &&
//                !upperName.Contains("MICROSOFT.SERVICEFABRIC.DATA.IMPL") &&
//                !upperName.Contains("SYSTEM.FABRIC.BACKUPRESTORE") &&
//                !upperName.Contains(".RESOURCES,") &&
//                !upperName.Contains(".XMLSERIALIZERS") &&
//                !upperName.Contains("APP_WEB"))
//            {
//                try
//                {
//                    AppDomain ad = sender as AppDomain;
//                    throw new Exception(string.Format("cannot load: {0} from requesting Assembly {1}, application domain: {2}", args.Name, args.RequestingAssembly, ad == null ? null : ad.FriendlyName));
//                }
//                catch (Exception ex)
//                {
//                    this.Log.Error("CurrentDomain_AssemblyResolve", ex);
//                }
//            }

//            return area.Assembly;
//        }

//        public IGlobalContext GlobalContext
//        {
//            get;
//            private set;
//        }

//        public void SetGlobalContext(IGlobalContext globalContext, bool forceReset = false)
//        {
//            lock (this)
//            {
//                if (forceReset == false && this.GlobalContext != null)
//                {
//                    throw new InvalidOperationException("cannot reset global context without forceReset = true");
//                }

//                this.GlobalContext = globalContext;
//            }
//        }

//        public static T CreateClassFromConfig<T>(string configName)
//        {
//            return CreateClassFromConfig<T>(configName, false);
//        }

//        public static T CreateClassFromConfig<T>(string configName, bool returnNullIfMissing)
//        {
//            var name = ApplicationEx.Instance.GetConfigurationValue(configName, string.Empty);
//            if (returnNullIfMissing == true && string.IsNullOrWhiteSpace(name))
//            {
//                return default(T);
//            }

//            var type = Type.GetType(name, true, true);
//            var retVal = type.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, new Type[0], null).Invoke(null);

//            if (retVal is IInitializeAfterCreate)
//            {
//                ((IInitializeAfterCreate)retVal).InitializeAfterCreate();
//            }

//            return (T)retVal;
//        }

//        private WeakReferenceEventList assemblyResolverEventList = new Base.Collections.WeakReferenceEventList();

//        private ILogger Log
//        {
//            get
//            {
//                return this.logLazy.Value;
//            }
//        }

//        public event AssemblyResolveEventHandler AssemblyResolve
//        {
//            add
//            {
//                lock (this)
//                {
//                    this.assemblyResolverEventList.Add(value);
//                }
//            }
//            remove
//            {
//                lock (this)
//                {
//                    this.assemblyResolverEventList.Remove(value);
//                }
//            }
//        }

//        public IDataAffinityProvider DataAffinityProvider { get; set; }
//    }
//}
