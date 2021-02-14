﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using PdfLexer.IO;
using PdfLexer.Parsers.Nested;

namespace PdfLexer.Parsers
{
    public class ParsingContext
    {
        internal long CurrentOffset { get; set; }
        internal IPdfDataSource CurrentSource { get; set; }
        public bool CacheNumbers { get; set; } = true;

        internal NameParser NameParser { get; } = new NameParser();
        internal LazyNestedSeqParser NestedSeqParser { get; }
        internal LazyNestedSpanParser NestedSpanParser { get; }
        internal DictionaryParser DictionaryParser { get; }
        public ParsingContext()
        {
            NestedSeqParser = new LazyNestedSeqParser(this);
            NestedSpanParser = new LazyNestedSpanParser(this);
            DictionaryParser = new DictionaryParser(this);
        }

        internal IPdfObject GetPdfItem(PdfObjectType type, in ReadOnlySequence<byte> data, SequencePosition start, SequencePosition end)
        {
            return null;
        }

        internal IPdfObject GetPdfItem(PdfObjectType type, ReadOnlySpan<byte> data, int start, int length)
        {
            return null;
        }

        internal PdfLazyObject CreateLazy(PdfObjectType type, long subOffset, int length)
        {
            return new PdfLazyObject {
                    Offset = CurrentOffset + subOffset,
                    Length = length,
                    IsIndirect = false,
                    Type = type
                };
        }

        public void Clear()
        {
            CachedNumbers.Clear();
        }

        internal byte[] NumberBuffer = new byte[30];
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
