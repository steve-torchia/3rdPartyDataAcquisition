using CsvHelper;
using CsvHelper.Configuration;
using DP.Base.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DP.Base.Test
{
    public class AliasMapRecord
    {
        public Guid SettlementLocationId;
        public string Market;
        public string Source;
        public string Alias;
        public string PublicationDate;
    }

    public class AliasMapRecordMap : ClassMap<AliasMapRecord>
    {
        public AliasMapRecordMap()
        {
            Map(m => m.SettlementLocationId);
            Map(m => m.Market);
            Map(m => m.Source);
            Map(m => m.Alias);
            Map(m => m.PublicationDate);
        }
    }

    public class AliasMapTests
    {
        // TODO:  right now the SettlementLocationToPriceSourceAliasMap contains records where multiple SettlementLocationIds map
        // to a single 3rd-party price node name (alias), as well as records where multiple 3rd-party price node names map to a
        // single SettlementLocationId
        //
        // There might be valid reasons for these cases, but allowing any kind of N:M mapping makes it difficult to write tests
        // to verify the correctness of the data.  Most of the mappings should be 1:1, and the 1:N cases should be considered 
        // exceptions that have a clear reason why they are there.
        //
        // One idea is to split this mapping file into multiple files -- one file that contains the regular 1:1 (Alias:SLID) mappings, and
        // a 2nd file that contains the 1:N mappings, and a 3rd file that contains the N:1 mappings.
        // Then the cardinality rules can be verified via unit tests by making sure all the data in these files conforms.  Also it
        // would be helpful to add a column which contains an explanation for why the 1:N or N:1 mapping exists

#if NOT_READY_YET
        [Fact]
        public void CardinalityCheck()
        {
            var root = Utils.FindBuildTreeRoot();
            var aliasMapFile = Path.Combine(root, @"data/ThirdParty/SettlementLocationToPriceSourceAliasMap.csv");
            var stream = File.OpenRead(aliasMapFile);

            var streamReader = new StreamReader(stream);
            var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<AliasMapRecordMap>();
            var records = csvReader.GetRecords<AliasMapRecord>().ToList();
        }
#endif
    }
}

