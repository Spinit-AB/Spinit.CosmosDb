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
        private readonly TestDatabase _database;

        public FunctionalTests(TestDatabase database)
        {
            _database = database;
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task CreateDatabase()
        {
            await _database.Operations.CreateIfNotExistsAsync();
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task AddData(IEnumerable<TodoItem> todoItems)
        {
            await _database.Todos.UpsertAsync(todoItems);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task UpdateDataWithTags(TodoItem todoItem)
        {
            todoItem.Tags = new[] { "Tag1", "Tag2" };
            await _database.Todos.UpsertAsync(todoItem);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCountAll()
        {
            var count = await _database.Todos.CountAsync(new SearchRequest<TodoItem> { });
            Assert.Equal(10, count);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestCountWithFilter()
        {
            var count = await _database.Todos.CountAsync(new SearchRequest<TodoItem> { Filter = x => x.Status == TodoStatus.Done });
            Assert.Equal(4, count);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestEmptySearch()
        {
            var expectedItemCount = GetTodoItems().Count();
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            Assert.Equal(expectedItemCount, result.Documents.Count());
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitle(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = todoItem.Title, PageSize = int.MaxValue });
            Assert.Single(result.Documents);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchOnTitleWordsReversed(TodoItem todoItem)
        {
            var titleReversed = string.Join(' ', todoItem.Title.Split(' ').Reverse());
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Query = titleReversed, PageSize = int.MaxValue });
            Assert.Single(result.Documents);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchFilterOnStatus(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync(new SearchRequest<TodoItem> { Filter = x => x.Status == todoItem.Status, PageSize = int.MaxValue });
            Assert.Contains(result.Documents, x => x.Id == todoItem.Id);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestSearchWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.SearchAsync<TodoItemProjection>(new SearchRequest<TodoItem> { PageSize = int.MaxValue });
            Assert.Contains(result.Documents, x => x.Id == todoItem.Id);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGet(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync(todoItem.Id);
            Assert.NotNull(result);
            Assert.Equal(todoItem.Id, result.Id);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task GetShouldReturnTitle(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync(todoItem.Id);
            Assert.NotNull(result);
            Assert.NotEmpty(todoItem.Title);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestGetWithProjection(TodoItem todoItem)
        {
            var result = await _database.Todos.GetAsync<TodoItemProjection>(todoItem.Id);
            Assert.NotNull(result);
            Assert.Equal(todoItem.Id, result.Id);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItems))]
        public async Task TestDelete(TodoItem todoItem)
        {
            await _database.Todos.DeleteAsync(todoItem.Id);

            var result = await _database.Todos.GetAsync(todoItem.Id);
            Assert.Null(result);
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task TestBulkDelete(IEnumerable<TodoItem> todoItems)
        {
            await _database.Todos.UpsertAsync(todoItems);
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                Assert.NotNull(result);
            }

            await _database.Todos.DeleteAsync(todoItems.Select(x => x.Id));
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                Assert.Null(result);
            }
        }

        [Theory(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        [MemberData(nameof(GetTodoItemList))]
        public async Task TestBulkDeleteWhenOneIdMissing(IEnumerable<TodoItem> todoItems)
        {
            var missingId = todoItems.First().Id;
            await _database.Todos.UpsertAsync(todoItems.Skip(1));
            foreach (var todoItem in todoItems.Skip(1))
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                Assert.NotNull(result);
            }

            await _database.Todos.DeleteAsync(todoItems.Select(x => x.Id));
            foreach (var todoItem in todoItems)
            {
                var result = await _database.Todos.GetAsync(todoItem.Id);
                if (todoItem.Id == missingId)
                {
                    Assert.NotNull(todoItem);
                }
                else
                {
                    Assert.Null(result);
                }
            }
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReadCollectionThroughput()
        {
            await _database.Todos.GetThroughputAsync();
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReplaceCollectionThroughput()
        {
            await _database.Todos.SetThroughputAsync(1000);
        }


        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReadNewlyReplacedThrouhgput()
        {
            var throughput = await _database.Todos.GetThroughputAsync();
            Assert.Equal(1000, throughput);
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task TestReplaceThroughputWithInvalidThroughput()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await _database.Todos.SetThroughputAsync(399));
        }

        [Fact(Skip = FunctionTestsConfiguration.SkipTests)]
        [TestOrder]
        public async Task DeleteDatabase()
        {
            await _database.Operations.DeleteAsync();
        }

        public static IEnumerable<TodoItem> GetItems()
        {
            const int recordCount = 10;
            var result = new List<TodoItem>();

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
            public TestDatabase()
                : base(new DatabaseOptionsBuilder<TestDatabase>().UseConnectionString(GenerateConnectionString()).Options)
            { }

            private static string GenerateConnectionString()
            {
                var databaseId = $"db-{Guid.NewGuid().ToString("N")}";
                return $"{FunctionTestsConfiguration.CosmosDbConnectionString};DatabaseId={databaseId}";
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
