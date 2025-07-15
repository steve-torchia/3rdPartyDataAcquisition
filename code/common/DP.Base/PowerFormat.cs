using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base
{
    public static class PowerFormat
    {
        private static string[] suffixes = new[] { "Wh", "kWh", "MWh", "GWh", "TWh" };

        public static string ToShortPower(this float value, string currentUnits = "Wh")
        {
            var ok = !string.IsNullOrEmpty(currentUnits) ? true : throw new ArgumentNullException("currentUnits");
            int suffixIndex = Array.FindIndex(suffixes, e => e.ToLower() == currentUnits.ToLower());
            ok = (suffixIndex >= 0) ? true : throw new ArgumentException($"Unknown value for currentUnits: '{currentUnits}'");

            // zero is a special case
            if (value == 0f)
            {
                return "0.00 MWh";
            }

            var negSign = (value < 0f) ? "-" : string.Empty;
            value = System.Math.Abs(value);

            while (true)
            {
                if (value < 1)
                {
                    // we need to scale down to smaller unit (e.g. 0.003 MWh to 3.0 kWh)
                    if (suffixIndex > 0)
                    {
                        suffixIndex--;
                        value *= 1000f;
                    }
                    else
                    {
                        break;  // can't scale down any more
                    }
                }
                else if (value > 1000f)
                {
                    // we need to scale up to larger unit (e.g. 2000 kWh to 2.0 MWh)
                    if (suffixIndex < suffixes.Length)
                    {
                        suffixIndex++;
                        value /= 1000f;
                    }
                    else
                    {
                        break;  // can't scale up any more
                    }
                }
                else
                {
                    // no scaling required, we are done
                    break;
                }
            }

            var valueStr = value.ToString("0.00");
            var ret = $"{negSign}{valueStr} {suffixes[suffixIndex]}";
            return ret;
        }
    }
}
