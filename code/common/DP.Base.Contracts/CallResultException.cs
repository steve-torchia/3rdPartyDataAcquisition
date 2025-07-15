using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public class CallResultException : Exception
    {
        public CallResultException(CallResult cr)
        {
            this.CallResult = cr;
        }

        public CallResult CallResult { get; }
    }
}
