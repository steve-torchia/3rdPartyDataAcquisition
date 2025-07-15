using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DP.Base.Extensions;
using DP.Base.Utilities;

namespace DP.Base.Math
{
    public static class MathUtils
    {
        // https://stackoverflow.com/questions/38789183/subset-sum-algorithm-efficiency/38824421#38824421
        public static List<T[]> SubsetSums<T>(IEnumerable<T> items, decimal target, Func<T, decimal> amountGetter, decimal tolerance)
        {
            var counter = new ComboCounter { MaxAttempts = 500000 };

            Stack<T> unusedItems = new Stack<T>(items.OrderByDescending(amountGetter));
            Stack<T> usedItems = new Stack<T>();
            List<T[]> results = new List<T[]>();
            SubsetSumsRec(unusedItems, usedItems, target, results, amountGetter, tolerance, counter);
            return results;
        }

        public static void SubsetSumsRec<T>(Stack<T> unusedItems, Stack<T> usedItems, decimal targetSum, List<T[]> results, Func<T, decimal> amountGetter, decimal tolerance, ComboCounter counter)
        {
            counter.Count++;
            if (counter.Count > counter.MaxAttempts)
            {
                return;    // stop recursing, but return what's been found so far
            }

            if (System.Math.Abs(targetSum) <= tolerance)
            {
                results.Add(usedItems.ToArray());
            }

            if (System.Math.Abs(targetSum) < 0 || unusedItems.Count == 0)
            {
                return;
            }

            var item = unusedItems.Pop();
            decimal currentAmount = amountGetter(item);
            if (targetSum >= currentAmount - tolerance)
            {
                // case 1: use current element
                usedItems.Push(item);
                SubsetSumsRec(unusedItems, usedItems, targetSum - currentAmount, results, amountGetter, tolerance, counter);
                usedItems.Pop();
                // case 2: skip current element
                SubsetSumsRec(unusedItems, usedItems, targetSum, results, amountGetter, tolerance, counter);
            }

            unusedItems.Push(item);
        }

        public static IEnumerable<List<decimal>> FindSumCombinations(IEnumerable<decimal> set, decimal sum, decimal tolerance = 0m)
        {
            var ret = SubsetSums<decimal>(set, sum, e => e, tolerance);
            return ret.Where(e => e.Count() > 0).Select(e => e.ToList());
        }

        // NOTE: this only looks at the precision after the decimal place.  For example "0.12" and "34.12" will both be precision 2
        public static int GetPrecision(decimal d)
        {
            // don't ask
            // we might want to find a different way to do this. Lots of discussion on SO
            //https://stackoverflow.com/questions/13477689/find-number-of-decimal-places-in-decimal-value-regardless-of-culture
            return BitConverter.GetBytes(decimal.GetBits(d)[3])[2];
        }

        /// <summary>
        /// Compares two decimals for equality, allowing for rounding after the decimal point. Returns the # of digits that are equal, or zero if the numbers are not equal.
        /// For example, 12.01 and 12.012 would be considered equal (return 4), but 12.01 and 12.018 are not equal (return 0)
        /// </summary>
        public static int EqualDecimalDigits(decimal a, decimal b)
        {
            // normalizing into strings lets us avoid the complexity of trailing zeros after the decimal point, align places
            var astr = a.ToString("00000000000000.########").Replace(".", string.Empty);
            var bstr = b.ToString("00000000000000.########").Replace(".", string.Empty);

            var matchingDigits = 0;
            var inLeadingZeros = true;

            // keep looking at digits until one of the numbers has no more
            var lastDigitToExamine = System.Math.Min(astr.Length, bstr.Length);
            for (int i = 0; i < lastDigitToExamine; i++)
            {
                var adigit = astr[i];
                var bdigit = bstr[i];

                if (inLeadingZeros)
                {
                    if (adigit == '0' && bdigit == '0')
                    {
                        continue;   // leading zeros don't count
                    }
                    else
                    {
                        inLeadingZeros = false;
                    }
                }

                // when we get to the last digit for one of the numbers, we need to round off the other number
                // TODO: maybe round the #s first when they are decimals at top of method?
                if (i == lastDigitToExamine - 1)
                {
                    var next_a_digit = (i + 1 < astr.Length) ? astr[i + 1] : '0';
                    var next_b_digit = (i + 1 < bstr.Length) ? bstr[i + 1] : '0';
                    if (next_a_digit >= '5')
                    {
                        adigit++;
                    }

                    if (next_b_digit >= '5')
                    {
                        bdigit++;
                    }
                }

                var digitDiff = adigit - bdigit;
                if (digitDiff == 0)
                {
                    matchingDigits++;
                    continue;
                }
                else
                {
                    return 0;   // the numbers are not equal
                }
            }

            return matchingDigits;
        }

