using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM
{
    public class XRefStream
    {
        private PdfStream _stream;

        public XRefStream(PdfStream stream)
        {
            _stream = stream;
        }

        // /Type /Xref required
        // /Size number required (same as trailer)
        // Index (optional array) [0 Size] default, one for each section
        // Prev -> byet offset for previous  xref stream
        // W required [N1 N1 N1] array, total size per record is N1+N2+N3 = NT
        // stream contents -> repeated entries on NT size
    }
}
