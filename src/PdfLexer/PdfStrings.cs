using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    // TODO support binary data
    /// <summary>
    /// Pdf string object.
    /// </summary>
    public class PdfString : PdfObject, IEquatable<PdfString>
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

        public override bool Equals(object obj)
        {
            return obj is PdfString str && Value.Equals(str?.Value);
        }

        public virtual bool Equals(PdfString other)
        {
            return Value.Equals(other?.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(PdfString n1, PdfString n2)
        {
            if (ReferenceEquals(n1, n2)) { return true; }
            if (ReferenceEquals(n1, null)) { return false; }
            if (ReferenceEquals(n2, null)) { return false; }
            return n1.Equals(n2);
        }

        public static bool operator !=(PdfString n1, PdfString n2) => !(n1 == n2);

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
