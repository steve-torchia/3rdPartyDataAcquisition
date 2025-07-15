using System;

namespace DP.Base.Contracts
{
    public interface IOwnedDataObject
    {
        Guid? OwnerUserGroupId { get; set; }
    }
}
