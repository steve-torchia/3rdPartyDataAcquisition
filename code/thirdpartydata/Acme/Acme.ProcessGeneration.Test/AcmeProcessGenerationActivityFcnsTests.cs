using DP.Base.Extensions;
using Ingress.Lib.Base;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System;
using System.IO;
using System.IO.Compression;
using Xunit;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;
using Acme.Contracts;
using Ingress.Lib.Base.Contracts;
using System.Threading.Tasks;

namespace Acme.ProcessGeneration.Test
{
    public class AcmeProcessGenerationActivityFcnsTests
    {
        private readonly Mock<ILogger<AcmeProcessGenerationActivityFcns>> loggerMock;

        private const string HistoryId = "9999";
        private const string DefaultFailureMessage = "It Failed";
        private const string DefaultSuccessMessage = "It succeeded!";
        private const string BlobDestinationPath = "this is a blob destination path";
        private const string DownloadAndSaveFalureMsg = "Error trying to download and/or save results";

        private readonly Mock<CloudStorageAccount> cloudStorageMock;

        private readonly Mock<IBlobContainerWrapper> blobContainerWrapperMock = new Mock<IBlobContainerWrapper>();

        const string AcmeGenFileStartsWith = "Templates_Gens_";

        static readonly string WorkingDirectory = $"{Environment.CurrentDirectory}\\TestData\\UnitTestData";

        public AcmeProcessGenerationActivityFcnsTests()
        {
            loggerMock = new Mock<ILogger<AcmeProcessGenerationActivityFcns>>();
            cloudStorageMock = new Mock<CloudStorageAccount>();
        }

        private void SetupBlobContainerMock(byte[] zipFileData, string destinationFileLocation)
        {
            // Mock the download of zip file data
            blobContainerWrapperMock.Setup(
                b => b.DownloadBytesAsync(It.IsAny<string>()))
                .ReturnsAsync(zipFileData);

            // Mock the uploading of the results
            var resultsFileLocation =
            blobContainerWrapperMock.Setup(
                b => b.UploadResult(destinationFileLocation, It.IsAny<byte[]>()))
                .Returns(Task.FromResult<string>("Here is the result"));
        }

        [Fact]
        public void ProcessAcmeGenerationZipFile_Success()
        {
            // Set up the Zip file we want to process:
            var toBeProcessedDirectory = $"{WorkingDirectory}\\ToBeProcessed";
            var outputDirectory = $"{WorkingDirectory}\\Processed";

            // mock a bytearray of the Zip Archive based on some test csv files we get back from Acme
            var csvFiles = Directory.GetFiles(toBeProcessedDirectory, "*.csv");
            var zipArchiveByteArray = CreateZipArchiveByteArray(csvFiles);

            // Setup blob mock
            SetupBlobContainerMock(zipArchiveByteArray, outputDirectory);

            // Set up function under test:
            var fcn = new AcmeProcessGenerationActivityFcns(blobContainerWrapperMock.Object, loggerMock.Object);
            var mockDestinationBlobConfigInfo = new Mock<BlobConfigInfo>();
            var ctx = GetMockAcmeProcessGenerationContext(null, mockDestinationBlobConfigInfo.Object);

            // Execute Function:
            var ret = fcn.ProcessZipFile(ctx.Object).Result;

            // Results:
            Assert.True(ret.Success);
        }

        [Fact]
        public void ProcessAcmeGenerationZipFile_EmptyZipFile()
        {
            // Set up the Zip file we want to process:
            var toBeProcessedDirectory = $"{WorkingDirectory}\\ToBeProcessed\\EmptyZipFile";

            var outputDirectory = $"{WorkingDirectory}\\Processed";

            // mock a bytearray of the Zip Archive based on some test csv files we get back from Acme
            var csvFiles = Directory.GetFiles(toBeProcessedDirectory, "*.csv");
            var zipArchiveByteArray = CreateZipArchiveByteArray(csvFiles);

            // Setup blob mock
            SetupBlobContainerMock(zipArchiveByteArray, outputDirectory);

            // Set up function under test:
            var fcn = new AcmeProcessGenerationActivityFcns(blobContainerWrapperMock.Object, loggerMock.Object);
            var mockDestiationBlobConfigInfo = new Mock<BlobConfigInfo>();
            var ctx = GetMockAcmeProcessGenerationContext(null, mockDestiationBlobConfigInfo.Object);

            // Execute Function:
            var ret = fcn.ProcessZipFile(ctx.Object).Result;

            // Results:
            Assert.False(ret.Success);
            Assert.Contains("Exception in ProcessZipFile", ret.DisplayMessage);
            Assert.Contains("is empty", ret.DisplayMessage);
        }

