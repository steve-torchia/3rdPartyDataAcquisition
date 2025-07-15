using DP.Base.Contracts;
using DP.Base.Contracts.ServiceLocator;

namespace DP.Base.Context
{
    public partial class ObjectContext : ContextBase, IObjectContext
    {
        public ObjectContext()
            : base()
        {
        }

        protected override void DisposeCore(bool isDisposing)
        {
            base.DisposeCore(isDisposing);
            if (this.serviceLocator != null)
            {
                this.serviceLocator.Dispose();
            }
        }

        private bool isInitializeAfterCreate;
        public override void InitializeAfterCreate()
        {
            base.InitializeAfterCreate();
            if (this.isInitializeAfterCreate == true)
            {
                return;
            }

            this.isInitializeAfterCreate = true;

            IReflectiveMonitor tmpMonitor;
            if (this.ServiceLocator.TryGetInstance<IReflectiveMonitor>(null, out tmpMonitor))
            {
                this.ReflectiveMonitor = tmpMonitor;
                if (this.Container == null)
                {
                    this.ReflectiveMonitor.Target = this;
                }
                else
                {
                    this.ReflectiveMonitor.Target = this.Container;
                }

                this.ReflectiveMonitor.InitializeAfterCreate();
            }
        }

        private IServiceLocatorProvider localServiceLocatorProvider;
        private IServiceLocator serviceLocator;
        public override IServiceLocatorProvider ServiceLocatorProvider
        {
            get
            {
                if (this.localServiceLocatorProvider != null)
                {
                    return this.localServiceLocatorProvider;
                }

                return this.Parent.ServiceLocatorProvider;
            }
            set
            {
                this.localServiceLocatorProvider = value;
            }
        }

        public override IServiceLocator ServiceLocator
        {
            get
            {
                if (this.serviceLocator != null)
                {
                    return this.serviceLocator;
                }

                if (this.Parent != null)
                {
                    this.serviceLocator = new ContextServiceLocatorWrapper(this, this.ServiceLocatorProvider.GetChildServiceLocator(this.UserGroupInfo, this.Parent.ServiceLocator));
                }
                else
                {
                    this.serviceLocator = new ContextServiceLocatorWrapper(this, this.ServiceLocatorProvider.GetServiceLocator(this.UserGroupInfo));
                }

                return this.serviceLocator;
            }
            set
            {
                if (value is ContextServiceLocatorWrapper)
                {
                    this.serviceLocator = value;
                }
                else
                {
                    this.serviceLocator = new ContextServiceLocatorWrapper(this, value);
                }
            }
        }

        public IReflectiveMonitor ReflectiveMonitor { get; protected set; }
    }
}
