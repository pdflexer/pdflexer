using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    public class InMemoryDataSource : IPdfDataSource
    {
        private readonly byte[] _data;
        private readonly MemoryStream _ms;
        // TODO in memory larger than int.maxvalue bytes
        // TODO -> Memory<byte>??
        public InMemoryDataSource(byte[] data)
        {
            _data = data;
            _ms = new MemoryStream(_data);
        }


        public long TotalBytes => _data.LongLength;
        public bool ReturnsCompleteData => true;
        public bool SupportsCloning => true;
        public IPdfDataSource Clone() => this;

        public Stream GetStream(long startPosition)
        {
            _ms.Seek(startPosition, SeekOrigin.Begin);
            return _ms;
        }

        public void FillData(long startPosition, int desiredBytes, out ReadOnlySpan<byte> data)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }
            var start = (int) startPosition;
            if (desiredBytes > _data.Length - start)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            data = new Span<byte>(_data, start, _data.Length - start);
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }

            var start = (int) startPosition;
            if (requiredBytes > _data.Length - start)
            {
                throw new ApplicationException();
            }
            stream.Write(_data, (int) startPosition, requiredBytes);
        }
    }
}