        // (moved from DataProviderTestBase)
        public static float InterlockedAdd(ref float location1, float value)
        {
            float newCurrentValue = location1; // non-volatile read, so may be stale
            while (true)
            {
                float currentValue = newCurrentValue;
                float newValue = currentValue + value;
                newCurrentValue = System.Threading.Interlocked.CompareExchange(ref location1, newValue, currentValue);
                if (newCurrentValue == currentValue)
                {
                    return newValue;
                }
            }
        }

        public static bool SequenceAlmostEquals(IList<float> list1, IList<float> list2, float delta = 1e-6f)
        {
            if (list1.Count() != list2.Count())
            {
                return false;
            }

            for (int i = 0; i < list1.Count(); ++i)
            {
                if (System.Math.Abs(list1[i] - list2[i]) > delta)
                {
                    return false;
                }

                // RS JH todo use this version when tested
                //if (! IsAlmostEqual(list1[i],  list2[i], delta))
                //{
                //    return false;
                //}
            }

            return true;
        }

        public static bool IsAlmostEqual(float a, float b, float delta = 1e-6f)
        {
            return System.Math.Abs(a - b) < delta;
        }

        public static bool IsAlmostEqual(double a, double b, double delta = 1e-6f)
        {
            return System.Math.Abs(a - b) < delta;
        }

        public static List<double> PercentileList(double[] sortedArray, IEnumerable<double> percentileList)
        {
            return percentileList
                .Select(e => Percentile(sortedArray, e))
                .ToList();
        }

        public static bool CloseEnough(float val1, float val2, float moe = 0.10f) //default to 10% cutoff
        {
            if (val1 == val2)
            {
                return true;
            }

            double diff = System.Math.Abs(val1 - val2);
            double scale = (val1 + val2) / 2;

            return (diff / scale < moe);
        }

        public static List<float> PercentileList(float[] sortedArray, IEnumerable<float> percentileList)
        {
            return percentileList
                .Select(e => Percentile(sortedArray, e))
                .ToList();
        }

