using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PdfLexer.Serializers
{
    public class DocumentWriter
    {
        private WritingContext _ctx;
        public DocumentWriter(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

    }
}
