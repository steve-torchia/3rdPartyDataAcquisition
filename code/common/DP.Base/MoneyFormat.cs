using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DP.Base
{
    public static class MoneyFormat
    {
        private static string[] suffixes = new[] { string.Empty, "k", "M", "B", "T" };

        public static string ToShortDollars(this float value)
        {
            var negSign = (value < 0f) ? "-" : string.Empty;
            value = System.Math.Abs(value);

            int suffixIndex = 0;
            while (value > 1000f)
            {
                value /= 1000f;
                suffixIndex++;
            }

            var valueStr = value.ToString("0.00");
            var ret = $"{negSign}${valueStr}{suffixes[suffixIndex]}";
            return ret;
        }
    }
}