        [Fact]
        public void ProcessAcmeGenerationZipFile_MissingOrIncorrectTimeZone()
        {
            // NOTE THIS DOES NOT TEST THE CONTENTS OF THE UPLOADED FILE

            // Set up the Zip file we want to process:
            var toBeProcessedDirectory = $"{WorkingDirectory}\\ToBeProcessed\\BogusTimeZone";

            var outputDirectory = $"{WorkingDirectory}\\Processed";

            // mock a bytearray of the Zip Archive based on some test csv files we get back from Acme
            var csvFiles = Directory.GetFiles(toBeProcessedDirectory, "*.csv");
            var zipArchiveByteArray = CreateZipArchiveByteArray(csvFiles);

            // Setup blob mock
            SetupBlobContainerMock(zipArchiveByteArray, outputDirectory);

            // Set up function under test:
            var fcn = new AcmeProcessGenerationActivityFcns(blobContainerWrapperMock.Object, loggerMock.Object);
            var mockDestiationBlobConfigInfo = new Mock<BlobConfigInfo>();
            var ctx = GetMockAcmeProcessGenerationContext(null, mockDestiationBlobConfigInfo.Object);

            // Execute Function:
            var ret = fcn.ProcessZipFile(ctx.Object).Result;

            // Results:
            Assert.False(ret.Success);
            Assert.Contains("Exception in ProcessZipFile", ret.DisplayMessage);
            Assert.Contains("not found in UtcOffsetMap", ret.DisplayMessage);
        }

        [Fact]
        public void ProcessAcmeGenerationZipFile_TooManyGenerationFilesInZip()
        {
            // Set up the Zip file we want to process:
            var toBeProcessedDirectory = $"{WorkingDirectory}\\ToBeProcessed\\TooManyGen";

            var outputDirectory = $"{WorkingDirectory}\\Processed";

            // mock a bytearray of the Zip Archive based on some test csv files we get back from Acme
            var csvFiles = Directory.GetFiles(toBeProcessedDirectory, "*.csv");
            var zipArchiveByteArray = CreateZipArchiveByteArray(csvFiles);

            // Setup blob mock
            SetupBlobContainerMock(zipArchiveByteArray, outputDirectory);

            // Set up function under test:
            var fcn = new AcmeProcessGenerationActivityFcns(blobContainerWrapperMock.Object, loggerMock.Object);
            var mockDestiationBlobConfigInfo = new Mock<BlobConfigInfo>();
            var ctx = GetMockAcmeProcessGenerationContext(null, mockDestiationBlobConfigInfo.Object);

            // Execute Function:
            var ret = fcn.ProcessZipFile(ctx.Object).Result;

            // Results:
            Assert.False(ret.Success);
            Assert.Contains("Exception in ProcessZipFile", ret.DisplayMessage);
            Assert.Contains($"contains > 1 files that start with: {AcmeProcessGenerationHelpers.AcmeGenFileStartsWith}", ret.DisplayMessage);
        }


        [Fact]
        public void ProcessAcmeGenerationZipFile_TooManyTemplateFilesInZip()
        {
            // Set up the Zip file we want to process:
            var toBeProcessedDirectory = $"{WorkingDirectory}\\ToBeProcessed\\TooManyTmp";

            var outputDirectory = $"{WorkingDirectory}\\Processed";

            // mock a bytearray of the Zip Archive based on some test csv files we get back from Acme
            var csvFiles = Directory.GetFiles(toBeProcessedDirectory, "*.csv");
            var zipArchiveByteArray = CreateZipArchiveByteArray(csvFiles);

            // Setup blob mock
            SetupBlobContainerMock(zipArchiveByteArray, outputDirectory);

            // Set up function under test:
            var fcn = new AcmeProcessGenerationActivityFcns(blobContainerWrapperMock.Object, loggerMock.Object);
            var mockDestiationBlobConfigInfo = new Mock<BlobConfigInfo>();
            var ctx = GetMockAcmeProcessGenerationContext(null, mockDestiationBlobConfigInfo.Object);

            // Execute Function:
            var ret = fcn.ProcessZipFile(ctx.Object).Result;

            // Results:
            Assert.False(ret.Success);
            Assert.Contains("Exception in ProcessZipFile", ret.DisplayMessage);
            Assert.Contains($"contains > 1 Template file that starts with: {AcmeProcessGenerationHelpers.AcmeMasterTemplateFileStartsWith}", ret.DisplayMessage);
        }

