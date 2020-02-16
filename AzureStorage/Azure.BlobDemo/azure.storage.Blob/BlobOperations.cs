using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.IO;

namespace azure.storage.Blob
{
    public class BlobOperations : IBlobOperations
    {

        public BlobOperations(IConfiguration configuration, ILogger<BlobOperations> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<BlobOperations> Logger { get; }

        public async Task UploadToBlob(IFormFile asset)
        {
            string containerName = "videos";
            CloudBlockBlob blockBlob = (await SetupContainer(containerName)).GetBlockBlobReference(asset.FileName);
            using var stream = asset.OpenReadStream();
            await blockBlob.UploadFromStreamAsync(stream); ;
        }

        private CloudBlobContainer GetContainerReference(string containerName)
        {
            var cnt = "DefaultEndpointsProtocol=https;AccountName=anishstoragedemo;AccountKey=KRohowEAfydP3O7KtPlXqlyoUPyPlJI5cp9dQ5cPpmkncnQpPNVDGszUH6p7FptXqn23D6/sRI1mKdtFwgX1qw==;EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cnt);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(containerName);
        }

        public async Task<List<BlobModel>> ListAllBlobs()
        {
            CloudBlobContainer blobContainer = await SetupContainer("videos");
            List<CloudBlockBlob> blobList = new List<CloudBlockBlob>();

            BlobContinuationToken continuationToken = null;

            // wen we use ListBlobsSegmentedAsync, we get blobs of type page, append and block.
            //Since we need only the block blobs we will filter them out.As shown below
            //Max 500 blobs will be returned .. then a continutation token will be returned
            // which shud be passed to get the next set of blobs.. the  currentToken passed above is  the 
            // continuation token
            //BlobResultSegment blobResultSegment = await blobContainer.ListBlobsSegmentedAsync(continuationToken);
            // var newToken = blobResultSegment.ContinuationToken; // use if needed
            // To get the first 5000 blobs
            //IEnumerable<CloudBlockBlob> blobList = blobResultSegment.Results.OfType<CloudBlockBlob>();




            //or loop till all blobs have been returned
            do
            {
                BlobResultSegment blobResultSegment = await blobContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = blobResultSegment.ContinuationToken;
                blobList.AddRange(blobResultSegment.Results.OfType<CloudBlockBlob>());
            } while (continuationToken != null);

            return blobList.Select(i => new BlobModel { ContainerName = i.Container.Name, Name = i.Name, Uri = i.Uri.AbsoluteUri }).ToList();
        }

        public async Task<(Stream, string, string)> DownloadFile(string fileName)
        {
            CloudBlockBlob file = (await SetupContainer("videos")).GetBlockBlobReference(fileName);
           
            if (!await file.ExistsAsync()) throw new Exception("file doesnt exist");

            //convert the file to a memory stream
            using var stream = new MemoryStream();
            await file.DownloadToStreamAsync(stream);
            Stream blobStream = await file.OpenReadAsync();
            return (blobStream, file.Properties.ContentType, file.Name);

        }

        public async Task<List<CloudBlockBlob>> DisplaySoftDeletedBlobs()
        {
            CloudBlobContainer blobContainer = await SetupContainer();
            List<CloudBlockBlob> blobList = new List<CloudBlockBlob>();

            BlobContinuationToken continuationToken = null;
            //use  blobListingDetails:BlobListingDetails.Deleted, to get soft deleted blobs
            BlobResultSegment blobResultSegment1 = await blobContainer
                .ListBlobsSegmentedAsync(
                 prefix: null,
                 useFlatBlobListing: true,
                 blobListingDetails: BlobListingDetails.Deleted,
                 null,
                 currentToken: null,
                 null,
                 null);

            blobList.AddRange(blobResultSegment1.Results.OfType<CloudBlockBlob>().Where(i => i.IsDeleted));
            return blobList;
        }

        public async Task<bool> DeleteBlobAsync(string fileName = "")
        {
            // Need to specify the extension also
            fileName ??= "tree-736885__340.jpg";
            var container = await SetupContainer("videos");
            var blobFileToDelete = container.GetBlockBlobReference(fileName);

            return await blobFileToDelete.DeleteIfExistsAsync();
        }

        //Download blobs using sarath's code.. using SPA.

        private async Task<CloudBlobContainer> SetupContainer(string containerName = "images")
        {
            // string containerName = "images";
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=anishstoragedemo;AccountKey=KRohowEAfydP3O7KtPlXqlyoUPyPlJI5cp9dQ5cPpmkncnQpPNVDGszUH6p7FptXqn23D6/sRI1mKdtFwgX1qw==;EndpointSuffix=core.windows.net");
            CloudBlobClient cloudeBlobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer blobContainer = cloudeBlobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;

        }
    }
}
