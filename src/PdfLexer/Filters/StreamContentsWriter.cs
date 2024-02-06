using System.IO.Compression;

namespace PdfLexer.Filters;

public interface IStreamContentsWriter
{
    Stream Stream { get; }
    PdfStreamContents Complete();
}


public class ZLibLexerStream : Stream, IStreamContentsWriter
{
    private Stream RawStream { get; }
    private Stream ZLibStream { get; }

    public ZLibLexerStream() : this(new MemoryStream(), CompressionLevel.Fastest)
    {

    }

    public ZLibLexerStream(Stream stream, CompressionLevel level)
    {
        ZLibStream = new ZLibStream(stream, level, true);
        RawStream = stream;
    }
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => ZLibStream.Length;

    private long position;
    public override long Position { get => position;  set => throw new NotSupportedException(); }

    public Stream Stream => ZLibStream;

    public override void Flush()
    {
        ZLibStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ZLibStream.Write(buffer, offset, count);
        position += count;
    }

    bool completed = false;

    public new void Dispose()
    {
        if (!completed)
        {
            End();
        }
    }
    private void End()
    {
        ZLibStream.Flush();
        ZLibStream.Dispose();
        base.Dispose();
        completed = true;
    }
    public PdfStreamContents Complete()
    {
        if (!completed)
        {
            End();
        }
        if (RawStream is MemoryStream ms)
        {
            var dat = ms.ToArray();
            ms.Dispose();
            return new PdfByteArrayStreamContents(dat, PdfName.FlateDecode, null);
        }
        else
        {
            var nms = new MemoryStream();
            RawStream.CopyTo(nms);
            var dat = nms.ToArray();
            RawStream.Dispose();
            nms.Dispose();
            return new PdfByteArrayStreamContents(dat, PdfName.FlateDecode, null);
        }
    }
}
