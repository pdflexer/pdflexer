using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers.Nested;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer.Parsers
{
    public class ParsingContext
    {
        internal int SourceId {get;set;}
        internal long CurrentOffset { get; set; }
        internal IPdfDataSource CurrentSource { get; set; }
        internal Dictionary<int, PdfIntNumber> CachedInts = new Dictionary<int, PdfIntNumber>();
        internal Dictionary<XRef, IPdfObject> IndirectCache = new Dictionary<XRef, IPdfObject>();
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
        
        internal XRefParser XRefParser { get; }

        public ParsingOptions Options { get; }


        public ParsingContext(ParsingOptions options=null)
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
        }

        public async ValueTask<(Dictionary<XRef, XRefEntry>, PdfDictionary)> Initialize(IPdfDataSource pdf)
        {
            MainDocSource = pdf;
            CurrentSource = MainDocSource;
            var (refs, trailer) = await XRefParser.LoadCrossReference(MainDocSource);
            var entries = new Dictionary<XRef, XRefEntry>();
            foreach (var entry in refs.OrderBy(x => x.Reference.ObjectNumber))
            {
                entries[entry.Reference] = entry;
            }
            if (refs.Any())
            {
                var ordered = refs.Where(x => x.Type == XRefType.Normal && !x.IsFree).OrderBy(x => x.Offset).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    Debug.Assert(ordered[i].Offset < pdf.TotalBytes);
                    if (i + 1 < ordered.Count)
                    {
                        ordered[i].MaxLength = (int)(ordered[i + 1].Offset - ordered[i].Offset);
                    }
                }
                ordered[^1].MaxLength = (int)(MainDocSource.TotalBytes - ordered[^1].Offset - 1);
            }
            return (entries, trailer);
        }

        internal IPdfObject GetIndirectObject(PdfIndirectRef ir)
        {
            return ir.GetObject(); // Is this needed anymore?
            // TODO allow lazy loading here -> especially for ShouldLoadIndirects
            // return GetIndirectObject(ir.Reference);
        }
        internal IPdfObject GetIndirectObject(XRef xref)
        {
            if (Options.CacheObjects && IndirectCache.TryGetValue(xref, out var cached))
            {
                return cached;
            }

            if (!Document.XrefEntries.TryGetValue(xref, out var value) || value.IsFree)
            {
                // A indirect reference to an undefined object shall not be considered an error by
                // a conforming reader; it shall be treated as a reference to the null object.
                return PdfNull.Value;
            }

            if (value.Type == XRefType.Compressed)
            {
                throw new NotImplementedException("PDF 1.5 Object streams not yet supported.");
            }

            // TODO split data sources
            MainDocSource.FillData(value.Offset, value.MaxLength, out var buffer);


            // SKIP INDIRECT REF INFO
            var i = PdfSpanLexer.TryReadNextToken(buffer, out var type, 0, out var length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i+length, out length);
            Debug.Assert(type == PdfTokenType.NumericObj);
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, i+length, out length);
            Debug.Assert(type == PdfTokenType.StartObj);
            var os = i + length;
            var obj = GetPdfItem(buffer, os, out length);
            // obj.IndirectRef = xref;
            // GET NEXT TOKEN
            i = PdfSpanLexer.TryReadNextToken(buffer, out type, os+length, out length);
            if (type == PdfTokenType.EndObj)
            {
                if (Options.CacheObjects) // TODO add obj/endobj offsets for copying of data later
                {
                    IndirectCache[xref] = obj;
                }
                return obj;
            } else if (type == PdfTokenType.StartStream)
            {
                if (!(obj is PdfDictionary dict))
                {
                    throw new ApplicationException("Indirect object followed by start stream token but was not dictionary.");
                }
                if (!dict.TryGetValue<PdfNumber>(PdfName.Length, out var streamLength))
                {
                    throw new ApplicationException("Pdf dictionary followed by start stream token did not contain /Length.");
                }
                var stream = new PdfStream(dict, new PdfStreamContents(MainDocSource, value.Offset + i + length, streamLength));
                i = PdfSpanLexer.TryReadNextToken(buffer, out type, i+length+(int)streamLength, out length);
                Debug.Assert(type == PdfTokenType.EndStream);
                if (Options.CacheObjects)
                {
                    IndirectCache[xref] = stream;
                }
                return stream;
            }
            throw new ApplicationException("Indirect object not followed by endobj token.");
        }

        internal void LoadIndirectRefs(PdfDictionary dict)
        {
            var replacements = new List<KeyValuePair<PdfName, IPdfObject>>();
            foreach (var kvp in dict)
            {
                if (kvp.Value is PdfIndirectRef ir)
                {
                    replacements.Add(new KeyValuePair<PdfName, IPdfObject>(kvp.Key, GetIndirectObject(ir)));
                }
            }
            foreach (var item in replacements)
            {
                dict[item.Key] = item.Value;
            }
        }
        internal void LoadIndirectRefs(PdfArray array)
        {                    
            var replacements = new List<KeyValuePair<int, IPdfObject>>();
            for (var i = 0; i < array.Count; i++)
            {
                var item = array[i];
                if (item is PdfIndirectRef ir)
                {
                    replacements.Add(new KeyValuePair<int, IPdfObject>(i, GetIndirectObject(ir)));
                }
            }
            foreach (var item in replacements)
            {
                array[item.Key] = item.Value;
            }
        }

        internal void SerializeObject(IPdfObject obj, Stream stream)
        {

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
            var next = PdfSpanLexer.TryReadNextToken(data, out var type, start, out length);
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
                var ea = next+length;
                NestedUtil.AdvanceToArrayEnd(data, ref ea, out _);
                length = ea-next;
            } else if (type == PdfTokenType.DictionaryStart)
            {
                var ed = next+length;
                NestedUtil.AdvanceToDictEnd(data, ref ed, out _);
                length = ed-next;
            }

            var item = GetKnownPdfItem((PdfObjectType)type, data, next, length);
            length = length + next - start;
            return item;
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

        internal struct CacheItem
        {
            internal byte[] Data;
            internal int Length;
        }
        class FNVByteComparison : IEqualityComparer<CacheItem>
        {
            public bool Equals(CacheItem x, CacheItem y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }

                for (var i = 0; i < x.Length; i++)
                {
                    if (x.Data[i] != y.Data[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(CacheItem obj)
            {
                var hash = FnvHash.Create();
                int i = 0;
                foreach (var t in obj.Data)
                {
                    hash.Combine(t);
                    i++;
                    if (i == obj.Length)
                    {
                        return hash.HashCode;
                    }
                }

                return hash.HashCode;
            }
        }

        /// <summary>
        /// A hash combiner that is implemented with the Fowler/Noll/Vo algorithm (FNV-1a). This is a mutable struct for performance reasons.
        /// </summary>
        struct FnvHash
        {
            /// <summary>
            /// The starting point of the FNV hash.
            /// </summary>
            public const int Offset = unchecked((int)2166136261);

            /// <summary>
            /// The prime number used to compute the FNV hash.
            /// </summary>
            private const int Prime = 16777619;

            /// <summary>
            /// Gets the current result of the hash function.
            /// </summary>
            public int HashCode { get; private set; }

            /// <summary>
            /// Creates a new FNV hash initialized to <see cref="Offset"/>.
            /// </summary>
            public static FnvHash Create()
            {
                var result = new FnvHash();
                result.HashCode = Offset;
                return result;
            }

            /// <summary>
            /// Adds the specified byte to the hash.
            /// </summary>
            /// <param name="data">The byte to hash.</param>
            public void Combine(byte data)
            {
                unchecked
                {
                    HashCode ^= data;
                    HashCode *= Prime;
                }
            }

            /// <summary>
            /// Adds the specified integer to this hash, in little-endian order.
            /// </summary>
            /// <param name="data">The integer to hash.</param>
            public void Combine(int data)
            {
                Combine(unchecked((byte)data));
                Combine(unchecked((byte)(data >> 8)));
                Combine(unchecked((byte)(data >> 16)));
                Combine(unchecked((byte)(data >> 24)));
            }
        }
    }
}
