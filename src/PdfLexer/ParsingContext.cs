﻿using System.Buffers;
using System.Buffers.Text;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.IO;
using PdfLexer.Content;
using PdfLexer.Filters;
using PdfLexer.Fonts;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Nested;
using PdfLexer.Parsers.Structure;

namespace PdfLexer;


public class ParsingContext : IDisposable
{
    public static ParsingContext CreateLowMemory()
    {
        var options = new ParsingOptions();
        options.CacheNames = false;
        options.CacheNumbers = false;
        options.LowMemoryMode = true;
        var ctx = new ParsingContext(options);
        return ctx;
    }
    public static ParsingContext Current
    {
        get
        {
            var val = CurrentCtx.Value;
            if (val == null || val.disposed)
            {
                val = new ParsingContext();
                CurrentCtx.Value = val;
            }
            return val;
        }
    }

    public static ParsingContext Reset()
    {
        var ctx = CurrentCtx.Value = new ParsingContext();
        return ctx;
    }
    private static AsyncLocal<ParsingContext?> CurrentCtx = new AsyncLocal<ParsingContext?>();
    // tracked here to support encryption
    internal ulong CurrentReference { get; set; }
    // tracked here to support lazy parsing
    internal long CurrentOffset { get; set; }
    // tracked here to support lazy parsing
    internal IPdfDataSource? CurrentSource { get; set; }
    internal Dictionary<int, PdfIntNumber> CachedInts = new Dictionary<int, PdfIntNumber>();
    internal NumberCache NumberCache = new NumberCache();
    internal NameCache NameCache = new NameCache();
    internal NumberParser NumberParser { get; }
    internal DecimalParser DecimalParser { get; }
    internal ArrayParser ArrayParser { get; }
    internal BoolParser BoolParser { get; }
    internal NameParser NameParser { get; }
    internal NestedParser NestedParser { get; }
    internal DictionaryParser DictionaryParser { get; }
    internal StringParser StringParser { get; }
    internal ICMapProvider CMapProvider { get; }
    internal static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();
    internal XRefParser XRefParser { get; }
    public ParsingOptions Options { get; }

    public ParsingContext(ParsingOptions? options = null, bool setAsCurrent = true)
    {
        Options = options ?? new ParsingOptions() { Eagerness = Eagerness.FullEager };
        ArrayParser = new ArrayParser(this);
        BoolParser = new BoolParser();
        DictionaryParser = new DictionaryParser(this);
        NameParser = new NameParser(this);
        NumberParser = new NumberParser(this);
        DecimalParser = new DecimalParser();
        StringParser = new StringParser(this);
        XRefParser = new XRefParser(this);
        NestedParser = new NestedParser(this);
        CurrentOffset = 0;
        CMapProvider = GlobalCMapProvider.Instance;
        if (setAsCurrent)
        {
            CurrentCtx.Value = this;
        }
    }

    internal List<string> Errors { get; set; } = new List<string>();
    public IReadOnlyList<string> ParsingErrors => Errors;

    private int errors = 0;
    public void Error(string info)
    {
        if (Options.ThrowOnErrors)
        {
            throw new PdfLexerException(info);
        }

        errors++;

        if (Errors.Count > Options.MaxErrorRetention)
        {
            Errors.RemoveAt(0);
        }
        Errors.Add(info);
    }
    public int ErrorCount { get => errors; }

    internal Stream GetTemporaryStream()
    {
        if (Options.LowMemoryMode)
        {
            var path = Path.Combine(Path.GetTempPath(), "pdflexer_" + Guid.NewGuid().ToString());
            return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }
        else
        {
            return StreamManager.GetStream();
        }
    }

    internal decimal? GetHeaderVersion(IPdfDataSource mainSource)
    {
        // %PDF-X.N
        mainSource.GetData(this, 0, mainSource.TotalBytes > 50 ? 50 : (int)mainSource.TotalBytes, out var data);
        // correct for bad start
        var i = data.IndexOf((byte)'%');
        if (i == -1)
        {
            return null;
        }

        if (data.Length < i + 8)
        {
            return null;
        }

        if (!Utf8Parser.TryParse(data.Slice(i + 5, 3), out decimal value, out int _))
        {
            return null;
        }

        return value;
    }

