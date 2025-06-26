using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Azure;
using Newtonsoft.Json;


namespace WEBREATILFUNCTION2
{
    public class AddToCartFunction
    {
        [Function("addToCart")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("Processing a request to add an item to the cart.");

            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var cartItem = JsonConvert.DeserializeObject<CartItem>(requestBody);

            // Save cart item in Table Storage
            var tableServiceClient = new TableServiceClient("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net");
            var tableClient = tableServiceClient.GetTableClient("Cart");

            var cartEntity = new CartEntity
            {
                PartitionKey = "Cart",
                RowKey = cartItem.Id, // Use Product ID as RowKey for simplicity
                Name = cartItem.Name,
                Price = decimal.Parse(cartItem.Price),
                Quantity = 1 // You can modify this based on your requirements
            };

            await tableClient.AddEntityAsync(cartEntity);

            return new OkObjectResult(new { message = "Item added to cart successfully!" });
        }

        public class CartItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Price { get; set; }
            public int Quantity { get; set; }
        }

        public class CartEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }
    }
}