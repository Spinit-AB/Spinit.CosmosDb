using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Spinit.CosmosDb.Tests")]

// Message supressions
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Must continue to use lowercase due to legacy", Scope = "member", Target = "~M:Spinit.CosmosDb.EntityExtensions.CreateNormalized``1(``0)~``0")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "This class is intended to use lowercase", Scope = "member", Target = "~M:Spinit.CosmosDb.LowercaseTokenFilter.Execute(System.Collections.Generic.IEnumerable{System.String},Spinit.CosmosDb.AnalyzeContext)~System.Collections.Generic.IEnumerable{System.String}")]