//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using DP.Base.Context;
//using DP.Base.Contracts;
//using DP.Base.Contracts.ComponentModel;
//using DP.Base.Contracts.Logging;
//using DP.Base.Contracts.Security;
//using DP.Base.Contracts.ServiceLocator;
//using DP.Base.Utilities;

//namespace DP.Base.ServiceLocator
//{
//    /// <summary>
//    /// Represents a partial class DP.Base.ServiceLocator.ServiceLocatorBase.
//    /// </summary>
//    public abstract partial class ServiceLocatorBase : IServiceLocator
//    {
//        private static ILogger log = ApplicationEx.Instance.GlobalContext.LoggerFactory.GetLogger(typeof(ServiceLocatorBase));

//        protected ServiceLocatorBase()
//        {
//        }

//        public virtual void Initialize(IServiceLocator parentUserGroupServiceLocator, UserGroupInformation userGroupInfo, IComponentRegistrationInfoProvider componentRegistrationInfoProvider)
//        {
//            this.ParentUserGroupLocator = parentUserGroupServiceLocator;
//            if (this.ParentUserGroupLocator != null)
//            {
//                this.ParentUserGroupLocator.AddChildUserGroupLocator(this);
//            }

//            this.ComponentRegistrationInfoProvider = componentRegistrationInfoProvider;

//            this.UserGroupInfo = userGroupInfo;

//            this.ComponentRegistrationInfos = componentRegistrationInfoProvider.GetComponentRegistrationInfos(this.UserGroupInfo);

//            this.RegisterComponents(this.ComponentRegistrationInfos,
//                                        a => this.OnPreparing(a),
//                                        a => this.OnActivating(a),
//                                        a => this.OnActivated(a),
//                                        a => this.OnRelease(a));
//        }

//        private object refeshSync = new object();

//        public virtual void Refresh()
//        {
//            lock (this.refeshSync)
//            {
//                this.ComponentRegistrationInfoProvider.Refresh();
//                this.ComponentRegistrationInfos = this.ComponentRegistrationInfoProvider.GetComponentRegistrationInfos(this.UserGroupInfo);

//                this.RegisterComponents(this.ComponentRegistrationInfos,
//                                            a => this.OnPreparing(a),
//                                            a => this.OnActivating(a),
//                                            a => this.OnActivated(a),
//                                            a => this.OnRelease(a));
//            }
//        }

//        //[ThreadStatic]
//        //private Stack<ResolutionActionEventArgs> _resolutionStack;
//        //protected Stack<ResolutionActionEventArgs> ResolutionStack
//        //{
//        //    get
//        //    {
//        //        //dont need to lock because threadStatic
//        //        if (_resolutionStack == null)
//        //            _resolutionStack = new Stack<ResolutionActionEventArgs>();

//        //        return _resolutionStack;
//        //    }
//        //}

//        protected virtual void OnPreparing(ResolutionActionEventArgs raea)
//        {
//        }

//        protected virtual void OnActivating(ResolutionActionEventArgs raea)
//        {
//            //System.Diagnostics.Debug.WriteLine("Push: " + raea.Instance.ToString());

//            //?RMM should this be IContextContainer, right now using IObjectContextContainer
//            //because not sure thre is any pratical difference between
//            //IContextContainer and IObjectContextContainer and might make one go away.

//            using (var createScope = new ObjectContextCreationScope())
//            {
//                var objectContextContainer = raea.Instance as IObjectContextContainer;
//                if (objectContextContainer != null &&
//                    objectContextContainer.Context == null)
//                {
//                    IContext parentContext = createScope.GetThreadCurrentObjectContext();

//                    if (parentContext == null)
//                    {
//                        parentContext = ApplicationEx.Instance.GlobalContext;
//                    }

//                    //!RMM maybe use service locator.. but could be a chicken and egg problem
//                    var newContext = new DP.Base.Context.ObjectContext();
//                    newContext.Initialize(parentContext, objectContextContainer, parentContext.UserGroupInfo);

//                    createScope.PushContext(newContext);
//                }

//                this.AutoWireProperties(raea.Instance);

//                if (objectContextContainer != null &&
//                    objectContextContainer.Context != null)
//                {
//                    objectContextContainer.Context.InitializeAfterCreate();
//                }

//                if (raea.Instance is IInitializeAfterCreate)
//                {
//                    ((IInitializeAfterCreate)raea.Instance).InitializeAfterCreate();
//                }
//            }
//        }

