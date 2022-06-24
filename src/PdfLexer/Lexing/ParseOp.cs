using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Lexing
{
    public class ParseOp
    {
        public ParseOpType Type { get; set; }
    }

    public enum ParseOpType
    {
        ScanTo,
        ScanToAndRead,
        ReadToken
    }
}
