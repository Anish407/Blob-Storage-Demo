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

        [HttpGet("GetAllBlobs")]
        public async Task<IActionResult> GetAllBlobs()
        {
            try
            {
                var result= await BlobOperations.ListAllBlobs();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("DownloadBlob/{name}")]
        public async Task<IActionResult> DownloadBlob(string name)
        {
            try
            {
                var result = await BlobOperations.DownloadFile(name);
                return File(result.content,result.contentType,result.name);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("DeleteBlob/{name}")]
        public async Task<IActionResult> DeleteBlob(string name)
        {
            try
            {
                var result = await BlobOperations.DeleteBlobAsync(name);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}