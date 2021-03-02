using PdfLexer.IO;
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
            Contents = contents;
        }
        public PdfDictionary Dictionary { get; }
        public PdfStreamContents Contents { get; set; }
        public bool IsIndirect => true;
        public PdfObjectType Type => PdfObjectType.StreamObj;

        // /Length required
        // /Filter /DecodeParms -> if filters
        // /F, /FFilter, /FDecodeParms -> external file
    }

    public class PdfStreamContents
    {
        internal IPdfDataSource Source { get; }
        internal long Offset { get; }
        public PdfStreamContents(IPdfDataSource source, long offset, int length)
        {
            Source = source;
            Offset = offset;
            Length = length;
        }
        public int Length { get;}
        public void CopyRawContents(Stream destination)
        {
            Source.CopyData(Offset, Length, destination);
        }
    }
}
