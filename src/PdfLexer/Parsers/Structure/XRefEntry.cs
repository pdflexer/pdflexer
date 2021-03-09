﻿using PdfLexer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Parsers.Structure
{
    public enum XRefType
    {
        Normal,
        Compressed
    }

    public struct XRef
    {
        public XRef(int objectNumber, int generation)
        {
            ObjectNumber = objectNumber;
            Generation = generation;
        }
        public int ObjectNumber { get; internal set; }
        public int Generation { get; internal set; }
        public override int GetHashCode()
        {
            return unchecked(ObjectNumber.GetHashCode() + Generation.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is XRef key)
            {
                return key.ObjectNumber.Equals(ObjectNumber) && key.Generation.Equals(Generation);
            }
            return false;
        }

        public override string ToString()
        {
            return $"{ObjectNumber} {Generation}";
        }

        public ulong GetId() => ((ulong)ObjectNumber << 16) | ((uint)Generation & 0xFFFF);
        public static ulong GetId(int objectNumber, int generation) => ((ulong)objectNumber << 16) | ((uint)generation & 0xFFFF);
    }

    public class XRefEntry
    {
        public XRefType Type { get; set; }
        public XRef Reference { get; set; }
        public bool IsFree { get; set; }
        /// <summary>
        /// Offset of the start of object. If <see cref="Type"/> is Compressed, this may be 0 if the 
        /// object stream has not been read.
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// Maximum length of the object data.
        /// </summary>
        public int MaxLength { get; set; }
        /// <summary>
        /// Object number of the object stream the referenced object is contained in.
        /// </summary>
        public int ObjectStreamNumber { get; set; }
        /// <summary>
        /// Index in the object stream.
        /// </summary>
        public int ObjectIndex { get; set; }
        /// <summary>
        /// Data source containing the referenced data.
        /// This will be the main document source for uncompressed objects.
        /// For compressed objects it will be a wrapper around the object stream
        /// MAY BE INITIALLY NULL
        /// </summary>
        public IPdfDataSource Source { get; set; }
        /// <summary>
        /// Gets the object this entry points to.
        /// </summary>
        /// <returns>IPdfObject</returns>
        public IPdfObject GetObject() => Source.GetIndirectObject(this);
        /// <summary>
        /// Copies the data for the object this XRef points to the provided stream.
        /// Excludes object header and trailer (obj/endobj).
        /// </summary>
        /// <param name="destination">Stream to write to</param>
        public void CopyUnwrappedData(Stream destination) => Source.CopyIndirectObject(this, destination);
    }
}