using DP.Base.Contracts.Security;

namespace DP.Base.Contracts.ServiceLocator
{
    //different service locators for different users
    //not sure if need to have the commonTypeId GetInstance methods, or if can require a valid key when have multiple of same type
    //still a WIP
    public interface IServiceLocatorProvider 
    {
        IServiceLocator GetServiceLocator(UserGroupInformation userGroupInfo);

        IServiceLocator GetChildServiceLocator(UserGroupInformation userGroupInfo, IServiceLocator parentLocator);

        void Initialize(IComponentRegistrationInfoProvider componentRegistrationInfoProvider);

        IComponentRegistrationInfoProvider ComponentRegistrationInfoProvider { get; }

        event GetServiceLocatorEventHandler GetServiceLocator_Pre;
        event GetServiceLocatorEventHandler GetServiceLocator_Post;
    }
}
