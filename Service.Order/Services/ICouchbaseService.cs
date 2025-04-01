using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Transactions;

namespace Service.Order.Services;

public interface ICouchbaseService
{
    Task<T> GetDocumentAsync<T>(string bucketName, string scopeName, string collectionName, string documentId);
    Task RemoveDocumentAsync(string bucketName, string scopeName, string collectionName, string documentId);
    Task UpsertDocumentAsync<T>(string bucketName, string scopeName, string collectionName, string documentId, T document);
    Task<ICouchbaseCollection> GetCollectionAsync(string bucketName, string scopeName, string collectionName);
    Transactions GetTransactions();

}