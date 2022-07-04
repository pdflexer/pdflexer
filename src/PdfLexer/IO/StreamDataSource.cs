using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;

namespace PdfLexer.IO
{
    internal class StreamDataSource : StreamBase
    {
        public StreamDataSource(ParsingContext ctx, Stream stream) : base(ctx, stream, true)
        {

        }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
        {
            try
            {
                return ParseObject(xref);
            }
            catch (PdfLexerException e)
            {
                Context.Error($"XRef offset for {xref.Reference} was not valid: " + e.Message);
                if (!TryRepairXRef(xref, out var repaired))
                {
                    return PdfNull.Value;
                }
                Context.Error("XRef offset repairs to " + repaired.Offset);
                UpdateXref(repaired);
                return ParseObject(xref);
            }
        }

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
            using var so = GetStreamOfContents(xref, filterName, streamLength);
            so.CopyTo(stream);
            stream.WriteByte((byte)'\n');
            stream.Write(IndirectSequences.endstream);
            
        }

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            try
            {
                CopyData(xref, destination.Stream);
            }
            catch (PdfLexerException e)
            {
                Context.Error($"XRef offset for {xref.Reference} was not valid: " + e.Message);
                if (!TryRepairXRef(xref, out var repaired))
                {
                    return; // will be null;
                }
                Context.CurrentOffset = repaired.Offset;
                Context.Error("XRef offset repairs to " + repaired.Offset);
                UpdateXref(repaired);
                CopyData(repaired, destination.Stream);
            }
        }

        public override IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);

            IPdfObject toReturn = null;
            var orig = Context.Options.Eagerness;
            Context.Options.Eagerness = Eagerness.FullEager;

            long nextOs = 0;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < _stream.Length - 1)
            {
                var cur = scanner.GetOffset();
                if (nextOs > cur) { scanner.Advance((int)(nextOs - cur)); cur = nextOs; }
                nextOs = cur + 1;
                scanner.ScanToToken(IndirectSequences.obj);
                var sl = scanner.ScanBackTokens(2, 20);
                if (sl == -1)
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                scanner.SkipCurrent();
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                scanner.SkipCurrent();
                if (scanner.Peek() != PdfTokenType.StartObj)
                {
                    continue;
                }
                scanner.SkipCurrent();

                if (scanner.Peek() != type)
                {
                    continue;
                }
                var obj = scanner.GetCurrentObject();
                if (matcher(obj))
                {
                    toReturn = obj;
                }
            }
            Context.Options.Eagerness = orig;
            reader.Complete();
            return toReturn;
        }

        private void UpdateXref(XRefEntry repaired)
        {
            Context.Document.xrefEntries[repaired.Reference.GetId()] = repaired;
        }

        private bool TryRepairXRef(XRefEntry entry, out XRefEntry repaired)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);
            repaired = new XRefEntry
            {
                IsFree = entry.IsFree,
                Reference = entry.Reference,
                Type = XRefType.Normal,
                Source = this,
            };
            long nextOs = 0;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < _stream.Length)
            {
                var cur = scanner.GetOffset();
                if (nextOs > cur) { scanner.Advance((int)(nextOs - cur)); cur = nextOs; }
                nextOs = cur + 1;
                scanner.ScanToToken(IndirectSequences.obj);
                var sl = scanner.ScanBackTokens(2, 20);
                if (sl == -1)
                {
                    continue;
                }

                var os = scanner.GetOffset();
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                var on = scanner.GetCurrentObject().GetValue<PdfNumber>();
                if (on != entry.Reference.ObjectNumber)
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                var gen = scanner.GetCurrentObject().GetValue<PdfNumber>();
                if (gen != entry.Reference.Generation)
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.StartObj)
                {
                    continue;
                }

                if (repaired.Offset == 0)
                {
                    repaired.Offset = os;
                    continue;
                }

                // todo should this just be last one in file?
                var currDiff = (int)Math.Abs(repaired.Offset - entry.Offset);
                var newDiff = (int)Math.Abs(os - entry.Offset);
                if (newDiff < currDiff)
                {
                    repaired.Offset = os;
                }
            }

            // find end
            if (repaired.Offset != 0)
            {
                _stream.Seek(repaired.Offset, SeekOrigin.Begin);
                reader = Context.Options.CreateReader(_stream);
                scanner = new PipeScanner(Context, reader);
                scanner.SkipExpected(PdfTokenType.NumericObj); // on
                scanner.SkipExpected(PdfTokenType.NumericObj); // gen
                scanner.SkipExpected(PdfTokenType.StartObj);
                var ot = scanner.Peek();
                if (ot == PdfTokenType.DictionaryStart)
                {
                    var dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();
                    scanner.SkipCurrent();
                    var after = scanner.Peek();
                    if (after == PdfTokenType.StartStream)
                    {
                        var l = dict.GetOptionalValue<PdfNumber>(PdfName.Length);
                        if (l == null)
                        {
                            if (scanner.ScanToToken(IndirectSequences.endstream))
                            {
                                scanner.SkipCurrent();
                                repaired.MaxLength = (int)(scanner.GetOffset() - repaired.Offset);
                                return true;
                            } else
                            {
                                return false;
                            }
                        } else
                        {
                            scanner.Advance(l.GetValue<PdfNumber>());
                            var eos = scanner.Peek();
                            if (eos == PdfTokenType.EndStream)
                            {
                                scanner.SkipCurrent();
                                repaired.MaxLength = (int)(scanner.GetOffset() - repaired.Offset);
                                return true;
                            } else
                            {
                                var fake = Math.Min(scanner.GetOffset() + 100, _stream.Length);
                                repaired.MaxLength = (int)(fake - repaired.Offset);
                                return true;
                            }
                        }
                    }
                } else
                {
                    scanner.SkipCurrent();
                }
                repaired.MaxLength =  (int)(scanner.GetOffset() - repaired.Offset);
            }

            return repaired.Offset != 0;

        }

        public override Stream GetStreamOfContents(XRefEntry xref, PdfName? filter, int predictedLength)
        {
            if (xref.KnownStreamStart == 0)
            {
                // this shouldn't happen -> bug
                throw new ApplicationException($"GetStreamOfContents called on non stream: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
            }
            if (xref.KnownStreamLength > 0)
            {
                _sub.Reset(xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
                return _sub;
            }
            _stream.Seek(xref.Offset + xref.KnownStreamStart + predictedLength, SeekOrigin.Begin);
            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);
            var nxt = scanner.Peek();
            reader.Complete();
            if (nxt == PdfTokenType.EndStream)
            {
                xref.KnownStreamLength = predictedLength;
                _sub.Reset(xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
                return _sub;
            }
            Context.Error($"Stream did not end with endstream: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
            if (!FindStreamEnd(xref, filter, predictedLength))
            {
                xref.KnownStreamLength = predictedLength;
                Context.Error($"Unable to find endstream, using provided length");
            }
            Context.Error($"Found endstream in contents, using repaired length: {xref.KnownStreamLength}");
            _sub.Reset(xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
            return _sub;
        }

        private bool FindStreamEnd(XRefEntry xref, PdfName filter, int predictedLength)
        {
            var bt = Math.Min(predictedLength, 50);
            var startOs = xref.Offset + xref.KnownStreamStart;
            _stream.Seek(startOs, SeekOrigin.Begin);
            var reader = Context.Options.CreateReader(_stream);
            var scanner = new PipeScanner(Context, reader);
            if (!scanner.TrySkipToToken(IndirectSequences.endstream, 5))
            {
                return false;
            }
            scanner.Reader.Rewind(2);
            scanner.Reader.TryRead(out var b1);
            scanner.Reader.TryRead(out var b2);
            var sub = 0;
            if (b1 == '\r' && b2 == '\n')
            {
                sub = 2;
            }
            else if (b2 == '\n')
            {
                sub = 1;
            }
            xref.KnownStreamLength = (int)(scanner.GetOffset() - sub);
            reader.Complete();
            return true;
        }
    }
}
