namespace PdfLexer.Filters;

internal class CryptFilter : IDecoder
{
    private PdfDocument _doc;

    public CryptFilter(PdfDocument doc)
    {
        _doc = doc;
    }
    public Stream Decode(Stream stream, PdfDictionary? filterParams)
    {
        var ctx = ParsingContext.Current;
        return _doc.Decryption.DecryptCryptStream(ctx, ctx.CurrentReference, filterParams, stream);
    }
        
    
}
