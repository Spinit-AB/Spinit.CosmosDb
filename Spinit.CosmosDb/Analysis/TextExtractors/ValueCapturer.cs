using System.Collections.Generic;
using System.Globalization;
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

        protected void Capture(short number)
        {
            _data.Add(number.ToString(CultureInfo.InvariantCulture));
        }

        protected void Capture(int number)
        {
            _data.Add(number.ToString(CultureInfo.InvariantCulture));
        }

        protected void Capture(long number)
        {
            _data.Add(number.ToString(CultureInfo.InvariantCulture));
        }

        protected void Capture(decimal number)
        {
            _data.Add(number.ToString(CultureInfo.InvariantCulture));
        }

        protected void Capture(double number)
        {
            _data.Add(number.ToString(CultureInfo.InvariantCulture));
        }

        protected void Capture(bool b)
        {
            _data.Add(b.ToString(CultureInfo.InvariantCulture));
        }
    }
}
