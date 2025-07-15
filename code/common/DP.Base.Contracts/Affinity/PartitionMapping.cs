using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP.Base.Contracts
{
    public class PartitionMapping
    {
        public DataAffinityTypes DataAffinityType { get; set; }

        public int StartOfPartitionRange { get; set; }

        public int SizeOfPartitionRange { get; set; }
    }

    public enum DataAffinityTypes : ushort
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Other.
        /// </summary>
        Other = 1,

        /// <summary>
        /// ScenarioGroup.
        /// </summary>
        ScenarioGroup = 2,

        /// <summary>
        /// Weather.
        /// </summary>
        Weather = 3,

        /// <summary>
        /// HistoricMarketData.
        /// </summary>
        HistoricMarketData = 4,

        /// <summary>
        /// Visualization.
        /// </summary>
        Visualization = 5,

        /// <summary>
        /// Portfolio.
        /// </summary>
        Portfolio = 6,

        /// <summary>
        /// Configuration.
        /// </summary>
        Configuration = 7,

        /// <summary>
        /// Report.
        /// </summary>
        Report = 8,

        /// <summary>
        /// HistoricAudit.
        /// </summary>
        HistoricAudit = 9,

        /// <summary>
        /// DataImport.
        /// </summary>
        DataImport = 10,

        /// <summary>
        /// DataTransformation.
        /// </summary>
        DataTransformation = 11,

        /// <summary>
        /// MarketFactors.
        /// </summary>
        MarketFactors = 12,

        /// <summary>
        /// SimpleGraphTest.
        /// </summary>
        SimpleGraphTest = 13,

        /// <summary>
        /// InstrumentGroup.
        /// </summary>
        InstrumentGroup = 14
    }
}
