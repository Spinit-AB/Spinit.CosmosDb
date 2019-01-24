using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Spinit.CosmosDb
{
    public abstract class CosmosDatabase
    {
        protected CosmosDatabase(IDatabaseOptions options)
            : this(new DocumentClient(new Uri(options.Endpoint), options.Key, connectionPolicy: CreateConnectionPolicy(options)))
        { }

        internal CosmosDatabase(IDocumentClient client)
        {
            SetupCollectionProperties(client);
        }

        internal static ConnectionPolicy CreateConnectionPolicy(IDatabaseOptions options)
        {
            var connectionPolicy = new ConnectionPolicy();
            connectionPolicy.PreferredLocations.Add(options.PreferredLocation);
            return connectionPolicy;
        }

        private void SetupCollectionProperties(IDocumentClient client)
        {
            foreach (var collectionProperty in GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(ICosmosDbCollection<>)))
            {
                var collectionType = typeof(CosmosDbCollection<>).MakeGenericType(collectionProperty.PropertyType.GenericTypeArguments.Single());

                var databaseId = GetType().GetCustomAttribute<DatabaseIdAttribute>()?.DatabaseId;
                if (string.IsNullOrEmpty(databaseId))
                    throw new Exception($"No valid DatabaseId attribute found on type {GetType().Name}");

                var collectionId = collectionProperty.GetCustomAttribute<CollectionIdAttribute>()?.CollectionId; 
                if (string.IsNullOrEmpty(collectionId))
                    throw new Exception($"No valid CollectionId attribute found on property {collectionProperty.Name} on type {GetType().Name}");

                var collectionInstance = Activator.CreateInstance(collectionType, client, databaseId, collectionId);
                collectionProperty.SetValue(this, collectionInstance);
            }
        }
    }
}
