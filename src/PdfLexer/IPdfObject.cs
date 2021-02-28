using System;
using System.Threading;
using PdfLexer.Parsers.Structure;

namespace PdfLexer
{
    public interface IPdfObject
    {
        // public bool IsIndirect { get; }
        public PdfObjectType Type { get; }
    }

    public abstract class PdfObject : IPdfObject
    {
        //internal XRef IndirectRef { get; set; }
        //public virtual bool IsIndirect => IndirectRef.ObjectNumber != 0;
        public abstract PdfObjectType Type { get; }
        //public abstract PdfObject Clone();
    }

    public static class PdfObjectExtensions
    {
        public static IPdfObject Resolve(this IPdfObject item)
        {
            if (item is PdfIndirectRef ir)
            {
                var resolved = ir.GetObject();
                return resolved;
            }
            else if (item is PdfLazyObject lz)
            {
                var resolved = lz.Resolve();
                return resolved;
            }
            return item;
        }

        public static T GetValue<T>(this IPdfObject item) where T : IPdfObject
        {
            if (item is T typed)
            {
                return typed;
            }

            item = item.Resolve();

            if (item is T retyped)
            {
                return retyped;
            }

            throw new ApplicationException($"Mismatched data type requested, got {item.Type} expected {typeof(T)}");
        }

        internal static bool CheckForIndirect(this PdfLazyObject obj)
        {
            if (obj.Parsed != null)
            {
                return obj.Parsed.HasLazyIndirect;
            }

            return obj.HasLazyIndirect;
        }

        internal static bool IsModified(this IPdfObject obj)
        {
            switch (obj)
            {
                case PdfLazyObject lz:
                    return lz.Parsed?.IsModified ?? false;
                case IParsedLazyObj parsed:
                    return parsed.IsModified;
                default:
                    return false; // rest immutable
            }
        }
    }
}
