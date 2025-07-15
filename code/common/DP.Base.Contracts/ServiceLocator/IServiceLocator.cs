using System;
using System.Collections.Generic;
using DP.Base.Contracts.ComponentModel;
using DP.Base.Contracts.Security;

namespace DP.Base.Contracts.ServiceLocator
{
    //different service locators for different users
    //not sure if need to have the commonTypeId GetInstance methods, or if can require a valid key when have multiple of same type
    //still a WIP
    public interface IServiceLocator : IDisposableEx
    {
        T GetInstance<T>(object key/*, Action<T> preInitializeAction*/);

        bool TryGetInstance<T>(object key, out T retVal/*, Action<T> preInitializeAction*/);

        object GetInstance(Type exportType, object key/*, Action<T> preInitializeAction*/);

        bool TryGetInstance(Type exportType, object key, out object retVal/*, Action<T> preInitializeAction*/);

        bool IsRegistered(System.Type exportType, object key);

        bool IsKeyRegistered(object key);

        void Initialize(IServiceLocator parentUserGroupServiceLocator, UserGroupInformation userGroupInfo, IComponentRegistrationInfoProvider componentRegistrationInfoProvider);

        void Refresh();

        IServiceLocator CreateSubScope();

        event GetInstanceEventHandler GetInstance_Pre;
        event GetInstanceEventHandler GetInstance_Post;

        UserGroupInformation UserGroupInfo { get; set; }

        IServiceLocator ParentUserGroupLocator { get; }
        
        IList<IServiceLocator> ChildUserGroupLocators { get; }

        void AddChildUserGroupLocator(IServiceLocator childScopeLocator);

        IServiceLocator ParentScopeLocator { get; }

        IList<IServiceLocator> ChildScopeLocators { get; }

        void AddChildScopeLocator(IServiceLocator childScopeLocator);

        void ActivateInstance(object instance, object innerEventArgs);
    }
}
