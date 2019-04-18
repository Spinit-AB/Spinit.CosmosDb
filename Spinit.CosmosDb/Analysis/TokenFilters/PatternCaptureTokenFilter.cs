using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// Emits a token for every capture group in the regular expression.
    /// <para>
    /// Example use is for splitting product number into parts, eg YZF1000R => ["YZF", "1000", "R"]
    /// </para>
    /// </summary>
    public class PatternCaptureTokenFilter : ITokenFilter
    {
        private readonly Regex _regex;
        private readonly bool _preserveOriginal;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatternCaptureTokenFilter"/> class.
        /// </summary>
        /// <param name="pattern">The regex pattern to use</param>
        /// <param name="preserveOriginal">If set (the default) also emit the original token.</param>
        public PatternCaptureTokenFilter(string pattern, bool preserveOriginal = true)
        {
            _regex = new Regex(pattern, RegexOptions.Compiled);
            _preserveOriginal = preserveOriginal;
        }

        public IEnumerable<string> Execute(IEnumerable<string> tokens, AnalyzeContext context)
        {
            foreach (var token in tokens)
            {
                var matches = _regex.Matches(token);
                foreach (Match match in matches)
                {
                    if (match.Success)
                        foreach (Group group in match.Groups)
                        {
                            if (!group.Success || group is Match)
                                continue;
                            yield return group.Value;
                        }
                }
                if (_preserveOriginal)
                    yield return token;
            }
        }
    }
}
