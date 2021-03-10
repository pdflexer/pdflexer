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
        internal NumberCache NumberCache = new NumberCache();
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
            Options = options;
            Options ??= new ParsingOptions();

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

        public ValueTask<(Dictionary<ulong, XRefEntry>, PdfDictionary)> Initialize(IPdfDataSource pdf)
        {
            MainDocSource = pdf;
            CurrentSource = MainDocSource;
            return XRefParser.LoadCrossReferences(pdf);
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

        internal void UnwrapAndCopyObjData(ReadOnlySpan<byte> data, Stream stream)
        {
            var scanner = new Scanner(this, data, 0);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            var objLength = scanner.SkipObject();
            var objStart = scanner.Position - objLength;
            var type = scanner.Peak();
            if (type == PdfTokenType.EndObj)
            {
                stream.Write(data.Slice(objStart, objLength));
                return;
            }
            else if (type == PdfTokenType.StartStream)
            {
                scanner.SkipCurrent();
                // TODO look into this.. feels wrong parsing dict here
                var existing = Options.Eagerness;
                Options.Eagerness = Eagerness.Lazy;
                var obj = GetKnownPdfItem(PdfObjectType.DictionaryObj, data, objStart, objLength);
                if (!(obj is PdfDictionary dict))
                {
                    throw CommonUtil.DisplayDataErrorException(data, scanner.Position, "Indirect object followed by start stream token but was not dictionary");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }
                scanner.Advance(streamLength);
                var endstream = scanner.Peak();
                // TODO debug
                scanner.SkipCurrent();
                stream.Write(data.Slice(objStart, scanner.Position - objStart));
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
            var stream = GetIndirectObject(XRef.GetId(entry.ObjectStreamNumber, 0)).GetValue<PdfStream>();
            var data = stream.Contents.GetDecodedData(this);
            var os = GetOffsets(data, stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.N));
            var start = stream.Dictionary.GetRequiredValue<PdfNumber>(PdfName.First);
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
            var scanner = new Scanner(this, data, 0);
            while (c < count)
            {
                scanner.SkipObject(); // don't use object numbers currently
                offsets.Add(scanner.GetCurrentObject().GetValue<PdfNumber>());
                c++;
            }
            return offsets;
        }

        internal IPdfObject GetWrappedIndirectObject(XRefEntry xref, ReadOnlySpan<byte> data)
        {
            var scanner = new Scanner(this, data, 0);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            var obj = scanner.GetCurrentObject();
            var nxt = scanner.Peak();
            if (nxt == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                if (!(obj is PdfDictionary dict))
                {
                    throw new ApplicationException("Indirect object followed by start stream token but was not dictionary.");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }
                var contents = new PdfExistingStreamContents(MainDocSource, xref.Offset + scanner.Position + scanner.CurrentLength, streamLength);
                contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                var stream = new PdfStream(dict, contents);
                scanner.SkipCurrent();
                scanner.Advance(streamLength);
                var endstream = scanner.Peak();
                // TODO validated endstream
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
                    return (PdfObject)NestedParser.ParseNestedItem(data, start, out _); // TODO lazy support?
            }
            throw new NotImplementedException($"Pdf Object type {type} was passed for parsing but is not known.");
        }

        public void Clear()
        {
            CachedInts.Clear();
            // CachedNumbers.Clear();
        }

    }
}
