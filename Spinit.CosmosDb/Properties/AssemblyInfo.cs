using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Spinit.CosmosDb.UnitTests")]

// Message supressions
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Must continue to use lowercase due to legacy", Scope = "member", Target = "~M:Spinit.CosmosDb.TermAnalyzer.Analyze(System.String)~System.Collections.Generic.IEnumerable{System.String}")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Must continue to use lowercase due to legacy", Scope = "member", Target = "~M:Spinit.CosmosDb.EntityExtensions.CreateNormalized``1(``0)~``0")]