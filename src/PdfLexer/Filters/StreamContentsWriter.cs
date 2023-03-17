using System.IO.Compression;

namespace PdfLexer.Filters;

public interface IStreamContentsWriter
{
    Stream Stream { get; }
    PdfStreamContents Complete();
}


#if NET6_0_OR_GREATER

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

    public override long Position { get => ZLibStream.Position; set => throw new NotSupportedException(); }

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

    public override void Write(byte[] buffer, int offset, int count) => ZLibStream.Write(buffer, offset, count);

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
#else
public class ZLibLexerStream : Stream, IStreamContentsWriter
{
    private uint s1 = 1;
    private uint s2 = 0;
    private const int mod = 65521;
    private Stream FlateStream { get; }
    private Stream RawStream { get; }

    public ZLibLexerStream() : this(new MemoryStream(), CompressionLevel.Fastest)
    {

    }

    public ZLibLexerStream(Stream stream, CompressionLevel level)
    {
        stream.WriteByte(120);
        stream.WriteByte(1);
        FlateStream = new DeflateStream(stream, level, true);
        RawStream = stream;
    }
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => FlateStream.Length;

    public override long Position { get => FlateStream.Position; set => throw new NotSupportedException(); }

    public Stream Stream => this;

    public override void Flush()
    {
        FlateStream.Flush();
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
        for (var i=offset;i<offset+count;i++)
        {
            s1 += buffer[i];
            s2 += s1;
            s1 %= mod;
            s2 %= mod;
        }

        FlateStream.Write(buffer, offset, count);
    }

    bool completed = false;
    public new void Dispose()
    {
        if (!completed)
        {
            WriteEnd();
        }
    }

    private void WriteEnd()
    {
        var cs = s2 * 65536 + s1;
        base.Dispose();
        FlateStream.Flush();
        FlateStream.Dispose();
        RawStream.WriteByte((byte)((cs >> 24) & 0xff));
        RawStream.WriteByte((byte)((cs >> 16) & 0xff));
        RawStream.WriteByte((byte)((cs >> 8) & 0xff));
        RawStream.WriteByte((byte)(cs & 0xff));
        completed = true;
    }
    public PdfStreamContents Complete()
    {
        if (!completed)
        {
            WriteEnd();
        }
        if (RawStream is MemoryStream ms)
        {
            var dat = ms.ToArray();
            ms.Dispose();
            return new PdfByteArrayStreamContents(dat, PdfName.FlateDecode, null);
        } else
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
#endif
