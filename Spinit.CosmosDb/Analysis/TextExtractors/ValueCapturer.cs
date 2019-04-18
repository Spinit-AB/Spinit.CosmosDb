using System.Collections.Generic;
using Newtonsoft.Json;

namespace Spinit.CosmosDb
{
    public abstract class ValueCapturer : JsonWriter
    {
        private readonly IList<string> _data = new List<string>();

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> Data => _data;

        protected void Capture(string text)
        {
            _data.Add(text);
        }
    }
}
