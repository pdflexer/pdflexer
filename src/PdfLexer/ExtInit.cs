global using PdfLexer.Operators;
global using System.Diagnostics.CodeAnalysis;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PdfLexer.Tests")]
[assembly: InternalsVisibleTo("PdfLexer.ImageSharpExts")] // shouldn't need this but will need to clean up images / colorspace refs
[assembly: InternalsVisibleTo("PdfLexer.Benchmarks")]

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}