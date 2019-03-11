﻿using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Spinit.CosmosDb
{
    public class DatabaseOptionsBuilder<TDatabase>
        where TDatabase : CosmosDatabase
    {
        public DatabaseOptions<TDatabase> Options { get; private set; } = new DatabaseOptions<TDatabase> { };

        public DatabaseOptionsBuilder<TDatabase> UseConfiguration(IConfiguration configuration)
        {
            Options = configuration.Get<DatabaseOptions<TDatabase>>();
            return this;
        }

        public DatabaseOptionsBuilder<TDatabase> UseConnectionString(string connectionString)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder()
            {
                ConnectionString = connectionString
            };

            var preferredLocation = connectionStringBuilder.ContainsKey("PreferredLocation")
                ? connectionStringBuilder["PreferredLocation"].ToString()
                : null;

            Options = new DatabaseOptions<TDatabase>
            {
                Endpoint = connectionStringBuilder["AccountEndpoint"].ToString(),
                Key = connectionStringBuilder["AccountKey"].ToString(),
                PreferredLocation = preferredLocation
            };
            return this;
        }
    }
}
