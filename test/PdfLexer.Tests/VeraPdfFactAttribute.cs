using Xunit;

namespace PdfLexer.Tests;

internal static class VeraPdfTestCollection
{
    public const string Name = "veraPDF";
}

[CollectionDefinition(VeraPdfTestCollection.Name, DisableParallelization = true)]
public sealed class VeraPdfTestCollectionDefinition
{
}

public sealed class VeraPdfFactAttribute : FactAttribute
{
    public VeraPdfFactAttribute()
    {
        Skip = VeraPdfValidation.GetSkipReason();
    }
}
