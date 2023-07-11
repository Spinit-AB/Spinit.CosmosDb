using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spinit.CosmosDb;
using TodoApi.Features.Shared;
using TodoApi.Features.Todo;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCosmosDatabase<TodoDatabase>(builder.Configuration.GetConnectionString("CosmosDb"));
builder.Services.AddHostedService<EnsureDatabaseCreated>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("todo/search", ([FromServices] ICosmosDbCollection<TodoItem> collection, [FromBody] TodoItemSearchRequest? search) => collection.SearchAsync(new SearchRequest<TodoItem>
{
    Query = search?.Query,
    PageSize = 100,
    ContinuationToken = search?.ContinuationToken,
    Filter = search?.Filter?.AsExpression(),
    SortBy = x => x.CreatedDate,
    SortDirection = SortDirection.Descending
}));
app.MapGet("todo/{id}", ([FromServices] ICosmosDbCollection<TodoItem> collection, [FromRoute] string id) => collection.GetAsync(id));
app.MapPost("todo", ([FromServices] ICosmosDbCollection<TodoItem> collection, [FromBody] TodoItem todoItem) => collection.UpsertAsync(todoItem));
app.MapDelete("todo/{id}", ([FromServices] ICosmosDbCollection<TodoItem> collection, [FromRoute] string id) => collection.DeleteAsync(id));

app.Run();