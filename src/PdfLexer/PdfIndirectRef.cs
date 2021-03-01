using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PdfLexer
{
    internal class ExistingIndirectRef : PdfIndirectRef
    {
        public ExistingIndirectRef(ParsingContext ctx, long objectNumber, int generation)
        {
            Context = ctx;
            Reference = new XRef 
            {
                ObjectNumber = (int)objectNumber,
                Generation = generation
            };
        }
        public ParsingContext Context { get; }
        internal override XRef Reference { get; set; }
        public override PdfObjectType Type => PdfObjectType.IndirectRefObj;
        public override IPdfObject GetObject() 
        {
            if (Object == null)
            {
                Object = Context.GetIndirectObject(Reference);
            }
            return Object;
        } 
        public override bool IsOwned(int sourceId) => Context.SourceId == sourceId;

        internal IPdfObject Object {get;set;}
    }

    public abstract class PdfIndirectRef : PdfObject
    {
        public abstract IPdfObject GetObject();
        public abstract bool IsOwned(int sourceId);
        internal abstract XRef Reference { get; set; }
        public override int GetHashCode()
        {
            if (this is ExistingIndirectRef ir)
            {
                return unchecked(ir.Context.SourceId + ir.Reference.Generation + ir.Reference.ObjectNumber);
            } else if (this is PdfIndirectReference im)
            {
                if (im.IsDefined)
                {
                    return unchecked(im.SourceId + im.Reference.Generation + im.Reference.ObjectNumber);
                } else
                {
                    return RuntimeHelpers.GetHashCode(im.Object);
                }
            }
            return RuntimeHelpers.GetHashCode(this);
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (this is ExistingIndirectRef tir)
            {
                if (!(obj is ExistingIndirectRef oir))
                {
                    return false;
                }
                return tir.Context.SourceId == oir.Context.SourceId && tir.Reference.Equals(oir.Reference);
            } else if (this is PdfIndirectReference tor)
            {
                if (!(obj is PdfIndirectReference oor))
                {
                    return false;
                }
                // TODO consider how this would work with defined references
                if (tor.Object.Equals(oor.Object))
                {
                    return true;
                }
            }
            return base.Equals(obj);
        }

        public static PdfIndirectRef Create(IPdfObject obj) => new PdfIndirectReference(obj);
    }

    internal class PdfIndirectReference : PdfIndirectRef
    {
        internal bool IsDefined => Reference.ObjectNumber != 0;
        internal override XRef Reference { get; set; }
        internal int SourceId { get; set; }
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

        internal PdfIndirectReference()
        {

        }

        public PdfIndirectReference(IPdfObject obj)
        {
            Object = obj;
        }
    }

    internal class IndirectRefToken : IPdfObject
    {
        public static IndirectRefToken Value => new IndirectRefToken();
        public bool IsIndirect => throw new NotImplementedException();
        public PdfObjectType Type => throw new NotImplementedException();
    }
}
