using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.Serializers;

namespace PdfLexer
{
    public interface IPdfObject
    {
        public bool IsIndirect { get; }
        public PdfObjectType Type { get; }
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

        private static NameSerializer NameSerializer = new NameSerializer();
        internal static void WriteObject(this IPdfObject obj, Stream stream)
        {
            switch (obj)
            {
                case PdfLazyObject lz:
                    lz.WriteObject(stream);
                    return;
                case PdfDictionary dict:
                    if (dict.LazyWrapper != null)
                    {
                        dict.LazyWrapper.WriteObject(stream);
                    }
                    else
                    {
                        // TODO 255 line length limit
                        stream.Write(PdfDictionary.start);
                        foreach (var item in dict)
                        {
                            NameSerializer.WriteToStream(item.Key, stream);
                            if (item.Value.Type != PdfObjectType.NameObj
                                && item.Value.Type != PdfObjectType.ArrayObj
                                && item.Value.Type != PdfObjectType.DictionaryObj
                                && item.Value.Type != PdfObjectType.StreamObj)
                            {
                                stream.WriteByte((byte)' ');
                            }
                            item.Value.WriteObject(stream);
                        }
                        stream.Write(PdfDictionary.end);
                    }

                    return;
                case PdfBoolean bl:
                    stream.Write(bl.Value ? PdfBoolean.TrueBytes : PdfBoolean.FalseBytes);
                    return;
                case PdfName nm:
                    //
                    bool escapeNeeded = false;
                    if (nm.NeedsEscaping == null)
                    {
                        // todo
                    }

                    if (!escapeNeeded)
                    {
                        Span<byte> buffer = stackalloc byte[nm.Value.Length];
                        Encoding.ASCII.GetBytes(nm.Value, buffer);
                        stream.Write(buffer);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    break;
                default:
                    throw new ApplicationException($"Unknown object type for writing: {obj.GetType()}");
            }
        }
    }




}
