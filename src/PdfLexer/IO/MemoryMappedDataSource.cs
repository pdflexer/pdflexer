using PdfLexer.Lexing;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System.IO.MemoryMappedFiles;

#if NET6_0_OR_GREATER

using DotNext.IO.MemoryMappedFiles;

namespace PdfLexer.IO;

internal class MemoryMappedDataSource : IPdfDataSource
{
    private readonly MemoryMappedFile _mm;
    private readonly FileStream _str;
    private readonly List<MemoryMappedDirectAccessor> _accessors;

    public MemoryMappedDataSource(PdfDocument doc, string filePath)
    {
        Document = doc;
        TotalBytes = new FileInfo(filePath).Length;
        _mm = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, TotalBytes, MemoryMappedFileAccess.Read);
        _str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // .CreateViewStream();
        _accessors = new List<MemoryMappedDirectAccessor>();
        if (TotalBytes <= int.MaxValue)
        {
            _accessors.Add(_mm.CreateDirectAccessor(0, TotalBytes, MemoryMappedFileAccess.Read));
        }
        else
        {
            // 2,147,483,647 max
            // split into 1,5g chunks (overlapping)
            var t = Math.Ceiling(TotalBytes / 1500000000.0);
            for (var i = 0; i < t; i++)
            {
                var start = 1500000000L * i;
                var length = (long)Int32.MaxValue;
                if (length + start > TotalBytes) { length = TotalBytes - start; }
                _accessors.Add(_mm.CreateDirectAccessor(start, length, MemoryMappedFileAccess.Read));
            }
        }
    }
    public bool Disposed { get; private set; }

    public long TotalBytes { get; private set; }

    public bool IsEncrypted => Document.IsEncrypted;

    public PdfDocument Document { get; }

    private (MemoryMappedDirectAccessor, long) GetAccessor(long startPosition, int requiredBytes)
    {
        if (_accessors.Count == 1) { return (_accessors[0], 0); }
        var i = (int)(startPosition / 1500000000L);
        return (_accessors[i], i * 1500000000L);
    }


    public void CopyData(long startPosition, int requiredBytes, Stream stream)
    {
        if (startPosition > TotalBytes)
        {
            throw new NotSupportedException(
                "More data requested from data source than available");
        }
        if (requiredBytes > TotalBytes - startPosition)
        {
            throw new PdfLexerException("More data requested from data source than available.");
        }
        var (accessor, offset) = GetAccessor(startPosition, requiredBytes);
        var buffer = accessor.Bytes.Slice((int)(startPosition - offset), requiredBytes);
        stream.Write(buffer);
    }


    public void GetData(ParsingContext ctx, long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
    {
        if (startPosition > TotalBytes)
        {
            throw new NotSupportedException(
                "More data requested from data source than available");
        }
        if (requiredBytes > TotalBytes - startPosition)
        {
            throw new PdfLexerException("More data requested from data source than available.");
        }

        var (accessor, offset) = GetAccessor(startPosition, requiredBytes);

        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        if (requiredBytes == -1)
        {
            buffer = accessor.Bytes.Slice((int)(startPosition-offset));
        }
        else
        {
            buffer = accessor.Bytes.Slice((int)(startPosition - offset), requiredBytes);
        }
    }

    public Stream GetDataAsStream(ParsingContext ctx, long startPosition, int desiredBytes)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        return new SubStream(_str, startPosition, desiredBytes, false);
    }

    public Stream GetStream(ParsingContext ctx, long startPosition)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        _str.Seek(startPosition, SeekOrigin.Begin);
        return _str;
    }

    public IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref) => this.GetWrappedFromSpan(ctx, xref);

    public void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination) => this.UnwrapAndCopyFromSpan(ctx, xref, destination);


    public void Dispose()
    {
        Disposed = true;
        _accessors.ForEach(x => x.Dispose());
        _str.Dispose();
        _mm.Dispose();
    }

}


#endif
