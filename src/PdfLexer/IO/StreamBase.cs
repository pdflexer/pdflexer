using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.IO;

internal abstract class StreamBase : IPdfDataSource
{
    public long TotalBytes { get; }


    public PdfDocument Document => _doc;
    private readonly PdfDocument _doc;

    protected Stream _stream;
    private readonly bool _leaveOpen;
    protected readonly SubStream _sub;

    public StreamBase(PdfDocument doc, Stream stream, bool leaveOpen)
    {
        if (!stream.CanSeek)
        {
            throw new NotSupportedException("Streams must be seekable.");
        }
        TotalBytes = stream.Length;
        _doc = doc;
        _stream = stream;
        _leaveOpen = leaveOpen;
        _sub = new SubStream(stream, 0, stream.Length, false);
    }
    public void CopyData(long startPosition, int requiredBytes, Stream stream)
    {
        if (requiredBytes > TotalBytes - startPosition)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }
        _sub.Reset(startPosition, requiredBytes);
        _sub.CopyTo(stream);
    }

    public bool Disposed { get; private set; }

    public abstract bool IsEncrypted { get; }

    public void Dispose()
    {
        if (!_leaveOpen) { _stream.Dispose(); }
        _stream = null!;
        _sub.ActuallyDispose(true);
        Disposed = true;
    }
    public abstract IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref);

    public abstract void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination);

    public void GetData(ParsingContext ctx, long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
    {
        if (requiredBytes > TotalBytes - startPosition)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }

        var data = new byte[requiredBytes];
        _stream.Seek(startPosition, SeekOrigin.Begin);
        int total = 0;
        int read;
        while ((read = _stream.Read(data, total, requiredBytes - total)) > 0)
        {
            total += read;
        }
        buffer = data;
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
    }

    public Stream GetDataAsStream(ParsingContext ctx, long startPosition, int desiredBytes)
    {
        if (desiredBytes > TotalBytes - startPosition)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        _sub.Reset(startPosition, desiredBytes);
        return _sub;
    }

    public Stream GetStream(ParsingContext ctx, long startPosition)
    {
        if (startPosition > TotalBytes - 1)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        _sub.Reset(startPosition, TotalBytes - startPosition);
        return _sub;
    }
}
