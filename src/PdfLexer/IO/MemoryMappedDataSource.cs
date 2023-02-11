using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System.IO.MemoryMappedFiles;
using System;

#if NET6_0_OR_GREATER

using DotNext.IO.MemoryMappedFiles;

namespace PdfLexer.IO;

internal class MemoryMappedDataSource : IPdfDataSource
{
    private readonly MemoryMappedDirectAccessor _accessor;
    private readonly MemoryMappedFile _mm;

    public MemoryMappedDataSource(PdfDocument doc, string filePath)
    {
        Document = doc;
        TotalBytes = new FileInfo(filePath).Length;
        if (TotalBytes > int.MaxValue)
        {
            throw new NotSupportedException(
                "MemoryMapped source does not support sizes greater than Int32.MaxValue");
        }
        _mm = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
        _accessor = _mm.CreateDirectAccessor(0, TotalBytes, MemoryMappedFileAccess.Read);
    }
    public bool Disposed { get; private set; }

    public long TotalBytes { get; private set; }

    public bool IsEncrypted => Document.IsEncrypted;

    public PdfDocument Document { get; }


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
        var buffer = _accessor.Bytes.Slice((int)startPosition, requiredBytes);
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
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        if (requiredBytes == -1)
        {
            buffer = _accessor.Bytes.Slice((int)startPosition);
        }
        else
        {
            buffer = _accessor.Bytes.Slice((int)startPosition, requiredBytes);
        }
    }

    public Stream GetDataAsStream(ParsingContext ctx, long startPosition, int desiredBytes)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        var str = _accessor.AsStream();
        return new SubStream(str, startPosition, desiredBytes, true);
    }

    public Stream GetStream(ParsingContext ctx, long startPosition)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        var str = _accessor.AsStream();
        str.Seek(startPosition, SeekOrigin.Begin);
        return str;
    }

    public IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref) => this.GetWrappedFromSpan(ctx, xref);

    public void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination) => this.UnwrapAndCopyFromSpan(ctx, xref, destination);


    public void Dispose()
    {
        Disposed = true;
        _accessor.Dispose();
        _mm.Dispose();
    }

}


#endif