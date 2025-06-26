using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace WEBREATILFUNCTION2
{
    public class QueueFunction
    {
        // Function to send order messages to the queue
        [Function("ProcessOrder")]
        public static async Task<IActionResult> ProcessOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                // Read request body and deserialize
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var orderDetails = JsonConvert.DeserializeObject<OrderDetails>(requestBody); // Assuming OrderDetails is a class you define

                // Get the Azure Storage connection string from environment variables
                string storageConnectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");
                if (string.IsNullOrEmpty(storageConnectionString))
                {
                    log.LogError("Azure storage connection string is missing or empty.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Create the Queue client
                QueueServiceClient queueServiceClient = new QueueServiceClient(storageConnectionString);
                QueueClient queueClient = queueServiceClient.GetQueueClient("order-processing-queue");

                // Ensure the queue exists
                await queueClient.CreateIfNotExistsAsync();

                // Serialize order details to JSON string
                string messageBody = JsonConvert.SerializeObject(orderDetails);

                // Send the order details to the queue
                await queueClient.SendMessageAsync(messageBody);

                log.LogInformation($"Order processed and sent to queue: {messageBody}");

                return new OkObjectResult($"Order processed: {messageBody}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing order: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

       
        // Function to receive messages from the order queue
        [Function("ReceiveOrderMessage")]
        public static async Task<IActionResult> ReceiveOrderMessage(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string storageConnectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");
                if (string.IsNullOrEmpty(storageConnectionString))
                {
                    log.LogError("Azure storage connection string is missing or empty.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Create the Queue client for the order-processing queue
                QueueServiceClient queueServiceClient = new QueueServiceClient(storageConnectionString);
                QueueClient queueClient = queueServiceClient.GetQueueClient("order-processing-queue");

                // Receive a message from the order queue
                var receivedMessage = await queueClient.ReceiveMessageAsync();

                if (receivedMessage == null || receivedMessage.Value.MessageText == null)
                {
                    return new OkObjectResult("No messages in the order queue.");
                }

                log.LogInformation($"Received order message: {receivedMessage.Value.MessageText}");

                // Optionally delete the message after processing
                await queueClient.DeleteMessageAsync(receivedMessage.Value.MessageId, receivedMessage.Value.PopReceipt);

                return new OkObjectResult($"Received order message: {receivedMessage.Value.MessageText}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error receiving order message: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        // Function to receive messages from the notification queue
        [Function("ReceiveNotificationMessage")]
        public static async Task<HttpResponseData> ReceiveNotificationMessage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "receive-notification")] HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger("ReceiveNotificationMessage");
            string responseMessage;

            try
            {
                // Parse the incoming JSON as needed
                var requestBody = await req.ReadAsStringAsync();
                var formData = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);

                // Extract form fields
                string name = formData.ContainsKey("name") ? formData["name"] : null;
                string contactReason = formData.ContainsKey("contactReason") ? formData["contactReason"] : null;

                // Validate required fields
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(contactReason))
                {
                    responseMessage = "Invalid message payload: 'name' and 'contactReason' are required.";
                    log.LogWarning(responseMessage);
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badResponse.WriteString(responseMessage);
                    return badResponse;
                }

                // Log the message has been delivered
                responseMessage = $"Message from {name} with contact reason '{contactReason}' has been delivered.";
                log.LogInformation(responseMessage);

                // Respond with a success message
                var okResponse = req.CreateResponse(HttpStatusCode.OK);
                okResponse.WriteString("Message delivered successfully.");
                return okResponse;
            }
            catch (Exception ex)
            {
                responseMessage = $"Error processing notification message: {ex.Message}";
                log.LogError(responseMessage);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.WriteString(responseMessage);
                return errorResponse;
            }


        }
    }

    // Assuming you have these classes for deserialization
    public class OrderDetails
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Item { get; set; }
    }

    
}
