using PdfLexer.Lexing;
using PdfLexer.Parsers.Structure;
using System;
using System.IO;

namespace PdfLexer.Parsers
{
    internal class StructuralRepairs
    {
        public static IPdfObject RepairFindLastMatching(ParsingContext ctx, Stream stream, PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var reader = ctx.Options.CreateReader(stream);
            var scanner = new PipeScanner(ctx, reader);

            IPdfObject toReturn = null;
            var orig = ctx.Options.Eagerness;
            ctx.Options.Eagerness = Eagerness.FullEager;

            long nextOs = 0;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < stream.Length - 1)
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
            ctx.Options.Eagerness = orig;
            reader.Complete();
            return toReturn;
        }

        public static bool TryRepairXRef(ParsingContext ctx, XRefEntry entry, out XRefEntry repaired)
        {
            var stream = entry.Source.GetStream(0);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = ctx.Options.CreateReader(stream);
            var scanner = new PipeScanner(ctx, reader);
            repaired = new XRefEntry
            {
                IsFree = entry.IsFree,
                Reference = entry.Reference,
                Type = XRefType.Normal,
                Source = entry.Source,
            };
            long nextOs = 0;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < stream.Length)
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
                stream.Seek(repaired.Offset, SeekOrigin.Begin);
                reader = ctx.Options.CreateReader(stream);
                scanner = new PipeScanner(ctx, reader);
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
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            scanner.Advance(l.GetValue<PdfNumber>());
                            var eos = scanner.Peek();
                            if (eos == PdfTokenType.EndStream)
                            {
                                scanner.SkipCurrent();
                                repaired.MaxLength = (int)(scanner.GetOffset() - repaired.Offset);
                                return true;
                            }
                            else
                            {
                                var fake = Math.Min(scanner.GetOffset() + 100, stream.Length);
                                repaired.MaxLength = (int)(fake - repaired.Offset);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    scanner.SkipCurrent();
                }
                repaired.MaxLength = (int)(scanner.GetOffset());
            }

            return repaired.Offset != 0;

        }

        public static bool TryFindStreamEnd(ParsingContext ctx, XRefEntry xref, PdfName filter, int predictedLength)
        {
            var startOs = xref.Offset + xref.KnownStreamStart;
            var stream = xref.Source.GetStream(startOs);
            var reader = ctx.Options.CreateReader(stream);
            var scanner = new PipeScanner(ctx, reader);
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
