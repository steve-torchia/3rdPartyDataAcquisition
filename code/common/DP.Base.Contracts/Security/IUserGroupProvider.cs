using System;

namespace DP.Base.Contracts.Security
{
    public interface IUserGroupProvider
    {
        UserGroupInformation LookupUserGroup(Guid userGroupId);

        UserInformation LookupUser(Guid userId);

        UserInformation LookupUser(string fullLoginName);
    }
}
