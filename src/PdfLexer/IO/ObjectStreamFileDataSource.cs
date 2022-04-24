using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    internal class ObjectStreamFileDataSource : StreamBase
    {
        private List<int> _offsets;
        private int _start;
        private int SourceObject;
        public ObjectStreamFileDataSource(ParsingContext ctx, int sourceObject, Stream data, List<int> oss, int start) : base(ctx, data, false)
        {
            SourceObject = sourceObject;
            _offsets = oss;
            _start = start;
        }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
        {
            Debug.Assert(xref.ObjectStreamNumber == SourceObject);
            var data = GetRented(xref);
            var orig = Context.Options.Eagerness;
            Context.Options.Eagerness = Eagerness.FullEager; // TODO fix so this works lazy
            var obj = Context.GetPdfItem(data, 0, out _);
            Context.Options.Eagerness = orig;
            ArrayPool<byte>.Shared.Return(data);
            return obj;
        }

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            var data = GetRented(xref);
            Context.UnwrapAndCopyObjData(data, destination);
            ArrayPool<byte>.Shared.Return(data);
        }


        public override IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            throw new NotImplementedException();
        }

        private byte[] GetRented(XRefEntry xref)
        {
            var os = _offsets[xref.ObjectIndex] + _start;
            var length = (int)(_stream.Length - os);
            if (_offsets.Count > xref.ObjectIndex + 1)
            {
                length = _offsets[xref.ObjectIndex + 1] + _start - os;
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = os;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            _stream.Seek(os, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = _stream.Read(buffer, total, length - total)) > 0)
            {
                total += read;
            }
            return buffer;
        }
    }
}
