Spinit.CosmosDb
===============
![GitHub](https://img.shields.io/github/license/Spinit-AB/Spinit.CosmosDb.svg)
![Build Status](https://github.com/Spinit-AB/Spinit.CosmosDb/actions/workflows/build-and-test.yml/badge.svg)
![Nuget](https://img.shields.io/nuget/v/Spinit.CosmosDb.svg)

Lightweight CosmosDb wrapper


Install
-------

Package Manager:

    PM> Install-Package Spinit.CosmosDb

.NET CLI:

    > dotnet add package Spinit.CosmosDb

Usage
-----

## Define a database model
### Automatic initialization
```csharp
public class MyEntity : ICosmosEntity
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

/*
 Name of the cosmos database, optional.
 If not provided, name of class will be used (MyDatabase in this case).
*/
[DatabaseId("FooDatabase")]
public class MyDatabase : CosmosDatabase
{
    public MyDatabase(DatabaseOptions<MyDatabase> options)
        : base(options)
    { }

    /*
    Name of the cosmos collection, optional.
    If not provided, name of class will be used (MyEntities in this case).
    */
    [CollectionId("FooCollection")]
    public ICosmosDbCollection<MyEntity> MyEntities { get; private set; }
}
```

### Manual initialization
```csharp
public class MyEntity : ICosmosEntity
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}

public class MyDatabase : CosmosDatabase
{
    public MyDatabase(DatabaseOptions<MyDatabase> options)
        : base(options, initialize: false)
    { }

    public ICosmosDbCollection<MyEntity> MyEntities { get; private set; }

    protected override void OnModelCreating(DatabaseModelBuilder<MyDatabase> modelBuilder)
    {
        modelBuilder.DatabaseId("FooDatabase");

        modelBuilder.Collection(x => x.MyEntities)
            .CollectionId("FooCollection");
    }
}
```

### Add to services (IoC):

```csharp
var myConnectionString = configuration.GetConnectionString("CosmosDb");

services
    ...
    // This will register the database type and all it's collections
    .AddCosmosDatabase<MyDatabase>(options => options.UseConnectionString(myConnectionString))
    ...
    ;
```

### Inject and use:

```csharp
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

License
-----
MIT License

Copyright (c) 2019- Spinit AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
