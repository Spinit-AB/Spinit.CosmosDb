using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Spinit.CosmosDb
{
    public abstract class CosmosDatabase
    {
        private readonly IDocumentClient _documentClient;
        private readonly IDatabaseOptions _options;

        protected CosmosDatabase(IDatabaseOptions options)
            : this(new DocumentClient(new Uri(options.Endpoint), options.Key, connectionPolicy: CreateConnectionPolicy(options)), options)
        { }

        public IDatabaseOperations Database { get => new DatabaseOperations(this, _documentClient); }

        internal CosmosDatabase(IDocumentClient documentClient, IDatabaseOptions options)
        {
            _documentClient = documentClient;
            _options = options;
            SetupCollectionProperties();
        }

        internal static ConnectionPolicy CreateConnectionPolicy(IDatabaseOptions options)
        {
            var connectionPolicy = new ConnectionPolicy();
            if (!string.IsNullOrEmpty(options.PreferredLocation))
                connectionPolicy.PreferredLocations.Add(options.PreferredLocation);
            return connectionPolicy;
        }

        private void SetupCollectionProperties()
        {
            foreach (var collectionProperty in GetCollectionProperties())
            {
                var collectionType = typeof(CosmosDbCollection<>).MakeGenericType(collectionProperty.PropertyType.GenericTypeArguments.Single());

                var databaseId = GetDatabaseId();
                var collectionId = GetCollectionId(collectionProperty);

                var entityType = collectionProperty.PropertyType.GenericTypeArguments.Single();
                var factoryMethod = typeof(CosmosDatabase).GetMethod(nameof(CreateCollection), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(entityType);
                var collectionInstance = factoryMethod.Invoke(this, new[] { databaseId, collectionId });
                collectionProperty.SetValue(this, collectionInstance);
            }
        }

        private ICosmosDbCollection<TEntity> CreateCollection<TEntity>(string databaseId, string collectionId)
            where TEntity : class, ICosmosEntity
        {
            return new CosmosDbCollection<TEntity>(_documentClient, databaseId, collectionId);
        }

        internal IEnumerable<PropertyInfo> GetCollectionProperties()
        {
            return GetType().GetProperties().Where(x =>
                x.PropertyType.IsGenericType &&
                x.PropertyType.GetGenericTypeDefinition() == typeof(ICosmosDbCollection<>));
        }

        internal string GetDatabaseId()
        {
            return !string.IsNullOrEmpty(_options.DatabaseId)
                ? _options.DatabaseId
                : GetType().GetCustomAttribute<DatabaseIdAttribute>()?.DatabaseId ?? GetType().Name;
        }

        internal string GetCollectionId(PropertyInfo collectionPropertyInfo)
        {
            var result = collectionPropertyInfo.GetCustomAttribute<CollectionIdAttribute>()?.CollectionId;
            if (string.IsNullOrEmpty(result))
                return collectionPropertyInfo.Name;
            return result;
        }
    }
}
