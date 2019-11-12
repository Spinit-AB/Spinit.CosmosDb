namespace Spinit.CosmosDb
{
    /// <summary>
    /// Extracts <see cref="string"/> properties from en entity.
    /// </summary>
    public class BoolPropertyTextExtractor : BasePropertyTextExtractor
    {
        protected override ValueCapturer CreateValueCapturer()
        {
            return new TextValueCapturer();
        }

        private class TextValueCapturer : ValueCapturer
        {
            public override void WriteValue(bool value)
            {
                base.WriteValue(value);
                Capture(value);
            }

            public override void WriteValue(bool? value)
            {
                base.WriteValue(value);
                if (!value.HasValue)
                    return;

                Capture(value.Value);
            }
        }
    }
}
