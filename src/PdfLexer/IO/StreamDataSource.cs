using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;

namespace PdfLexer.IO
{
    internal class StreamDataSource : StreamBase
    {
        public StreamDataSource(ParsingContext ctx, Stream stream) : base(ctx, stream, true)
        {

        }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
        {
            var buffer = GetRented(xref);
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                return Context.GetWrappedIndirectObject(xref, buffer); ;
            }
            catch (PdfLexerException e)
            {
                Context.Error($"XRef offset for {xref.Reference} was not valid: " + e.Message);
                if (!TryRepairXRef(xref, out var repaired))
                {
                    return PdfNull.Value;
                }
                Context.CurrentOffset = repaired.Offset;
                Context.Error("XRef offset repairs to " + repaired.Offset);
                UpdateXref(repaired);
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = GetRented(repaired);
                return Context.GetWrappedIndirectObject(repaired, buffer); ;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private byte[] GetRented(XRefEntry xref)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(xref.MaxLength);
            _stream.Seek(xref.Offset, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = _stream.Read(buffer, total, xref.MaxLength - total)) > 0)
            {
                total += read;
            }
            return buffer;
        }

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            var buffer = GetRented(xref);
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                Context.UnwrapAndCopyObjData(buffer, destination);
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
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = GetRented(repaired); 
                Context.UnwrapAndCopyObjData(buffer, destination);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var reader = PipeReader.Create(_stream);
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
            return toReturn;
        }

        private void UpdateXref(XRefEntry repaired)
        {
            Context.Document.xrefEntries[repaired.Reference.GetId()] = repaired;
        }

        private bool TryRepairXRef(XRefEntry entry, out XRefEntry repaired)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var reader = PipeReader.Create(_stream);
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
                reader = PipeReader.Create(_stream);
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
    }
}
