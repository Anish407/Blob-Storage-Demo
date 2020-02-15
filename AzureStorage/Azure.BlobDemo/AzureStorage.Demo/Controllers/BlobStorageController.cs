using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using azure.storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace BlobDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobStorageController : ControllerBase
    {
        public BlobStorageController(IBlobOperations blobOperations)
        {
            BlobOperations = blobOperations;
        }

        public IBlobOperations BlobOperations { get; }

        [HttpPost("InsertFile")]
        public async Task<IActionResult> InsertFile(IFormFile asset)
        {
            try
            {
                await BlobOperations.UploadToBlob(asset);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
    }
}