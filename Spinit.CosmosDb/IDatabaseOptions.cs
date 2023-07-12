using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;

namespace Spinit.CosmosDb
{
    public interface IDatabaseOptions
    {
        string Endpoint { get; set; }

        string Key { get; set; }

        /// <summary>
        /// Database id/name
        /// </summary>
        string DatabaseId { get; set; }

        /// <summary>
        /// Optional preferred location
        /// </summary>
        string PreferredLocation { get; set; }

        /// <summary>
        /// Configure document client json serializer settings
        /// </summary>
        Action<JsonSerializerSettings> ConfigureJsonSerializerSettings { get; set; }

        /// <summary>
        /// Configure CosmosClient options
        /// </summary>
        Action<(CosmosClientOptions CosmosClientOptions, bool BulkClient)> ConfigureCosmosClientOptions { get; set; }
    }
}
