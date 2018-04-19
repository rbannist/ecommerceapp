using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq.Expressions;
using System.Net;
using System.IO;

namespace SportsDirect
{
    public class CosmosDBClient
    {
        public static string CosmosDBURI { get; set; }
        public static string CosmosDBKey { get; set; }

        public static string CosmosDBDatabaseName { get; set; }
    }

    public static class CosmosDBClient<T> where T : class
    {
        private static DocumentClient client;

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(CosmosDBClient.CosmosDBURI), CosmosDBClient.CosmosDBKey);
        }

        //Display all users & their details where predicate (condition) is met
        public static async Task<IEnumerable<T>> GetItemsAsync(string collectionId, Expression<Func<T, bool>> predicate)
        {
            var CrossPartitionEnabled = new FeedOptions { EnableCrossPartitionQuery = true }; //Need this for cross-partition searching

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(CosmosDBClient.CosmosDBDatabaseName, collectionId), CrossPartitionEnabled)// Cross-partition search enabled
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            try
            {
                while (query.HasMoreResults)
                {
                    results.AddRange(await query.ExecuteNextAsync<T>());
                }
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now} :: Exception: {e.Message} :: ERROR FETCHING DATA FROM COSMOSDB, with CollectionId: {collectionId} and predicate: {predicate}. RETURNING NULL ITEMS.");
                    
                    results = null;
                }
                else
                {
                    string msg = $"{DateTime.Now} :: Exception: {e.Message}";
                    throw new Exception(msg);
                }
            }
            return results;
        }

        //Code to create an item in the database - takes an item as input and persists it in CosmosDB
        public static async Task<Document> CreateItemAsync(T item, string collectionId)
        {
            try
            {
                return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(CosmosDBClient.CosmosDBDatabaseName, collectionId), item);
            }
            catch (DocumentClientException e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now} :: Exception: {e.Message} :: ERROR CREATING DATA IN COSMOSDB, with CollectionId: {collectionId}.");
                return null;
            }
        }

        //Code to edit an item in the database - fetches item from db then replaces with edited version and posts back
        public static async Task<Document> UpdateItemAsync(string id, T item, string collectionId)
        {
            try
            {
                return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(CosmosDBClient.CosmosDBDatabaseName, collectionId, id), item);
            }
            catch (DocumentClientException e)
            {
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now} :: Exception: {e.Message} :: ERROR UPDATING DATA IN COSMOSDB, with CollectionId: {collectionId}.");
                return null;
            }
            
        }

        //GET INDIVIDUAL ITEM
        public static async Task<T> GetItemAsync(string id, string collectionId)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(CosmosDBClient.CosmosDBDatabaseName, collectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR FETCHING ITEM FROM COSMOSDB - ITEM DOES NOT EXIST - item id: {0} CollectionId: {1}. RETURNING NULL ITEM.", id, collectionId);
                    return null;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR FETCHING ITEM FROM COSMOSDB, with item id: {0} CollectionId: {1}. RETURNING NULL ITEM.", id, collectionId);
                    return null;
                }
            }
        }

        public static async Task DeleteItemAsync(string id, string collectionId)
        {
            try
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(CosmosDBClient.CosmosDBDatabaseName, collectionId, id));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR DELETING ITEM IN COSMOSDB - ITEM DOES NOT EXIST - item id: {0} & CollectionId: {1}.", id, collectionId);
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR DELETING ITEM IN COSMOSDB, with item id: {0} & CollectionId: {1}.", id, collectionId);
                    return;
                }
            }
        }
    }
}

