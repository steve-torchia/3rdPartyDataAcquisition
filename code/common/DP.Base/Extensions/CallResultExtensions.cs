using System;
using System.Linq;
using DP.Base.Contracts;

namespace DP.Base.Extensions
{
    public static class CallResultExtensions
    {
        /// <summary>
        /// Returns the stack of DisplayMessage properties from Children & all descendants
        /// </summary>
        public static string GetDisplayMessageTree(this ICallResult callResult, int indent = 0)
        {
            var ret = string.IsNullOrWhiteSpace(callResult.DisplayMessage) ? string.Empty : callResult.DisplayMessage.PadLeft(callResult.DisplayMessage.Length + indent * 2);
            callResult.Children?.ForEach(e => ret += Environment.NewLine + (e?.GetDisplayMessageTree(indent + 1)));
            return ret;
        }
    }
}
