using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Azure;



namespace WEBREATILFUNCTION2
{
    public class AddProductFunstion
    {
        [Function("AddProduct")]
        public static async Task<IActionResult> Run(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
         ILogger log)
        {
            log.LogInformation("Processing a request to add a product.");

            // Retrieve data from the form
            var productName = req.Form["productName"];
            var brand = req.Form["brand"];
            var colour = req.Form["colour"];
            var size = req.Form["size"];
            var productPrice = req.Form["productPrice"];
            var productImage = req.Form.Files["productImage"];

            // Upload image to Blob Storage
            var blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net");
            var containerClient = blobServiceClient.GetBlobContainerClient("productimages");
            var blobClient = containerClient.GetBlobClient(productImage.FileName);

            using (var stream = productImage.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Save product details in Table Storage
            var tableServiceClient = new TableServiceClient("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net");
            var tableClient = tableServiceClient.GetTableClient("Products");

            var productEntity = new ProductEntity
            {
                PartitionKey = "Products",
                RowKey = Guid.NewGuid().ToString(),
                Name = productName,
                Brand = brand,
                Colour = colour,
                Size = size,
                Price = decimal.Parse(productPrice),
                ImageUrl = blobClient.Uri.ToString()
            };

            await tableClient.AddEntityAsync(productEntity);

            return new OkObjectResult(new { message = "Product added successfully!" });
        }

        public class ProductEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Name { get; set; }
            public string Brand { get; set; }

            public string Colour { get; set; }

            public string Size { get; set; }
            public decimal Price { get; set; }
            public string ImageUrl { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }
    }
}