using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public interface ITicketLocation
    {
        uint DataAffinityInstance { get; set; }

        ushort DataAffinityType { get; set; }

        string Endpoint { get; set; }

        Guid Id { get; set; }

        string InstanceId { get; set; }

        Guid PartitionId { get; set; }

        long PartitionMinKey { get; set; }

        long PartitionKey { get; set; }
    }
}
