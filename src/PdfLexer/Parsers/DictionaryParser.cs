namespace PdfLexer.Parsers;

internal class DictionaryParser : Parser<PdfDictionary>
{
    private readonly ParsingContext _ctx;

    public DictionaryParser(ParsingContext ctx)
    {
        _ctx = ctx;
    }

    public override PdfDictionary Parse(ReadOnlySpan<byte> buffer) => Parse(buffer, 0, buffer.Length);

    public override PdfDictionary Parse(ReadOnlySpan<byte> buffer, int start, int length)
    {
        var dict = _ctx.NestedParser.ParseNestedItem(_ctx.CurrentSource?.Document, buffer, start, out _) as PdfDictionary;
        return dict!;
    }
}
