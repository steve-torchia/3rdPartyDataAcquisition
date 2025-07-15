using System;
using System.Collections.Generic;

namespace DP.Base.Contracts
{
    public interface ICallResult
    {
        List<ICallResult> Children { get; set; }
        string DisplayMessage { get; set; }
        Exception Exception { get; set; }
        bool Success { get; set; }
    }

    public interface ICallResult<T> : ICallResult
    {
        T ReturnValue { get; set; }
    }
}