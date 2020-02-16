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
        readonly string containerName = "videos";
        //Assume that our blob has 2 properties(metadata with author and rating as keys)
        readonly string keyName = "Author";
        readonly string keyName2 = "rating";
        public BlobOperations(IConfiguration configuration, ILogger<BlobOperations> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<BlobOperations> Logger { get; }

        public async Task UploadToBlob(IFormFile asset)
        {
            CloudBlockBlob blockBlob = (await SetupContainer(containerName)).GetBlockBlobReference(asset.FileName);
            using var stream = asset.OpenReadStream();
            await blockBlob.UploadFromStreamAsync(stream); ;
        }


        /// <summary>
        /// Add the meta data which is a keyalue pair to the blob and use SetMetadataAsync to save
        /// </summary>
        /// <param name="blobName">Blob to which the metadata is to be added</param>
        /// <param name="keyName">Key for the metadata dictionary</param>
        /// <param name="keyValue">Value to be saved</param>
        /// <returns></returns>
        public async Task CreateMetaData(string blobName, string keyName, string keyValue)
        {
            // update is also similar
            var blockBlob = (await SetupContainer(containerName)).GetBlockBlobReference(blobName);
            blockBlob.Metadata[keyName] = keyValue;
            await blockBlob.SetMetadataAsync();
        }

        /// <summary>
        /// Remove the metaData from the blob
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public async Task RemoveMetaData(string blobName, string keyName)
        {
            // update is also similar
            var blockBlob = (await SetupContainer(containerName)).GetBlockBlobReference(blobName);
            blockBlob.Metadata.Remove(keyName);
            await blockBlob.SetMetadataAsync();
        }

        /// <summary>
        /// Get MetaData by blob name.. Assume that we have 2 metadata that store Author and rating
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<(string rating, string author)> GetBlobMetaData(string blobName)
        {
            CloudBlockBlob blockBlob = (await SetupContainer(containerName)).GetBlockBlobReference(blobName);

            return (GetMetaDataByBlobName(blockBlob,keyName),
                    GetMetaDataByBlobName(blockBlob, keyName2));

        }

        private string GetMetaDataByBlobName(CloudBlockBlob blockBlob,string key)
        {
            return blockBlob.Metadata.ContainsKey(key) ? blockBlob.Metadata[key] : "no MetaData found";
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
            // use Pipe and list MetaData while fetching the blobs, along with the soft deleted blobs
            BlobResultSegment blobResultSegment1 = await blobContainer
                .ListBlobsSegmentedAsync(
                 prefix: null,
                 useFlatBlobListing: true,
                 blobListingDetails: BlobListingDetails.Deleted | BlobListingDetails.Metadata,
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
