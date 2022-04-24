using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PdfLexer
{
    /// <summary>
    /// PDF reference to a indirect object.
    /// </summary>
    public abstract class PdfIndirectRef : PdfObject
    {
        /// <summary>
        /// Gets the referenced PDF object.
        /// </summary>
        /// <returns>PDF object</returns>
        public abstract IPdfObject GetObject();
        /// <summary>
        /// Creates a new reference to a PDF object.
        /// </summary>
        /// <param name="obj">Object to point to</param>
        /// <returns>PdfIndirectRef</returns>
        public static PdfIndirectRef Create(IPdfObject obj) => new NewIndirectRef(obj);
        internal abstract bool IsOwned(int sourceId);
        internal abstract XRef Reference { get; set; }
        internal abstract int SourceId { get; set; }
        internal abstract bool DeferWriting { get; set; }

    }

    /// <summary>
    /// Indirect reference from an existing document.
    /// </summary>
    internal class ExistingIndirectRef : PdfIndirectRef
    {
        public ExistingIndirectRef(ParsingContext ctx, long objectNumber, int generation)
        {
            Context = ctx;
            SourceId = ctx.SourceId;
            Reference = new XRef 
            {
                ObjectNumber = (int)objectNumber,
                Generation = generation
            };
        }

        public ExistingIndirectRef(ParsingContext ctx, XRef reference)
        {
            Context = ctx;
            SourceId = ctx.SourceId;
            Reference = reference;
        }

        public ParsingContext Context { get; }
        internal override XRef Reference { get; set; }
        internal override int SourceId { get; set; }
        public override PdfObjectType Type => PdfObjectType.IndirectRefObj;
        public override IPdfObject GetObject() 
        {
            if (Object == null)
            {
                Object = Context.GetIndirectObject(Reference);
            }
            return Object;
        } 
        internal override bool IsOwned(int sourceId) => SourceId == sourceId;

        internal IPdfObject Object {get;set;}
        internal override bool DeferWriting { get; set; }

        public override int GetHashCode()
        {
            return unchecked(SourceId + Reference.Generation + Reference.ObjectNumber).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is ExistingIndirectRef oir))
            {
                return false;
            }

            return SourceId == oir.SourceId && Reference.Equals(oir.Reference);
        }
    }


    /// <summary>
    /// Indirect reference created by the library to be written at a later point.
    /// </summary>
    internal class NewIndirectRef : PdfIndirectRef
    {
        public override PdfObjectType Type => PdfObjectType.IndirectRefObj;
        public override IPdfObject GetObject() => Object;

        internal bool IsDefined => Reference.ObjectNumber != 0;
        internal override XRef Reference { get; set; }
        internal override int SourceId { get; set; }

        internal override bool DeferWriting { get; set; }
        public IPdfObject Object { get; internal set; }

        internal override bool IsOwned(int sourceId) 
        {
            if (SourceId == 0)
            {
                SourceId = sourceId;
                return true;
            }
            return SourceId == sourceId;
        }

        internal NewIndirectRef()
        {

        }

        public NewIndirectRef(IPdfObject obj)
        {
            Object = obj;
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(Object);
        }

        public override bool Equals(object obj)
        {
            if (System.Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is NewIndirectRef oor))
            {
                return false;
            }

            if (Object.Equals(oor.Object))
            {
                return true;
            }
            return false;
        }
    }

    // dummy object representing the R in a indirect reference.
    // this is just used to simplify parsing dictionary and arrays
    internal class IndirectRefToken : IPdfObject
    {
        public IndirectRefToken()
        {

        }
        public static IndirectRefToken Value { get; } = new IndirectRefToken();
        public bool IsIndirect => throw new NotImplementedException();
        public PdfObjectType Type => throw new NotImplementedException();
        public bool IsLazy => throw new NotImplementedException();
        public bool IsModified => throw new NotImplementedException();
    }
}
