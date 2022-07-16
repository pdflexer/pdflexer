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

        public static bool operator ==(PdfName n1, PdfName n2)
        {
            if (ReferenceEquals(n1, n2)) { return true; }
            if (ReferenceEquals(n1, null)) { return false; }
            if (ReferenceEquals(n2, null)) { return false; }
            return n1.Equals(n2);
        }

        public static bool operator !=(PdfName n1, PdfName n2) => !(n1 == n2);

        public static readonly PdfName Prev = new PdfName("/Prev", false);
        public static readonly PdfName Length = new PdfName("/Length", false);
        public static readonly PdfName Root = new PdfName("/Root", false);
        public static readonly PdfName Pages = new PdfName("/Pages", false);
        public static readonly PdfName Page = new PdfName("/Page", false);
        public static readonly PdfName TypeName = new PdfName("/Type", false);
        public static readonly PdfName Count = new PdfName("/Count", false);
        public static readonly PdfName Kids = new PdfName("/Kids", false);
        public static readonly PdfName Contents = new PdfName("/Contents", false);
        public static readonly PdfName Resources = new PdfName("/Resources", false);
        public static readonly PdfName MediaBox = new PdfName("/MediaBox", false);
        public static readonly PdfName CropBox = new PdfName("/CropBox", false);
        public static readonly PdfName BleedBox = new PdfName("/BleedBox", false);
        public static readonly PdfName TrimBox = new PdfName("/TrimBox", false);
        public static readonly PdfName ArtBox = new PdfName("/ArtBox", false);
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
        public static readonly PdfName XObject = new PdfName("/XObject", false);
        public static readonly PdfName ProcSet = new PdfName("/ProcSet", false);
        public static readonly PdfName Font = new PdfName("/Font", false);
        public static readonly PdfName Shading = new PdfName("/Shading", false);
        public static readonly PdfName Pattern = new PdfName("/Pattern", false);
        public static readonly PdfName ColorSpace = new PdfName("/ColorSpace", false);
        public static readonly PdfName ExtGState = new PdfName("/ExtGState", false);
        public static readonly PdfName Form = new PdfName("/Form", false);


        public static readonly PdfName Widths = new PdfName("/Widths", false);
        public static readonly PdfName FontName = new PdfName("/FontName", false);
        public static readonly PdfName FontBBox = new PdfName("/FontBBox", false);
        public static readonly PdfName Type0 = new PdfName("/Type0", false);
        public static readonly PdfName Type3 = new PdfName("/Type3", false);
        public static readonly PdfName DescendantFonts = new PdfName("/DescendantFonts", false);
        public static readonly PdfName BaseFont = new PdfName("/BaseFont", false);
        public static readonly PdfName FontFile = new PdfName("/FontFile", false);
        public static readonly PdfName FontFile2 = new PdfName("/FontFile2", false);
        public static readonly PdfName FontFile3 = new PdfName("/FontFile3", false);
        public static readonly PdfName Length1 = new PdfName("/Length1", false);
        public static readonly PdfName Length2 = new PdfName("/Length2", false);
        public static readonly PdfName Length3 = new PdfName("/Length3", false);
        public static readonly PdfName FontMatrix = new PdfName("/FontMatrix", false);
        public static readonly PdfName Ascent = new PdfName("/Ascent", false);
        public static readonly PdfName Descent = new PdfName("/Descent", false);
        public static readonly PdfName XHeight = new PdfName("/XHeight", false);
        public static readonly PdfName CapHeight = new PdfName("/CapHeight", false);
        public static readonly PdfName Flags = new PdfName("/Flags", false);
        public static readonly PdfName ItalicAngle = new PdfName("/ItalicAngle", false);

        public static readonly PdfName Encoding = new PdfName("/Encoding", false);


        public static readonly PdfName DeviceGray = new PdfName("/DeviceGray", false);
        public static readonly PdfName DeviceRGB = new PdfName("/DeviceRGB", false);
        public static readonly PdfName DeviceCMYK = new PdfName("/DeviceCMYK", false);
        public static readonly PdfName Indexed = new PdfName("/Indexed", false);
        public static readonly PdfName CalGray = new PdfName("/CalGray", false);
        public static readonly PdfName CalRGB = new PdfName("/CalRGB", false);

        public static readonly PdfName Decode = new PdfName("/Decode", false);
        public static readonly PdfName ASCIIHexDecode = new PdfName("/ASCIIHexDecode", false);
        public static readonly PdfName ASCII85Decode = new PdfName("/ASCII85Decode", false);
        public static readonly PdfName LZWDecode = new PdfName("/LZWDecode", false);
        public static readonly PdfName FlateDecode = new PdfName("/FlateDecode", false);
        public static readonly PdfName RunLengthDecode = new PdfName("/RunLengthDecode", false);
        public static readonly PdfName CCITTFaxDecode = new PdfName("/CCITTFaxDecode", false);
        public static readonly PdfName DCTDecode = new PdfName("/DCTDecode", false);

        public static readonly PdfName Height = new PdfName("/Height", false);
        public static readonly PdfName Width = new PdfName("/Width", false);
        public static readonly PdfName Interpolate = new PdfName("/Interpolate", false);
        public static readonly PdfName ImageMask = new PdfName("/ImageMask", false);
        public static readonly PdfName Mask = new PdfName("/Mask", false);
        public static readonly PdfName SMask = new PdfName("/SMask", false);
        public static readonly PdfName Image = new PdfName("/Image", false);

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
