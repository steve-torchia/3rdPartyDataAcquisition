//using System;
//using System.IO;
//using System.Reflection;
//using DP.Base.Contracts;
//using DP.Base.Contracts.ServiceLocator;
//using DP.Base.ServiceLocator;
//using DP.Base.Utilities;

//namespace DP.Base.TestFramework
//{
//    public class BaseFixture
//    {
//        // This is the xUnit way to do [AssemblyInitialize], which is just preserved context between tests

//        public IGlobalContext Context { get; set; }

//        public BaseFixture()
//        {
//            ApplicationEx.Instance.SetGlobalContext(new TestGlobalContext(), true); //assign early eventhough not fully initialized so have access to logger
//            ApplicationEx.Instance.GlobalContext.Initialize(null, new TestContextContainer(), null);

//            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
//            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
//            var dirPath = Path.GetDirectoryName(codeBasePath);
//            var tryAddRet = ApplicationEx.Instance.GlobalContext.PropertyBag.TryAdd("TestAssemblyDir", dirPath);

//            var slProvider = new ServiceLocatorProviderWrapper(ApplicationEx.CreateClassFromConfig<IComponentRegistrationInfoProvider>(ApplicationEx.ConfigurationNames.DefaultComponentRegistrationInfoProvider));
//            slProvider.GetServiceLocator_Post += SLProvider_GetServiceLocator_Post;
//            ApplicationEx.Instance.GlobalContext.ServiceLocatorProvider = slProvider;

//            //warmup
//            var t = ApplicationEx.Instance.GlobalContext.ServiceLocator.TryGetInstance<object>("someboguskeysdafsdfdsfsdsdfddsdsd", out object dummy);

//            // Access to logging
//            this.Context = ApplicationEx.Instance.GlobalContext;
//        }

//        public void Dispose()
//        {
//            // do some cleanup here.....
//            return;
//        }

//        private static void SLProvider_GetServiceLocator_Post(IServiceLocatorProvider serviceLocatorProvider, GetServiceLocatorEventArgs gslea)
//        {
//            gslea.ServiceLocator.GetInstance_Pre += ServiceLocator_GetInstance_Pre;
//            gslea.ServiceLocator.GetInstance_Post += ServiceLocator_GetInstance_Post;
//        }

//        private static void ServiceLocator_GetInstance_Post(IServiceLocator locator, GetInstanceEventArgs giea)
//        {
//        }

//        private static void ServiceLocator_GetInstance_Pre(IServiceLocator locator, GetInstanceEventArgs giea)
//        {
//        }
//    }
//}
