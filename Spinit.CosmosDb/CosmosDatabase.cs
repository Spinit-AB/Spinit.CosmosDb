using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Documents = Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.Azure.Documents.Client;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Abstact base class for a Cosmos database with support for customizing model/analyzer
    /// <para>
    /// Extend this and add your collections as public properties
    /// </para>
    /// </summary>
    public abstract class CosmosDatabase<TImplementation> : CosmosDatabase
        where TImplementation : CosmosDatabase
    {
        protected CosmosDatabase(DatabaseOptions<TImplementation> options)
            : base(options)
        { }

        /// <summary>
        /// Extension point for customizing model and analysis
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected virtual void OnModelCreating(DatabaseModelBuilder<TImplementation> modelBuilder)
        { }

        private protected override DatabaseModel CreateModel()
        {
            var databaseModel = base.CreateModel();
            var databaseModelBuilder = new DatabaseModelBuilder<TImplementation>(databaseModel);
            OnModelCreating(databaseModelBuilder);
            return databaseModelBuilder.Build();
        }
    }

    /// <summary>
    /// Abstact base class for a Cosmos database with default model/analyzer
    /// <para>
    /// Extend this and add your collections as public properties
    /// </para>
    /// </summary>
    public abstract class CosmosDatabase
    {
        private readonly Documents.IDocumentClient _documentClient;
        private readonly IDatabaseOptions _options;

        protected CosmosDatabase(IDatabaseOptions options)
            : this(
                new Documents.Client.DocumentClient(new Uri(options.Endpoint), options.Key, connectionPolicy: CreateConnectionPolicy(options), serializerSettings: CreateJsonSerializerSettings()),
                new CosmosClient(
                    new Uri(options.Endpoint).AbsoluteUri,
                    options.Key,
                    new CosmosClientOptions()
                ), options)
        { }

        internal static Documents.Client.ConnectionPolicy CreateConnectionPolicy(IDatabaseOptions options)
        {
            var connectionPolicy = new Documents.Client.ConnectionPolicy
            {
                ConnectionMode = Documents.Client.ConnectionMode.Direct,
                ConnectionProtocol = Documents.Client.Protocol.Tcp
            };

            if (!string.IsNullOrEmpty(options.PreferredLocation))
                connectionPolicy.PreferredLocations.Add(options.PreferredLocation);

            return connectionPolicy;
        }

        internal static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                Converters = new[]
                {
                    new IsoDateTimeConverter()
                }
            };
        }

        internal CosmosDatabase(Documents.IDocumentClient documentClient, CosmosClient cosmosClient, IDatabaseOptions options)
        {
            _documentClient = documentClient;
            CosmosClient = cosmosClient;
            _options = options;
            Initialize();
        }

        /// <summary>
        /// Access to lowlevel document client
        /// </summary>
        public CosmosClient CosmosClient { get; }

        /// <summary>
        /// Database model used
        /// </summary>
        public DatabaseModel Model { get; private set; }

        /// <summary>
        /// Database operations
        /// </summary>
        public IDatabaseOperations Operations { get => new DatabaseOperations(this, CosmosClient); }

        private protected virtual DatabaseModel CreateModel()
        {
            var databaseId = !string.IsNullOrEmpty(_options.DatabaseId)
                ? _options.DatabaseId
                : GetType().GetCustomAttribute<DatabaseIdAttribute>()?.DatabaseId ?? GetType().Name;

            return new DatabaseModel()
            {
                DatabaseId = databaseId,
                CollectionModels = GetCollectionProperties()
                    .Select(x => new CollectionModel
                    {
                        DatabaseId = databaseId,
                        CollectionId = x.GetCollectionId(),
                        Analyzer = new DefaultAnalyzer()
                    })
                    .ToList()
            };
        }

        private void Initialize()
        {
            Model = CreateModel();
            SetupCollectionProperties();
        }

        private void SetupCollectionProperties()
        {
            foreach (var collectionProperty in GetCollectionProperties())
            {
                var entityType = collectionProperty.PropertyType.GenericTypeArguments.Single();
                var setupCollectionPropertyMethod = typeof(CosmosDatabase).GetMethod(nameof(SetupCollectionProperty), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(entityType);
                setupCollectionPropertyMethod.Invoke(this, new object[] { collectionProperty });
            }
        }

        private void SetupCollectionProperty<TEntity>(PropertyInfo collectionProperty)
            where TEntity : class, ICosmosEntity
        {
            var collectionId = collectionProperty.GetCollectionId();
            var collectionModel = Model.CollectionModels.Single(x => x.CollectionId == collectionId); // TODO: add indexed property => Model.CollectionModels[collectionId]
            var collection = new CosmosDbCollection<TEntity>(CosmosClient.GetContainer(collectionModel.DatabaseId, collectionId), _documentClient, collectionModel);
            collectionProperty.SetValue(this, collection);
        }

        private IEnumerable<PropertyInfo> GetCollectionProperties()
        {
            return GetType().GetProperties().Where(x =>
                x.PropertyType.IsGenericType &&
                x.PropertyType.GetGenericTypeDefinition() == typeof(ICosmosDbCollection<>));
        }
    }
}
