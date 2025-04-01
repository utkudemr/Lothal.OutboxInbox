using Couchbase;
using Couchbase.Client.Transactions.Config;
using Couchbase.KeyValue;
using Couchbase.Transactions;
using Couchbase.Transactions.Config;

namespace Service.Order.Services;

public class CouchbaseService(ICluster cluster) : ICouchbaseService
{
    public Transactions GetTransactions()
    {
        var transactionConfig = TransactionConfigBuilder.Create()
            .DurabilityLevel(DurabilityLevel.None) // Tek node çalışıyorsan None yapmalısın
            .Build();
        return Transactions.Create(cluster,transactionConfig);
    }
    public async Task UpsertDocumentAsync<T>(string bucketName, string scopeName, string collectionName, string documentId, T document)
    {
        var collection = await GetCollectionAsync(bucketName, scopeName, collectionName);
        await collection.UpsertAsync(documentId, document);
    }

    public async Task<T> GetDocumentAsync<T>(string bucketName, string scopeName, string collectionName, string documentId)
    {
        var collection = await GetCollectionAsync(bucketName, scopeName, collectionName);
        var result = await collection.GetAsync(documentId);
        return result.ContentAs<T>();
    }

    public async Task RemoveDocumentAsync(string bucketName, string scopeName, string collectionName, string documentId)
    {
        var collection = await GetCollectionAsync(bucketName, scopeName, collectionName);
        await collection.RemoveAsync(documentId);
    }

    private async Task<IBucket> GetBucketAsync(string bucketName)
    {
        var bucket = await cluster.BucketAsync(bucketName);
        return bucket;
    }

    private async Task<IScope> GetScopeAsync(string bucketName, string scopeName)
    {
        var bucket = await GetBucketAsync(bucketName);
        return bucket.Scope(scopeName);
    }

    public async Task<ICouchbaseCollection> GetCollectionAsync(string bucketName, string scopeName, string collectionName)
    {
        var scope = await GetScopeAsync(bucketName, scopeName);
        return await scope.CollectionAsync(collectionName);
    }
}