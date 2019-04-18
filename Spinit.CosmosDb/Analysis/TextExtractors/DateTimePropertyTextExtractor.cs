using System;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Extracts <see cref="DateTime"/> properties as strings from en entity.
    /// </summary>
    public class DateTimePropertyTextExtractor : BasePropertyTextExtractor
    {
        private readonly string _format;

        public DateTimePropertyTextExtractor(string format = "G")
        {
            _format = format;
        }

        protected override ValueCapturer CreateValueCapturer()
        {
            return new DateTimeValueCapturer(_format);
        }

        private class DateTimeValueCapturer : ValueCapturer
        {
            private readonly string _format;

            public DateTimeValueCapturer(string format)
            {
                _format = format;
            }

            public override void WriteValue(DateTime value)
            {
                base.WriteValue(value);
                Capture(value.ToString(_format));
            }
        }
    }
}
