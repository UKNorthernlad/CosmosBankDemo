using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Core;

namespace CosmosDemo1
{
    class Program
    {
        private const string EndpointUrl = "https://xxxxx.documents.azure.com:443/";
        private const string PrimaryKey = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        private const string DbId = "Bank01";
        private const string ColId = "CurrentAccounts";
        private const string KeyPath = "/accountNumber";
        private const int itemCount = 1000;

        static async Task Main(string[] args)
        {
            await CreateDatabase();
            await CreateCollection();
            await CreateItems();
        }

        static async Task CreateDatabase()
        {
            using (var client = new CosmosClient(EndpointUrl, PrimaryKey))
            {
                var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(DbId);
                Console.WriteLine(databaseResponse.StatusCode.ToString());
            }
        }

        static async Task CreateCollection()
        {
            using (var client = new CosmosClient(EndpointUrl, PrimaryKey))
            {
                var database = client.GetDatabase(DbId);
                var collectionResponse = await database.CreateContainerIfNotExistsAsync(ColId, KeyPath, 5000);
                Console.WriteLine(collectionResponse.StatusCode.ToString());
            }
        }

        // TODO: Add more metrics - https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query-metrics-performance
        static async Task CreateItems()
        {
            UserBankAccount account = new UserBankAccount
            {
                Id = Guid.NewGuid().ToString(),
                accountNumber = "123456",
                Balance = "1000",
                UpdateTime = DateTime.Now
            };

            var client = new CosmosClient(EndpointUrl, PrimaryKey);

            var database = client.GetDatabase(DbId);
            var container = database.GetContainer(ColId);

            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
            var startTime = DateTime.Now;
            Console.WriteLine("Start time: {0:H:mm:ss:fff zzz}", startTime);

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < itemCount; i++)
            {
                account.Id = Guid.NewGuid().ToString();
                tasks.Add(container.CreateItemAsync(account));
            }

            await Task.WhenAll(tasks);
            var endTime = DateTime.Now;
            Console.WriteLine("End time:   {0:H:mm:ss:fff zzz}", endTime);

            foreach (var task in tasks)
            {
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("Warning: Failed to insert item. Error: {0}", task.Exception);
                }
            }

            var actionTime = endTime - startTime;
            Console.WriteLine("Done. Written {0} items. Time difference was {1} milliseconds.", itemCount, actionTime.TotalMilliseconds);
            Console.ReadKey();
        }
    }

}




