using System;
using System.Collections.Generic;
using System.Linq;

namespace DP.Base.Extensions
{
    public static class IEnumerableExtensions
    {
        public static List<Guid> ToGuids(this IEnumerable<int> range, int count = 250) => range.Select(_ => Guid.NewGuid()).ToList();

        public static List<TSource> AsList<TSource>(this IEnumerable<TSource> source) => source as List<TSource> ?? source?.ToList();

        public static bool AnySafe<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate = null) => (predicate == null ? source?.Any() : source?.Any(predicate)) == true;

        public static bool AllSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source?.All(predicate) == true;

        public static IEnumerable<T> CloneSafe<T>(this IEnumerable<T> source) => source == null ? Enumerable.Empty<T>() : source.ToList();

        public static bool ContainsSafe<TSource>(this IEnumerable<TSource> source, TSource value) => source?.Contains(value) == true;

        public static int CountSafe<TSource>(this List<TSource> source) => (source?.Count).GetValueOrDefault();
        public static int CountSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate = null) => (predicate == null ? source?.Count() : source?.Count(predicate)).GetValueOrDefault();

        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source) => source ?? Enumerable.Empty<TSource>();

        public static IEnumerable<TResult> SelectSafe<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => source?.Select(selector).EmptyIfNull();
        public static IEnumerable<TResult> SelectSafe<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) => source?.Select(selector).EmptyIfNull();

        public static string StringJoin<T>(this IEnumerable<T> source, string separator) => source == null ? null : string.Join(separator, source);
        public static string StringJoin<T>(this IEnumerable<T> source, Func<T, string> selector, string separator) => source == null ? null : string.Join(separator, source.Select(selector));

        public static IEnumerable<TSource> TakeUntil<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (var item in source)
            {
                yield return item;
                if (predicate(item))
                {
                    yield break;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> ToGroupsOf<T>(this IEnumerable<T> items, int size) =>
            items.Select((item, i) => new { item, i })
                 .GroupBy(_ => _.i / size)
                 .Select(g => g.Select(_ => _.item));

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) => new HashSet<TSource>(source);

        public static IEnumerable<TSource> UnionSafe<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => first?.Union(second.EmptyIfNull()) ?? second.EmptyIfNull();

        public static IEnumerable<TSource> WhereSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => source?.Where(predicate).EmptyIfNull();
        public static IEnumerable<TSource> WhereSafe<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => source?.Where(predicate).EmptyIfNull();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static double AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, double defaultValue)
        {
            if (source.AnySafe())
            {
                return source.Average(selector);
            }

            return defaultValue;
        }

        /// <summary>
        /// return Dictionary where the value is the rank (1st, 2nd, etc..) that each object would be in a sorted list
        /// </summary>
        public static Dictionary<T, int> RankLookup<T>(this IEnumerable<T> objList)
        {
            // generate a Dict<T, int> where the value is the index of the object in a sorted list
            return objList.Distinct()
                .OrderBy(e => e)
                .Select((e, index) => new { Obj = e, Index = index })
                .ToDictionary(e => e.Obj, e => e.Index);
        }

        /// <summary>
        /// If there are enough elements in the sequence, return a new sequence with every Nth element.  Will never return less than
        /// 'minSampleSize' elements.
        /// </summary>
        public static IEnumerable<T> SampleMinimum<T>(this IEnumerable<T> seq, int minSampleSize = 1000)
        {
            bool shouldSample = true;
            var minCollectionSizeForSampling = 2 * minSampleSize;
            var collection = seq as ICollection<T>;
            if (collection != null)
            {
                // for collections, we can get the count without enumerating the whole list
                shouldSample = collection.Count >= minCollectionSizeForSampling;
            }
            else
            {
                // for enumerables, we don't want to do Count() as it enumerates the entire list - so instead just check to see if there are the minimum # of elements to do sampling
                shouldSample = seq.Skip(minCollectionSizeForSampling).Any();
            }

            if (!shouldSample)
            {
                return seq;
            }

            var totalElems = seq.Count();
            var nth = totalElems / minSampleSize;
            var sampledSeq = seq.Where((e, i) => i % nth == 0);

            return sampledSeq;
        }

        /// <summary>
        /// Gets the immediate children of the given exception (not recursive).
        /// This can be used in conjunction with Flatten() to recursively return all child/grandchild/etc... exceptions
        /// </summary>
        public static IEnumerable<Exception> GetChildExceptions(Exception ex)
        {
            var aggEx = ex as AggregateException;
            if (aggEx != null && aggEx.InnerExceptions.AnySafe())
            {
                return aggEx.InnerExceptions;
            }

            if (ex.InnerException != null)
            {
                return new[] { ex.InnerException };
            }

            return null;
        }

        public static IEnumerable<T> Flatten<T>(this T obj, Func<T, IEnumerable<T>> childSelectorFunc)
        {
            var ret = new List<T>() { obj };

            var children = childSelectorFunc(obj);
            if (children.AnySafe())
            {
                foreach (var child in children)
                {
                    var childRet = Flatten(child, childSelectorFunc);
                    ret.AddRange(childRet);
                }
            }

            return ret;
        }

        // https://stackoverflow.com/questions/10297124/how-to-combine-more-than-two-generic-lists-in-c-sharp-zip
        public static IEnumerable<TResult> ZipThree<T1, T2, T3, TResult>(
            this IEnumerable<T1> source,
            IEnumerable<T2> second,
            IEnumerable<T3> third,
            Func<T1, T2, T3, TResult> func)
        {
            using (var e1 = source.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            using (var e3 = third.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                {
                    yield return func(e1.Current, e2.Current, e3.Current);
                }
            }
        }

        public static IEnumerable<TResult> ZipThreeFillNulls<T1, T2, T3, TResult>(
            this IEnumerable<T1> source,
            IEnumerable<T2> second,
            IEnumerable<T3> third,
            Func<T1, T2, T3, TResult> func)
        {
            using (var e1 = source.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            using (var e3 = third.GetEnumerator())
            {
                while (true)
                {
                    var e1Val = e1.MoveNext();
                    var e2Val = e2.MoveNext();
                    var e3Val = e3.MoveNext();
                    if (e1Val || e2Val || e3Val)
                    {
                        yield return func(
                            e1Val ? e1.Current : default(T1),
                            e2Val ? e2.Current : default(T2),
                            e3Val ? e3.Current : default(T3));
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
        }

        public static IEnumerable<TResult> ZipSafeStruct<T, TResult>(IEnumerable<IEnumerable<T>> listOfEnumerables, Func<IEnumerable<T?>, TResult> func)
            where T : struct
        {
            var enumerators = listOfEnumerables.Select(e => e?.GetEnumerator()).ToList();
            try
            {
                while (true)
                {
                    var enumeratorValues = enumerators.Select(e =>
                    {
                        if (e == null || !e.MoveNext())
                        { 
                            return null;
                        }

                        return (T?)e.Current;
                    }).ToList();

                    if (enumeratorValues.Any(e => e.HasValue))
                    {
                        yield return func(enumeratorValues);
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            finally
            {
                foreach (var enumerator in enumerators)
                {
                    enumerator?.Dispose();
                }
            }
        }

        public static IEnumerable<TResult> ZipSafe<T, TResult>(IEnumerable<IEnumerable<T>> listOfEnumerables, Func<IEnumerable<T>, TResult> func)
            where T : class
        {
            var enumerators = listOfEnumerables.Select(e => e?.GetEnumerator()).ToList();
            try
            {
                while (true)
                {
                    var enumeratorValues = enumerators.Select(e =>
                    {
                        if (e == null || !e.MoveNext())
                        {
                            return null;
                        }

                        return e.Current;
                    }).ToList();

                    if (enumeratorValues.Any(e => e != null))
                    {
                        yield return func(enumeratorValues);
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            finally
            {
                foreach (var enumerator in enumerators)
                {
                    enumerator?.Dispose();
                }
            }
        }

        public static bool HasDuplicate<T>(this IEnumerable<T> source, out T firstDuplicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var checkBuffer = new HashSet<T>();
            foreach (var t in source)
            {
                if (checkBuffer.Add(t))
                {
                    continue;
                }

                firstDuplicate = t;
                return true;
            }

            firstDuplicate = default(T);
            return false;
        }

        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKeyFunc,
            Func<TRight, TKey> rightKeyFunc,
            Func<TLeft, TRight, TKey, TResult> outputFunc,
            TLeft leftDefault = default,
            TRight rightDefault = default,
            IEqualityComparer<TKey> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<TKey>.Default;
            var leftIdx = left.ToLookup(leftKeyFunc, comparer);
            var rightIdx = right.ToLookup(rightKeyFunc, comparer);

            var keys = new HashSet<TKey>(leftIdx.Select(p => p.Key), comparer);
            keys.UnionWith(rightIdx.Select(p => p.Key));

            var ret = from key in keys
                       from xa in leftIdx[key].DefaultIfEmpty(leftDefault)
                       from xb in rightIdx[key].DefaultIfEmpty(rightDefault)
                       select outputFunc(xa, xb, key);

            return ret;
        }

        /// <summary>Equivalent to Sum() except will return null if all elements are null; Sum() returns 0</summary>
        public static float? SumNullable(this IEnumerable<float?> input)
        {
            bool allNull = true;
            var ret = 0f;
            foreach (var val in input)
            {
                if (val.HasValue)
                {
                    allNull = false;
                    ret += val.Value;
                }
            }

            return allNull ? null : (float?)ret;
        }

        /// <summary>Equivalent to Sum() except will return null if all elements are null; Sum() returns 0</summary>
        public static double? SumNullable(this IEnumerable<double?> input)
        {
            bool allNull = true;
            var ret = 0.0;
            foreach (var val in input)
            {
                if (val.HasValue)
                {
                    allNull = false;
                    ret += val.Value;
                }
            }

            return allNull ? null : (double?)ret;
        }

        /// <summary>Equivalent to Sum() except will return null if all elements are null; Sum() returns 0</summary>
        public static double? SumNullable<T>(this IEnumerable<T> input, Func<T, double?> selector)
        {
            bool allNull = true;
            var ret = 0.0;
            foreach (var val in input)
            {
                var doubleVal = selector(val);
                if (doubleVal.HasValue)
                {
                    allNull = false;
                    ret += doubleVal.Value;
                }
            }

            return allNull ? null : (double?)ret;
        }

        /// <summary>Equivalent to Average() except will return null if all elements are null; Average() returns 0</summary>
        public static float? AverageNullable(this IEnumerable<float?> input)
        {
            int nonNullValues = 0;
            var sum = 0f;
            foreach (var val in input)
            {
                if (val.HasValue)
                {
                    nonNullValues++;
                    sum += val.Value;
                }
            }

            return (nonNullValues == 0) ? null : (float?)sum / nonNullValues;
        }

        /// <summary>Equivalent to Average() except will return null if all elements are null; Average() returns 0</summary>
        public static double? AverageNullable(this IEnumerable<double?> input)
        {
            int nonNullValues = 0;
            var sum = 0.0;
            foreach (var val in input)
            {
                if (val.HasValue)
                {
                    nonNullValues++;
                    sum += val.Value;
                }
            }

            return (nonNullValues == 0) ? null : (double?)sum / nonNullValues;
        }

        /// <summary>Equivalent to Average() except will return null if all elements are null; Average() returns 0</summary>
        public static double? AverageNullable<T>(this IEnumerable<T> input, Func<T, double?> selector)
        {
            int nonNullValues = 0;
            var sum = 0.0;
            foreach (var val in input)
            {
                var doubleVal = selector(val);
                if (doubleVal.HasValue)
                {
                    nonNullValues++;
                    sum += doubleVal.Value;
                }
            }

            return (nonNullValues == 0) ? null : (double?)sum / nonNullValues;
        }
    }
}
