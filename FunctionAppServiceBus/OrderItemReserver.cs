using System;
using System.Configuration;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionAppServiceBus
{
    public static class OrderItemReserver
    {
        [FunctionName("OrderItemReserver")]
        public static void Run([ServiceBusTrigger("orders", Connection = "Connection")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            var conn = "DefaultEndpointsProtocol=https;AccountName=eshopblobstorage;AccountKey=bULwx5X7kuo8Zh0QejBhqDW771sY/oFUv7eFLIEXdpbIsqrw0PLwU87nfn9rc2lUhqhzoSdoRuDAEJV+06gEnQ==;EndpointSuffix=core.windows.net";
            var name = Guid.NewGuid().ToString();

            BlobServiceClient blobServiceClient = new BlobServiceClient(conn);

            // Get the container (folder) the file will be saved in
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("orders");

            // Get the Blob Client used to interact with (including create) the blob

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient($"{name}.json");

            //Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);
            var data = new BinaryData(myQueueItem);
            // Upload data from the local file
            blobClient.Upload(data);
        }
    }
}
