using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace azure.storage.Blob
{
    public interface IBlobOperations
    {
        Task UploadToBlob(IFormFile asset);
    }
}