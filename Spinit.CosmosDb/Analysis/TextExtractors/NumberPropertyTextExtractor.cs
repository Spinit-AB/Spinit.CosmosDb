namespace Spinit.CosmosDb
{
    /// <summary>
    /// Extracts <see cref="string"/> properties from en entity.
    /// </summary>
    public class NumberPropertyTextExtractor : BasePropertyTextExtractor
    {
        protected override ValueCapturer CreateValueCapturer()
        {
            return new IntValueCapturer();
        }

        private class IntValueCapturer : ValueCapturer
        {
            public override void WriteValue(short value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(short? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }

            public override void WriteValue(int value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(int? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }

            public override void WriteValue(long value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(long? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }

            public override void WriteValue(decimal value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(decimal? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }

            public override void WriteValue(double value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(double? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }
        }
    }
}
