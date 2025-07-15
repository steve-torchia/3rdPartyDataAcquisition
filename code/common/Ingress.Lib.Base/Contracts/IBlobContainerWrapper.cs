using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ingress.Lib.Base.Contracts
{
    public interface IBlobContainerWrapper
    {
        Task<byte[]> DownloadBytesAsync(string name);
        Task<string> DownloadStringAsync(string name);
        string GetAbsoluteUri();
        BlobClient GetBlobClient(string fileName);
        string GetBlobContainerName();
        Task<IEnumerable<BlobItem>> GetBlobItemsAsync(string prefix, bool useFlatBlobListing = false);
        Task UploadResult(Dictionary<string, byte[]> batch);
        Task UploadResult(string fileName, byte[] bytes);
        Task UploadResult(string fileName, Stream stream);

        BlobServiceClient GetBlobServiceClient();
        BlobContainerClient GetBlobContainerClient();
        public Task<bool> DeleteIfExistsAsync(string filePath);

        public Task<IEnumerable<T>> ReadCsvFile<T>(string filepath);
        public void WriteCsvFile<T>(IEnumerable<T> list, string filepath);
    }
}