using System.Collections.Generic;

namespace DP.Base
{
    public static class HashUtils
    {
        public static int GetOrderIndependentHashCode<T>(IEnumerable<T> source)
        {
            // https://stackoverflow.com/questions/670063/getting-hash-of-a-list-of-strings-regardless-of-order
            int hash = 0;
            foreach (T element in source)
            {
                hash = unchecked(hash +
                    EqualityComparer<T>.Default.GetHashCode(element));
            }

            return hash;
        }
    }
}
