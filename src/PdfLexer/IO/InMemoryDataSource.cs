using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.IO
{
    public class InMemoryDataSource : IPdfDataSource
    {

        private byte[] _data;
        // TODO in memory larger than int.maxvalue bytes
        // TODO -> Memory<byte>??
        public InMemoryDataSource(ParsingContext ctx, byte[] data)
        {
            Context = ctx;
            _data = data;
        }

        public long TotalBytes => _data.LongLength;
        public bool ReturnsCompleteData => true;
        public bool SupportsCloning => true;

        public ParsingContext Context { get; }

        public bool Disposed { get; private set; }
        public IPdfDataSource Clone() => throw new NotImplementedException(); // TODO currently setting Context.Current* on source so not sharable

        public Stream GetStream(long startPosition)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new MemoryStream(_data, (int)startPosition, _data.Length - (int)startPosition, false, true);
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new MemoryStream(_data, (int)startPosition, desiredBytes, false, true);
        }

        public void GetData(long startPosition, int desiredBytes, out ReadOnlySpan<byte> data)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }
            var start = (int)startPosition;
            if (desiredBytes > _data.Length - start)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            data = new Span<byte>(_data, start, _data.Length - start);
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }

            var start = (int)startPosition;
            if (requiredBytes > _data.Length - start)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            stream.Write(_data, (int)startPosition, requiredBytes);
        }

        public IPdfObject GetIndirectObject(XRefEntry xref)
        {
            ReadOnlySpan<byte> buffer = _data;
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            return Context.GetWrappedIndirectObject(xref, buffer.Slice((int)xref.Offset, xref.MaxLength));
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            ReadOnlySpan<byte> buffer = _data;
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            Context.UnwrapAndCopyObjData(buffer.Slice((int)xref.Offset, xref.MaxLength), destination);
        }

        public void Dispose()
        {
            _data = null;
            Disposed = true;
        }
    }
}
