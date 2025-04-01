using System.Text.Json;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;
using MassTransit;
using Quartz;
using Service.Order.Publisher.Entities;
using Shared.Events;

namespace Service.Order.Publisher.Jobs;

public class OrderOutboxPublishJob(IPublishEndpoint publishEndpoint, ICluster cluster) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var bucket = await cluster.BucketAsync("lothal");
            var scope = bucket.Scope("order");
            var collection = scope.Collection("outbox");

            if (collection == null)
            {
                throw new Exception("Couchbase collection not found.");
            }

            var selectQuery = "SELECT META(o).id AS docId, o.* FROM `lothal`.`order`.`outbox` o WHERE o.processedDate IS NULL";
            var result = await cluster.QueryAsync<dynamic>(selectQuery);

            await foreach (var row in result) 
            {
                string docId = row.docId;
                var existsResult = await collection.ExistsAsync(docId);
                if (!existsResult.Exists)
                {
                    Console.WriteLine($"Document {docId} does not exist. Skipping...");
                    continue;
                }

                var getResult = await collection.GetAndLockAsync(docId, TimeSpan.FromSeconds(30));
                var doc = getResult.ContentAs<OrderOutbox<OrderCreatedEvent>>();

                try
                {
                    await publishEndpoint.Publish(doc.Payload);

                    doc.ProcessedDate = DateTimeOffset.UtcNow.DateTime;
                    var replaceOptions = new ReplaceOptions().Cas(getResult.Cas);
                    await collection.ReplaceAsync(docId, doc, replaceOptions);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {docId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}