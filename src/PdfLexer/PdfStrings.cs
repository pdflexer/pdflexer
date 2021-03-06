using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{

    // public class PdfByteString : PdfObject
    // {
    //     public PdfStringType StringType {get; set; }
    //     // TODO support lazy writing
    //     public PdfByteString(string value)
    //     {
    //         Value = value;
    //     }
    //     public string Value { get; }
    //     public override PdfObjectType Type => PdfObjectType.StringObj;
    // }
    // 
    // public class PdfAsciiString : PdfObject
    // {
    //     public PdfStringType StringType {get; set; }
    //     // TODO support lazy writing
    //     public PdfAsciiString(string value)
    //     {
    //         Value = value;
    //     }
    //     public string Value { get; }
    //     public override PdfObjectType Type => PdfObjectType.StringObj;
    // }

    // TODO support binary data
    /// <summary>
    /// Pdf string object.
    /// </summary>
    public class PdfString : PdfObject
    {
        /// <summary>
        /// Type of Pdf string.
        /// </summary>
        public PdfStringType StringType {get; }
        /// <summary>
        /// Encoding of the Pdf string.
        /// When creating a new PDF String <see cref="PdfTextEncodingType.Calculate"/>
        /// can be used to automatically calculate which encoding shoudl be used
        /// </summary>
        public PdfTextEncodingType Encoding {get; }

        /// <summary>
        /// Creates a new Pdf string object with PdfDocument encoding
        /// and literal string type.
        /// </summary>
        /// <param name="value"></param>
        public PdfString(string value)
        {
            Value = value;
            Encoding = PdfTextEncodingType.PdfDocument;
            StringType = PdfStringType.Literal;
        }
        /// <summary>
        /// Creates a new Pdf string object.
        /// </summary>
        /// <param name="value">Text value of string</param>
        /// <param name="type">Type of string to create.</param>
        /// <param name="encoding">Encoding to use when writing string.</param>
        public PdfString(string value, PdfStringType type, PdfTextEncodingType encoding)
        {
            Value = value;
            Encoding = encoding;
            StringType = type;
        }
        public string Value { get; }
        public override PdfObjectType Type => PdfObjectType.StringObj;

    }

    /// <summary>
    /// Type of pdf string
    /// </summary>
    public enum PdfStringType
    {
        Literal,
        Hex
    }

    /// <summary>
    /// Pdf text encoding type
    /// </summary>
    public enum PdfTextEncodingType
    {
        Calculate,
        PdfDocument,
        UTF16BE
    }
}
