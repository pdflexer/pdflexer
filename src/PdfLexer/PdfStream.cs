using PdfLexer.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace PdfLexer
{
    /// <summary>
    /// Pdf stream object
    /// </summary>
    public class PdfStream : PdfObject
    {
        // TODO support external content?
        public PdfStream(PdfDictionary dictionary, PdfStreamContents contents)
        {
            Dictionary = dictionary;
            Contents = contents;
        }
        /// <summary>
        /// Dictionary portion of the Stream object.
        /// </summary>
        public PdfDictionary Dictionary { get; }
        /// <summary>
        /// Stream portion of the Pdf Object
        /// </summary>
        public PdfStreamContents Contents { get; set; }
        public bool IsIndirect => true;
        public override PdfObjectType Type => PdfObjectType.StreamObj;
        public override bool IsModified => Dictionary.IsModified; // TODO STREAM SUPPORT
        // /Length required
        // /Filter /DecodeParms -> if filters
        // /F, /FFilter, /FDecodeParms -> external file
    }

    /// <summary>
    /// Contents of a Pdf stream.
    /// </summary>
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
        /// <summary>
        /// Copies contents to the provided stream.
        /// </summary>
        /// <param name="destination"></param>
        public void CopyRawContents(Stream destination)
        {
            Source.CopyData(Offset, Length, destination);
        }
    }
}
