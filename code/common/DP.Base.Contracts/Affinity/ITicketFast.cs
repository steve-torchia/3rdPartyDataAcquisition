using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public interface ITicketFast
    {
        Guid RootTicketId { get; set; }
        Guid Id { get; set; }
        List<Guid> ChildTicketIds { get; set; }
        //might come in handy for the ticket itself to have knowledge of both parent and children
        string DataQuery { get; }
        string NormalizedDataQuery { get; }
        Guid DataTypeId { get; }
        Guid DataLocationId { get; }
        Guid DataFormatId { get; }
        DateTime DeliveryEstimateUtc { get; }
        DateTime CreatedOnUtc { get; }
        DateTime ExpiresOnUtc { get; }

        DateTime LastModifiedUtc { get; }
        Guid StatusId { get; }
        string StatusText { get; }
        // Use to mark as deleted.  It is much morere flexible than a boolean since it can also be used to age out a ticket
        DateTime DeleteAfterUtc { get; }
        bool IsValid(Func<ITicketFast, bool> predicate);
        List<string> TagsList { get; }

        uint DataAffinityInstance { get; set; }

        ushort DataAffinityType { get; set; }

        bool CanProcessOnAnyPartition { get; set; }
    }
}
