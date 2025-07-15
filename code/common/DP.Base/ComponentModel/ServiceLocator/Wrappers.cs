//using DP.Base.Contracts.Security;
//using DP.Base.Contracts.ServiceLocator;
//using DP.Base.Utilities;

//namespace DP.Base.ServiceLocator
//{
//    //public class ComponentRegistrarProviderWrapper : IComponentRegistrarProvider
//    //{
//    //    private IComponentRegistrarProvider innerComponentRegistrarProvider;

//    //    public ComponentRegistrarProviderWrapper()
//    //    {
//    //        this.innerComponentRegistrarProvider = ApplicationEx.CreateClassFromConfig<IComponentRegistrarProvider>(ApplicationEx.ConfigurationNames.DefaultServiceLocatorComponentRegistrarProvider);
//    //        this.innerComponentRegistrarProvider.GetComponentRegistrationInfo_Post += innerComponentRegistrarProvider_GetComponentRegistrationInfo_Post;
//    //        this.innerComponentRegistrarProvider.GetComponentRegistrationInfo_Pre += innerComponentRegistrarProvider_GetComponentRegistrationInfo_Pre;
//    //    }

//    //    public GetComponentRegistrarEventArgs GetComponentRegistrationInfo(DP.Base.Security.UserGroupInformation userGroupInfo)
//    //    {
//    //        var gcrea = innerComponentRegistrarProvider.GetComponentRegistrationInfo(userGroupInfo);
//    //        if (gcrea.ComponentRegistrar != null)
//    //            gcrea.ComponentRegistrar = new ComponentRegistrarWrapper(gcrea.ComponentRegistrar);

//    //        return gcrea;
//    //    }

//    //    public event DP.Base.ServiceLocator.GetComponentRegistrarEventHandler GetComponentRegistrationInfo_Pre;

//    //    public event DP.Base.ServiceLocator.GetComponentRegistrarEventHandler GetComponentRegistrationInfo_Post;

//    //    private void innerComponentRegistrarProvider_GetComponentRegistrationInfo_Pre(IComponentRegistrarProvider ComponentRegistrarProvider, DP.Base.ServiceLocator.GetComponentRegistrarEventArgs gcrea)
//    //    {
//    //        if (this.GetComponentRegistrationInfo_Pre != null)
//    //            this.GetComponentRegistrationInfo_Pre(this, gcrea);
//    //    }

//    //    private void innerComponentRegistrarProvider_GetComponentRegistrationInfo_Post(IComponentRegistrarProvider ComponentRegistrarProvider, DP.Base.ServiceLocator.GetComponentRegistrarEventArgs gcrea)
//    //    {
//    //        if (this.GetComponentRegistrationInfo_Post != null)
//    //            this.GetComponentRegistrationInfo_Post(this, gcrea);
//    //    }

//    //}

//    //public class ComponentRegistrarWrapper : IComponentRegistrar
//    //{
//    //    private IComponentRegistrar innerComponentRegistrar;

//    //    public ComponentRegistrarWrapper(IComponentRegistrar innerComponentRegistrar)
//    //    {
//    //        this.innerComponentRegistrar = innerComponentRegistrar;
//    //    }

//    //    public virtual void RegisterComponents(IServiceLocator serviceLocator, ComponentRegistrationInfoCollection registrationCollection)
//    //    {
//    //        if (this.innerComponentRegistrar == null)// no registrar, nothing to do
//    //            return;

//    //        this.innerComponentRegistrar.RegisterComponents(serviceLocator, registrationCollection);
//    //    }
//    //}

//    public class ServiceLocatorProviderWrapper : IServiceLocatorProvider
//    {
//        private IServiceLocatorProvider innerServiceLocatorProvider;

//        public ServiceLocatorProviderWrapper(IComponentRegistrationInfoProvider componentRegistrationInfoProvider)
//        {
//            this.innerServiceLocatorProvider = ApplicationEx.CreateClassFromConfig<IServiceLocatorProvider>(ApplicationEx.ConfigurationNames.DefaultServiceLocatorProvider);

//            this.innerServiceLocatorProvider.Initialize(componentRegistrationInfoProvider);

//            this.innerServiceLocatorProvider.GetServiceLocator_Post += this.InnerServiceLocatorProvider_GetServiceLocator_Post;
//            this.innerServiceLocatorProvider.GetServiceLocator_Pre += this.InnerServiceLocatorProvider_GetServiceLocator_Pre;

//            this.Initialize(componentRegistrationInfoProvider);
//        }

//        private void InnerServiceLocatorProvider_GetServiceLocator_Pre(IServiceLocatorProvider serviceLocatorProvider, GetServiceLocatorEventArgs gslea)
//        {
//            if (this.GetServiceLocator_Pre != null)
//            {
//                this.GetServiceLocator_Pre(this, gslea);
//            }
//        }

//        private void InnerServiceLocatorProvider_GetServiceLocator_Post(IServiceLocatorProvider serviceLocatorProvider, GetServiceLocatorEventArgs gslea)
//        {
//            if (this.GetServiceLocator_Post != null)
//            {
//                this.GetServiceLocator_Post(this, gslea);
//            }
//        }

//        public IServiceLocator GetServiceLocator(UserGroupInformation userGroupInfo)
//        {
//            return this.innerServiceLocatorProvider.GetServiceLocator(userGroupInfo);
//        }

//        public void Initialize(IComponentRegistrationInfoProvider componentRegistrationInfoProvider)
//        {
//            this.innerServiceLocatorProvider.Initialize(componentRegistrationInfoProvider);
//        }

//        public IComponentRegistrationInfoProvider ComponentRegistrationInfoProvider
//        {
//            get { return this.innerServiceLocatorProvider.ComponentRegistrationInfoProvider; }
//        }

//        public event GetServiceLocatorEventHandler GetServiceLocator_Pre;

//        public event GetServiceLocatorEventHandler GetServiceLocator_Post;

//        public IServiceLocator GetChildServiceLocator(UserGroupInformation userGroupInfo, IServiceLocator parentLocator)
//        {
//            return this.innerServiceLocatorProvider.GetChildServiceLocator(userGroupInfo, parentLocator);
//        }
//    }
//}
