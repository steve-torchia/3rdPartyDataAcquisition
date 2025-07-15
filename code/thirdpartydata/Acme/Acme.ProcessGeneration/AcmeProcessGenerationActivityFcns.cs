using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DP.Base.Contracts;
using Ingress.Lib.Base.Contracts;
using Microsoft.Extensions.Logging;
using Acme.Contracts;
using DurableFunctionsCommon;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.IO.Compression;
using DP.Base.Extensions;
using CsvHelper;
using System.Globalization;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Functions.Worker;


namespace Acme.ProcessGeneration
{
    /// <summary>
    /// Class which encapsulates the Durable Activity Functions used to fetch the Acme Generation Data
    /// </summary>
    public class AcmeProcessGenerationActivityFcns : ActivityFunctionBase<AcmeProcessGenerationActivityFcns>
    {
        public AcmeProcessGenerationActivityFcns(IBlobContainerWrapper blobContainerWrapper = null, ILogger<AcmeProcessGenerationActivityFcns> log = null)
            : base(blobContainerWrapper, log)
        {
        }

        [Function(nameof(GetListOfGenerationFilesToProcess))]
        public Task<CallResult<List<string>>> GetListOfGenerationFilesToProcess(
            [ActivityTrigger] AcmeProcessGenerationContext ctx)
        {
            try
            {
                var retVal = new List<string>();

                var blobContainerWrapper = GetBlobContainerWrapper(ctx.BlobConfigInfo);

                // Get the list of files:
                var path = $"{AcmeHelpers.FileSpecRootPath}{AcmeHelpers.ToBeProcessedPath}";

                var blobUriList = blobContainerWrapper.GetBlobItemsAsync(path).GetAwaiter().GetResult();

                if (blobUriList.Count() == 0)
                {
                    // not really an error condition - there are simply no files to process at this time
                    var diagMsg = $"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] No Files found for processing at: {blobContainerWrapper.GetAbsoluteUri()}/{path}";
                    Log.LogInformation(diagMsg);
                    return Task.FromResult(new CallResult<List<string>>()
                    {
                        Success = true,
                        DisplayMessage = diagMsg,
                    });
                }

                var fileList = blobUriList
                    .Cast<CloudBlockBlob>()
                    .Select(b => b.Name)
                    .ToList();

                return Task.FromResult(CallResult.CreateSuccessResult<List<string>>(fileList));
            }
            catch (Exception ex)
            {
                return Task.FromResult(CallResult.CreateFailedResult<List<string>>($"Exception in {nameof(GetListOfGenerationFilesToProcess)}", ex));
            }
        }

