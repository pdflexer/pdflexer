using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer.IO;

internal class InMemoryDataSource : IPdfDataSource
{

    private byte[] _data;
    private readonly long _os;

    // TODO in memory larger than int.maxvalue bytes
    // TODO -> Memory<byte>??
    public InMemoryDataSource(PdfDocument doc, byte[] data, long startOs = 0)
    {
        Document = doc;
        _data = data;
        _os = startOs;
    }

    public long TotalBytes => _data.LongLength;

    public PdfDocument Document { get; }

    public bool Disposed { get; private set; }

    public bool IsEncrypted => Document.IsEncrypted;

    public void CopyData(long startPosition, int requiredBytes, Stream stream)
    {
        if (startPosition > int.MaxValue)
        {
            throw new PdfLexerException(
                "In memory data source does not support offsets greater than Int32.MaxValue");
        }

        var start = (int)(startPosition - _os);
        if (requiredBytes > _data.Length - start)
        {
            throw new PdfLexerException("More data requested from data source than available.");
        }
        stream.Write(_data, start, requiredBytes);
    }

    public void GetData(ParsingContext ctx, long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
    {
        ReadOnlySpan<byte> span = _data;
        if (startPosition > int.MaxValue)
        {
            throw new NotSupportedException(
                "In memory data source does not support offsets greater than Int32.MaxValue");
        }
        var start = (int)(startPosition - _os);
        if (requiredBytes > _data.Length - start)
        {
            throw new PdfLexerException("More data requested from data source than available.");
        }
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        if (requiredBytes == -1)
        {
            buffer = span[start..];
        }
        else
        {
            buffer = span.Slice(start, requiredBytes);
        }
    }

    public Stream GetDataAsStream(ParsingContext ctx, long startPosition, int desiredBytes)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        return new MemoryStream(_data, (int)(startPosition - _os), desiredBytes, false, true);
    }

    public Stream GetStream(ParsingContext ctx, long startPosition)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        var s = (int)(startPosition - _os);
        var l = _data.Length - s;
        return new MemoryStream(_data, s, l, false, true);
    }

    public IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref) => this.GetWrappedFromSpan(ctx, xref);

    public void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination) => this.UnwrapAndCopyFromSpan(ctx, xref, destination);

    public void Dispose()
    {
        _data = null!;
        Disposed = true;
    }

}
