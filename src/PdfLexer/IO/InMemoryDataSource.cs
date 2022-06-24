﻿using PdfLexer.Parsers;
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
        public InMemoryDataSource(ParsingContext ctx, byte[] data)
        {
            Context = ctx;
            _data = data;
            _ms = new MemoryStream(_data);
        }

        public long TotalBytes => _data.LongLength;
        public bool ReturnsCompleteData => true;
        public bool SupportsCloning => true;

        public ParsingContext Context {get;}

        public IPdfDataSource Clone() => throw new NotImplementedException(); // TODO currently setting Context.Current* on source so not sharable

        public Stream GetStream(long startPosition)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            _ms.Seek(startPosition, SeekOrigin.Begin);
            return _ms;
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
            var start = (int) startPosition;
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

            var start = (int) startPosition;
            if (requiredBytes > _data.Length - start)
            {
                throw new ApplicationException();
            }
            stream.Write(_data, (int) startPosition, requiredBytes);
        }

        public bool IsDataInMemory(long startPosition, int length) => true;
    }
}
