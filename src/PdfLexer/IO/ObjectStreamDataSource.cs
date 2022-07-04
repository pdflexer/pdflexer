using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PdfLexer.IO
{
    internal class ObjectStreamDataSource : IPdfDataSource
    {
        private byte[] _data;
        private List<int> _offsets;
        private int _start;
        private int SourceObject;

        public ObjectStreamDataSource(ParsingContext ctx, int sourceObject, byte[] data, List<int> oss, int start)
        {
            Context = ctx;
            SourceObject = sourceObject;
            _data = data;
            _offsets = oss;
            _start = start;
        }
        public long TotalBytes => _data.Length;

        public bool SupportsCloning => false;
        public ParsingContext Context { get;}

        public bool Disposed { get; private set; }

        public bool SupportsXRefRepair => false;

        public IPdfDataSource Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            stream.Write(_data, (int)startPosition, requiredBytes);
        }

        public void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
        {
            buffer = _data;
            buffer = buffer.Slice((int)startPosition, requiredBytes);
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            throw new NotImplementedException();
        }

        public IPdfObject GetIndirectObject(XRefEntry xref)
        {
            Debug.Assert(xref.ObjectStreamNumber == SourceObject);
            var os = _offsets[xref.ObjectIndex] + _start;
            Context.CurrentSource = this;
            Context.CurrentOffset = os;
            ReadOnlySpan<byte> data = _data;
            data = data.Slice(os);
            var orig = Context.Options.Eagerness;
            Context.Options.Eagerness = Eagerness.FullEager; // TODO fix so this works lazy
            var obj = Context.GetPdfItem(data, 0, out _);
            Context.Options.Eagerness = orig;
            return obj;
        }

        public Stream GetStream(long startPosition)
        {
            throw new NotImplementedException();
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            var os = _offsets[xref.ObjectIndex] + _start;
            Context.CurrentSource = this;
            Context.CurrentOffset = os;
            ReadOnlySpan<byte> data = _data;
            Context.UnwrapAndCopyObjData(data.Slice(os), destination);
        }

        public IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _data = null;
            Disposed = true;
        }

        public Stream GetStreamOfContents(XRefEntry xref, PdfName? filter, int predictedLength)
        {
            throw new NotImplementedException();
        }
    }
}