    internal static IDecoder GetDecoder(PdfName name, PdfDocument? doc)
    {
        // Not technically valid outside of inline image
        // but used (eg. ghostscript)
        //   AHx -> ASCIIHexDecode
        //   A85 -> ASCII85Decode
        //   LZW -> LZWDecode
        //   Fl -> FlateDecode
        //   RL -> RunLengthDecode
        //   CCF -> CCITTFaxDecode
        //   DCT -> DCTDecode
        switch (name.Value)
        {
            case "FlateDecode":
            case "Fl":
                return FlateFilter.Instance;
            case "ASCIIHexDecode":
            case "AHx":
                return AsciiHexFilter.Instance;
            case "ASCII85Decode":
            case "A85":
                return Ascii85Filter.Instance;
            case "RL":
            case "RunLengthDecode":
                return RunLengthFilter.Instance;
            case "CCF":
            case "CCITTFaxDecode":
                return CCITTFilter.Instance;
            case "LZW":
            case "LZWDecode":
                return LZWFilter.Instance;
            case "Crypt":
                if (doc == null)
                {
                    throw new PdfLexerException("Crypt filter used in stream without source document attached.");
                }
                return doc.CryptFilter;
            default:
                throw new NotImplementedException($"Stream decoding of type {name.Value} has not been implemented.");
        }
    }

    internal bool IsDataCopyable(IPdfDataSource source, XRef entry)
    {
        if (source.IsEncrypted || Options.ForceSerialize)
        {
            return false;
        }

        ulong id = (ulong)entry.ObjectNumber << 16 | (uint)entry.Generation & 0xFFFF;
        if (source.Document.IndirectCache.TryGetValue(id, out var weak) && weak.TryGetTarget(out var value))
        {
            switch (value.Type)
            {
                case PdfObjectType.ArrayObj:
                    var arr = (PdfArray)value;
                    return !arr.IsModified;
                case PdfObjectType.DictionaryObj:
                    var dict = (PdfDictionary)value;
                    return !dict.IsModified;
                case PdfObjectType.StreamObj:
                    var str = (PdfStream)value;
                    return !str.IsModified;
            }
            return true;
        }
        return true;
    }


    private ConditionalWeakTable<PdfDictionary, IReadableFont> fontCache = new ConditionalWeakTable<PdfDictionary, IReadableFont>();

    internal IReadableFont? GetFont(IPdfObject obj)
    {
        // built in encoding:
        // type1 -> Encoding array in font program but overridden by Encoding / BaseEncoding values
        // type3
        // pdfjs:
        // default -> StandardEncoding
        // if truetype && doesn't have nonsymbolic flag -> WinAnsiEncoding
        // is has symbolic flag -> MacRomanEncoding unless
        //      unembedded or isinternalfont
        //          Symbol in name -> SymbolSetEncoding
        //          Dingbats|Wingdings in name -> ZapfDingbats

        var dict = obj.GetAs<PdfDictionary>();

        if (fontCache.TryGetValue(dict, out var font))
        {
            return font;
        }

        var type = dict.Get<PdfName>(PdfName.Subtype);
        if (type == null)
        {
            return null;
        }
        var created = type.Value switch
        {
            "Type0" => Type0Font.CreateReadable(this, dict),
            "Type1" => Type1Font.CreateReadable(this, dict),
            "TrueType" => TrueTypeFont.CreateReadable(this, dict),
            "Type3" => GetType3(dict),
            _ => throw new PdfLexerException("Uknown font type: " + type.Value)
        };
        fontCache.AddOrUpdate(dict, created);

        return created;
    }

    private IReadableFont GetType3(PdfDictionary dict)
    {
        return Type1Font.CreateReadable(this, dict);
    }

    internal IPdfObject GetIndirectObject(PdfDocument doc, XRef xref) => GetIndirectObject(doc, xref.GetId());

    internal IPdfObject GetIndirectObject(PdfDocument doc, ulong id)
    {
        CurrentReference = id;
        if (doc.IndirectCache.TryGetValue(id, out var weak) && weak.TryGetTarget(out var cached))
        {
            return cached;
        }

        if (doc.XRefs == null || !doc.XRefs.TryGetValue(id, out var value) || value.IsFree)
        {
            // A indirect reference to an undefined object shall not be considered an error by
            // a conforming reader; it shall be treated as a reference to the null object.
            return PdfNull.Value;
        }

        if (value.Type == XRefType.Compressed && value.Source == null)
        {
            try
            {
                LoadObjectStream(doc, value);
            }
            catch (PdfLexerException ex)
            {
                Error($"Error loading object stream, return null obj: " + ex.Message);
                return PdfNull.Value;
            }
        }

        var obj = value.GetObject(this);
        doc.IndirectCache[id] = new WeakReference<IPdfObject>(obj);
        doc.IndirectLookup.AddOrUpdate(obj, value);

        return obj;
    }

    
    internal void LoadObjectStream(PdfDocument doc, XRefEntry entry)
    {
        if (doc.LoadedStreams.TryGetValue(entry.ObjectStreamNumber, out var match))
        {
            entry.Source = match;
            return;
        }

        var stream = GetIndirectObject(doc, XRef.GetId(entry.ObjectStreamNumber, 0)).GetValue<PdfStream>();
        var start = stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.First);

