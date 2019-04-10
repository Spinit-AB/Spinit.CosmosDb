using System.Threading.Tasks;

namespace Spinit.CosmosDb
{
    public interface IDatabaseOperations
    {
        /// <summary>
        /// Creates the Cosmos database and defined collections if not exists.
        /// </summary>
        /// <returns></returns>
        Task CreateIfNotExistsAsync();

        /// <summary>
        /// Deletes the Cosmos database.
        /// </summary>
        /// <returns></returns>
        Task DeleteAsync();
    }
}
