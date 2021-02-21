using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.Parsers;
using PdfLexer.Serializers;

namespace PdfLexer
{
    public interface IPdfObject
    {
        public bool IsIndirect { get; }
        public PdfObjectType Type { get; }
    }

    public abstract class PdfObject : IPdfObject
    {
        internal XRef IndirectRef { get; set; }
        public virtual bool IsIndirect => IndirectRef.ObjectNumber > 0 || IndirectRef.ObjectNumber == -1;
        public abstract PdfObjectType Type { get; }
    }

    public static class PdfObjectExtensions
    {
        public static PdfNull GetNull(this IPdfObject obj)
        {
            if (obj is PdfNull pdfNull)
            {
                return pdfNull;
            }

            throw new NotSupportedException($"Null object requested from {obj.GetType()}");
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
                case PdfDictionary dict:
                    return dict.IsModified;
                default:
                    return false; // rest immutable
                    // throw new ApplicationException($"Unknown object type for checking modification: {obj.GetType()}");
            }
        }

    }
}
