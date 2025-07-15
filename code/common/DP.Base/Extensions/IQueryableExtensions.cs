using System;
using System.Linq;
using System.Linq.Expressions;

namespace DP.Base.Extensions
{
    public static class IQueryableExtensions
    {
        public static bool AnySafe<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate = null) => (predicate == null ? source?.Any() : source?.Any(predicate)) == true;

        public static bool AllSafe<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) => source?.All(predicate) == true;

        public static bool ContainsSafe<TSource>(this IQueryable<TSource> source, TSource value) => source?.Contains(value) == true;

        public static int CountSafe<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate = null) => (predicate == null ? source?.Count() : source?.Count(predicate)).GetValueOrDefault();

        public static IQueryable<TResult> SelectSafe<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector) => source?.Select(selector) ?? Enumerable.Empty<TResult>().AsQueryable();
        public static IQueryable<TResult> SelectSafe<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, TResult>> selector) => source?.Select(selector) ?? Enumerable.Empty<TResult>().AsQueryable();

        public static IQueryable<TSource> WhereSafe<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate) => source?.Where(predicate) ?? Enumerable.Empty<TSource>().AsQueryable();
        public static IQueryable<TSource> WhereSafe<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate) => source?.Where(predicate) ?? Enumerable.Empty<TSource>().AsQueryable();
    }
}
