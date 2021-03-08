using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IO;
using PdfLexer.Filters;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers.Nested;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer.Parsers
{
    public class ParsingContext
    {
        internal int SourceId { get; set; }
        internal long CurrentOffset { get; set; }
        internal IPdfDataSource CurrentSource { get; set; }
        internal Dictionary<int, PdfIntNumber> CachedInts = new Dictionary<int, PdfIntNumber>();
        internal Dictionary<ulong, IPdfObject> IndirectCache = new Dictionary<ulong, IPdfObject>();
        internal NameCache NameCache = new NameCache();
        internal NumberParser NumberParser { get; }
        internal DecimalParser DecimalParser { get; }
        internal ArrayParser ArrayParser { get; }
        internal BoolParser BoolParser { get; }
        internal NameParser NameParser { get; }
        public IPdfDataSource MainDocSource { get; private set; }
        public PdfDocument Document { get; internal set; }
        internal NestedParser NestedParser { get; }
        internal DictionaryParser DictionaryParser { get; }
        internal StringParser StringParser { get; }
        internal static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        internal XRefParser XRefParser { get; }

        public ParsingOptions Options { get; }

        private IDecoder FlateDecoder;
        public ParsingContext(ParsingOptions options = null)
        {
            Options = options ??= new ParsingOptions();

            ArrayParser = new ArrayParser(this);
            BoolParser = new BoolParser();
            DictionaryParser = new DictionaryParser(this);
            NameParser = new NameParser(this);
            NumberParser = new NumberParser(this);
            DecimalParser = new DecimalParser();
            StringParser = new StringParser(this);
            XRefParser = new XRefParser(this);
            NestedParser = new NestedParser(this);
            FlateDecoder = new FlateFilter(this);
        }

        public async ValueTask<(Dictionary<ulong, XRefEntry>, PdfDictionary)> Initialize(IPdfDataSource pdf)
        {
            MainDocSource = pdf;
            CurrentSource = MainDocSource;
            var orig = Options.Eagerness;
            Options.Eagerness = Eagerness.FullEager; // lazy objects don't currently work for sequencereader.. need to track offsets.
            var (refs, trailer) = await XRefParser.LoadCrossReference(MainDocSource);
            Options.Eagerness = orig;
            var entries = new Dictionary<ulong, XRefEntry>();
            refs.Reverse(); // oldest prev entries returned first... look into cleaning this up
            foreach (var entry in refs)
            {
                entries[entry.Reference.GetId()] = entry;
            }
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
                        ordered[i].MaxLength = ordered[i+1].MaxLength;
                    }
                }
                ordered[^1].MaxLength = (int)(MainDocSource.TotalBytes - ordered[^1].Offset - 1);

                foreach (var stream in refs.Where(x => x.Type == XRefType.Compressed).GroupBy(x => x.ObjectStreamNumber))
                {

                }
            }
            return (entries, trailer);
        }

        internal IDecoder GetDecoder(PdfName name)
        {
            switch (name.Value)
            {
                case "/FlateDecode":
                    return FlateDecoder;
                default:
                    throw new NotImplementedException($"Stream decoding of type {name.Value} has not been implemented.");
            }
        }

        internal bool IsDataCopyable(XRef entry)
        {
            ulong id = ((ulong)entry.ObjectNumber << 16) | ((uint)entry.Generation & 0xFFFF);
            if (IndirectCache.TryGetValue(id, out var value))
            {
                switch (value.Type)
                {
                    case PdfObjectType.ArrayObj:
                        var arr = (PdfArray)value;
                        return !arr.IsModified;
                    case PdfObjectType.DictionaryObj:
                        var dict = (PdfDictionary)value;
                        return !dict.IsModified;
                }
                return true;
            }
            return true;
        }

        internal void UnwrapAnyCopyIrObj(ReadOnlySpan<byte> data, Stream stream)
        {
            ReadOnlySpan<byte> buffer = data;
            // SKIP INDIRECT REF INFO
            var i = PdfSpanLexer.TryReadNextToken(buffer, out var type, 0, out var length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
            Debug.Assert(type == PdfTokenType.StartObj);
            var os = i + length;

            length = GetCompleteLength(buffer, ref os, out var objType);
            var initObj = os;
            var initLength = length;
            // GET NEXT TOKEN
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, os + length, out length);
            if (type == PdfTokenType.EndObj)
            {
                stream.Write(buffer.Slice(initObj, initLength));
                return;
            }
            else if (type == PdfTokenType.StartStream)
            {
                // TODO look into this.. feels wrong parsing dict here
                var existing = Options.Eagerness;
                Options.Eagerness = Eagerness.Lazy;
                var obj = GetKnownPdfItem(PdfObjectType.DictionaryObj, buffer, initObj, initLength);
                if (!(obj is PdfDictionary dict))
                {
                    throw new ApplicationException("Indirect object followed by start stream token but was not dictionary.");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }
                var se = i + length + streamLength;
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, se, out length);
                // TODO logging
                // Debug.Assert(type == PdfTokenType.EndStream);
                var cse = i + length;
                stream.Write(buffer.Slice(initObj, cse - initObj));
                Options.Eagerness = existing;
                return;
            }
        }

        internal IPdfObject GetIndirectObject(XRef xref) => GetIndirectObject(xref.GetId());

        internal IPdfObject GetIndirectObject(ulong id)
        {
            if (IndirectCache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            if (!Document.XrefEntries.TryGetValue(id, out var value) || value.IsFree)
            {
                // A indirect reference to an undefined object shall not be considered an error by
                // a conforming reader; it shall be treated as a reference to the null object.
                return PdfNull.Value;
            }

            if (value.Type == XRefType.Compressed && value.Source == null)
            {
                LoadObjectStream(value);
            }

            var obj = value.GetObject();
            IndirectCache[id] = obj;
            return obj;
        }

        internal void LoadObjectStream(XRefEntry entry)
        {
            var wrapper = GetIndirectObject(XRef.GetId(entry.ObjectStreamNumber, 0)).GetValue<PdfStream>();
            var data = wrapper.GetDecodedData(this);
            var os = GetOffsets(data, wrapper.Dictionary.GetRequiredValue<PdfNumber>(PdfName.N));
            var start = wrapper.Dictionary.GetRequiredValue<PdfNumber>(PdfName.First);
            var source = new ObjectStreamDataSource(this, entry.ObjectStreamNumber, data, os, start);
            foreach (var item in Document.XrefEntries.Values.Where(x => x.ObjectStreamNumber == entry.ObjectStreamNumber))
            {
                item.Source = source;
            }
        }

        private List<int> GetOffsets(ReadOnlySpan<byte> data, int count)
        {
            var offsets = new List<int>(count);
            var c = 0;
            var i = 0;
            while (c < count)
            {
                i = PdfSpanLexer.TryReadNextToken(data, out var type, i, out int len);
                var on = GetKnownPdfItem((PdfObjectType)type, data, i, len);
                i += len;
                i = PdfSpanLexer.TryReadNextToken(data, out type, i, out len);
                var os = GetKnownPdfItem((PdfObjectType)type, data, i, len);
                i += len;
                offsets.Add(os.GetValue<PdfNumber>());
                c++;
            }
            return offsets;
        }

        internal IPdfObject GetObjectAtOffset(long offset, int maxLength)
        {
            ReadOnlySpan<byte> buffer;
            byte[] rented = null;
            if (MainDocSource.IsDataInMemory(offset, maxLength))
            {
                MainDocSource.GetData(offset, maxLength, out buffer);
            }
            else
            {
                rented = ArrayPool<byte>.Shared.Rent(maxLength);
                buffer = rented;
            }
            try
            {
                // SKIP INDIRECT REF INFO
                var i = PdfSpanLexer.TryReadNextToken(buffer, out var type, 0, out var length);
                Debug.Assert(type == PdfTokenType.NumericObj);
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
                Debug.Assert(type == PdfTokenType.NumericObj);
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
                Debug.Assert(type == PdfTokenType.StartObj);
                var os = i + length;
                var obj = GetPdfItem(buffer, os, out length);
                // GET NEXT TOKEN
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, os + length, out length);
                if (type == PdfTokenType.EndObj)
                {
                    return obj;
                }
                else if (type == PdfTokenType.StartStream)
                {
                    if (!(obj is PdfDictionary dict))
                    {
                        throw new ApplicationException("Indirect object followed by start stream token but was not dictionary.");
                    }
                    if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                    {
                        throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                    }
                    var stream = new PdfStream(dict, new PdfExistingStreamContents(MainDocSource, offset + i + length, streamLength));
                    i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length + (int)streamLength, out length);
                    // TODO add logging
                    // Debug.Assert(type == PdfTokenType.EndStream);
                    return stream;
                }
            }
            finally
            {
                if (rented != null) { ArrayPool<byte>.Shared.Return(rented); }
            }
            throw new ApplicationException("Indirect object not followed by endobj token.");
        }

        internal IPdfObject GetWrappedIndirectObject(XRefEntry xref, ReadOnlySpan<byte> data)
        {
            ReadOnlySpan<byte> buffer = data;
            // SKIP INDIRECT REF INFO
            var i = PdfSpanLexer.TryReadNextToken(buffer, out var type, 0, out var length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length, out length);
            Debug.Assert(type == PdfTokenType.StartObj);
            var os = i + length;
            var obj = GetPdfItem(buffer, os, out length);
            // GET NEXT TOKEN
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, os + length, out length);
            if (type == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (type == PdfTokenType.StartStream)
            {
                if (!(obj is PdfDictionary dict))
                {
                    throw new ApplicationException("Indirect object followed by start stream token but was not dictionary.");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }
                var stream = new PdfStream(dict, new PdfExistingStreamContents(MainDocSource, xref.Offset + i + length, streamLength));
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, i + length + (int)streamLength, out length);
                // TODO add logging
                // Debug.Assert(type == PdfTokenType.EndStream);
                return stream;
            }
            throw new ApplicationException("Indirect object not followed by endobj token.");
        }

        internal IPdfObject GetPdfItem(PdfObjectType type, in ReadOnlySequence<byte> data, SequencePosition start, SequencePosition end)
        {
            switch (type)
            {
                // TODO ? switch parser to take positions for no slice if not needed?
                case PdfObjectType.NullObj:
                    {
                        return PdfNull.Value;
                    }
                case PdfObjectType.NumericObj:
                    {
                        var slice = data.Slice(start, end);
                        return NumberParser.Parse(in slice);
                    }
                case PdfObjectType.DecimalObj:
                    {
                        var slice = data.Slice(start, end);
                        return DecimalParser.Parse(in slice);
                    }
                case PdfObjectType.NameObj:
                    {
                        var slice = data.Slice(start, end);
                        return NameParser.Parse(in slice);
                    }
                case PdfObjectType.DictionaryObj:
                    {
                        var slice = data.Slice(start, end);
                        return DictionaryParser.Parse(in slice);
                    }
                case PdfObjectType.ArrayObj:
                    {
                        var slice = data.Slice(start, end);
                        return ArrayParser.Parse(in slice);
                    }
                case PdfObjectType.StringObj:
                    {
                        var slice = data.Slice(start, end);
                        return StringParser.Parse(in slice);
                    }
                case PdfObjectType.BooleanObj:
                    {
                        var slice = data.Slice(start, end);
                        return BoolParser.Parse(slice);
                    }
            }
            return null;
        }

        internal PdfObject GetPdfItem(ReadOnlySpan<byte> data, int start, out int length)
        {
            var orig = start;
            length = GetCompleteLength(data, ref start, out var type);

            var item = GetKnownPdfItem((PdfObjectType)type, data, start, length);
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

        internal PdfObject GetKnownPdfItem(PdfObjectType type, ReadOnlySpan<byte> data, int start, int length)
        {
            switch (type)
            {
                // TODO ? switch parser to take positions for no slice if not needed?
                case PdfObjectType.NullObj:
                    {
                        return PdfNull.Value;
                    }
                case PdfObjectType.NumericObj:
                    {
                        var slice = data.Slice(start, length);
                        return NumberParser.Parse(slice);
                    }
                case PdfObjectType.DecimalObj:
                    {
                        var slice = data.Slice(start, length);
                        return DecimalParser.Parse(slice);
                    }
                case PdfObjectType.NameObj:
                    {
                        var slice = data.Slice(start, length);
                        return NameParser.Parse(slice);
                    }
                case PdfObjectType.BooleanObj:
                    {
                        var slice = data.Slice(start, length);
                        return BoolParser.Parse(slice);
                    }
                case PdfObjectType.StringObj:
                    {
                        var slice = data.Slice(start, length);
                        return StringParser.Parse(slice);
                    }
                case PdfObjectType.DictionaryObj:
                case PdfObjectType.ArrayObj:
                    return (PdfObject)NestedParser.ParseNestedItem(data, start, out _); // TODO lazy support
            }
            return null;
        }

        public void Clear()
        {
            CachedInts.Clear();
            // CachedNumbers.Clear();
        }

    }
}
