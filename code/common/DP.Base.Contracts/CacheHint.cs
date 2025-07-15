using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public enum CacheHint
    {
        /// <summary>
        /// CacheImmediately.
        /// </summary>
        CacheImmediately,

        /// <summary>
        /// NeverCache.
        /// </summary>
        NeverCache,

        /// <summary>
        /// CacheOnFirstTouch.
        /// </summary>
        CacheOnFirstTouch,

        /// <summary>
        /// CacheEverywhere.
        /// </summary>
        CacheEverywhere,

        /// <summary>
        /// CacheForParents.
        /// </summary>
        CacheForParents,
    }
}
