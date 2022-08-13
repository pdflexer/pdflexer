using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using PdfLexer.IO;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Structure;

internal class XRefParser
{
    private const int XrefBackScan = 250;
    public static byte[] startxref = new byte[9] {
        (byte)'s',(byte)'t',(byte)'a',(byte)'r',(byte)'t',(byte)'x',(byte)'r',(byte)'e',(byte)'f'
    };
    public static byte[] Trailer = new byte[7] {
        (byte)'t',(byte)'r',(byte)'a',(byte)'i',(byte)'l',(byte)'e',(byte)'r'
    };
    public static byte[] xref = new byte[4] {
        (byte)'x',(byte)'r',(byte)'e',(byte)'f'
    };
    public static byte[] oel = new byte[] {
        (byte)'\r',(byte)'\n',(byte)'e',(byte)'f'
    };
    private readonly ParsingContext _ctx;

    public XRefParser(ParsingContext ctx)
    {
        _ctx = ctx;
    }

    public (Dictionary<ulong, XRefEntry>, PdfDictionary?) LoadCrossReferences(IPdfDataSource pdf)
    {
        (List<XRefEntry> Refs, PdfDictionary Trailer) results = default;
        try
        {
            results = LoadCrossReference(pdf);
            results.Refs.Reverse(); // oldest prev entries returned first... look into cleaning this up
        }
        catch (Exception e)
        {
            _ctx.Error("Error during crossref parsing: " + e.Message);
            results = StructuralRepairs.BuildFromRawData(_ctx, pdf.GetStream(0));
        }

        var entries = new Dictionary<ulong, XRefEntry>();
        foreach (var entry in results.Refs)
        {
            entries[entry.Reference.GetId()] = entry;
        }
        UpdateRefs(results.Refs);

        pdf.Context.IsEncrypted = results.Trailer?.ContainsKey(PdfName.Encrypt) ?? false;
        return (entries, results.Trailer);

        void UpdateRefs(List<XRefEntry> refs)
        {
            if (refs.Any())
            {
                var ordered = refs.Where(x => x.Type == XRefType.Normal && !x.IsFree).OrderBy(x => x.Offset).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    var entry = ordered[i];
                    entry.Source = pdf;
                    Debug.Assert(entry.Offset < pdf.TotalBytes);
                    if (i + 1 < ordered.Count)
                    {
                        entry.MaxLength = (int)(ordered[i + 1].Offset - entry.Offset);
                    }
                }
                for (var i = 0; i < ordered.Count; i++)
                {
                    // if two have same offset.. fix logic later
                    if (i + 1 < ordered.Count && ordered[i].MaxLength == 0)
                    {
                        ordered[i].MaxLength = ordered[i + 1].MaxLength;
                    }
                }
                ordered[^1].MaxLength = (int)(pdf.TotalBytes - ordered[^1].Offset - 1);
            }
        }
    }

    // TODO clean this up
    internal (List<XRefEntry>, PdfDictionary) LoadCrossReference(IPdfDataSource source)
    {
        var orig = _ctx.Options.Eagerness;
        _ctx.Options.Eagerness = Eagerness.FullEager; // lazy objects don't currently work for sequencereader.. need to track offsets.
        var readStart = source.TotalBytes - XrefBackScan;
        if (readStart < 0)
        {
            readStart = 0;
        }

        var stream = source.GetStream(readStart);
        var pipe = _ctx.Options.CreateReader(stream);
        var scanner = new PipeScanner(_ctx, pipe);
        if (!scanner.TrySkipToToken(IndirectSequences.strartxref, 0))
        {
            throw new PdfLexerException("Unable to find startxref token.");
        }
        scanner.SkipCurrent();
        var nt = scanner.Peek();
        if (nt != PdfTokenType.NumericObj)
        {
            throw new PdfLexerException("Token following startxref was not numeric.");
        }
        var num = scanner.GetCurrentObject().GetValue<PdfNumber>();
        if (num > source.TotalBytes)
        {
            throw new PdfLexerException($"XRef offset larger than document size, {num} ({source.TotalBytes} size)");
        }
        long strStart = num;
        stream = source.GetStream(strStart);
        pipe = _ctx.Options.CreateReader(stream);
        scanner = new PipeScanner(_ctx, pipe);
        var result = scanner.Peek();
        var entries = new List<XRefEntry>();
        if (result == PdfTokenType.Xref)
        {
            scanner.SkipCurrent();
            var seq = scanner.ReadTo(Trailer);

            GetEntries(seq.IsSingleSegment ? seq.FirstSpan : seq.ToArray(), entries);

            scanner.SkipExpected(PdfTokenType.Trailer);
            var dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();
            var original = dict;
            while (true)
            {
                if (!dict.TryGetValue<PdfNumber>(PdfName.Prev, out var lastOffset))
                {
                    break;
                }

                var offset = (long)lastOffset;
                stream = source.GetStream(offset);
                pipe = _ctx.Options.CreateReader(stream);
                scanner = new PipeScanner(_ctx, pipe);
                result = scanner.Peek();
                if (result != PdfTokenType.Xref)
                {
                    throw new PdfLexerException("Prev entry did not point to xref token.");
                }
                scanner.SkipCurrent();
                seq = scanner.ReadTo(Trailer);
                GetEntries(seq.IsSingleSegment ? seq.FirstSpan : seq.ToArray(), entries);
                scanner.SkipExpected(PdfTokenType.Trailer);
                dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();
                foreach (var item in dict)
                {
                    if (!original.ContainsKey(item.Key))
                    {
                        original[item.Key] = item.Value;
                    }
                }
            }
            _ctx.Options.Eagerness = orig;
            return (entries, original);
        }
        else if (result == PdfTokenType.NumericObj)
        {
            var oss = num.GetValue<PdfNumber>();

            PdfDictionary original = null;
            while (true)
            {
                scanner.SkipCurrent(); // objnum
                scanner.SkipCurrent(); // gen
                scanner.SkipCurrent(); // R

                var dt = scanner.Peek();
                if (dt != PdfTokenType.DictionaryStart)
                {
                    throw new PdfLexerException("Xref offset pointed to object that was not dictionary.");
                }

                var dict = scanner.GetCurrentObject().GetValue<PdfDictionary>();

                var nxt = scanner.Peek();
                if (nxt != PdfTokenType.StartStream)
                {
                    throw new PdfLexerException("Xref offset pointed to dictionary that did not have stream.");
                }
                scanner.SkipCurrent();

                var len = dict.GetRequiredValue<PdfNumber>(PdfName.Length);

                PdfStreamContents contents = new PdfExistingStreamContents(source, strStart + scanner.GetOffset(), len);

                contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                var str = new PdfStream(dict, contents);
                var data = str.Contents.GetDecodedData();

                var index = dict.GetOptionalValue<PdfArray>(PdfName.Index);
                AddEntries(data, dict.GetRequiredValue<PdfArray>(PdfName.W), index, entries);
                oss = dict.GetOptionalValue<PdfNumber>(PdfName.Prev);

                if (original != null)
                {
                    foreach (var item in dict)
                    {
                        if (!original.ContainsKey(item.Key))
                        {
                            original[item.Key] = item.Value;
                        }
                    }
                } else
                {
                    original = dict;
                }
                if (oss == null)
                {
                    _ctx.Options.Eagerness = orig;
                    return (entries, original);
                }

                stream = source.GetStream(oss);
                strStart = oss;
                pipe = _ctx.Options.CreateReader(stream);
                scanner = new PipeScanner(_ctx, pipe);
                result = scanner.Peek();
                if (result != PdfTokenType.NumericObj)
                {
                    // TODO handle mixed XREf
                    throw new PdfLexerException($"Token that /Prev entry pointed to at {strStart} was not a numeric, expected N N obj. Got: " + result);
                }
            }
        }
        else
        {
            throw new PdfLexerException("Invalid token found at xref offset: " + result);
        }
    }

    private void AddXRefStream(PdfStream stream, List<XRefEntry> entries)
    {
        var data = stream.Contents.GetDecodedData();
        var index = stream.Dictionary.GetOptionalValue<PdfArray>(PdfName.Index);
        AddEntries(data, stream.Dictionary.GetRequiredValue<PdfArray>(PdfName.W), index, entries);
    }



    private void AddEntries(Span<byte> xd, PdfArray w, PdfArray index, List<XRefEntry> entries)
    {
        int l1 = w[0].GetValue<PdfNumber>();
        int l2 = w[1].GetValue<PdfNumber>();
        int l3 = w[2].GetValue<PdfNumber>();
        var lt = l1 + l2 + l3;
        var recCount = xd.Length / lt;
        var section = 0;
        var sectionObjectStart = 0;
        var sectionCount = 0;
        if (index == null)
        {
            sectionObjectStart = 0;
            sectionCount = recCount;
        }
        else
        {
            sectionObjectStart = (int)index[0].GetValue<PdfNumber>();
            sectionCount = (int)index[1].GetValue<PdfNumber>();
        }

        var total = 0;
        while (total < recCount)
        {
            for (var i = 0; i < sectionCount; i++)
            {
                var os = lt * total;
                var f1t = 0;
                for (var b = 0; b < l1; b++)
                {
                    f1t += xd[os + b] << (l1 - 1 - b) * 8;
                }
                var f2t = 0;
                for (var b = 0; b < l2; b++)
                {
                    f2t += xd[os + l1 + b] << (l2 - 1 - b) * 8;
                }
                var f3t = 0;
                for (var b = 0; b < l3; b++)
                {
                    f3t += xd[os + l1 + l2 + b] << (l3 - 1 - b) * 8;
                }
                switch (f1t)
                {
                    case 0:
                        entries.Add(new XRefEntry
                        {
                            IsFree = true,
                            Type = XRefType.Normal,
                            Reference = new XRef
                            {
                                ObjectNumber = f2t,
                                Generation = f3t
                            }
                        });
                        break;
                    case 1:
                        entries.Add(new XRefEntry
                        {
                            IsFree = false,
                            Type = XRefType.Normal,
                            Offset = f2t,
                            Reference = new XRef
                            {
                                ObjectNumber = sectionObjectStart + i,
                                Generation = f3t
                            }
                        });
                        break;
                    case 2:
                        entries.Add(new XRefEntry
                        {
                            IsFree = false,
                            Type = XRefType.Compressed,
                            ObjectStreamNumber = f2t,
                            ObjectIndex = f3t,
                            Reference = new XRef
                            {
                                ObjectNumber = sectionObjectStart + i,
                                Generation = 0
                            }
                        });
                        break;
                    default:
                        // ignore... will turn into null object by spec
                        break;
                }

                total++;
            }
            section++;
            if (index != null && section * 2 < index.Count - 1)
            {
                sectionObjectStart = (int)index[section * 2].GetValue<PdfNumber>();
                sectionCount = (int)index[section * 2 + 1].GetValue<PdfNumber>();
            }

        }
    }

    private async ValueTask<List<XRefEntry>> GetEntries(PipeReader pipe)
    {
        var entries = new List<XRefEntry>();
        while (true)
        {
            var result = await pipe.ReadAsync();
            if (TryProcess(result, entries, out var pos))
            {
                pipe.AdvanceTo(pos);
                return entries;
            }

            if (result.IsCanceled || result.IsCompleted)
            {
                throw new ApplicationException("Unable to find end of xref table.");
            }

            pipe.AdvanceTo(pos, result.Buffer.End);
        }
    }

    private bool TryProcess(ReadResult result, List<XRefEntry> entries, out SequencePosition position)
    {
        // TODO optimize... easy for now
        var reader = new SequenceReader<byte>(result.Buffer);
        var start = reader.Position;
        while (reader.TryAdvanceTo((byte)'t', false))
        {
            if (reader.IsNext(Trailer, false))
            {
                GetEntries(result.Buffer.Slice(start, reader.Position).ToArray(), entries);
                reader.Advance(7); // trailer
                position = reader.Position;
                return true;
            }
        }
        position = start;
        return false;
    }

    internal List<XRefEntry> GetEntries(ReadOnlySpan<byte> data, List<XRefEntry> entries)
    {
        var i = 0;
        while (data.Length > i)
        {
            var info = GetSectionHeader(data, ref i);
            GetSection(info.firtObjectNum, info.totalCount, entries, data, ref i);
            CommonUtil.SkipWhiteSpace(data, ref i);
        }

        return entries;
    }

    private (int firtObjectNum, int totalCount) GetSectionHeader(ReadOnlySpan<byte> data, ref int position)
    {
        var first = PdfSpanLexer.TryReadNextToken(data, out var type, position, out var length);
        if (first == -1 || type != PdfTokenType.NumericObj)
        {
            throw CommonUtil.DisplayDataErrorException(data, position, "Bad xref header entry");
        }
        var num1 = _ctx.NumberParser.Parse(data, first, length);

        var second = PdfSpanLexer.TryReadNextToken(data, out type, first + length, out length);
        if (second == -1 || type != PdfTokenType.NumericObj)
        {
            throw CommonUtil.DisplayDataErrorException(data, position, "Bad xref header entry");
        }

        var num2 = _ctx.NumberParser.Parse(data, second, length);

        position = second + length;
        return (num1, num2);
    }

    private void GetSection(int firtObjectNum, int totalCount, List<XRefEntry> entries, ReadOnlySpan<byte> data, ref int position)
    {
        for (var i = 0; i < totalCount; i++)
        {
            var entry = GetSingleEntry(data, firtObjectNum + i, position, out int nextPos);
            position = nextPos;
            entries.Add(entry);
        }
    }

    private XRefEntry GetSingleEntry(ReadOnlySpan<byte> data, int objectNumber, int startPos, out int nextPos)
    {
        var first = PdfSpanLexer.TryReadNextToken(data, out var type, startPos, out var length);
        if (first == -1 || type != PdfTokenType.NumericObj)
        {
            throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
        }
        Debug.Assert(length == 10, "Xref offset is 10 bytes");
        var num1 = _ctx.NumberParser.Parse(data, first, length);

        var second = PdfSpanLexer.TryReadNextToken(data, out type, first + length, out length);
        if (second == -1 || type != PdfTokenType.NumericObj)
        {
            throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
        }
        Debug.Assert(length == 5, "Xref gen number is 5 bytes");
        var num2 = _ctx.NumberParser.Parse(data, second, length);

        var pos = second + length;
        CommonUtil.SkipWhiteSpace(data, ref pos);

        bool isFree = false;
        var b = data[pos];
        if (b == (byte)'f')
        {
            isFree = true;
        }
        else if (b == (byte)'n')
        {
            isFree = false;
        }
        else
        {
            throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
        }

        if (!(num1 is PdfLongNumber ln))
        {
            throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry, offset not long");
        }
        if (!(num2 is PdfIntNumber intNum))
        {
            throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry, generation not int");
        }

        nextPos = pos + 1;
        CommonUtil.SkipWhiteSpace(data, ref nextPos);
        // TODO: add logging
        // Debug.Assert(nextPos - pos == 3, "nextPos - pos == 3, 2 EOL chars");

        return new XRefEntry
        {
            Offset = ln.Value,
            Reference = new XRef(objectNumber, intNum.Value),
            IsFree = isFree
        };
    }
}


