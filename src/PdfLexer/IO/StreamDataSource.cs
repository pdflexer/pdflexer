using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.IO;

namespace PdfLexer.IO
{
    internal class StreamDataSource : StreamBase
    {
        public StreamDataSource(ParsingContext ctx, Stream stream) : base(ctx, stream, true)
        {

        }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
            => ParseObject(xref);

        private IPdfObject ParseObject(XRefEntry xref)
        {
            _stream.Seek(xref.Offset, SeekOrigin.Begin);

            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            xref.KnownObjType = scanner.Peek();
            xref.KnownObjStart = (int)scanner.GetStartOffset();
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset + xref.KnownObjStart;
            var obj = scanner.GetCurrentObject();
            xref.KnownObjLength = (int)scanner.GetOffset() - xref.KnownObjStart;
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                scanner.SkipCurrent();
                xref.KnownStreamStart = (int)scanner.GetOffset();
            }
            else
            {
                Context.Error($"XRef obj did not end with endobj: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
                return obj;
            }

            if (!(obj is PdfDictionary dict))
            {
                Context.Error($"Obj followed by startstream was {obj.Type} instead of dictionary ({xref.Reference.ObjectNumber} {xref.Reference.Generation})");
                xref.KnownStreamStart = 0;
                return obj;
            }

            if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
            {
                Context.Error("Pdf dictionary followed by start stream token did not contain /Length.");
                streamLength = PdfCommonNumbers.Zero;
            }

            var contents = new PdfXRefStreamContents(this, xref, streamLength);
            contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
            contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
            return new PdfStream(dict, contents);
        }

        private void CopyData(XRefEntry xref, Stream stream)
        {
            _stream.Seek(xref.Offset, SeekOrigin.Begin);
            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            xref.KnownObjType = scanner.Peek();
            xref.KnownObjStart = (int)scanner.GetStartOffset();

            var dat = scanner.GetAndSkipCurrentData();
            foreach (var seg in dat)
            {
                stream.Write(seg.Span);
            }

            xref.KnownObjLength = (int)scanner.GetOffset() - xref.KnownObjStart;
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                scanner.SkipCurrent();
                xref.KnownStreamStart = (int)scanner.GetOffset();
            }
            else
            {
                Context.Error($"XRef obj did not end with endobj: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
            }
            var obj = Context.GetPdfItem(PdfObjectType.DictionaryObj, in dat);
            
            reader.Complete();

            if (!(obj is PdfDictionary dict))
            {
                Context.Error($"Obj followed by startstream was {obj.Type} instead of dictionary ({xref.Reference.ObjectNumber} {xref.Reference.Generation})");
                xref.KnownStreamStart = 0;
                return;
            }

            if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
            {
                Context.Error("Pdf dictionary followed by start stream token did not contain /Length.");
                streamLength = PdfCommonNumbers.Zero;
            }

            PdfName filterName = CommonUtil.GetFirstFilter(dict);

            stream.Write(IndirectSequences.stream);
            stream.WriteByte((byte)'\n');
            using var so = Context.GetStreamOfContents(xref, filterName, streamLength);
            so.CopyTo(stream);
            stream.WriteByte((byte)'\n');
            stream.Write(IndirectSequences.endstream);
            
        }

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
            => CopyData(xref, destination.Stream);

    }
}
