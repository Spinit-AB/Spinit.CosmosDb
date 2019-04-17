using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// The pattern tokenizer uses a regular expression to split text into tokens.
    /// <para>
    /// The default pattern is \W+, which splits text whenever it encounters non-word characters.
    /// </para>
    /// </summary>
    public class PatternTokenizer : ITokenizer
    {
        private const string defaultPattern = "\\W+";
        private readonly Regex _regex;

        public PatternTokenizer()
            : this(defaultPattern)
        { }

        public PatternTokenizer(string pattern)
        {
            Pattern = pattern;
            _regex = new Regex(Pattern, RegexOptions.Compiled);
        }

        public string Pattern { get; }

        public IEnumerable<string> Tokenize(string text)
        {
            return _regex.Split(text).Where(x => !string.IsNullOrEmpty(x));
        }
    }
}
