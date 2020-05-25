using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    // TODO: no need for this interface
    public interface IDatabaseOperations
    {
        /// <summary>
        /// Creates the Cosmos database and defined collections if not exists.
        /// </summary>
        /// <param name="createOptions">Optional parameter to specify the throughput (RU/s) the database or container should be created with.</param>
        /// <returns></returns>
        public Task CreateIfNotExistsAsync(CreateDbOptions createOptions = null);

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();

        /// <summary>
        /// Gets the throughput (RU/s) set for the collection.
        /// </summary>
        /// <returns></returns>
        Task<int?> GetThroughputAsync();

        /// <summary>
        /// Sets the throughput (RU/s) for the database.
        /// </summary>
        /// <param name="throughput">The new throughput to set. Must be between 400 and 1000000 in increments of 100.</param>
        /// <returns></returns>
        Task SetThroughputAsync(int throughput);
    }
}
