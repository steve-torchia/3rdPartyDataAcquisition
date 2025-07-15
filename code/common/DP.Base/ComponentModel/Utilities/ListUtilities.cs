using System.Collections.Generic;

namespace DP.Base.Utilities
{
    public class ListUtilities
    {
        /// <summary>
        /// Split a list into chunks of specified size
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> list, int size = 30)
        {
            for (int i = 0; i < list.Count; i += size)
            {
                yield return list.GetRange(i, System.Math.Min(size, list.Count - i));
            }
        }
    }
}
