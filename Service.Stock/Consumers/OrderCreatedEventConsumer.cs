using System.Text.Json;
using Couchbase;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;
using MassTransit;
using Service.Stock.Entities;
using Shared.Events;

namespace Service.Stock.Consumers;

public class OrderCreatedEventConsumer(ICluster cluster): IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        try
        {
            var bucket = await cluster.BucketAsync("lothal");
            var scope = await bucket.ScopeAsync("order");
            var collection = await scope.CollectionAsync("inbox");

            if (collection == null)
            {
                throw new Exception("Couchbase collection not found.");
            }
            
            var result = await TryGetAndLockAsync(collection,context.Message.IdempotentToken.ToString(),TimeSpan.FromSeconds(30));
            if (result == null)
            {
                var inboxOrder = new OrderInbox<OrderCreatedEvent>
                {
                    Processed = false,
                    Payload = context.Message,
                    IdempotentToken = context.Message.IdempotentToken
                };
                await collection.InsertAsync(inboxOrder.IdempotentToken.ToString(),inboxOrder);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    private static async Task<IGetResult?> TryGetAndLockAsync(ICouchbaseCollection collection, string docId, TimeSpan lockTime)
    {
        IGetResult? result = null;
        try
        {
            result = await collection.GetAndLockAsync(docId, lockTime);
            return result;
        }
        catch (DocumentNotFoundException)
        {
            Console.WriteLine($"Document not found: {docId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error while locking document {docId}: {ex.Message}");
        }
        return result;
    }
}