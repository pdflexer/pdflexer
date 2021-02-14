using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace PdfLexer
{
    public class PdfStream : IPdfObject
    {
        // TODO support external content?
        public PdfStream(PdfDictionary dictionary, PdfStreamContents contents)
        {
            Dictionary = dictionary;
        }
        public PdfDictionary Dictionary { get; }
        public bool IsIndirect => true;
        public PdfObjectType Type => PdfObjectType.StreamObj;

        // /Length required
        // /Filter /DecodeParms -> if filters
        // /F, /FFilter, /FDecodeParms -> external file
    }

    public class PdfStreamContents
    {
        public int Length { get;}
        public void CopyRawContents(Stream destination)
        {
        }
    }
}
