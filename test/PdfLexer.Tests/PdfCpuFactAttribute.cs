using Xunit;

namespace PdfLexer.Tests;

public sealed class PdfCpuFactAttribute : FactAttribute
{
    public PdfCpuFactAttribute()
    {
        Skip = SyntaxValidation.GetSkipReason();
    }
}