        [Function(nameof(ProcessZipFile))]
        public async Task<CallResult<string>> ProcessZipFile(
            [ActivityTrigger] AcmeProcessGenerationContext ctx)
        {
            try
            {
                Log.LogInformation($"START: {nameof(ProcessZipFile)}");
              
                var blobContainerWrapper = GetBlobContainerWrapper(ctx.BlobConfigInfo);

                var zipFileName = $"{Path.GetFileNameWithoutExtension(ctx.ZipFile)}";
                
                var projectId = AcmeProcessGenerationHelpers.GetProjectIdFromFileName(zipFileName);
                var weatherYear = AcmeProcessGenerationHelpers.GetWeatherYearFromFileName(zipFileName);
                var projectName = AcmeProcessGenerationHelpers.GetProjectNameFromFileName(zipFileName);

                var csvFileName = $"{projectId}_{projectName}_{weatherYear}.csv";

                // Normalizes the path.
                var extractTargetPath = $"{AcmeHelpers.FileSpecRootPath}{AcmeHelpers.ProcessedPath}";
                var targetFileName = $"{extractTargetPath}{csvFileName}";
                var zipFileByteArray = await blobContainerWrapper.DownloadBytesAsync(ctx.ZipFile);

                if (zipFileByteArray.Length < 1)
                {
                    throw new Exception($"Empty Zip File: {ctx.ZipFile}");
                }

                using (var zipFileStream = new MemoryStream(zipFileByteArray))
                using (ZipArchive archive = new ZipArchive(zipFileStream))
                {
                    // Empty Zip File
                    if (archive.Entries.Count == 0)
                    {
                        throw new Exception($"Zip Archive ({ctx.ZipFile}) is empty");
                    }
                    
                    // if there is > 1 generation files in this archive, it means Acme may have processed > 1 weather year and returned them all
                    // in the same zip file.  This can happen if we ask for multiple weather years in a single acquire request.  We currently do NOT
                    // support > 1 weather year per request so we have to error out here
                    if (archive.Entries.Count(e => e.Name.StartsWithIgnoreCase(AcmeProcessGenerationHelpers.AcmeGenFileStartsWith)) > 1)
                    {
                        throw new Exception($"Zip Archive ({ctx.ZipFile}) contains > 1 files that start with: {AcmeProcessGenerationHelpers.AcmeGenFileStartsWith}");
                    }

                    // Same thing with the Template.  We only expect one per zip file
                    if (archive.Entries.Count(e => e.Name.StartsWithIgnoreCase(AcmeProcessGenerationHelpers.AcmeMasterTemplateFileStartsWith)) > 1)
                    {
                        throw new Exception($"Zip Archive ({ctx.ZipFile}) contains > 1 Template file that starts with: {AcmeProcessGenerationHelpers.AcmeMasterTemplateFileStartsWith}");
                    }

                    // Get the timezone out of the template file so we can convert the local time to utc
                    var masterTemplateEntry = archive.Entries
                        .Where(e => e.Name.StartsWithIgnoreCase(AcmeProcessGenerationHelpers.AcmeMasterTemplateFileStartsWith))
                        .FirstOrDefault();

                    string timeZoneAbbreviation = null;

                    using (var masterTemplateStream = new StreamReader(masterTemplateEntry.Open()))
                    using (var csv = new CsvReader(masterTemplateStream, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<AcmeGenMasterTemplateMap>();
                        var templateRecords = csv.GetRecords<AcmeGenMasterTemplate>().ToList();
                        timeZoneAbbreviation = templateRecords.FirstOrDefault()?.TimeZone;

                        if (!AcmeHelpers.UtcOffsetMap.ContainsKey(timeZoneAbbreviation))
                        {
                            throw new Exception($"Timezone ({timeZoneAbbreviation}) not found in UtcOffsetMap");
                        }
                    }

                    var utcOffset = AcmeHelpers.UtcOffsetMap[timeZoneAbbreviation];

                    // Get the generation file 
                    var entry = archive.Entries
                        .Where(e => e.Name.StartsWithIgnoreCase(AcmeProcessGenerationHelpers.AcmeGenFileStartsWith))
                        .FirstOrDefault();

                    var destList = new List<AcmeGenProcessed>();

                    // 1. Read/Stream in the contents of the csv file in the zip archive
                    using (var sr = new StreamReader(entry.Open()))
                    using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
                    {
                        csv.Context.RegisterClassMap<AcmeGenProcessedMap>();

                        var sourceRecords = csv.GetRecords<AcmeGenProcessed>().AsList();

                        // 2. Add new columns to the csv file we read in
                        foreach (var sourceRecord in sourceRecords)
                        {
                            sourceRecord.Source = "Acme";
                            sourceRecord.ProjectId = projectId;
                            sourceRecord.WeatherYear = weatherYear;

                            // ** Convert the local time to UTC **
                            // NOTE: Acme originally sent us dates in UTC and we stored them as such in our DataWarehouse. The newer version of the Acme API
                            // now sends dates in as localtime.  To avoid having to change any processing/fields in the DW, we will convert here and save the UTC to the Dt 
                            // field the DW is looking at.  We will also save the original, local date/time in the LocalDt field
                            sourceRecord.LocalDt = sourceRecord.Dt;

                            sourceRecord.Dt =
                                DateTime.Parse(sourceRecord.LocalDt)
                                .AddHours(utcOffset)
                                .ToString(AcmeHelpers.AcmeTimeStampFormat);

                            destList.Add(sourceRecord);
                        }
                    }

                    // 3. Write out the new csv file that contains the additional columns
                    using (var memoryStream = new MemoryStream())
                    using (var streamWriter = new StreamWriter(memoryStream))
                    using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(destList);
                        csv.Flush();
                        streamWriter.Flush();
                        var csvBytes = memoryStream.ToArray();
                        await blobContainerWrapper.UploadResult(targetFileName, csvBytes);
                    }

                    return CallResult.CreateSuccessResult(targetFileName);
                }
            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<string>($"Exception in {nameof(ProcessZipFile)}: {ex.Message}", ex);
            }
            finally
            {
                Log.LogInformation($"END: {nameof(ProcessZipFile)}");
            }
        }

        [Function(nameof(MoveZipFilesToRawInputFolder))]
        public async Task<CallResult<string>> MoveZipFilesToRawInputFolder(
            [ActivityTrigger] AcmeProcessGenerationContext ctx)
        {
            try
            {
                var retVal = new List<string>();

                var blobContainerWrapper = GetBlobContainerWrapper(ctx.BlobConfigInfo);

                // Get the file to be moved
                var spec = $"{ctx.ZipFile}";
                var sourceBlobUriList = await blobContainerWrapper.GetBlobItemsAsync(spec);

                if (sourceBlobUriList.Count() == 0)
                {
                    // not really an error condition - there are simply no files to process at this time, move on...
                    var diagMsg = $"tid={Thread.CurrentThread.ManagedThreadId} [{DateTime.Now}] No Files found for processing at: {blobContainerWrapper.GetAbsoluteUri()}/{AcmeHelpers.ToBeProcessedPath}";
                    Log.LogInformation(diagMsg);
                    return new CallResult<string>()
                    {
                        Success = true,
                        DisplayMessage = diagMsg,
                    };
                }

                // Need a BlobContainerClient object with which to do copy/delete operations
                var containerClient = blobContainerWrapper.GetBlobContainerClient();

                // Copy files to new target location
                await Task.WhenAll(sourceBlobUriList.Select(async blob =>
                {
                    var targetBlobClient = containerClient.GetBlockBlobClient($"{AcmeHelpers.FileSpecRootPath}{AcmeHelpers.AcquiredPath}{blob.Uri.Segments.Last().UrlDecode()}");
                    var copyOp = await targetBlobClient.StartCopyFromUriAsync(blob.Uri);
                    return await copyOp.WaitForCompletionAsync();
                }).ToArray());

                // Delete source from ToBeProcessed once the copy is complete
                await Task.WhenAll(sourceBlobUriList.Select(async blob =>
                {
                    var targetBlobClient = containerClient.GetBlockBlobClient($"{AcmeHelpers.FileSpecRootPath}{AcmeHelpers.ToBeProcessedPath}{blob.Uri.Segments.Last().UrlDecode()}");
                    return await targetBlobClient.DeleteIfExistsAsync();
                }).ToArray());

                return CallResult.CreateSuccessResult(nameof(MoveZipFilesToRawInputFolder));

            }
            catch (Exception ex)
            {
                return CallResult.CreateFailedResult<string>($"Exception in {nameof(MoveZipFilesToRawInputFolder)}", ex);
            }
        }
    }
}