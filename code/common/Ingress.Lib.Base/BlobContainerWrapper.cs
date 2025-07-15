using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Ingress.Lib.Base.Contracts;

namespace Ingress.Lib.Base
{
    public class BlobContainerWrapper : IBlobContainerWrapper
    {
        private BlobContainerClient BlobContainerClient { get; set; }

        private string FilePathStart { get; set; }

        private string FilePathEnd { get; set; }

        public IBlobConfigInfo BlobConfigInfo { get; }

        public BlobContainerWrapper(IBlobConfigInfo blobConfigInfo, string pathStart = null, string pathEnd = null)
        {
            this.BlobContainerClient = new BlobContainerClient(blobConfigInfo.ConnectionString, blobConfigInfo.ContainerName);

            this.FilePathStart = string.IsNullOrEmpty(pathStart) ? pathStart : pathStart + "/";
            this.FilePathEnd = pathEnd;

            this.BlobConfigInfo = blobConfigInfo;
        }

        public BlobServiceClient GetBlobServiceClient()
        {
            return new BlobServiceClient(this.BlobConfigInfo.ConnectionString);
        }

        public BlobContainerClient GetBlobContainerClient()
        {
            return this.BlobContainerClient;
        }

        public string GetBlobContainerName()
        {
            return this.BlobContainerClient.Name;
        }

        public string GetAbsoluteUri()
        {
            return this.BlobContainerClient.Uri.AbsoluteUri;
        }

        public async Task<IEnumerable<BlobItem>> GetBlobItemsAsync(string prefix, bool useFlatBlobListing = false)
        {
            var resultSegment = this.BlobContainerClient.GetBlobsAsync(prefix: prefix).AsPages();

            var blobItems = new List<BlobItem>();
            await foreach (var blobPage in resultSegment)
            {
                blobItems.AddRange(blobPage.Values);
            }

            return blobItems;
        }

        public BlobClient GetBlobClient(string fileName)
        {
            return this.BlobContainerClient.GetBlobClient(fileName);
        }

        /// <summary>
        /// Uploads multiple byte arrays concurrently to the blob storage container
        /// </summary>
        /// <param name="batch">The set of byte arrays to upload. The key is the blobName</param>
        public async Task UploadResult(Dictionary<string, byte[]> batch)
        {
            // The azure blob storage client we are using does not actually have support for batch uploading.
            // Instead, I'll use tasks with throttling to make sure I don't create an absurd number of tasks

            var uploadTasks = new List<Func<Task>>();

            foreach (var kv in batch)
            {
                uploadTasks.Add(() => this.UploadResult(kv.Key, kv.Value));
            }

            // number represents the number of parallel "in-flight" upload tasks to allow.
            // This seems to work for a uploading many small files at once. Room for improvement
            await ThrottledTask.WhenAll(uploadTasks, 200);
        }

        public async Task UploadResult(string fileName, Stream stream)
        {
            var filePath = this.GetRawFilePath(fileName);
            var blobClient = this.BlobContainerClient.GetBlobClient(filePath);

            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task UploadResult(string fileName, byte[] bytes)
        {
            var filePath = this.GetRawFilePath(fileName);
            var blobClient = this.BlobContainerClient.GetBlobClient(filePath);

            using (var stream = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
        }

        public async Task<byte[]> DownloadBytesAsync(string name)
        {
            var blobClient = this.BlobContainerClient.GetBlobClient(name);
            var blobDownloadInfo = await blobClient.DownloadAsync();

            using (var memoryStream = new MemoryStream())
            {
                await blobDownloadInfo.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<string> DownloadStringAsync(string filePath)
        {
            var blobClient = this.BlobContainerClient.GetBlobClient(filePath);
            var blobDownloadInfo = await blobClient.DownloadAsync();

            using (var memoryStream = new MemoryStream())
            {
                await blobDownloadInfo.Value.Content.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public async Task<bool> DeleteIfExistsAsync(string filePath)
        {
            var blobClient = this.BlobContainerClient.GetBlobClient(filePath);
            return await blobClient.DeleteIfExistsAsync();
        }

        public async void WriteCsvFile<T>(IEnumerable<T> list, string filepath)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(list);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                await this.UploadResult(filepath, ms);
            }
        }

        public async Task<IEnumerable<T>> ReadCsvFile<T>(string filepath)
        {
            using (var ms = new MemoryStream())
            {
                var blobDownloadInfo = await this.GetBlobClient(filepath).DownloadAsync();
                await blobDownloadInfo.Value.Content.CopyToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<T>().ToList();
                }
            }
        }

        public async Task<IEnumerable<T>> ReadCsvFileWithMap<T, TMap>(string filepath)
            where TMap : ClassMap
        {
            using (var ms = new MemoryStream())
            {
                var blobDownloadInfo = await this.GetBlobClient(filepath).DownloadAsync();
                await blobDownloadInfo.Value.Content.CopyToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<TMap>();
                    return csv.GetRecords<T>().ToList();
                }
            }
        }

        private string GetRawFilePath(string fileName, string fileExtension)
        {
            var path = $"{this.FilePathStart}{fileName}{this.FilePathEnd}{fileExtension}";
            return path;
        }

        private string GetRawFilePath(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);

            _ = string.IsNullOrEmpty(fileExtension) ?
                throw new Exception($"Didn't find extension in fileName:{fileName}") :
                true;

            fileName = fileName.Replace(fileExtension, string.Empty);
            return this.GetRawFilePath(fileName, fileExtension);
        }
    }
}
