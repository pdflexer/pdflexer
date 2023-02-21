using PdfLexer.IO;

namespace PdfLexer;

/// <summary>
/// Wrapper object providing access to an unparsed PDF object.
/// </summary>
internal class PdfLazyObject : PdfObject
{
    public override bool IsLazy => true;
    public override PdfObjectType Type => LazyObjectType;
    public PdfObjectType LazyObjectType { get; set; }
    public bool HasLazyIndirect { get; set; }
    public long Offset {get; set; }
    public int Length {get; set; }
    public IPdfDataSource Source { get; set; }
    public IPdfObject? Parsed { get; set; }

    public PdfLazyObject(IPdfDataSource source, long offset, int length, PdfObjectType type)
    {
        Source = source;
        Offset = offset;
        Length = length;
        LazyObjectType = type;
    }

    public IPdfObject Resolve() => Resolve(ParsingContext.Current);
    public IPdfObject Resolve(ParsingContext ctx)
    {
        if (Parsed != null)
        {
            return Parsed;
        }
        if (Source.Disposed)
        {
            throw new PdfLexerException("Attempted to parse lazy object from disposed source.");
        }
        
        Source.GetData(ctx,Offset, Length, out var buffer);
        Parsed = ctx.GetKnownPdfItem(LazyObjectType, buffer, 0, Length, Source.Document);
        return Parsed;
    }
}