//        protected virtual void OnActivated(ResolutionActionEventArgs raea)
//        {
//            //!!!RMM here is were to call abstact method (not autofac) to hook up parent objectContext and
//            //call InitializeAfterCreate
//            //var lastRes = this.ResolutionStack.Pop();
//            //if(object.ReferenceEquals(lastRes.Instance, raea.Instance) == false)
//            //{
//            //  //wtf??
//            //}

//            //System.Diagnostics.Debug.WriteLine("pop: " + raea.Instance.ToString());
//        }

//        protected virtual void OnRelease(ResolutionActionEventArgs raea)
//        {
//        }

//        protected virtual void AutoWireProperties(object instance)
//        {
//            var instanceType = instance.GetType();

//            //cannot use "this" instance for locator because of user overrides.
//            //if a if autoWiring a property of a class that is only registered with the
//            //global locator, the activating event fires on the global locator, but needs
//            //to be handled on the calling locator (the user's locator)
//            var callingLocator = DP.Base.Context.ObjectContextCreationScope.CurrentObjectContext.ServiceLocator;

//            foreach (var property in instanceType
//                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
//                .Where(pi => pi.CanWrite))
//            {
//                var propertyType = property.PropertyType;

//                if (propertyType.IsValueType && !propertyType.IsEnum)
//                {
//                    continue;
//                }

//                if (propertyType.IsArray && propertyType.GetElementType().IsValueType)
//                {
//                    continue;
//                }

//                if (property.GetIndexParameters().Length != 0)
//                {
//                    continue;
//                }

//                if (property.GetValue(instance) != null)
//                {
//                    continue;
//                }

//                if (!callingLocator.IsRegistered(propertyType, null))
//                {
//                    continue;
//                }

//                var accessors = property.GetAccessors(false);
//                if (accessors.Length == 1 && accessors[0].ReturnType != typeof(void))
//                {
//                    continue;
//                }

//                var propertyValue = callingLocator.GetInstance(propertyType, null);
//                property.SetValue(instance, propertyValue, null);
//            }
//        }

//        public T GetInstance<T>(object key)
//        {
//            T retVal;
//            this.GetInstance<T>(key, true, out retVal);
//            return retVal;
//        }

//        public bool TryGetInstance<T>(object key, out T retVal)
//        {
//            return this.GetInstance<T>(key, false, out retVal);
//        }

//        public object GetInstance(Type exportType, object key)
//        {
//            object retVal;
//            this.GetInstance(exportType, key, true, out retVal);
//            return retVal;
//        }

//        public bool TryGetInstance(Type exportType, object key, out object retVal)
//        {
//            return this.GetInstance(exportType, key, true, out retVal);
//        }

//        protected virtual bool GetInstance<T>(object key, bool throwIfMissing, out T retVal)
//        {
//            var exportType = typeof(T);
//            object retValTmp = null;
//            var found = this.GetInstance(exportType, key, throwIfMissing, out retValTmp);

//            if (found)
//            {
//                retVal = (T)retValTmp;
//            }
//            else
//            {
//                retVal = default(T);
//            }

//            return found;
//        }

//        protected virtual bool GetInstance(Type exportType, object key, bool throwIfMissing, out object retVal)
//        {
//            retVal = null;

//            GetInstanceEventArgs giea = new GetInstanceEventArgs() { RequestedType = exportType, Key = key };
//            this.OnGetInstance_Pre(giea);
//            if (giea.NewInstance != null)
//            {
//                retVal = giea.NewInstance;
//                return true;
//            }

//            bool found = this.TryResolve(exportType, key, out retVal);

//            if (found == true)
//            {
//                giea.AssignNewInstance(retVal);
//            }

//            var assignedCount = giea.NewInstanceAssignedCount;
//            this.OnGetInstance_Post(giea);

//            if (assignedCount != giea.NewInstanceAssignedCount)
//            {//assigned or re-assigned
//                retVal = giea.NewInstance;
//                found = true;
//            }

//            if (throwIfMissing == true &&
//                found == false)
//            {
//                throw new InvalidOperationException(string.Format("cannot resolve class '{0}' for key '{1}'", exportType, key));
//            }

//            return found;
//        }

//        public bool IsRegistered(System.Type exportType, object key)
//        {
//            return this.IsRegisteredCore(exportType, key);
//        }

