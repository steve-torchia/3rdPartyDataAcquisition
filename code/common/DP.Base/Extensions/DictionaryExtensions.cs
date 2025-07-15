using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DP.Base.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value) => source[key] = value;
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value) => source[key] = value;

        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> source, KeyValuePair<TKey, TValue> pair) => source[pair.Key] = pair.Value;
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> source, KeyValuePair<TKey, TValue> pair) => source[pair.Key] = pair.Value;

        public static Dictionary<TKey, TValue> CloneSafe<TKey, TValue>(this IDictionary<TKey, TValue> source) => source == null ? new Dictionary<TKey, TValue>() : new Dictionary<TKey, TValue>(source);

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return source.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this ImmutableDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return source.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return source.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return source.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> mergeWith) => source.Merge((IEnumerable<KeyValuePair<TKey, TValue>>)mergeWith);
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, params IDictionary<TKey, TValue>[] mergeWith) => source.Merge(mergeWith.SelectMany(_ => _));
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> mergeWith) => source.Concat(mergeWith).ToDictionary(_ => _.Key, _ => _.Value);

        public static IDictionary<TKey, TVal> ToFlattenedDictionary<TKey, TVal>(this IDictionary<TKey, TVal> source, IEqualityComparer<TKey> comparer = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.ToFlattenedPairs().ToDictionary(_ => _.Key, _ => _.Value, comparer);
        }

        public static IEnumerable<KeyValuePair<TKey, TVal>> ToFlattenedPairs<TKey, TVal>(this IEnumerable<KeyValuePair<TKey, TVal>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var kvp in source)
            {
                var nested = kvp.Value as IEnumerable<KeyValuePair<TKey, TVal>>;
                if (nested != null)
                {
                    foreach (var nkvp in nested.ToFlattenedPairs())
                    {
                        yield return nkvp;
                    }

                    continue;
                }

                yield return kvp;
            }
        }
    }
}