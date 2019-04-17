namespace Spinit.CosmosDb
{
    /// <summary>
    /// The <see cref="WhitespaceTokenizer"/> breaks text into tokens whenever it encounters a whitespace character.
    /// </summary>
    public class WhitespaceTokenizer : PatternTokenizer
    {
        private const string whitespacePattern = "\\s+";
        public WhitespaceTokenizer()
            : base(whitespacePattern)
        { }
    }
}
