using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spinit.CosmosDb
{
    public abstract class BasePropertyTextExtractor : ITextExtractor
    {
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            ContractResolver = new ContractResolver()
        });

        public IEnumerable<string> ExtractText<TEntity>(TEntity entity)
            where TEntity : ICosmosEntity
        {
            using (var capturer = CreateValueCapturer())
            {
                jsonSerializer.Serialize(capturer, entity);
                return capturer.Data;
            }
        }

        protected abstract ValueCapturer CreateValueCapturer();
    }
}
