using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace WEBREATILFUNCTION2
{
    public class BlobFunction
    {
        [Function("UploadBlob")]
        public static async Task<IActionResult> RunUpload(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
         ILogger log)
        {
            string containerName = "sample-container";
            string blobName = req.Query["blobName"];

            if (string.IsNullOrEmpty(blobName))
            {
                return new BadRequestObjectResult("Please provide a blob name.");
            }

            BlobServiceClient blobServiceClient = new BlobServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = new MemoryStream())
            {
                await req.Body.CopyToAsync(stream);
                stream.Position = 0;
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return new OkObjectResult($"Blob {blobName} uploaded to container {containerName}.");
        }

        [Function("DownloadBlob")]
        public static async Task<IActionResult> RunDownload(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string containerName = "sample-container";
            string blobName = req.Query["blobName"];

            if (string.IsNullOrEmpty(blobName))
            {
                return new BadRequestObjectResult("Please provide a blob name.");
            }

            BlobServiceClient blobServiceClient = new BlobServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var blobDownloadInfo = await blobClient.DownloadAsync();

            return new FileStreamResult(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType)
            {
                FileDownloadName = blobName
            };
        }
    }
}