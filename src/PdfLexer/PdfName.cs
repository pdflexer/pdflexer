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

        public static readonly PdfName Prev = new PdfName("/Prev", false);
        public static readonly PdfName Length = new PdfName("/Length", false);
        public static readonly PdfName Root = new PdfName("/Root", false);
        public static readonly PdfName Pages = new PdfName("/Pages", false);
        public static readonly PdfName TypeName = new PdfName("/Type", false);
        public static readonly PdfName Count = new PdfName("/Count", false);
        public static readonly PdfName Kids = new PdfName("/Kids", false);
        public static readonly PdfName Contents = new PdfName("/Contents", false);
        public static readonly PdfName Resources = new PdfName("/Resources", false);
        public static readonly PdfName MediaBox = new PdfName("/MediaBox", false);
        public static readonly PdfName CropBox = new PdfName("/CropBox", false);
        public static readonly PdfName Rotate = new PdfName("/Rotate", false);
        public static readonly PdfName Catalog = new PdfName("/Catalog", false);
        public static readonly PdfName Filter = new PdfName("/Filter", false);
        public static readonly PdfName Predictor = new PdfName("/Predictor", false);
        public static readonly PdfName Columns = new PdfName("/Columns", false);
        public static readonly PdfName Colors = new PdfName("/Colors", false);
        public static readonly PdfName BitsPerComponent = new PdfName("/BitsPerComponent", false);
        public static readonly PdfName EarlyChange = new PdfName("/EarlyChange", false);
        public static readonly PdfName DecodeParms = new PdfName("/DecodeParms", false);
        public static readonly PdfName W = new PdfName("/W", false);
        public static readonly PdfName N = new PdfName("/N", false);
        public static readonly PdfName First = new PdfName("/First", false);
        public static readonly PdfName Index = new PdfName("/Index", false);
        public static readonly PdfName XRefStm = new PdfName("/XRefStm", false);
        public static readonly PdfName XRef = new PdfName("/XRef", false);
        public static readonly PdfName Parent = new PdfName("/Parent", false);
        public static readonly PdfName ObjStm = new PdfName("/ObjStm", false);
        public static readonly PdfName Encrypt = new PdfName("/Encrypt", false);
        public static readonly PdfName Size = new PdfName("/Size", false);
        public static readonly PdfName Annots = new PdfName("/Annots", false);
        public static readonly PdfName Subtype = new PdfName("/Subtype", false);
        public static readonly PdfName Link = new PdfName("/Link", false);
        public static readonly PdfName Dest = new PdfName("/Dest", false);

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
