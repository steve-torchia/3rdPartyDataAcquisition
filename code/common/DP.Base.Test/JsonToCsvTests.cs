using DP.Base.Extensions;
using System;

namespace DP.Base.Test
{
    public class JsonToCsvTests
    {
        [Fact]
        public void JsonToCsvTest_Success_1Row()
        {

            var str = @"
[
    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 100.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
        ""payoutId"": 1,
        ""payoutName"": ""Emina Alikadic"",
        ""payoutTypeName"": ""Consultant"",
        ""payoutValue"": 922.5675
    }
]";
            var result = str.ToCsv();

            Assert.True(result.IsNullOrEmpty() == false);
            Assert.True(result.Split('\n').Length == 2); // header row plus one data row
        }


        [Fact]
        public void JsonToCsvTest_Success_2Rows()
        {
            var str = @"
[
    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 100.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
        ""payoutId"": 1,
        ""payoutName"": ""Emina Alikadic"",
        ""payoutTypeName"": ""Consultant"",
        ""payoutValue"": 922.5675
    },
    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 200.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
        ""payoutId"": 1,
        ""payoutName"": ""Emina Alikadic"",
        ""payoutTypeName"": ""Consultant"",
        ""payoutValue"": 123.456
    }
]";
            var result = str.ToCsv();

            Assert.True(result.IsNullOrEmpty() == false);
            Assert.True(result.Split('\n').Length == 3); // header row plus two data rows
        }

        [Fact]
        public void JsonToCsvTest_Failure_No_Json_Array()
        {
            // Missing the Json array brackets
            var str = @"

    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 100.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
        ""payoutId"": 1,
        ""payoutName"": ""Emina Alikadic"",
        ""payoutTypeName"": ""Consultant"",
        ""payoutValue"": 922.5675
    },
    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 200.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
        ""payoutId"": 1,
        ""payoutName"": ""Emina Alikadic"",
        ""payoutTypeName"": ""Consultant"",
        ""payoutValue"": 123.456
    }
";
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => str.ToCsv());   
        }

        [Fact]
        public void JsonToCsvTest_Failure_BadJson()
        {
            // Missing the Json array brackets
            var str = @"

    {
        ""adminCost"": 0.0,
        ""consultantSplit"": 100.0,
        ""contractId"": 0,
        ""id"": 0,
        ""overrideShare"": null,
        ""payeeStructureId"": 3,
        ""payeeStructureName"": null,
        ""payeeType"": null,
        ""payeeTypeId"": 1,
";
            Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => str.ToCsv());
        }
    }
}