        public static float[] DefaultPercentiles = new[] { 0.01f, 0.05f, 0.25f, 0.5f, 0.75f, 0.95f, 0.99f };
        public static float[] DefaultPercentileWeights = new[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        public static float[] DefaultQuantiles = new[] { 0f, 0.20f, 0.40f, 0.60f, 0.80f, 1.00f };

        public static double Percentile(double[] sortedArray, double percentile)
        {
            double realIndex = percentile * (sortedArray.Length - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;
            if (index + 1 < sortedArray.Length)
            {
                return sortedArray[index] * (1 - frac) + sortedArray[index + 1] * frac;
            }
            else
            {
                return sortedArray[index];
            }
        }

        public static float Percentile(float[] sortedArray, float percentile)
        {
            float realIndex = percentile * (sortedArray.Length - 1);
            int index = (int)realIndex;
            float frac = realIndex - index;
            if (index + 1 < sortedArray.Length)
            {
                return sortedArray[index] * (1 - frac) + sortedArray[index + 1] * frac;
            }
            else
            {
                return sortedArray[index];
            }
        }

        internal static void BucketIndexToBounds(List<float> bucketSizes, int bucketIndex, out float lowerBound, out float upperBound)
        {
            lowerBound = bucketSizes.Take(bucketIndex).Sum();
            upperBound = lowerBound + bucketSizes[bucketIndex];
        }

        // given the percentile and a list of buckets, determine which bucket the percentile falls in and what portion of the way through
        // For example:
        //   input: percentile = 0.27, buckets = [ 0.2, 0.2, 0.2, 0.2, 0.2 ]
        //   output: bucketIndex = 1 (2nd bucket), bucketPart = 0.07  (0.27 - 0.2)
        internal static void FindBucketAndPart(float percentile, List<float> newBucketSizes, out int bucketIndex, out float bucketPart)
        {
            var newTotalSamples = newBucketSizes.Sum();
            bucketPart = percentile * newTotalSamples;

            bucketIndex = 0;
            for (bucketIndex = 0; bucketIndex < newBucketSizes.Count; bucketIndex++)
            {
                var currentBucketSize = newBucketSizes[bucketIndex];
                if (bucketPart < currentBucketSize)
                {
                    break;
                }

                bucketPart -= newBucketSizes[bucketIndex];
            }
        }

        public static double RandNormal(Random rand, double mean, double stdDev)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();

            double randStdNormal =
                System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                System.Math.Sin(2.0 * System.Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public static List<float> WeightedPercentiles(IEnumerable<float> percentileList, IEnumerable<float> weights)
        {
            #region validation
            bool ok = percentileList.AnySafe() ? true : throw new ArgumentNullException("percentileList");
            ok = weights.AnySafe() ? true : throw new ArgumentNullException("weights");
            ok = (weights.Count() == percentileList.Count() + 1) ? true : throw new ArgumentOutOfRangeException("weights.Length must equal percentileList.Length + 1");
            ok = weights.All(e => e >= 0) ? true : throw new ArgumentOutOfRangeException("weights must not have negative values");
            ok = percentileList.All(e => e >= 0) ? true : throw new ArgumentOutOfRangeException("percentileList must not have negative values");
            #endregion

            // Each "bucket" is the fraction of the samples between the current percentile & the previous percentile
            // (e.g. for the standard 1/5/25/50/75/95/99 percentiles, the 1st bucket is 0.01, 2nd is 0.04, 3rd is 0.20, etc...)
            var bucketSizes = percentileList.Select((e, i) =>
            {
                var prev = (i >= 1) ? percentileList.ElementAt(i - 1) : 0f;
                return e - prev;
            }).ToList();

            bucketSizes.Add(1.0f - bucketSizes.Sum());  // (the last bucket is the leftover values to the right of the last percentile)
            bucketSizes = bucketSizes.Select(e => (float)System.Math.Round((decimal)(e), 4)).ToList();

            // Use the given weights to adjust the bucket size
            var newBucketSizes = weights.Select((wgt, i) => bucketSizes[i] * wgt).ToList();

            // Using the new bucket sizes, figure out the new percentiles.  These new percentiles can be used on the original list to get the
            // percentile values with the weights taken into consideration.
            var newPercentileList = new List<float>();
            for (int percentileIndex = 0; percentileIndex < percentileList.Count(); percentileIndex++)
            {
                var originalPercentile = percentileList.ElementAt(percentileIndex);

                // figure out how many elements into each bucket the percentile would be.... 
                FindBucketAndPart(originalPercentile, newBucketSizes, out int newBucketIndex, out float newBucketPart);

                // determine the fraction of the way through the new buckets & map that back to the original buckets to determine
                // the new percentile
                var fractionBetweenBuckets = newBucketPart / newBucketSizes[newBucketIndex];
                BucketIndexToBounds(bucketSizes, newBucketIndex, out float origBucketLowerBound, out float origBucketUpperBound);
                var newPercentile = bucketSizes[newBucketIndex] * fractionBetweenBuckets + origBucketLowerBound;
                newPercentileList.Add(newPercentile);
            }

            // NOTE: frequently get lots of odd values due to floating point math dumbness.  Although it's not strictly necessary,
            // I'm rounding here just to keep the numbers "normal" (e.g. 0.25 instead of 0.249999985)
            newPercentileList = newPercentileList.Select(e => (float)System.Math.Round((decimal)(e), 4)).ToList();
            return newPercentileList;
        }

        public static float Npv(IEnumerable<float> values, float discountRate)
        {
            //var flow = new[] { -100000.0, 50000.0, 40000.0, 30000.0, 20000.0 };
            var npv = values
                .Select((c, n) => c / (float)System.Math.Pow(1 + discountRate, n))
                .Sum();
            return npv;
        }

        public static float ConvertAnnualRateToMonthly(float annualRate)
        {
            var ret = System.Math.Pow(1.0f + annualRate,  1.0f / 12.0f) - 1.0f;
            return (float)ret;
        }
    }

    public class ComboCounter
    {
        public int MaxAttempts { get; set; }
        public int Count { get; set; }
    }
}
