namespace PdfLexer.Parsers;

internal class ArrayParser : Parser<PdfArray>
{
    private ParsingContext _ctx;

    public ArrayParser(ParsingContext ctx)
    {
        _ctx = ctx;
    }
    public override PdfArray Parse(ReadOnlySpan<byte> buffer)
    {
        var obj = _ctx.NestedParser.ParseNestedItem(_ctx.CurrentSource?.Document, buffer, 0, out _) as PdfArray;
        return obj!;
    }
}
