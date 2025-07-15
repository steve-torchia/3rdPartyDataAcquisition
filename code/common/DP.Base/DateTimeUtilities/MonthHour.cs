using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.DateTimeUtilities
{
    public struct MonthHour
    {
        public int Month;
        public int Hour;

        public override bool Equals(object obj)
        {
            var that = (MonthHour)obj;
            return this.Month == that.Month && this.Hour == that.Hour;
        }

        public override int GetHashCode()
        {
            return this.Month.GetHashCode() ^ this.Hour.GetHashCode();
        }
    }
}
