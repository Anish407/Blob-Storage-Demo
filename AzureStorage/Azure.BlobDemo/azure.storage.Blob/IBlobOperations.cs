using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace azure.storage.Blob
{
    public interface IBlobOperations
    {
        Task UploadToBlob(IFormFile asset);
        Task<List<BlobModel>>  ListAllBlobs();

        Task<(Stream content, string contentType, string name)> DownloadFile(string fileName);

        Task<bool> DeleteBlobAsync(string fileName);

        Task<(string rating, string author)> GetBlobMetaData(string blobName);

        Task RemoveMetaData(string blobName, string keyName);

        Task<List<CloudBlockBlob>> DisplaySoftDeletedBlobs();

        Task CreateMetaData(string blobName, string keyName, string keyValue);
    }
}