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
        private long _os;

        // TODO in memory larger than int.maxvalue bytes
        // TODO -> Memory<byte>??
        public InMemoryDataSource(ParsingContext ctx, byte[] data, long startOs=0)
        {
            Context = ctx;
            _data = data;
            _os = startOs;
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
            Context.CurrentOffset = startPosition;
            var s = (int)(startPosition - _os);
            var l = _data.Length - s;
            return new MemoryStream(_data, s, l, false, true);
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new MemoryStream(_data, (int)(startPosition-_os), desiredBytes, false, true);
        }

        public void GetData(long startPosition, int desiredBytes, out ReadOnlySpan<byte> data)
        {
            ReadOnlySpan<byte> span = _data;
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }
            var start = (int)(startPosition-_os);
            if (desiredBytes > _data.Length - start)
            {
                throw new PdfLexerException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition;
            data = span.Slice(start, desiredBytes);
        }

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

        public IPdfObject GetIndirectObject(XRefEntry xref) => this.GetWrappedFromSpan(xref);

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination) => this.UnwrapAndCopyFromSpan(xref, destination);

        public void Dispose()
        {
            _data = null;
            Disposed = true;
        }
    }
}
