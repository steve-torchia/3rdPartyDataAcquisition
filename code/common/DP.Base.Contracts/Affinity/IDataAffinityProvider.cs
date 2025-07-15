using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public interface IDataAffinityProvider
    {
        bool IsLocallyOwned(ushort dataAffinityType, uint dataAffinityInstance, out long affinityKey, out long affinityPartitionMinKey);

        long GetPartitionKey(ushort dataAffinityType, uint dataAffinityInstance);

        Tuple<long, long> GetPartitionRange(long partitionKey);

        ITicketLocation GetPartitionEndpoint(ITicketLocation inputLocation);

        string GetPartitionEndpointByPartitionKey(long partitionKey);

        DataAffinityProviderValues GetPartitionsInfo();

        DataAffinityProviderValues GetPartitionsInfoWithEndpoints();

        PartitionMapping GetDataAffinityTypePartitionMapping(ushort typeId);
        
        void ResetAll();

        void AddOwnedPartitionIdByKey(long partitionMinKey);
    }
}
