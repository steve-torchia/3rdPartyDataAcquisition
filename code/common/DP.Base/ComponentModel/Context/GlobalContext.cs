//using DP.Base.Contracts;
//using DP.Base.Contracts.Logging;
//using DP.Base.Contracts.ServiceLocator;
//using DP.Base.ServiceLocator;
//using DP.Base.Utilities;

//namespace DP.Base.Context
//{
//    public class GlobalContext : ContextBase, IGlobalContext, IObjectContext
//    {
//        protected GlobalContext()
//            : base()
//        {
//            this.LoggerFactory = this.GetLoggerFactory();
//        }

//        public static IGlobalContext CreateGlobalContext(IContextContainer container)
//        {
//            var newGlobalContext = ApplicationEx.CreateClassFromConfig<IGlobalContext>(ApplicationEx.ConfigurationNames.GlobalContext);

//            var slProvider = new ServiceLocatorProviderWrapper(ApplicationEx.CreateClassFromConfig<IComponentRegistrationInfoProvider>(ApplicationEx.ConfigurationNames.DefaultComponentRegistrationInfoProvider));

//            newGlobalContext.ServiceLocatorProvider = slProvider;

//            newGlobalContext.Initialize(null, container, null);

//            return newGlobalContext;
//        }

//        public override IServiceLocatorProvider ServiceLocatorProvider { get; set; }

//        private IServiceLocator serviceLocator;
//        public override IServiceLocator ServiceLocator
//        {
//            get
//            {
//                if (this.serviceLocator != null)
//                {
//                    return this.serviceLocator;
//                }

//                this.serviceLocator = new DP.Base.Context.ObjectContext.ContextServiceLocatorWrapper(this, this.ServiceLocatorProvider.GetServiceLocator(null));

//                return this.serviceLocator;
//            }
//            set
//            {
//                this.serviceLocator = value;
//            }
//        }

//        protected virtual ILoggerFactory GetLoggerFactory()
//        {
//            return ApplicationEx.CreateClassFromConfig<ILoggerFactory>(ApplicationEx.ConfigurationNames.LoggerFactory);
//        }

//        public IReflectiveMonitor ReflectiveMonitor
//        {
//            get { return null; }
//        }
//    }
//}
