using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Spinit.CosmosDb;
using TodoApi.Features.Shared;

namespace TodoApi.Features.Todo
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        private readonly ICosmosDbCollection<TodoItem> _collection;

        public TodoController(ICosmosDbCollection<TodoItem> collection)
        {
            _collection = collection;
        }

        [HttpGet]
        public Task<SearchResponse<TodoItem>> SearchAsync([FromQuery] string query, [FromQuery] TodoItemFilters filters, [FromQuery] string continuationToken = null)
        {
            var request = new SearchRequest<TodoItem>
            {
                Query = query,
                PageSize = 100,
                ContinuationToken = continuationToken,
                Filter = filters?.AsExpression(),
                SortBy = x => x.CreatedDate,
                SortDirection = SortDirection.Descending
            };
            return _collection.SearchAsync(request);
        }

        [HttpGet("{id}")]
        public Task<TodoItem> GetAsync([FromRoute] string id)
        {
            return _collection.GetAsync(id);
        }

        [HttpPost]
        public Task UpsertAsync([FromBody] TodoItem todoItem)
        {
            return _collection.UpsertAsync(todoItem);
        }

        [HttpDelete("{id}")]
        public Task DeleteAsync([FromRoute] string id)
        {
            return _collection.DeleteAsync(id);
        }
    }
}
