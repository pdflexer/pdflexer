using PdfLexer.Parsers;

namespace PdfLexer.Filters;

internal class CryptFilter : IDecoder
{
    private ParsingContext _ctx;

    public CryptFilter(ParsingContext ctx)
    {
        _ctx = ctx;
    }
    public Stream Decode(Stream stream, PdfDictionary? filterParams) =>
        _ctx.Decryption.DecryptCryptStream(_ctx.CurrentReference, filterParams, stream);
    
}
