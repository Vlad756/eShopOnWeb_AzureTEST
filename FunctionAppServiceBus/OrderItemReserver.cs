using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionAppServiceBus
{
    public static class OrderItemReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task Run(
            [ServiceBusTrigger("orders", Connection = "Connection")] string myQueueItem,
            [Blob("orders/bus-{rand-guid}.json", FileAccess.Write, Connection = "BlobConnectionString")] Stream blob,
            ILogger log)
        {
            using var sw = new StreamWriter(blob);
            await sw.WriteAsync(myQueueItem);

            log.LogInformation($"OrderItemsReserver processed message: {myQueueItem}");
        }
    }
}
