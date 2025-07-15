using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base
{
    public static class TimeZoneUtils
    {
        public static Dictionary<string, TimeZoneInfo> MarketTimeZones = new Dictionary<string, TimeZoneInfo>
        {
            { "ISONE", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "NYISO", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "NYMEX", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "PJM", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "ERCOT", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "SPP", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "MISO", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "CAISO", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") }
        };
    }
}
