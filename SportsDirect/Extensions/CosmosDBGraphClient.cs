using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq.Expressions;
using System.Net;

namespace SportsDirect
{
    public class CosmosDBGraphClient
    {
        public static string CosmosDBURI { get; set; }
        public static string CosmosDBKey { get; set; }
    }

    public static class CosmosDBGraphClient<T> where T : class
    {
        private static DocumentClient client;

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(CosmosDBGraphClient.CosmosDBURI), CosmosDBGraphClient.CosmosDBKey);
        }
        
        //Display all users & their details where predicate (condition) is met - LEADERBOARD etc.
        public static async Task<IEnumerable<T>> GetItemsAsync(string databaseId, string collectionId, Expression<Func<T, bool>> predicate)
        {
            var CrossPartitionEnabled = new FeedOptions { EnableCrossPartitionQuery = true }; //Need this for cross-partition searching

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), CrossPartitionEnabled)// Cross-partition search enabled
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
            catch
            {
                System.Diagnostics.Debug.WriteLine("ERROR FETCHING CROSS-USER DATA FROM COSMOSDB, with CollectionId: {0} and predicate: {1}. RETURNING NULL ITEMS.", collectionId, predicate);
                results = null;
            }

            return results;
        }

        //Code to create an item in the database - takes an item as input and persists it in CosmosDB
        public static async Task<Document> CreateItemAsync(T item, string databaseId, string collectionId)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
        }

        //Code to edit an item in the database - fetches item from db then replaces with edited version and posts back
        public static async Task<Document> UpdateItemAsync(string id, T item, string databaseId, string collectionId)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
        }

        //GET INDIVIDUAL ITEM
        public static async Task<T> GetItemAsync(string id, string databaseId, string collectionId)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR FETCHING SINGLE-USER ITEM FROM COSMOSDB - ITEM DOES NOT EXIST - item id: {0} CollectionId: {1} and username/partition key: {2}. RETURNING NULL ITEMS.", id, collectionId);
                    return null;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR FETCHING SINGLE-USER ITEM FROM COSMOSDB, with item id: {0} CollectionId: {1} and username/partition key: {2}. RETURNING NULL ITEMS.", id, collectionId);
                    return null;
                }
            }
        }

        public static async Task DeleteItemAsync(string id, string databaseId, string collectionId)
        {
            try
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR DELETING ITEM IN COSMOSDB - ITEM DOES NOT EXIST - item id: {0} & CollectionId: {1}.", id, collectionId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR DELETING ITEM IN COSMOSDB, with item id: {0} & CollectionId: {1}.", id, collectionId);
                }
            }
        }
    }
}
