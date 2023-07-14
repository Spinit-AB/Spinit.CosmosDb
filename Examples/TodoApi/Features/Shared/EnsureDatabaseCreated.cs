using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TodoApi.Features.Shared
{
    public class EnsureDatabaseCreated : IHostedService
    {
        private readonly TodoDatabase _database;
        public EnsureDatabaseCreated(TodoDatabase database) => _database = database;
        public Task StartAsync(CancellationToken ct) => _database.Operations.CreateIfNotExistsAsync();
        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
