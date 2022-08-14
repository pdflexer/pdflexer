using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.IO;

internal abstract class StreamBase : IPdfDataSource
{
    public long TotalBytes { get; }

    public bool SupportsCloning => false;

    public ParsingContext Context => _ctx;

    private readonly ParsingContext _ctx;
    protected Stream _stream;
    private readonly bool _leaveOpen;
    protected readonly SubStream _sub;

    public StreamBase(ParsingContext ctx, Stream stream, bool leaveOpen)
    {
        if (!stream.CanSeek)
        {
            throw new NotSupportedException("Streams must be seekable.");
        }
        TotalBytes = stream.Length;
        _ctx = ctx;
        _stream = stream;
        _leaveOpen = leaveOpen;
        _sub = new SubStream(stream, 0, stream.Length, false);
    }

    public IPdfDataSource Clone()
    {
        throw new NotSupportedException("Stream Pdf data source does not support cloning");
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

    public void Dispose()
    {
        if (!_leaveOpen) { _stream.Dispose(); }
        _stream = null!;
        _sub.ActuallyDispose(true);
        Disposed = true;
    }

    public void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
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
        Context.CurrentSource = this;
        Context.CurrentOffset = startPosition;
    }

    public Stream GetDataAsStream(long startPosition, int desiredBytes)
    {
        if (desiredBytes > TotalBytes - startPosition)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }
        Context.CurrentSource = this;
        Context.CurrentOffset = startPosition;
        _sub.Reset(startPosition, desiredBytes);
        return _sub;
    }

    public Stream GetStream(long startPosition)
    {
        if (startPosition > TotalBytes - 1)
        {
            throw new ApplicationException("More data requested from data source than available.");
        }
        Context.CurrentSource = this;
        Context.CurrentOffset = startPosition;
        _sub.Reset(startPosition, TotalBytes - startPosition);
        return _sub;
    }

    public abstract IPdfObject GetIndirectObject(XRefEntry xref);

    public abstract void CopyIndirectObject(XRefEntry xref, WritingContext destination);

}
