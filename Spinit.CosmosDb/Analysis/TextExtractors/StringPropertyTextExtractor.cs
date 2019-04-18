namespace Spinit.CosmosDb
{
    /// <summary>
    /// Extracts <see cref="string"/> properties from en entity.
    /// </summary>
    public class StringPropertyTextExtractor : BasePropertyTextExtractor
    {
        protected override ValueCapturer CreateValueCapturer()
        {
            return new TextValueCapturer();
        }

        private class TextValueCapturer : ValueCapturer
        {
            public override void WriteValue(string value)
            {
                base.WriteValue(value);
                if (string.IsNullOrEmpty(value))
                    return;

                Capture(value);
            }
        }
    }
}
