using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Extensions
{
    public static class IListExtensions
    {
        public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
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

        public static void ForEach<T>(this IList<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static bool Exists<T>(this IList<T> source, Predicate<T> match)
        {
            foreach (var item in source)
            {
                if (match(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddRange<T>(this IList<T> source, IEnumerable<T> newList)
        {
            bool ok = (source != null) ? true : throw new ArgumentNullException(nameof(source));
            ok = (newList != null) ? true : throw new ArgumentNullException(nameof(newList));

            if (source is List<T> concreteList)
            {
                concreteList.AddRange(newList);
                return;
            }

            foreach (var element in newList)
            {
                source.Add(element);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int chunkSize)
        {
            for (int i = 0; i < list.Count; i += chunkSize)
            {
                yield return list.GetRange(i, System.Math.Min(chunkSize, list.Count - i));
            }
        }
    }
}
