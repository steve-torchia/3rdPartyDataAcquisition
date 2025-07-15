using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public class DataAffinityProviderValues
    {
        public List<Tuple<long, long>> OwnedRangeList { get; set; }
        public List<Tuple<long, long>> AllRangeList { get; } = new List<Tuple<long, long>>();
        public int PartitionCount { get; set; }
        public long MinKey { get; set; } = long.MaxValue;
        public List<Partition> PartitionList { get; set; }
        public long MaxKey { get; set; } = long.MinValue;
    }
}
