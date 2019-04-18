using Spinit.CosmosDb;

namespace TodoApi.Features.Shared
{
    public class TodoDatabase : CosmosDatabase
    {
        public TodoDatabase(DatabaseOptions<TodoDatabase> options)
            : base(options)
        { }

        public ICosmosDbCollection<TodoItem> Todos { get; set; }
    }
}
