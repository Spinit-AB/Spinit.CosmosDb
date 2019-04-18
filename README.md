Spinit.CosmosDb
===============

Lightweight CosmosDb wrapper


Install
-------

Package Manager:

    PM> Install-Package Spinit.CosmosDb

.NET CLI:

    > dotnet add package Spinit.CosmosDb

Usage
-----

### Define a database model:

```
public class MyEntity : ICosmosEntity
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

[DatabaseId("MyDatabase")] // name of the cosmos database, optional
public class MyDatabase : CosmosDatabase
{
    public MyDatabase(DatabaseOptions<MyDatabase> options)
        : base(options)
    { }

    [CollectionId("MyEntities")] // name of the cosmos collection, optional
    public ICosmosDbCollection<MyEntity> MyEntities { get; private set; }
}
```

### Add to services (IoC):

```
var myConnectionString = configuration.GetConnectionString("CosmosDb");

services
    ...
    // This will register the database type and all it's collections
    .AddCosmosDatabase<MyDatabase>(options => options.UseConnectionString(myConnectionString))
    ...
    ;
```

### Inject and use:

```
public class MyController : ControllerBase
{
    private readonly ICosmosDbCollection<MyEntity> _collection;

    public MyController(ICosmosDbCollection<MyEntity> collection)
    {
        _collection = collection;
    }

    [HttpGet]
    public Task<SearchResponse<MyEntity>> List(string query)
    {
        var searchRequest = new SearchRequest<MyEntity>
        {
            Query = query
        };
        return _collection.SearchAsync(searchRequest);
    }

    [HttpGet("{id}")]
    public Task<MyEntity> Get(string id)
    {
        return _collection.GetAsync(id);
    }

    [HttpPost]
    public Task Upsert(MyEntity input)
    {
        return _collection.UpsertAsync(input);
    }
}
```