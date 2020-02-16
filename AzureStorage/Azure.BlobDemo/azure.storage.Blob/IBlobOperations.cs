using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    }
}