        IPdfDataSource? source;
        if (Options.LowMemoryMode)
        {
            // TODO allow streams for offsets
            var data = ArrayPool<byte>.Shared.Rent(start);
            var decodedStream = stream.Contents.GetDecodedStream();
            var str = GetTemporaryStream();
            decodedStream.CopyTo(str);
            str.Seek(0, SeekOrigin.Begin);
            str.FillArray(data, start);
            Span<byte> spanned = data;
            var os = GetOffsets(spanned.Slice(0, start), stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.N));
            ArrayPool<byte>.Shared.Return(data);
            source = new ObjectStreamFileDataSource(doc, entry.ObjectStreamNumber, str, os, start);
        }
        else
        {
            var data = stream.Contents.GetDecodedData();
            var os = GetOffsets(data, stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.N));
            source = new ObjectStreamDataSource(doc, entry.ObjectStreamNumber, data, os, start);
        }

        doc.disposables.Add(source);

        doc.LoadedStreams[entry.ObjectStreamNumber] = source;
        entry.Source = source;
    }

    private List<int> GetOffsets(ReadOnlySpan<byte> data, int count)
    {
        var offsets = new List<int>(count);
        var c = 0;
        var scanner = new Scanner(this, data, 0);
        while (c < count)
        {
            scanner.SkipObject(); // don't use object numbers currently
            offsets.Add(scanner.GetCurrentObject().GetValue<PdfNumber>());
            c++;
        }
        return offsets;
    }

    // move this to sequence lexer or ext
    public Stream GetStreamOfContents(XRefEntry xref, PdfName? filter, int predictedLength)
    {
        if (xref.KnownStreamStart == 0)
        {
            // this shouldn't happen -> bug
            throw new ApplicationException($"GetStreamOfContents called on non stream: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
        }
        if (xref.KnownStreamLength > 0)
        {
            return xref.Source.GetDataAsStream(this, xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
        }
        var stream = xref.Source.GetStream(this, xref.Offset + xref.KnownStreamStart + predictedLength);
        var reader = Options.CreateReader(stream);
        var scanner = new PipeScanner(this, reader);
        var nxt = scanner.Peek();
        reader.Complete();
        if (nxt == PdfTokenType.EndStream)
        {
            xref.KnownStreamLength = predictedLength;
            return xref.Source.GetDataAsStream(this, xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
        }
        Error($"Stream did not end with endstream: {xref.Reference.ObjectNumber} {xref.Reference.Generation}");
        if (!StructuralRepairs.TryFindStreamEnd(this, xref, filter, predictedLength))
        {
            xref.KnownStreamLength = predictedLength;
            Error($"Unable to find endstream, using provided length");
        }
        Error($"Found endstream in contents, using repaired length: {xref.KnownStreamLength}");
        return xref.Source.GetDataAsStream(this, xref.Offset + xref.KnownStreamStart, xref.KnownStreamLength);
    }

    internal void FillGlyphsFromRawString<T>(GfxState<T> state, ReadOnlySpan<byte> data, List<GlyphOrShift<T>> glyphs) where T : struct, IFloatingPoint<T>
    {
        if (data.Length < 200)
        {
            Span<byte> writeBuffer = stackalloc byte[data.Length];
            var l = StringParser.ConvertBytes(data, writeBuffer);
            FillGlyphsNoReset(state, writeBuffer.Slice(0, l), glyphs);
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(data.Length);
            ReadOnlySpan<byte> spanned = rented;
            var l = StringParser.ConvertBytes(data, rented);
            FillGlyphsNoReset(state, spanned.Slice(0, l), glyphs);
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    internal void FillGlyphsNoReset<T>(GfxState<T> state, ReadOnlySpan<byte> data, List<GlyphOrShift<T>> glyphs) where T : struct, IFloatingPoint<T>
    {
        int i = 0;
        int u = 0;
        while (i < data.Length && (u = GetGlyph(state, data, i, out var glyph)) > 0)
        {
            glyphs.Add(new GlyphOrShift<T>(glyph, T.Zero, u));
            i += u;
        }
    }

    internal int GetGlyph<T>(GfxState<T> state, ReadOnlySpan<byte> data, int pos, out Glyph info) where T: struct, IFloatingPoint<T>
    {
        if (state.Font == null)
        {
            Error("Font data before font set, falling back to helvetica");
            return SingleByteFont.Fallback.GetGlyph(data, pos, out info);
        }
        return state.Font.GetGlyph(data, pos, out info);
    }

    internal IPdfObject GetPdfItem(PdfObjectType type, in ReadOnlySequence<byte> data)
    {
        // NOTE: this is on used during XRef Parsing -> no decryption support
        switch (type)
        {
            // TODO ? switch parser to take positions for no slice if not needed?
            case PdfObjectType.NullObj:
                {
                    return PdfNull.Value;
                }
            case PdfObjectType.NumericObj:
                {
                    return NumberParser.Parse(in data);
                }
            case PdfObjectType.DecimalObj:
                {
                    return DecimalParser.Parse(in data);
                }
            case PdfObjectType.NameObj:
                {
                    return NameParser.Parse(in data);
                }
            case PdfObjectType.DictionaryObj:
                {
                    return DictionaryParser.Parse(in data);
                }
            case PdfObjectType.ArrayObj:
                {
                    return ArrayParser.Parse(in data);
                }
            case PdfObjectType.StringObj:
                {
                    return StringParser.Parse(in data);
                }
            case PdfObjectType.BooleanObj:
                {
                    return BoolParser.Parse(in data);
                }
        }
        throw new NotImplementedException($"Pdf Object type {type} was passed for parsing but is not known.");
    }

    // review this, seems wasteful to scan twice
    // some paths don't need to know length
    internal PdfObject GetPdfItem(ReadOnlySpan<byte> data, int start, out int length, PdfDocument? doc)
    {
        var orig = start;
        length = GetCompleteLength(data, ref start, out var type);

        var item = GetKnownPdfItem(type, data, start, length, doc);
        length = length + start - orig;
        return item;
    }

    private int GetCompleteLength(ReadOnlySpan<byte> data, ref int start, out PdfObjectType objType)
    {
        var next = PdfSpanLexer.TryReadNextToken(data, out var type, start, out var length);
        if (next == -1)
        {
            throw CommonUtil.DisplayDataErrorException(data, start, "Object not found in provided data buffer");
        }

        if ((int)type > 7)
        {
            throw CommonUtil.DisplayDataErrorException(data, start, $"No object found at offset, found token of type {type.ToString()}");
        }

        if (type == PdfTokenType.ArrayStart)
        {
            var ea = next + length;
            NestedUtil.AdvanceToArrayEnd(data, ref ea, out _);
            length = ea - next;
        }
        else if (type == PdfTokenType.DictionaryStart)
        {
            var ed = next + length;
            NestedUtil.AdvanceToDictEnd(data, ref ed, out _);
            length = ed - next;
        }
        start = next;
        objType = (PdfObjectType)type;
        return length;
    }

    internal PdfObject GetKnownPdfItem(PdfObjectType type, ReadOnlySpan<byte> data, int start, int length, PdfDocument? doc)
    {
        switch (type)
        {
            case PdfObjectType.NullObj:
                return PdfNull.Value;
            case PdfObjectType.NumericObj:
                return NumberParser.Parse(data, start, length);
            case PdfObjectType.DecimalObj:
                return DecimalParser.Parse(data, start, length);
            case PdfObjectType.NameObj:
                return NameParser.Parse(data, start, length);
            case PdfObjectType.BooleanObj:
                return BoolParser.Parse(data, start, length);
            case PdfObjectType.StringObj:
                return StringParser.Parse(data, start, length);
            case PdfObjectType.DictionaryObj:
            case PdfObjectType.ArrayObj:
                return (PdfObject)NestedParser.ParseNestedItem(doc, data, start, out _); // TODO lazy support?
        }
        throw new NotImplementedException($"Pdf Object type {type} was passed for parsing but is not known.");
    }

    public void Clear()
    {
        CachedInts.Clear();
        // CachedNumbers.Clear();
    }


    public List<string> GetCachedNames() => NameCache.Cache.Select(x => x.Value.Value).ToList();

    internal bool disposed = false;
    public void Dispose()
    {
        CurrentCtx.Value = null;
        disposed = true;
        CachedInts = null!;
        NameCache = null!;
        NumberCache = null!;
    }
}
