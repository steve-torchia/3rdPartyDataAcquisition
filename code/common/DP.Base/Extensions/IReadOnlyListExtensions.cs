using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Extensions
{
    public static class IReadOnlyListExtensions
    {
        public static int FindIndex<T>(this IReadOnlyList<T> source, Predicate<T> match)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (match(item))
                {
                    return index;
                }

                ++index;
            }

            return -1;
        }

        public static void ForEach<T>(this IReadOnlyList<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }
    }
}
