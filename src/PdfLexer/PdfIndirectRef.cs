using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace PdfLexer
{
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
        public override bool IsOwned(int sourceId) => SourceId == sourceId;

        internal IPdfObject Object {get;set;}

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

        // internal override bool CanLazyCopy(int destinationId)
        // {
        //     if (destinationId != SourceId)
        //     {
        //         return false;
        //     }
        // 
        //     ulong id = ((ulong)Reference.ObjectNumber << 16) | ((uint)Reference.Generation & 0xFFFF);
        //     if (Context.IndirectCache.TryGetValue(id, out var value))
        //     {
        //         switch (value.Type)
        //         {
        //             case PdfObjectType.ArrayObj:
        //                 var arr = (PdfArray)value;
        //                 return !arr.IsModified;
        //             case PdfObjectType.DictionaryObj:
        //                 var dict = (PdfDictionary)value;
        //                 return !dict.IsModified;
        //         }
        //         return true;
        //     }
        //     return true;
        // 
        // }

        // internal override void CopyExistingData(Stream stream)
        // {
        //     Context.CopyExsitingData(Reference, stream);
        // }
    }

    public abstract class PdfIndirectRef : PdfObject
    {
        public abstract IPdfObject GetObject();
        public abstract bool IsOwned(int sourceId);
        internal abstract XRef Reference { get; set; }
        internal abstract int SourceId { get; set; }
        // internal abstract bool CanLazyCopy(int destinationId);
        // internal abstract void CopyExistingData(Stream stream);
        public static PdfIndirectRef Create(IPdfObject obj) => new NewIndirectRef(obj);
    }

    internal class NewIndirectRef : PdfIndirectRef
    {
        internal bool IsDefined => Reference.ObjectNumber != 0;
        internal override XRef Reference { get; set; }
        internal override int SourceId { get; set; }
        public IPdfObject Object { get; internal set; }
        public override PdfObjectType Type => PdfObjectType.IndirectRefObj;
        public override IPdfObject GetObject() => Object;

        public override bool IsOwned(int sourceId) 
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

        // internal override bool CanLazyCopy(int destinationId) => false;
        // 
        // internal override void CopyExistingData(Stream stream)
        // {
        //     throw new NotSupportedException();
        // }
    }

    internal class IndirectRefToken : IPdfObject
    {
        public static IndirectRefToken Value => new IndirectRefToken();
        public bool IsIndirect => throw new NotImplementedException();
        public PdfObjectType Type => throw new NotImplementedException();
        public bool IsLazy => throw new NotImplementedException();
    }

    internal struct SourcedXRef
    {
        public int SourceId;
        public XRef Reference;
        public override int GetHashCode()
        {
            return unchecked(100000*SourceId + Reference.Generation + Reference.ObjectNumber);
        }

        public override bool Equals(object obj)
        {
            if (obj is SourcedXRef sxr)
            {
                return SourceId == sxr.SourceId 
                    && Reference.Generation == sxr.Reference.Generation 
                    && Reference.ObjectNumber == sxr.Reference.ObjectNumber;
            }
            return false;
        }
    }
}
