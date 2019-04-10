using System;
using System.Collections.Generic;
using System.Linq;

namespace Spinit.CosmosDb
{
    internal static class TermAnalyzer
    {
        internal static IEnumerable<string> Analyze(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            return text
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .ToArray();
        }

        internal static IEnumerable<string> Analyze(IEnumerable<string> texts)
        {
            return texts
                .SelectMany(x => Analyze(x))
                .ToArray();
        }
    }
}
