using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Azure.Functions.Worker;

namespace WEBREATILFUNCTION2
{
    public class TableFunction
    {
        [Function("InsertToTableStorage")]
        public static async Task<IActionResult> InsertToTable(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
         ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<CustomerEntity>(requestBody);

            TableServiceClient tableServiceClient = new TableServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net"));
            TableClient tableClient = tableServiceClient.GetTableClient("customers");

            await tableClient.CreateIfNotExistsAsync();
            await tableClient.AddEntityAsync(customer);

            return new OkObjectResult($"Customer with RowKey: {customer.RowKey} added.");
        }

        [Function("RetrieveFromTableStorage")]
        public static async Task<IActionResult> RetrieveFromTable(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string partitionKey = req.Query["partitionKey"];
            string rowKey = req.Query["rowKey"];

            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                return new BadRequestObjectResult("Please provide both partitionKey and rowKey.");
            }

            TableServiceClient tableServiceClient = new TableServiceClient(System.Environment.GetEnvironmentVariable("DefaultEndpointsProtocol=https;AccountName=abcretailstoragestor;AccountKey=Nwcr0R/73xn13EBXWdBfYKGyaxNJarMAjj8KZr4QwV6gvORG5QYxXYcvFXLX1qQ573Q+D5+TAfUU+AStgBaCXQ==;EndpointSuffix=core.windows.net"));
            TableClient tableClient = tableServiceClient.GetTableClient("customers");

            var customer = await tableClient.GetEntityAsync<CustomerEntity>(partitionKey, rowKey);

            return new OkObjectResult(customer);
        }
    }

    public class CustomerEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}