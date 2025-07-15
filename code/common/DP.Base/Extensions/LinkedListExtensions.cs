using System;
using System.Collections.Generic;

namespace DP.Base.Extensions
{
    public static class LinkedListExtensions
    {
        public static void RemoveLastUntil<T>(this LinkedList<T> source, Func<T, bool> predicate)
        {
            while (!predicate(source.Last.Value))
            {
                source.RemoveLast();
            }
        }
    }
}