using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    // TODO: no need for this interface
    public interface IDatabaseOperations
    {
        /// <summary>
        /// Creates the Cosmos database and defined collections if not exists.
        /// </summary>
        /// <param name="containerThroughput">Optional parameter to specify the throughput (RU/s) the containers should be created with. Defaults to 400.</param>
        /// <returns></returns>
        Task CreateIfNotExistsAsync(int containerThroughput = 400);

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();
    }
}
