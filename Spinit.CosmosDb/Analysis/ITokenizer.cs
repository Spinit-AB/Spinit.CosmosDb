using System.Collections.Generic;

namespace Spinit.CosmosDb
{
    /// <summary>
    /// A tokenizer receives text data and should break it up into individual tokens (usually individual words), and outputs a stream of tokens.
    /// </summary>
    public interface ITokenizer
    {
        IEnumerable<string> Tokenize(string text);
    }
}
