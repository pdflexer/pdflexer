using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer
{
    /// <summary>
    /// PDF Name Object
    /// </summary>
    public class PdfName : PdfObject, IEquatable<PdfName>
    {
        internal ulong CacheValue {get;set;}
        internal bool? NeedsEscaping { get;} 
        /// <summary>
        /// PdfName CTOR
        /// </summary>
        /// <param name="value">Pdf name including '/'</param>
        public PdfName(string value)
        {
            Value = value;
            NeedsEscaping = null;
        }

        internal PdfName(string value, bool needsEscaping)
        {
            Value = value;
            NeedsEscaping = needsEscaping;
        }

        /// <summary>
        /// Value of the PDF Name including '/'
        /// </summary>
        public string Value { get; }

        public override PdfObjectType Type => PdfObjectType.NameObj;
        public override bool Equals(object obj)
        {
            return obj is PdfName name && Value.Equals(name?.Value);
        }

        public virtual bool Equals(PdfName other)
        {
            return Value.Equals(other?.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static PdfName Prev => new PdfName("/Prev", false);
        public static PdfName Length => new PdfName("/Length", false);
        public static PdfName Root => new PdfName("/Root", false);
        public static PdfName Pages => new PdfName("/Pages", false);
        public static PdfName TypeName => new PdfName("/Type", false);
        public static PdfName Count => new PdfName("/Count", false);
        public static PdfName Kids => new PdfName("/Kids", false);
        public static PdfName Contents => new PdfName("/Contents", false);
        public static PdfName Resources => new PdfName("/Resources", false);
        public static PdfName MediaBox => new PdfName("/MediaBox", false);
        public static PdfName CropBox => new PdfName("/CropBox", false);
        public static PdfName Rotate => new PdfName("/Rotate", false);
        public static PdfName Catalog => new PdfName("/Catalog", false);
        public static PdfName Filter => new PdfName("/Filter", false);
        public static PdfName Predictor => new PdfName("/Predictor", false);
        public static PdfName Columns => new PdfName("/Columns", false);
        public static PdfName Colors => new PdfName("/Colors", false);
        public static PdfName BitsPerComponent => new PdfName("/BitsPerComponent", false);
        public static PdfName EarlyChange => new PdfName("/EarlyChange", false);
        public static PdfName DecodeParms => new PdfName("/DecodeParms", false);
        public static PdfName W => new PdfName("/W", false);
        public static PdfName N => new PdfName("/N", false);
        public static PdfName First => new PdfName("/First", false);
        public static PdfName Index => new PdfName("/Index", false);
        public static PdfName XRefStm => new PdfName("/XRefStm", false);
        public static PdfName XRef => new PdfName("/XRef", false);
        public static PdfName Parent => new PdfName("/Parent", false);
        public static PdfName ObjStm => new PdfName("/ObjStm", false);
        public static PdfName Encrypt => new PdfName("/Encrypt", false);
        public static PdfName Size => new PdfName("/Size", false);
        
        public static implicit operator PdfName(string name)
        {
            // TODO common lookups for above?
            if (name[0] == '/')
            {
                return new PdfName(name);
            } else
            {
                return new PdfName("/"+name);
            }
            
        }

        public override string ToString()
        {
            return Value;
        }


    }
}
