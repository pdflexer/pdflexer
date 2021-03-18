using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
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
        internal bool IsEncrypted { get; set; } = false;

        internal ulong CurrentIndirectObject { get; set; }
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

        public (Dictionary<ulong, XRefEntry>, PdfDictionary) Initialize(IPdfDataSource pdf)
        {
            MainDocSource = pdf;
            CurrentSource = MainDocSource;
            return XRefParser.LoadCrossReferences(pdf);
        }

        internal List<string> Errors { get; set; } = new List<string>();
        public IReadOnlyList<string> ParsingErrors => Errors;

        internal void Error(string info)
        {
            if (Options.ThrowOnErrors)
            {
                throw new PdfLexerException(info);
            }
            Errors.Add(info);
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
            if (IsEncrypted || Options.ForceSerialize)
            {
                return false;
            }

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

        internal void UnwrapAndCopyObjData(ReadOnlySpan<byte> data, WritingContext wtx)
        {
            if (IsEncrypted)
            {
                throw new NotSupportedException("Copying raw data from encrypted PDF is not supported.");
            }
            var scanner = new Scanner(this, data, 0);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.NumericObj);
            scanner.SkipExpected(PdfTokenType.StartObj);
            var objLength = scanner.SkipObject();
            var objStart = scanner.Position - objLength;
            var type = scanner.Peek();
            if (type == PdfTokenType.EndObj)
            {
                wtx.Stream.Write(data.Slice(objStart, objLength));
                return;
            }
            else if (type == PdfTokenType.StartStream)
            {
                // TODO look into this.. feels wrong parsing dict here
                scanner.SkipCurrent(); // startstream
                var startPos = scanner.Position;
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
                var eosByLength = scanner.Position;
                var endstream = scanner.Peek();
                if (endstream == PdfTokenType.EndStream)
                {
                    scanner.SkipCurrent();
                    wtx.Stream.Write(data.Slice(objStart, scanner.Position - objStart));
                }
                else
                {
                    Error("Endstream not found at end of stream when parsing when copying data.");
                    if (!scanner.TryFindEndStream())
                    {
                        Error("Unable to find endstream in contents, writing provided length.");
                        // no way to repair this.. simply write existing data length
                        wtx.Stream.Write(data.Slice(objStart, eosByLength - objStart));
                        wtx.Stream.WriteByte((byte)'\n');
                        wtx.Stream.Write(IndirectSequences.endstream);
                        Options.Eagerness = existing;
                        return;
                    }

                    Error("Found endstream in contents, using repaired length.");
                    streamLength = new PdfIntNumber(scanner.Position - startPos);
                    dict[PdfName.Length] = streamLength;
                    var contents = new PdfByteArrayStreamContents(data.Slice(startPos, scanner.Position - startPos).ToArray());
                    var stream = new PdfStream(dict, contents);
                    contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                    contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                    wtx.SerializeObject(stream, true);
                }
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

            CurrentIndirectObject = id;
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
            scanner.SkipExpected(PdfTokenType.StartObj); // TODO repair
            var obj = scanner.GetCurrentObject();
            var nxt = scanner.Peek();
            if (nxt == PdfTokenType.EndObj)
            {
                return obj;
            }
            else if (nxt == PdfTokenType.StartStream)
            {
                if (!(obj is PdfDictionary dict))
                {
                    Error($"Pdf dictionary followed by startstream was {obj.Type} instead of dictionary.");
                    return obj;
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    Error("Pdf dictionary followed by start stream token did not contain /Length.");
                    streamLength = PdfCommonNumbers.Zero;
                }

                var startPos = scanner.Position + scanner.CurrentLength;
                scanner.SkipCurrent(); // start stream
                scanner.Advance(streamLength);
                var endstream = scanner.Peek();
                if (endstream != PdfTokenType.EndStream)
                {
                    Error("Endstream not found at end of stream when parsing indirect object.");
                    if (scanner.TryFindEndStream())
                    {
                        Error("Found endstream in contents, using repaired length.");
                        streamLength = new PdfIntNumber(scanner.Position - startPos);
                        dict[PdfName.Length] = streamLength;
                    }
                }
                var contents = new PdfExistingStreamContents(MainDocSource, xref.Offset + startPos, streamLength);
                contents.Filters = dict.GetOptionalValue<IPdfObject>(PdfName.Filter);
                contents.DecodeParams = dict.GetOptionalValue<IPdfObject>(PdfName.DecodeParms);
                var stream = new PdfStream(dict, contents);

                return stream;
            }
            Error("Indirect object not follwoed by endobj token: " + CommonUtil.GetDataErrorInfo(data, scanner.Position));
            return obj;
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
