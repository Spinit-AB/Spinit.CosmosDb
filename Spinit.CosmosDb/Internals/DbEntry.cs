using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spinit.CosmosDb
{
    internal class DbEntry<TEntity> : ICosmosEntity
        where TEntity : class, ICosmosEntity
    {
        public DbEntry()
        { }

        public DbEntry(TEntity entity, Analyzer analyzer)
        {
            Id = entity.Id;
            Original = entity;
            Normalized = entity.CreateNormalized();
            All = analyzer.AnalyzeEntity(entity);
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public TEntity Original { get; set; }

        public TEntity Normalized { get; set; }

        [JsonProperty(PropertyName = "_all")]
        public IEnumerable<string> All { get; set; }
    }
}
