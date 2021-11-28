using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Text;

namespace CosmosOrder
{
    public static class DelveryOrderProcessor
    {
        public class Order
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            [JsonProperty(PropertyName = "partitionKey")]
            public string PartitionKey { get; set; }
            public IFormCollection Details { get; set; }
            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        [FunctionName("DelveryOrderProcessor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var conn = "A3JrWZ17p7cN0pPuA7JTlVVZsDpJoFl7kYooM8nA0e9YRVXwFPfDTguB6xj50tZC7boQYBFKeIN5Ivyktg9M3A==";

            CosmosClient cosmosClient = new CosmosClient(@"https://eshopcosmosfinal.documents.azure.com:443/", conn, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });

            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("Shop");
            Container container = await database.CreateContainerIfNotExistsAsync("Orders", "/partitionKey");

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                Details = req.Form
            };

            ItemResponse<Order> andersenFamilyResponse = await container.CreateItemAsync(order, new PartitionKey(order.PartitionKey));

            return new OkObjectResult(andersenFamilyResponse);
        }
    }
}
