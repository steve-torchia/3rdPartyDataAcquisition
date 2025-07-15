using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base
{
    public static class StringFuzzyMatch
    {
        private static char[] defaultIgnoreChars = new char[] { ',', '(', ')', '-', ':', '/' };
        public static float WordOverlap(string x, string y, char[] ignoreChars = null, IEnumerable<string> ignoreWords = null, List<List<string>> synonymListList = null)
        {
            #region validation
            if (x == null || y == null)
            {
                return 0;
            }
            #endregion

            // case insensitive
            x = x.ToLower();
            y = y.ToLower();

            // remove common characters which add no value
            ignoreChars = ignoreChars ?? defaultIgnoreChars;
            x = string.Join(" ", x.Split(ignoreChars));
            y = string.Join(" ", y.Split(ignoreChars));

            if (synonymListList != null)
            {
                foreach (var synonymList in synonymListList)
                {
                    // example: if the list contains "TOU", "Time of use" and "Time-of-use", replace them all with "TOU" 
                    // (1st elem in list is considered the normalized value
                    var lowerSynonymList = synonymList.Select(e => e.ToLower());
                    var normalizedVal = lowerSynonymList.First();
                    foreach (var synonym in lowerSynonymList.Skip(1))
                    {
                        x = (x.Contains(normalizedVal)) ? x : x.Replace(synonym, normalizedVal);
                        y = (y.Contains(normalizedVal)) ? y : y.Replace(synonym, normalizedVal);
                    }
                }
            }

            IEnumerable<string> wordsX = x.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> wordsY = y.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (ignoreWords != null && ignoreWords.Any())
            {
                wordsX = wordsX.Except(ignoreWords).ToList();
                wordsY = wordsY.Except(ignoreWords).ToList();
            }

            var xOnly = wordsX.Count(wx => !wordsY.Contains(wx));
            var yOnly = wordsY.Count(wy => !wordsX.Contains(wy));
            var commonX = wordsX.Count() - xOnly;
            var commonY = wordsY.Count() - yOnly;
            float total = xOnly + yOnly;
            total += (commonX > commonY) ? commonX : commonY;
            // for strings such as "Energy Efficiency and Peak Demand Reduction Cost Recovery Charge" and 
            //"Energy Efficiency and Peak Demand Reduction Cost Recovery - Demand Charge"
            var common = (commonX < commonY) ? commonX : commonY;
            return common / total;
        }

        public static void ReplaceAll<T>(this IList<T> source, T oldValue, T newValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            int index = -1;
            do
            {
                index = source.IndexOf(oldValue);
                if (index != -1)
                {
                    source[index] = newValue;
                }
            }
            while (index != -1);
        }

        /// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
        /// <typeparam name="T">The type of the items in the enumerables.</typeparam>
        /// <returns>The edit distance.</returns>
        public static int EditDistance<T>(IEnumerable<T> x, IEnumerable<T> y)
            where T : IEquatable<T>
        {
            #region validation
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (y == null)
            {
                throw new ArgumentNullException("y");
            }
            #endregion

            // Convert the parameters into IList instances in order to obtain indexing capabilities
            var first = x as IList<T> ?? new List<T>(x);
            var second = y as IList<T> ?? new List<T>(y);

            // Get the length of both.  If either is 0, return the length of the other, since that number of insertions would be required.
            int n = first.Count, m = second.Count;
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Rather than maintain an entire matrix (which would require O(n*m) space), just store the current row and the next row, each of which has a length m+1,
            // so just O(m) space. Initialize the current row.
            int curRow = 0, nextRow = 1;
            int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
            for (int j = 0; j <= m; ++j)
            {
                rows[curRow][j] = j;
            }

            // For each virtual row (since we only have physical storage for two)
            for (int i = 1; i <= n; ++i)
            {
                // Fill in the values in the row
                rows[nextRow][0] = i;
                for (int j = 1; j <= m; ++j)
                {
                    int dist1 = rows[curRow][j] + 1;
                    int dist2 = rows[nextRow][j - 1] + 1;
                    int dist3 = rows[curRow][j - 1] +
                        (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                    rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
                }

                // Swap the current and next rows
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }

            // Return the computed edit distance
            return rows[curRow][m];
        }
    }
}