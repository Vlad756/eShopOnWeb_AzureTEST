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
        [FunctionName("DelveryOrderProcessor")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        [CosmosDB(
                databaseName: "Shop",
                collectionName: "Orders",
                ConnectionStringSetting = "ConnectionStrings:CosmosDBConnectionString",
                CreateIfNotExists = true)] out dynamic collector,
        ILogger log)
        {
            var order = req.ReadAsStringAsync().Result;
            var document = new { Description = order, id = Guid.NewGuid() };

            collector = document;

            return new OkObjectResult("ok");
        }
    }
}
