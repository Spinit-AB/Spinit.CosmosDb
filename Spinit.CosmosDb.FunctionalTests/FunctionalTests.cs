using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spinit.CosmosDb.FunctionalTests.Infrastructure;
using Xunit;

namespace Spinit.CosmosDb.FunctionalTests
{
    [TestCaseOrderer("Spinit.CosmosDb.FunctionalTests.Infrastructure.TestCaseByAttributeOrderer", "Spinit.CosmosDb.FunctionalTests")]
    public class FunctionalTests : IClassFixture<FunctionalTests.TestDatabase>
    {
        private const string ShouldSkip =
#if DEBUG
            null;
#else
            "Functional test should only run locally in debug mode";
#endif

        private readonly TestDatabase _database;

        public FunctionalTests(TestDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = ShouldSkip)]
        [TestOrder]
        public async Task CreateDatabase()
        {
            await _database.Database.CreateIfNotExistsAsync();
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task AddData(TodoItem todoItem)
        {
            await _database.Todos.UpsertAsync(todoItem);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task UpdateDataWithTags(TodoItem todoItem)
        {
            todoItem.Tags = new[] { "Tag1", "Tag2" };
            await _database.Todos.UpsertAsync(todoItem);
        }

        [Fact(Skip = ShouldSkip)]
        [TestOrder]
        public async Task TestEmptySearch()
        {
            var expectedItemCount = GetTodoItems().Count();
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            Assert.Equal(expectedItemCount, result.Documents.Count());
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitle(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = todoItem.Title, PageSize = int.MaxValue });
            Assert.Single(result.Documents);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitleWordsReversed(TodoItem todoItem)
        {
            var titleReversed = string.Join(' ', todoItem.Title.Split(' ').Reverse());
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = titleReversed, PageSize = int.MaxValue });
            Assert.Single(result.Documents);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchFilterOnStatus(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Filter = x => x.Status == todoItem.Status, PageSize = int.MaxValue });
            Assert.Contains(result.Documents, x => x.Id == todoItem.Id);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync<TodoItemProjection>(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            Assert.Contains(result.Documents, x => x.Id == todoItem.Id);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGet(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync(todoItem.Id);
            Assert.NotNull(result);
            Assert.Equal(todoItem.Id, result.Id);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGetWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync<TodoItemProjection>(todoItem.Id);
            Assert.NotNull(result);
            Assert.Equal(todoItem.Id, result.Id);
        }

        [Theory(Skip = ShouldSkip)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestDelete(TodoItem todoItem)
        {
            await _database.Todos.DeleteAsync(todoItem.Id);
        }

        [Fact(Skip = ShouldSkip)]
        [TestOrder]
        public async Task DeleteDatabase()
        {
            await _database.Database.DeleteAsync();
        }

        public static TheoryData<TodoItem> GetTodoItems()
        {
            var result = new TheoryData<TodoItem>();
            const int recordCount = 10;
            Enumerable.Range(1, recordCount).ToList().ForEach(x =>
            {
                var todoStatus = (x / (double)recordCount) < 0.3
                    ? TodoStatus.New
                        : (x / (double)recordCount) < 0.7
                            ? TodoStatus.Active
                            : TodoStatus.Done;
                result.Add(new TodoItem
                {
                    Id = x.ToString(),
                    Status = todoStatus,
                    CreatedDate = DateTime.Today.Date.AddDays(-recordCount),
                    Title = $"Title for item {x}",
                    Description = $"Title for item {x}"
                });
            });
            return result;
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;DatabaseId={databaseId}";
            }

            public ICosmosDbCollection<TodoItem> Todos { get; set; }
        }

        public class TodoItem : ICosmosEntity
        {
            public string Id { get; set; }
            public TodoStatus Status { get; set; }
            public DateTime CreatedDate { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public IEnumerable<string> Tags { get; set; }
        }

        public class TodoItemProjection : ICosmosEntity
        {
            public string Id { get; set; }
        }

        public enum TodoStatus
        {
            New = 1,
            Active = 2,
            Done = 3,
        }
    }
}
