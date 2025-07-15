using System;
using System.Collections.Generic;
using DP.Base.Contracts;
using DP.Base.Contracts.Security;
using DP.Base.Contracts.ServiceLocator;

namespace DP.Base.Context
{
    public partial class ObjectContext
    {
        public class ContextServiceLocatorWrapper : IServiceLocator
        {
            private IObjectContext objectContext;
            private IServiceLocator serviceLocator;

            public ContextServiceLocatorWrapper(IObjectContext objectContext, IServiceLocator serviceLocator)
            {
                if (serviceLocator == null)
                {
                    throw new ArgumentNullException("serviceLocator");
                }

                this.objectContext = objectContext;
                this.serviceLocator = serviceLocator;
            }

            public T GetInstance<T>(object key)
            {
                using (var createScope = new ObjectContextCreationScope())
                {
                    createScope.PushContext(this.objectContext);

                    return this.serviceLocator.GetInstance<T>(key);
                }
            }

            public bool TryGetInstance<T>(object key, out T retVal)
            {
                using (var createScope = new ObjectContextCreationScope())
                {
                    createScope.PushContext(this.objectContext);

                    return this.serviceLocator.TryGetInstance<T>(key, out retVal);
                }
            }

            public object GetInstance(Type exportType, object key)
            {
                using (var createScope = new ObjectContextCreationScope())
                {
                    createScope.PushContext(this.objectContext);

                    return this.serviceLocator.GetInstance(exportType, key);
                }
            }

            public bool TryGetInstance(Type exportType, object key, out object retVal)
            {
                using (var createScope = new ObjectContextCreationScope())
                {
                    createScope.PushContext(this.objectContext);

                    return this.serviceLocator.TryGetInstance(exportType, key, out retVal);
                }
            }

            public bool IsRegistered(Type exportType, object key)
            {
                return this.serviceLocator.IsRegistered(exportType, key);
            }

            public bool IsKeyRegistered(object key)
            {
                return this.serviceLocator.IsKeyRegistered(key);
            }

            public void Initialize(IServiceLocator parentUserGroupServiceLocator, Contracts.Security.UserGroupInformation userGroupInfo, IComponentRegistrationInfoProvider componentRegistrationInfoProvider)
            {
                throw new NotImplementedException("Cannot initialize wrapper");
            }

            public void Refresh()
            {
                this.serviceLocator.Refresh();
            }

            public IServiceLocator CreateSubScope()
            {
                return this.serviceLocator.CreateSubScope();
            }

            public event GetInstanceEventHandler GetInstance_Pre
            {
                add
                {
                    this.serviceLocator.GetInstance_Pre += value;
                }
                remove
                {
                    this.serviceLocator.GetInstance_Pre -= value;
                }
            }

            public event GetInstanceEventHandler GetInstance_Post
            {
                add
                {
                    this.serviceLocator.GetInstance_Post += value;
                }
                remove
                {
                    this.serviceLocator.GetInstance_Post -= value;
                }
            }

            public UserGroupInformation UserGroupInfo
            {
                get
                {
                    return this.serviceLocator.UserGroupInfo;
                }
                set
                {
                    this.serviceLocator.UserGroupInfo = value;
                }
            }

            public IServiceLocator ParentUserGroupLocator
            {
                get { return this.serviceLocator.ParentUserGroupLocator; }
            }

            public IList<IServiceLocator> ChildUserGroupLocators
            {
                get { return this.serviceLocator.ChildUserGroupLocators; }
            }

            public void AddChildUserGroupLocator(IServiceLocator childScopeLocator)
            {
                if (childScopeLocator is ContextServiceLocatorWrapper)
                {
                    childScopeLocator = ((ContextServiceLocatorWrapper)childScopeLocator).serviceLocator;
                }

                this.serviceLocator.AddChildUserGroupLocator(childScopeLocator);
            }

            public IServiceLocator ParentScopeLocator
            {
                get { return this.serviceLocator.ParentScopeLocator; }
            }

            public IList<IServiceLocator> ChildScopeLocators
            {
                get { return this.serviceLocator.ChildScopeLocators; }
            }

            public void AddChildScopeLocator(IServiceLocator childScopeLocator)
            {
                if (childScopeLocator is ContextServiceLocatorWrapper)
                {
                    childScopeLocator = ((ContextServiceLocatorWrapper)childScopeLocator).serviceLocator;
                }

                this.serviceLocator.AddChildScopeLocator(childScopeLocator);
            }

            public bool IsDisposed
            {
                get { return this.serviceLocator.IsDisposed; }
            }

            public event EventHandler Disposing
            {
                add
                {
                    this.serviceLocator.Disposing += value;
                }
                remove
                {
                    this.serviceLocator.Disposing -= value;
                }
            }

            public void Dispose()
            {
                this.serviceLocator.Dispose();
            }

            public void ActivateInstance(object instance, object innerEventArgs)
            {
                using (var createScope = new ObjectContextCreationScope())
                {
                    createScope.PushContext(this.objectContext);

                    this.serviceLocator.ActivateInstance(instance, innerEventArgs);
                }
            }
        }
    }
}
