using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="initialize">Pass true of automatic initialization of database and collection objects is desired and false if manual initialization is desired.</param>
        protected CosmosDatabase(DatabaseOptions<TImplementation> options, bool initialize = true)
            : base(options, initialize)
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
        private readonly IDatabaseOptions _options;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        protected CosmosDatabase(IDatabaseOptions options, bool initialize = true)
            : this(
                  cosmosClient: new CosmosClient(
                      accountEndpoint: new Uri(options.Endpoint).AbsoluteUri,
                      authKeyOrResourceToken: options.Key,
                      clientOptions: CreateCosmosClientOptions(options, bulkClient: false)),
                  cosmosBulkClient: new CosmosClient(
                      accountEndpoint: new Uri(options.Endpoint).AbsoluteUri,
                      authKeyOrResourceToken: options.Key,
                      clientOptions: CreateCosmosClientOptions(options, bulkClient: true)),
                  options, 
                  initialize) 
        {
            
        }

        internal static CosmosClientOptions CreateCosmosClientOptions(IDatabaseOptions options = null, bool bulkClient = false)
        {
            var clientOptions = new CosmosClientOptions();
            if (bulkClient)
            {
                clientOptions.AllowBulkExecution = true;
            }

            options?.ConfigureCosmosClientOptions?.Invoke((clientOptions, bulkClient));

            return clientOptions;
        }

        internal static JsonSerializerSettings CreateJsonSerializerSettings(IDatabaseOptions options = null)
        {
            var settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                Converters = new List<JsonConverter>
                {
                    new IsoDateTimeConverter()
                }
            };

            options?.ConfigureJsonSerializerSettings?.Invoke(settings);

            return settings;
        }

        protected CosmosDatabase(CosmosClient cosmosClient, CosmosClient cosmosBulkClient, IDatabaseOptions options, bool initialize = true)
        {
            CosmosClient = cosmosClient;
            CosmosBulkClient = cosmosBulkClient;
            _jsonSerializerSettings = CreateJsonSerializerSettings(options);
            _options = options;
            
            if (initialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Access to lowlevel client
        /// </summary>
        public CosmosClient CosmosClient { get; }

        /// <summary>
        /// Access to lowlevel bulk client
        /// </summary>
        public CosmosClient CosmosBulkClient { get; }

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
                        PropertyName = x.Name,
                        CollectionId = x.GetCollectionId(),
                        Analyzer = new DefaultAnalyzer()
                    })
                    .ToList()
            };
        }

        protected void Initialize()
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
            var collectionModel = Model.CollectionModels
                .SingleOrDefault(x => x.PropertyName == collectionProperty.Name); // TODO: add indexed property => Model.CollectionModels[collectionId]

            if (string.IsNullOrEmpty(collectionModel.CollectionId))
                collectionModel.CollectionId = collectionProperty.GetCollectionId();

            var collection = new CosmosDbCollection<TEntity>(
                CosmosClient.GetContainer(collectionModel.DatabaseId, collectionModel.CollectionId),
                CosmosBulkClient.GetContainer(collectionModel.DatabaseId, collectionModel.CollectionId),
                collectionModel, 
                _jsonSerializerSettings);
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
