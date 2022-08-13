using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers.Structure;
using System.Buffers;

namespace PdfLexer.Parsers;

// TODO
// unify the two very similar repair scans
// probably scan once if it hasn't been scanned and then use that for any further xref errors
// add test coverage
internal static class StructuralRepairs
{
    public static IPdfObject? RepairFindLastMatching(ParsingContext ctx, Stream stream, PdfTokenType type, Func<IPdfObject, bool> matcher)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var reader = ctx.Options.CreateReader(stream);
        var scanner = new PipeScanner(ctx, reader);

        IPdfObject? toReturn = null;
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

    /// <summary>
    /// Builds xref table by scanning through pdf and looking for obj sequences
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static (List<XRefEntry>, PdfDictionary) BuildFromRawData(ParsingContext ctx, Stream stream)
    {
        var orig = ctx.Options.Eagerness;
        ctx.Options.Eagerness = Eagerness.FullEager;
        var offsets = new List<XRefEntry>();
        var dicts = new List<PdfDictionary>();
        var pipe = ctx.Options.CreateReader(stream);
        var scanner = new PipeScanner(ctx, pipe);
        var start = 0;
        while (true)
        {
            if (!scanner.TrySkipToToken(IndirectSequences.obj, 40))
            {
                break;
            }
            var pos = scanner.GetStartOffset() + start;

            var count = scanner.ScanBackTokens(2, 40);

            AddCurrent(ctx, ref scanner, count, offsets, dicts, start);

            // special handling in case malformed dictionary / array made us read past obj
            if (stream.Length == pos)
            {
                break;
            }
            start = (int)pos + 1;
            stream.Seek(start, SeekOrigin.Begin);
            pipe = ctx.Options.CreateReader(stream);
            scanner = new PipeScanner(ctx, pipe);
        }

        stream.Seek(0, SeekOrigin.Begin);
        pipe = ctx.Options.CreateReader(stream);
        scanner = new PipeScanner(ctx, pipe);
        var trailer = new PdfDictionary();
        // TODO need determine ordering
        foreach (var dict in dicts)
        {
            foreach (var item in dict)
            {
                trailer[item.Key] = item.Value;
            }
        }
        while (true)
        {
            if (!scanner.TrySkipToToken(XRefParser.Trailer, 0))
            {
                break;
            }
            scanner.SkipCurrent(); // trailer
            if (scanner.Peek() != PdfTokenType.DictionaryStart)
            {
                continue;
            }

            var dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();
            // TODO need determine ordering
            foreach (var item in dict)
            {
                trailer[item.Key] = item.Value;
            }
        }
        ctx.Options.Eagerness = orig;
        return (offsets, trailer);
    }

    // adds xref entry for object at current location
    private static void AddCurrent(ParsingContext ctx, ref PipeScanner scanner, int maxScan, List<XRefEntry> entries, List<PdfDictionary> dicts, long scanStart)
    {
        long offset = 0;
        var start = scanner.GetOffset();
        while (true)
        {
            scanner.Peek();

            for (; scanner.CurrentTokenType != PdfTokenType.NumericObj; scanner.SkipCurrent())
            { if (scanner.GetOffset() - start > maxScan) { return; } }

            offset = scanner.GetStartOffset();
            var n = scanner.GetCurrentObject().GetValue<PdfNumber>(); // N
            if (scanner.Peek() != PdfTokenType.NumericObj)
            {
                continue;
            }
            var g = scanner.GetCurrentObject().GetValue<PdfNumber>(); // G
            if (scanner.Peek() != PdfTokenType.StartObj)
            {
                continue;
            }
            scanner.SkipCurrent(); // obj

            entries.Add(new XRefEntry { Offset = offset + scanStart, Reference = new XRef { ObjectNumber = n, Generation = g } });
            if (scanner.Peek() == PdfTokenType.DictionaryStart)
            {
                var dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();
                var type = dict.GetOptionalValue<PdfName>(PdfName.TypeName);
                if (type == null)
                {
                    return;
                }

                if (type.Equals(PdfName.XRef)) // ignoring xref entries (raw rebuild) and just use for trailer info
                {
                    dicts.Add(dict);
                    return;
                }

                if (!type.Equals(PdfName.ObjStm))
                {
                    return;
                }
                if (scanner.Peek() != PdfTokenType.StartStream)
                {
                    return;
                }

                // this object is an object stream, need to grab xref entries for it
                // TODO low memory mode

                var length = dict.GetRequiredValue<PdfNumber>(PdfName.Length);
                scanner.SkipCurrent();
                var sequence = scanner.Read(length);
                PdfStreamContents contents = new PdfByteArrayStreamContents(sequence.ToArray());
                contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                var str = new PdfStream(dict, contents);
                var data = str.Contents.GetDecodedData();

                var current = GetOSOffsets(ctx, data, str.Dictionary.GetRequiredValue<PdfNumber>(PdfName.N), n);
                var first = str.Dictionary.GetRequiredValue<PdfNumber>(PdfName.First);
                var source = new ObjectStreamDataSource(ctx, n, data, current.Select(x => (int)x.Offset).ToList(), first);
                foreach (var item in current)
                {
                    item.Source = source;
                }
                entries.AddRange(current);
            }
            return;
        }
    }

    internal static List<XRefEntry> GetOSOffsets(ParsingContext ctx, ReadOnlySpan<byte> data, int count, int objectNumber)
    {
        var entries = new List<XRefEntry>();
        var c = 0;
        var scanner = new Scanner(ctx, data, 0);
        while (c < count)
        {
            var num = scanner.GetCurrentObject().GetValue<PdfNumber>(); // don't use object numbers currently
            var os = scanner.GetCurrentObject().GetValue<PdfNumber>();
            entries.Add(new XRefEntry
            {
                Type = XRefType.Compressed,
                ObjectIndex = c,
                ObjectStreamNumber = objectNumber,
                Reference = new XRef { ObjectNumber = num, Generation = 0 },
                Offset = os
            });
            c++;
        }
        return entries;
    }
}