//        public bool IsKeyRegistered(object key)
//        {
//            var currentLocator = this;
//            while (currentLocator != null)
//            {
//                if (currentLocator.ComponentRegistrationInfoProvider != null && currentLocator.IsKeyRegisteredCore(key))
//                {
//                    {
//                        return true;
//                    }
//                }

//                currentLocator = (ServiceLocatorBase)currentLocator.ParentScopeLocator;
//            }

//            //check the userGroup locators
//            currentLocator = (ServiceLocatorBase)this.ParentUserGroupLocator;
//            while (currentLocator != null)
//            {
//                if (currentLocator.ComponentRegistrationInfoProvider != null && currentLocator.IsKeyRegisteredCore(key))
//                {
//                    {
//                        return true;
//                    }
//                }

//                currentLocator = (ServiceLocatorBase)currentLocator.ParentUserGroupLocator;
//            }

//            return false;
//        }

//        protected abstract bool IsRegisteredCore(System.Type exportType, object key);
//        protected virtual bool IsKeyRegisteredCore(object key)
//        {
//            if (this.ComponentRegistrationInfoProvider == null)
//            {
//                return false;
//            }

//            return this.ComponentRegistrationInfoProvider.IsKeyRegistered(key);
//        }

//        protected abstract bool TryResolve(Type exportType, object key, out object retVal);

//        protected abstract void RegisterComponents(ComponentRegistrationInfoCollection componentRegistrationInfos,
//                                                    Action<ResolutionActionEventArgs> onPreparingAction,
//                                                    Action<ResolutionActionEventArgs> onActivatingAction,
//                                                    Action<ResolutionActionEventArgs> onActivatedAction,
//                                                    Action<ResolutionActionEventArgs> onReleasedAction);

//        public abstract IServiceLocator CreateSubScope();

//        public event EventHandler Disposing;

//        public bool IsDisposed
//        {
//            get;
//            protected set;
//        }

//        public void Dispose()
//        {
//            if (this.IsDisposed == true)
//            {
//                return;
//            }

//            this.IsDisposed = true;

//            this.DisopseCore();

//            if (this.Disposing != null)
//            {
//                this.Disposing(this, EventArgs.Empty);
//            }
//        }

//        protected abstract void DisopseCore();

//        public event GetInstanceEventHandler GetInstance_Post;
//        protected virtual void OnGetInstance_Post(GetInstanceEventArgs giea)
//        {
//            if (this.GetInstance_Post != null)
//            {
//                this.GetInstance_Post(this, giea);
//            }
//        }

//        public event GetInstanceEventHandler GetInstance_Pre;

//        protected virtual void OnGetInstance_Pre(GetInstanceEventArgs giea)
//        {
//            if (this.GetInstance_Pre != null)
//            {
//                this.GetInstance_Pre(this, giea);
//            }
//        }

//        public UserGroupInformation UserGroupInfo
//        {
//            get;
//            set;
//        }

//        public IServiceLocator ParentUserGroupLocator { get; protected set; }

//        public IList<IServiceLocator> ChildUserGroupLocators { get; protected set; }

//        public IServiceLocator ParentScopeLocator { get; protected set; }

//        public IList<IServiceLocator> ChildScopeLocators { get; protected set; }

//        public void AddChildScopeLocator(IServiceLocator serviceLocator)
//        {
//            lock (this)
//            {
//                if (this.ChildScopeLocators == null)
//                {
//                    this.ChildScopeLocators = new List<IServiceLocator>();
//                }

//                ((List<IServiceLocator>)this.ChildScopeLocators).Add(serviceLocator);
//            }
//        }

//        public void AddChildUserGroupLocator(IServiceLocator serviceLocator)
//        {
//            lock (this)
//            {
//                if (this.ChildUserGroupLocators == null)
//                {
//                    this.ChildUserGroupLocators = new List<IServiceLocator>();
//                }

//                ((List<IServiceLocator>)this.ChildUserGroupLocators).Add(serviceLocator);
//            }
//        }

//        public void ActivateInstance(object instance, object innerEventArgs)
//        {
//            ResolutionActionEventArgs raea = new ResolutionActionEventArgs() { Instance = instance, InnerEventArgs = innerEventArgs };
//            this.OnActivating(raea);
//        }

//        public class ResolutionActionEventArgs
//        {
//            public object Instance { get; set; }

//            public object InnerEventArgs { get; set; }
//        }

//        public ComponentRegistrationInfoCollection ComponentRegistrationInfos { get; set; }
//        public IComponentRegistrationInfoProvider ComponentRegistrationInfoProvider { get; private set; }
//    }
//}
