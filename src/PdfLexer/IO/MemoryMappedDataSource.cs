using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

#if NET6_0_OR_GREATER

using DotNext.IO.MemoryMappedFiles;

namespace PdfLexer.IO
{
    public class MemoryMappedDataSource : IPdfDataSource
    {
        private MemoryMappedDirectAccessor _accessor;
        private MemoryMappedFile _mm;

        public MemoryMappedDataSource(ParsingContext ctx, string filePath)
        {
            Context = ctx;
            TotalBytes = new System.IO.FileInfo(filePath).Length;
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

        public bool SupportsCloning => false;

        public ParsingContext Context { get; }

        public IPdfDataSource Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            GetData(startPosition, requiredBytes, out var buffer);
            stream.Write(buffer);
        }

        public void Dispose()
        {
            Disposed = true;
            _accessor.Dispose();
            _mm.Dispose();
        }

        public void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "MemoryMapped source does not support offsets greater than Int32.MaxValue");
            }
            if (requiredBytes > TotalBytes-startPosition)
            {
                throw new PdfLexerException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition;
            buffer = _accessor.Bytes.Slice((int)startPosition, requiredBytes);
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination) => this.UnwrapAndCopyFromSpan(xref, destination);

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            var str = _accessor.AsStream();
            return new SubStream(str, startPosition, desiredBytes, true);
        }

        public IPdfObject GetIndirectObject(XRefEntry xref) => this.GetWrappedFromSpan(xref);

        public Stream GetStream(long startPosition)
        {
            var str = _accessor.AsStream();
            str.Seek(startPosition, SeekOrigin.Begin);
            return str;
        }
    }
}


#endif