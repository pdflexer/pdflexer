using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Lexing;
using PdfLexer.Parsers.Nested;

namespace PdfLexer.Parsers
{
    public enum ParseState 
    {
        None,
        Nested,
    }
    public class ParsingContext
    {
        // TODO different types
        internal bool IsEager { get; set; } = true;
        internal NestedSkipper Skipper { get; } = new NestedSkipper();
        internal ParseState ParseState { get; set; }
        internal List<IPdfObject> ObjectBag = new List<IPdfObject>();
        internal byte[] Buffer = new byte[5000];
        internal long CurrentOffset { get; set; }
        internal IPdfDataSource CurrentSource { get; set; }
        public bool CacheNumbers { get; set; } = true;

        internal NumberParser NumberParser { get; }
        internal NameParser NameParser { get; } = new NameParser();
        public IPdfDataSource MainDocument { get; private set; }
        internal LazyNestedSeqParser NestedSeqParser { get; }
        internal LazyNestedSpanParser NestedSpanParser { get; }
        internal DictionaryParser DictionaryParser { get; }
        internal StringParser StringParser {get;}
        internal XRefParser XRefParser { get; }
        internal PdfDictionary Trailer { get; } // move to pdf doc

        public ParsingContext()
        {
            NestedSeqParser = new LazyNestedSeqParser(this);
            NestedSpanParser = new LazyNestedSpanParser(this);
            DictionaryParser = new DictionaryParser(this);
            NumberParser =  new NumberParser(this);
            StringParser = new StringParser(this);
            XRefParser = new XRefParser(this);
        }

        public async ValueTask Initialize(IPdfDataSource pdf)
        {
            MainDocument = pdf;
            var (refs, trailer) = await XRefParser.LoadCrossReference(MainDocument);
            XrefEntries = new Dictionary<XRef, XRefEntry>();
            foreach (var entry in refs.Where(x=>!x.IsFree).OrderBy(x=>x.ObjectNumber))
            {
                XrefEntries[new XRef(entry.ObjectNumber, entry.Generation)] = entry;
            }
            if (refs.Any())
            {
                var ordered = refs.Where(x=>x.Type == XRefType.Normal && !x.IsFree).OrderBy(x=>x.Offset).ToList();
                for (var i = 0; i < ordered.Count; i++)
                {
                    Debug.Assert(ordered[i].Offset < pdf.TotalBytes);
                    if (i + 1 < ordered.Count)
                    {
                        ordered[i].MaxLength = (int)(ordered[i+1].Offset-ordered[i].Offset);
                    }
                }
                ordered[^1].MaxLength = (int)(MainDocument.TotalBytes-ordered[^1].Offset-1);
            }
        }

        internal Dictionary<XRef, XRefEntry> XrefEntries;


        internal IPdfObject GetIndirectObject(XRef xref)
        {
            if (!XrefEntries.TryGetValue(xref, out var value))
            {
                throw new ApplicationException($"Unknown indirect object {xref}");
            }
            if (value.Type == XRefType.Compressed)
            {
                throw new NotImplementedException("PDF 1.5 Object streams not yet supported.");
            }

            MainDocument.FillData(value.Offset, value.MaxLength, out var buffer);
            return GetPdfItem(buffer, 0);
        }

        internal IPdfObject GetPdfItem(PdfObjectType type, in ReadOnlySequence<byte> data, SequencePosition start, SequencePosition end)
        {
            switch (type)
            {
                // TODO ? switch parser to take positions for no slice if not needed?
                case PdfObjectType.NumericObj:
                    {
                    var slice = data.Slice(start, end); 
                    return NumberParser.Parse(in slice);
                    }
                case PdfObjectType.NameObj:
                    {
                    var slice = data.Slice(start, end);
                    return NameParser.Parse(in slice);
                    }
            }
            return null;
        }

        internal IPdfObject GetPdfItem(ReadOnlySpan<byte> data, int start)
        {
            var next = PdfSpanLexer.TryReadNextToken(data, out var type, start, out int actualLength);
            if (next == -1)
            {
                throw CommonUtil.DisplayDataErrorException(data, start, "Object not found in provided data buffer");
            }

            if ((int)type > 7)
            {
                throw CommonUtil.DisplayDataErrorException(data, start, $"No object found at offset, found token of type {type.ToString()}");
            }

            return GetKnownPdfItem((PdfObjectType) type, data, next, actualLength);
        }

        internal IPdfObject GetKnownPdfItem(PdfObjectType type, ReadOnlySpan<byte> data, int start, int length)
        {
            switch (type)
            {
                // TODO ? switch parser to take positions for no slice if not needed?
                case PdfObjectType.NumericObj:
                    {
                    var slice = data.Slice(start, length);
                    return NumberParser.Parse(slice);
                    }
                case PdfObjectType.NameObj:
                    {
                    var slice = data.Slice(start, length);
                    return NameParser.Parse(slice);
                    }
                case PdfObjectType.DictionaryObj:
                case PdfObjectType.ArrayObj:
                    return NestedSpanParser.ParseNestedItem(null, 0, data, start); // TODO lazy support
            }
            return null;
        }

        internal PdfLazyObject CreateLazy(PdfObjectType type, in ReadOnlySequence<byte> data, SequencePosition start, SequencePosition end)
        {
            return new PdfLazyObject {
                    Offset = 0,
                    Length = 0,
                    IsIndirect = false,
                    Type = type,
                    Source = CurrentSource
                };
        }

        public void Clear()
        {
            CachedNumbers.Clear();
        }

        internal Dictionary<CacheItem, PdfNumber> CachedNumbers = new Dictionary<CacheItem, PdfNumber>(new FNVByteComparison());

        internal struct CacheItem
        {
            internal byte[] Data;
            internal int Length;
        }
        class FNVByteComparison : IEqualityComparer<CacheItem>
        {
            public bool Equals(CacheItem x,CacheItem y)
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
