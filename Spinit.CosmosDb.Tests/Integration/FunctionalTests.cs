using Microsoft.Azure.Cosmos;
using Shouldly;
using Spinit.CosmosDb.Tests.Core;
using Spinit.CosmosDb.Tests.Core.Order;
using Xunit;

namespace Spinit.CosmosDb.Tests.Integration
{
    [TestCaseOrderer("Spinit.CosmosDb.Tests.Core.Order.TestCaseByAttributeOrderer", "Spinit.CosmosDb.Tests")]
    [Collection("DatabaseIntegrationTest")]
    public class FunctionalTests : IClassFixture<FunctionalTests.TestDatabaseFixture>
    {
        private TestDatabase _database;

        public FunctionalTests(TestDatabaseFixture fixture) 
        {
            _database = fixture.Database;
        }

        [Fact]
        [TestOrder]
        public async Task CreateDatabase()
        {
            await _database.Operations.CreateIfNotExistsAsync();
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task AddData(IEnumerable<TodoItem> todoItems)
        {
            await _database.Todos.UpsertAsync(todoItems);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task UpdateDataWithTags(TodoItem todoItem)
        {
            todoItem.Tags = new[] { "Tag1", "Tag2" };
            await _database.Todos.UpsertAsync(todoItem);
        }

        [Fact]
        [TestOrder]
        public async Task TestCountAll()
        {
            var count = await _database.Todos.CountAsync(new SearchRequest<TodoItem> { });
            count.ShouldBe(10);
        }

        [Fact]
        [TestOrder]
        public async Task TestCountWithFilter()
        {
            var count = await _database.Todos.CountAsync(new SearchRequest<TodoItem> { Filter = x => x.Status == TodoStatus.Done });
            count.ShouldBe(4);
        }

        [Fact]
        [TestOrder]
        public async Task TestEmptySearch()
        {
            var expectedItemCount = GetTodoItems().Count();
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            result.Documents.Count().ShouldBe(expectedItemCount);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitle(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = todoItem.Title, PageSize = int.MaxValue });
            result.Documents.ShouldHaveSingleItem();
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitleWordsReversed(TodoItem todoItem)
        {
            var titleReversed = string.Join(' ', todoItem.Title.Split(' ').Reverse());
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = titleReversed, PageSize = int.MaxValue });
            result.Documents.ShouldHaveSingleItem();
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchFilterOnStatus(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Filter = x => x.Status == todoItem.Status, PageSize = int.MaxValue });
            result.Documents.ShouldContain(x => x.Id == todoItem.Id);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync<TodoItemProjection>(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            result.Documents.ShouldContain(x => x.Id == todoItem.Id);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGet(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync(todoItem.Id);
            result.ShouldNotBeNull().Id.ShouldBe(todoItem.Id);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task GetShouldReturnTitle(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync(todoItem.Id);
            result.ShouldNotBeNull().Title.ShouldNotBeEmpty();
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGetWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync<TodoItemProjection>(todoItem.Id);
            result.ShouldNotBeNull().Id.ShouldBe(todoItem.Id);
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestDelete(TodoItem todoItem)
        {
            await _database.Todos.DeleteAsync(todoItem.Id);

            var result = await _database.Todos.GetAsync(todoItem.Id);
            result.ShouldBeNull();
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task TestBulkDelete(IEnumerable<TodoItem> todoItems)
        {
            await _database.Todos.UpsertAsync(todoItems);
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                result.ShouldNotBeNull();
            }

            await _database.Todos.DeleteAsync(todoItems.Select(x => x.Id));
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                result.ShouldBeNull();
            }
        }

        [Theory]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task TestBulkDeleteWhenOneIdMissing(IEnumerable<TodoItem> todoItems)
        {
            var missingId = todoItems.First().Id;
            await _database.Todos.UpsertAsync(todoItems.Skip(1));
            foreach (var todoItem in todoItems.Skip(1))
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                result.ShouldNotBeNull();
            }

            await _database.Todos.DeleteAsync(todoItems.Select(x => x.Id));
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                if (todoItem.Id == missingId)
                {
                    todoItem.ShouldNotBeNull();
                }
                else
                {
                    result.ShouldBeNull();
                }
            }
        }

        [Fact]
        [TestOrder]
        public async Task TestReadCollectionThroughput()
        {
            await _database.Todos.GetThroughputAsync();
        }

        [Fact]
        [TestOrder]
        public async Task TestReplaceCollectionThroughput()
        {
            await _database.Todos.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(1000));
        }


        [Fact]
        [TestOrder]
        public async Task TestReadNewlyReplacedThrouhgput()
        {
            var throughputProperties = await _database.Todos.GetThroughputAsync();
            throughputProperties.Throughput.ShouldBe(1000);
        }

        [Fact]
        [TestOrder]
        public async Task TestReplaceThroughputWithInvalidThroughput()
        {
            await Should.ThrowAsync<ArgumentException>(async () => await _database.Todos.SetThroughputAsync(ThroughputProperties.CreateManualThroughput(399)));
        }

        public static IEnumerable<TodoItem> GetItems()
        {
            const int recordCount = 10;
            var result = new List<TodoItem>();

            Enumerable.Range(1, recordCount).ToList().ForEach(x =>
            {
                var todoStatus = x / (double)recordCount < 0.3
                    ? TodoStatus.New
                        : x / (double)recordCount < 0.7
                            ? TodoStatus.Active
                            : TodoStatus.Done;
                result.Add(new TodoItem
                {
                    Id = x.ToString(),
                    Status = todoStatus,
                    CreatedDate = DateTime.Today.Date.AddDays(-recordCount),
                    Title = $"Title for item {x}",
                    Description = $"Title for item {x}",
                    Tags = Enumerable.Empty<string>()
                });
            });

            return result;
        }

        public static TheoryData<IEnumerable<TodoItem>> GetTodoItemList()
        {
            return new TheoryData<IEnumerable<TodoItem>>()
            {
                GetItems()
            };
        }

        public static TheoryData<TodoItem> GetTodoItems()
        {
            var result = new TheoryData<TodoItem>();
            GetItems().ToList().ForEach(item =>
            {
                result.Add(item);
            });

            return result;
        }

        public class TestDatabase : CosmosDatabase
        {
            public TestDatabase(DatabaseOptions<TestDatabase> options) : base(options, initialize: true) { }

            public ICosmosDbCollection<TodoItem> Todos { get; set; } = default!;
        }

        public class TodoItem : ICosmosEntity
        {
            public required string Id { get; set; }
            public required TodoStatus Status { get; set; }
            public required DateTime CreatedDate { get; set; }
            public required string Title { get; set; }
            public string? Description { get; set; }
            public required IEnumerable<string> Tags { get; set; }
        }

        public class TodoItemProjection : ICosmosEntity
        {
            public required string Id { get; set; }
        }

        public enum TodoStatus
        {
            New = 1,
            Active = 2,
            Done = 3,
        }

        public class TestDatabaseFixture : IAsyncLifetime
        {
            public TestDatabaseFixture()
            {
                Adapter = new DatabaseAdapter(databaseName: "Functional_Tests");
                Database = Adapter.CreateCosmosDbDatabase<TestDatabase>();
            }

            public TestDatabase Database { get; }
            private DatabaseAdapter Adapter { get; }

            // Do not create database in this step
            public Task InitializeAsync() => Task.CompletedTask;

            public Task DisposeAsync() => Adapter.TeardownDatabase();
        }
    }
}
