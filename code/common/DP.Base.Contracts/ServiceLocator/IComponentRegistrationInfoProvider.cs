using DP.Base.Contracts.Security;
namespace DP.Base.Contracts.ServiceLocator
{
    public interface IComponentRegistrationInfoProvider
    {
        ComponentRegistrationInfoCollection GetComponentRegistrationInfos(UserGroupInformation userGroupInfo);

        bool IsRegistered(System.Type exportType, object key);

        bool IsKeyRegistered(object key);

        void Refresh();

        event ComponentRegistrationInfoEventHandler GetComponentRegistrationInfos_Post;
        event ComponentRegistrationInfoEventHandler GetComponentRegistrationInfos_Pre;
    }
}