        [Fact]
        public void UtcOffsetTests()
        {
            // 2am local
            var strLocalTimeStamp = "2017-03-12 02:00:00";

            // PST
            Assert.Equal(
                "2017-03-12 10:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["PST"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // MST
            Assert.Equal(
                "2017-03-12 09:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["MST"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // CST
            Assert.Equal(
                "2017-03-12 08:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["CST"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // EST
            Assert.Equal(
                "2017-03-12 07:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["EST"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // WET
            Assert.Equal(
                "2017-03-12 02:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["WET"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // CET
            Assert.Equal(
                "2017-03-12 01:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["CET"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // EET
            Assert.Equal(
                "2017-03-12 00:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["EET"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // FET
            Assert.Equal(
                "2017-03-11 23:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["FET"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

            // GET
            Assert.Equal(
                "2017-03-11 22:00:00",
                DateTime.Parse(strLocalTimeStamp).AddHours(AcmeHelpers.UtcOffsetMap["GET"]).ToString(AcmeHelpers.AcmeTimeStampFormat));

        }

        //[Fact]
        [Fact(Skip = "manual only for debugging")]
        public void ProcessAcmeZipFile()
        {
            var workingDirectory = $"{Environment.CurrentDirectory}/TestData/ToBeProcessed";
            var zipFileList = Directory.GetFiles(workingDirectory, "*.zip");

            //var zipFile = zipFileList[0]; //File.GetFiles($"{Environment.CurrentDirectory}/TestData/{ZipFileSpec}");
            //var csvFileName = $"{Path.GetFileNameWithoutExtension(zipFile)}.csv"; // csv file needs to be the same name as the zip
            //ZipFile.ExtractToDirectory(zipFile, workingDirectory);

            // Normalizes the path.
            var extractPath = Path.GetFullPath(workingDirectory);

            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                extractPath += Path.DirectorySeparatorChar;

            foreach (var zipFile in zipFileList)
            {
                var csvFileName = $"{Path.GetFileNameWithoutExtension(zipFile)}.csv"; // csv file needs to be the same name as the zip

                var projectId = csvFileName.Split("_")[0];

                var weatherYear = csvFileName.Split("_")[3];

                using (ZipArchive archive = ZipFile.OpenRead(zipFile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.StartsWithIgnoreCase(AcmeGenFileStartsWith))
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            string destinationPath = Path.GetFullPath(Path.Combine(extractPath, csvFileName));

                            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                            // are case-insensitive.
                            if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            {
                                //entry.ExtractToFile(destinationPath);

                                var destList = new List<AcmeGenProcessed>();

                                using (var sr = new StreamReader(entry.Open()))
                                using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
                                {
                                    csv.Context.RegisterClassMap<AcmeGenProcessedMap>();

                                    var sourceRecords = csv.GetRecords<AcmeGenProcessed>().AsList();

                                    foreach (var sourceRecord in sourceRecords)
                                    {
                                        sourceRecord.Source = "Acme";
                                        sourceRecord.ProjectId = projectId;
                                        sourceRecord.WeatherYear = weatherYear;

                                        destList.Add(sourceRecord);
                                    }
                                }

                                // using (var memoryStream = new MemoryStream())
                                using (var streamWriter = new StreamWriter(destinationPath))
                                using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                                {
                                    csv.WriteRecords(destList);

                                    csv.Flush();

                                    streamWriter.Flush();

                                }
                            }
                        }
                    }
                }
            }
        }

        private static byte[] CreateZipArchiveByteArray(string[] fileList)
        {
            byte[] retVal = null;

            using (MemoryStream memStream = new System.IO.MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memStream, ZipArchiveMode.Create))
                {
                    foreach (string file in fileList)
                    {
                        // add file to the zip
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }
                retVal = memStream.ToArray();
            }
            return retVal;
        }


        internal static Mock<AcmeProcessGenerationContext> GetMockAcmeProcessGenerationContext(string requestJson, BlobConfigInfo blobConfigInfo, string zipFile = null)
        {
            var ctx = new Mock<AcmeProcessGenerationContext>();

            ctx.Object.ZipFile = zipFile ?? "Wind_PR-00009999_Cactus Flats_2017_31.15_-99.98_148_VESTAS_V1263450_87_1_1.zip";

            //ctx.ProcessRequestJson = requestJson;
            //ctx.BlobConfigInfo = blobConfigInfo;

            return ctx;
        }
    }
}
