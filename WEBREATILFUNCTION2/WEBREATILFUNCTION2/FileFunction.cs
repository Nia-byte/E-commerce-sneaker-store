using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace WEBREATILFUNCTION2
{
    public class FileFunction
    {
        [Function("UploadFileToShare")]
        public static async Task<IActionResult> UploadFile(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
         ILogger log)
        {
            string shareName = "myfileshare";
            string fileName = req.Query["fileName"];

            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestObjectResult("Please provide a file name.");
            }

            ShareServiceClient shareServiceClient = new ShareServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.netg"));
            ShareClient shareClient = shareServiceClient.GetShareClient(shareName);

            await shareClient.CreateIfNotExistsAsync();

            ShareDirectoryClient directoryClient = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directoryClient.GetFileClient(fileName);

            using (var stream = new MemoryStream())
            {
                await req.Body.CopyToAsync(stream);
                stream.Position = 0;
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);
            }

            return new OkObjectResult($"File {fileName} uploaded to share {shareName}.");
        }

        [Function("DownloadFileFromShare")]
        public static async Task<IActionResult> DownloadFile(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string shareName = "myfileshare";
            string fileName = req.Query["fileName"];

            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestObjectResult("Please provide a file name.");
            }

            ShareServiceClient shareServiceClient = new ShareServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net"));
            ShareClient shareClient = shareServiceClient.GetShareClient(shareName);
            ShareDirectoryClient directoryClient = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directoryClient.GetFileClient(fileName);

            var downloadInfo = await fileClient.DownloadAsync();

            return new FileStreamResult(downloadInfo.Value.Content, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }
    }
}