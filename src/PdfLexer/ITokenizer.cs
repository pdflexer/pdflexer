using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;


namespace PdfLexer
{

    internal interface IPdfDataSource
    {
        PdfLazyObject CreateLazy(long startPosition, int length, PdfObjectType type, bool indirect);
        int FillData(long startPosition, int desiredBytes, out Span<byte> data);
        void CopyData(long startPosition, int requiredBytes, Stream stream);
        void CopyData(PdfLazyObject obj, Stream stream);
    }
    internal class InMemoryDataSource : IPdfDataSource
    {
        private readonly byte[] _data;

        public InMemoryDataSource(byte[] data)
        {
            _data = data;
            _ms = new MemoryStream(_data);
        }

        // TODO lazy
        private Dictionary<PdfLazyObject, (long, int)> objects = new Dictionary<PdfLazyObject, (long, int)>();
        private MemoryStream _ms;

        public PdfLazyObject RegisterObject(long startPosition, int length, PdfObjectType type, bool indirect)
        {
            var obj = new PdfLazyObject
            {
                Source = this,
                IsIndirect = indirect,
                Parsed = null,
                Type = type
            };
            objects[obj] = (startPosition, length);
            return obj;
        }

        public PipeReader GetReader(long startPosition)
        {
            _ms.Seek(startPosition, SeekOrigin.Begin);
            return PipeReader.Create(_ms, new StreamPipeReaderOptions(leaveOpen: true));
        }

        public PdfLazyObject CreateLazy(long startPosition, int length, PdfObjectType type, bool indirect)
        {
            var obj = new PdfLazyObject
            {
                IsIndirect = indirect,
                Type = type,
                Source = this
            };
            objects[obj] = (startPosition, length);
            return obj;
        }

        public int FillData(long startPosition, int desiredBytes, out Span<byte> data)
        {
            // TODO
            var start = (int) startPosition;
            if (desiredBytes > _data.Length - start)
            {
                desiredBytes = _data.Length - start;
            }
            data = new Span<byte>(_data, start, desiredBytes);
            return desiredBytes;
        }
        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            var start = (int) startPosition;
            if (requiredBytes > _data.Length - start)
            {
                throw new ApplicationException();
            }
            stream.Write(_data, (int) startPosition, requiredBytes); // TODO
        }

        public void CopyData(PdfLazyObject obj, Stream stream)
        {
            var data = objects[obj];
            CopyData(data.Item1, data.Item2, stream);
        }
    }
}
