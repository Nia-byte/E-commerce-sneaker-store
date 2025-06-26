using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;




namespace WEBREATILFUNCTION2
{
    public class NotificationFunction
    {
        private readonly ILogger<NotificationFunction> _logger;

        public NotificationFunction(ILogger<NotificationFunction> logger)
        {
            _logger = logger;
        }

        [Function("SendNotification")]
        public static async Task<IActionResult> SendNotification(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "send-notification")] HttpRequest req,
           ILogger log)
        {
            try
            {
                // Read the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var notificationData = JsonConvert.DeserializeObject<NotificationMessage>(requestBody);

                // Retrieve the connection string from environment variables
                string storageConnectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString");

                if (string.IsNullOrEmpty(storageConnectionString))
                {
                    log.LogError("Azure storage connection string is missing or empty.");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                // Initialize QueueClient for the "notification-queue"
                QueueClient queueClient = new QueueClient(storageConnectionString, "notification-queue");

                // Ensure the queue exists
                await queueClient.CreateIfNotExistsAsync();

                // Serialize the notification message and send it to the queue
                string notificationMessage = JsonConvert.SerializeObject(notificationData);
                await queueClient.SendMessageAsync(notificationMessage);

                log.LogInformation($"Notification sent to queue: {notificationMessage}");
                return new OkObjectResult($"Notification sent: {notificationMessage}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to send notification: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }

    // Define the NotificationMessage class based on the structure of your notification data
    public class NotificationMessage
    {
        public int UserId { get; set; }
        public string Status { get; set; }
    }

}
