using System;
using System.Threading;
using PdfLexer.Parsers.Structure;

namespace PdfLexer
{
    public interface IPdfObject
    {
        public PdfObjectType Type { get; }
        public bool IsLazy { get; }
        public bool IsModified { get; }
    }

    public abstract class PdfObject : IPdfObject
    {
        //internal XRef IndirectRef { get; set; }
        //public virtual bool IsIndirect => IndirectRef.ObjectNumber != 0;
        public abstract PdfObjectType Type { get; }
        public virtual bool IsLazy { get => false; }
        public virtual bool IsModified { get => false; }
        //public abstract PdfObject Clone();
    }

    public static class PdfObjectExtensions
    {
        public static IPdfObject Resolve(this IPdfObject item)
        {
            if (item.IsLazy)
            {
                var lz = (PdfLazyObject)item;
                return lz.Resolve();
            }

            if (item.Type == PdfObjectType.IndirectRefObj)
            {
                var ir = (PdfIndirectRef)item;
                return ir.GetObject();
            }
            return item;
        }

        public static T GetValue<T>(this IPdfObject item) where T : IPdfObject
        {
            item = item.Resolve();

            if (item is T retyped)
            {
                return retyped;
            }

            throw new ApplicationException($"Mismatched data type requested, got {item.Type} expected {typeof(T)}");
        }
    }
}
