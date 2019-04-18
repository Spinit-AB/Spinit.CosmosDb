using System;
using System.Linq;
using Spinit.CosmosDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the cosmos database and all it's collections to the IoC
        /// </summary>
        /// <typeparam name="TDatabase"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="connectionString">Cosmos db connection string</param>
        /// <returns></returns>
        public static IServiceCollection AddCosmosDatabase<TDatabase>(this IServiceCollection serviceCollection, string connectionString)
            where TDatabase : CosmosDatabase
            => AddCosmosDatabase<TDatabase>(serviceCollection, options => options.UseConnectionString(connectionString));

        /// <summary>
        /// Adds the cosmos database and all it's collections to the IoC
        /// </summary>
        /// <typeparam name="TDatabase"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddCosmosDatabase<TDatabase>(this IServiceCollection serviceCollection, Action<DatabaseOptionsBuilder<TDatabase>> options)
           where TDatabase : CosmosDatabase
        {
            serviceCollection
                .AddSingleton(sp =>
                {
                    var optionsBuilder = new DatabaseOptionsBuilder<TDatabase>();
                    options(optionsBuilder);
                    return optionsBuilder.Options;
                })
                .AddSingleton<TDatabase>();

            foreach (var collectionProperty in typeof(TDatabase).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(ICosmosDbCollection<>)))
            {
                serviceCollection.AddSingleton(collectionProperty.PropertyType, sp =>
                {
                    var database = sp.GetRequiredService<TDatabase>();
                    return collectionProperty.GetValue(database);
                });
            }
            return serviceCollection;
        }
    }
}
