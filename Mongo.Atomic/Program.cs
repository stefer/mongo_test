using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using System.Diagnostics;

namespace Mongo.Atomic
{
    /// <summary>
    /// Document in db should look like this:
    ///   {"_id":"5be33e007dc32338648300bc","name":"counter1","value":12,"max":1200}
    /// Test by starting multiple processes using powershell command:
    ///  $(start -NoNewWindow -PassThru dotnet run ;  start -NoNewWindow -PassThru dotnet run ; start -NoNewWindow -PassThru dotnet run ; start -NoNewWindow -PassThru dotnet run ) | Wait-Process
    /// </summary>

    class Program
    {
        static async Task Main(string[] args)
        {
            Process currentProcess = Process.GetCurrentProcess();
            string pid = currentProcess.Id.ToString();

            string connectionString =
                @"mongodb://localhost:27017";
            MongoClientSettings settings = MongoClientSettings.FromUrl(
                new MongoUrl(connectionString));
            //settings.SslSettings = 
            //    new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var db = mongoClient.GetDatabase("stefane");

            var collection = db.GetCollection<Counter>("counters");

            for (int i = 0; i < 10; i++)
            {
                await IncreaseRetryAsync(collection, pid);
            }
        }

        private static async Task IncreaseRetryAsync(IMongoCollection<Counter> collection, string pid)
        {
            do
            {
                Console.WriteLine($"{pid} - Getting counter");

                var counter = await collection.Find(c => c.Name == "counter1").SingleAsync();

                if (counter.Value >= counter.Max)
                {
                    throw new InvalidOperationException("Counter max value encountered");
                }

                Console.WriteLine($"{pid} -   Counter is: {counter.Value}");

                var updated = await collection.UpdateOneAsync(
                    c => c.Name == "counter1" && c.Value == counter.Value,
                    Builders<Counter>.Update.Inc(c => c.Value, 1));

                if (updated.IsModifiedCountAvailable && updated.ModifiedCount > 0)
                {
                    Console.WriteLine($"{pid} -   Set to {counter.Value + 1}");
                    break;
                }
                else
                {
                    Console.Error.WriteLine($"{pid} -   Retry : {counter.Value}");
                }

            } while (true);
        }
    }
}
