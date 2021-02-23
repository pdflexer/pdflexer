using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    public class PdfByteString : PdfObject
    {
        public PdfStringType StringType {get; set; }
        // TODO support lazy writing
        public PdfByteString(string value)
        {
            Value = value;
        }
        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.StringObj;
    }

    public class PdfAsciiString : PdfObject
    {
        public PdfStringType StringType {get; set; }
        // TODO support lazy writing
        public PdfAsciiString(string value)
        {
            Value = value;
        }
        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.StringObj;
    }

    public class PdfString : PdfObject
    {
        public PdfStringType StringType {get; }
        public PdfTextEncodingType Encoding {get; }
        // TODO support lazy writing
        public PdfString(string value, PdfStringType type, PdfTextEncodingType encoding)
        {
            Value = value;
            Encoding = encoding;
            StringType = type;
        }
        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.StringObj;

    }

    public enum PdfStringType
    {
        Literal,
        Hex
    }

    public enum PdfTextEncodingType
    {
        Calculate,
        PdfDocument,
        UTF16BE
    }
}
