using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public interface IQueryTicketIdCreator
    {
        void BuildTicketId(ITicketFast ticket);
        //TreeNode<ITicketFast> CreateTicketTree(QueryNode rootQueryNode);

        void SetTicketId(ITicketFast ticket);

        ITicketLocation GetTicketIdParts(Guid ticketId);
    }
}
