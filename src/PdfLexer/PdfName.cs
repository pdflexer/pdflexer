namespace PdfLexer;

/// <summary>
/// PDF Name Object
/// </summary>
public class PdfName : PdfObject, IEquatable<PdfName>
{
    internal ulong CacheValue { get; set; }
    internal bool? NeedsEscaping { get; }
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

    public static bool operator ==(PdfName? n1, PdfName? n2)
    {
        if (ReferenceEquals(n1, n2)) { return true; }
        if (n1 is null) { return false; }
        if (n2 is null) { return false; }
        return n1.Equals(n2);
    }

    public static bool operator !=(PdfName? n1, PdfName? n2) => !(n1 == n2);

    
    public static readonly PdfName Name = new("/Name", false);
    public static readonly PdfName CharProcs = new("/CharProcs", false);
    public static readonly PdfName Prev = new("/Prev", false);
    public static readonly PdfName Length = new("/Length", false);
    public static readonly PdfName Root = new("/Root", false);
    public static readonly PdfName Pages = new("/Pages", false);
    public static readonly PdfName Page = new("/Page", false);
    public static readonly PdfName TYPE = new("/Type", false);
    public static readonly PdfName TypeName = new("/Type", false);
    public static readonly PdfName Count = new("/Count", false);
    public static readonly PdfName Kids = new("/Kids", false);
    public static readonly PdfName Contents = new("/Contents", false);
    public static readonly PdfName Resources = new("/Resources", false);
    public static readonly PdfName MediaBox = new("/MediaBox", false);
    public static readonly PdfName CropBox = new("/CropBox", false);
    public static readonly PdfName BleedBox = new("/BleedBox", false);
    public static readonly PdfName TrimBox = new("/TrimBox", false);
    public static readonly PdfName ArtBox = new("/ArtBox", false);
    public static readonly PdfName Rotate = new("/Rotate", false);
    public static readonly PdfName Catalog = new("/Catalog", false);
    public static readonly PdfName Filter = new("/Filter", false);
    public static readonly PdfName Predictor = new("/Predictor", false);
    public static readonly PdfName Columns = new("/Columns", false);
    public static readonly PdfName Colors = new("/Colors", false);
    public static readonly PdfName BitsPerComponent = new("/BitsPerComponent", false);
    public static readonly PdfName EarlyChange = new("/EarlyChange", false);
    public static readonly PdfName DecodeParms = new("/DecodeParms", false);
    public static readonly PdfName W = new("/W", false);
    public static readonly PdfName N = new("/N", false);
    public static readonly PdfName First = new("/First", false);
    public static readonly PdfName Index = new("/Index", false);
    public static readonly PdfName XRefStm = new("/XRefStm", false);
    public static readonly PdfName XRef = new("/XRef", false);
    public static readonly PdfName Parent = new("/Parent", false);
    public static readonly PdfName ObjStm = new("/ObjStm", false);
    public static readonly PdfName Encrypt = new("/Encrypt", false);
    public static readonly PdfName Size = new("/Size", false);
    public static readonly PdfName Annots = new("/Annots", false);
    public static readonly PdfName Subtype = new("/Subtype", false);
    public static readonly PdfName Link = new("/Link", false);
    public static readonly PdfName Dest = new("/Dest", false);
    public static readonly PdfName XObject = new("/XObject", false);
    public static readonly PdfName ProcSet = new("/ProcSet", false);
    public static readonly PdfName Font = new("/Font", false);
    public static readonly PdfName Shading = new("/Shading", false);
    public static readonly PdfName Pattern = new("/Pattern", false);
    public static readonly PdfName ColorSpace = new("/ColorSpace", false);
    public static readonly PdfName ExtGState = new("/ExtGState", false);
    public static readonly PdfName Form = new("/Form", false);
    public static readonly PdfName BBox = new("/BBox", false);

    


    public static readonly PdfName Widths = new("/Widths", false);
    public static readonly PdfName FontName = new("/FontName", false);
    public static readonly PdfName FontFamily = new("/FontFamily", false);
    public static readonly PdfName FontWeight = new("/FontWeight", false);
    public static readonly PdfName Leading = new("/Leading", false);
    public static readonly PdfName StemV = new("/StemV", false);

    public static readonly PdfName StemH = new("/StemH", false);
    public static readonly PdfName MissingWidth = new("/MissingWidth", false);
    public static readonly PdfName CharSet = new("/CharSet", false);
    

    public static readonly PdfName FontBBox = new("/FontBBox", false);
    public static readonly PdfName Type0 = new("/Type0", false);
    public static readonly PdfName Type1 = new("/Type1", false);
    public static readonly PdfName Type3 = new("/Type3", false);
    public static readonly PdfName CIDFontType0 = new("/CIDFontType0", false);
    public static readonly PdfName CIDFontType2 = new("/CIDFontType2", false);

    public static readonly PdfName DW = new("/DW", false);
    public static readonly PdfName DW2 = new("/DW2", false);
    public static readonly PdfName W2 = new("/W2", false);
    public static readonly PdfName CIDToGIDMap = new("/CIDToGIDMap", false);
    public static readonly PdfName CIDSystemInfo = new("/CIDSystemInfo", false);
    public static readonly PdfName Registry = new("/Registry", false);
    public static readonly PdfName Ordering = new("/Ordering", false);
    public static readonly PdfName Supplement = new("/Supplement", false);

    public static readonly PdfName Matrix = new("/Matrix", false);
    public static readonly PdfName DescendantFonts = new("/DescendantFonts", false);
    public static readonly PdfName BaseFont = new("/BaseFont", false);
    public static readonly PdfName FontFile = new("/FontFile", false);
    public static readonly PdfName FontFile2 = new("/FontFile2", false);
    public static readonly PdfName FontFile3 = new("/FontFile3", false);
    public static readonly PdfName Length1 = new("/Length1", false);
    public static readonly PdfName Length2 = new("/Length2", false);
    public static readonly PdfName Length3 = new("/Length3", false);
    public static readonly PdfName FontMatrix = new("/FontMatrix", false);
    public static readonly PdfName Ascent = new("/Ascent", false);
    public static readonly PdfName Descent = new("/Descent", false);
    public static readonly PdfName XHeight = new("/XHeight", false);
    public static readonly PdfName CapHeight = new("/CapHeight", false);
    public static readonly PdfName Flags = new("/Flags", false);
    public static readonly PdfName ItalicAngle = new("/ItalicAngle", false);

    public static readonly PdfName FirstChar = new("/FirstChar", false);
    public static readonly PdfName LastChar = new("/LastChar", false);
    public static readonly PdfName FontDescriptor = new("/FontDescriptor", false);
    public static readonly PdfName Encoding = new("/Encoding", false);
    public static readonly PdfName BaseEncoding = new("/BaseEncoding", false);
    public static readonly PdfName ToUnicode = new("/ToUnicode", false);
    public static readonly PdfName Differences = new("/Differences", false);

    public static readonly PdfName DeviceGray = new("/DeviceGray", false);
    public static readonly PdfName DeviceRGB = new("/DeviceRGB", false);
    public static readonly PdfName DeviceCMYK = new("/DeviceCMYK", false);
    public static readonly PdfName Indexed = new("/Indexed", false);
    public static readonly PdfName CalGray = new("/CalGray", false);
    public static readonly PdfName CalRGB = new("/CalRGB", false);
    public static readonly PdfName Lab = new("/Lab", false);

    public static readonly PdfName Decode = new("/Decode", false);
    public static readonly PdfName ASCIIHexDecode = new("/ASCIIHexDecode", false);
    public static readonly PdfName ASCII85Decode = new("/ASCII85Decode", false);
    public static readonly PdfName LZWDecode = new("/LZWDecode", false);
    public static readonly PdfName FlateDecode = new("/FlateDecode", false);
    public static readonly PdfName RunLengthDecode = new("/RunLengthDecode", false);
    public static readonly PdfName CCITTFaxDecode = new("/CCITTFaxDecode", false);
    public static readonly PdfName DCTDecode = new("/DCTDecode", false);

    public static readonly PdfName Height = new("/Height", false);
    public static readonly PdfName Width = new("/Width", false);
    public static readonly PdfName Interpolate = new("/Interpolate", false);
    public static readonly PdfName ImageMask = new("/ImageMask", false);
    public static readonly PdfName Mask = new("/Mask", false);
    public static readonly PdfName SMask = new("/SMask", false);
    public static readonly PdfName Image = new("/Image", false);

    public static readonly PdfName WinAnsiEncoding = new("/WinAnsiEncoding", false);
    public static readonly PdfName StandardEncoding = new("/StandardEncoding", false);
    public static readonly PdfName MacRomanEncoding = new("/MacRomanEncoding", false);
    public static readonly PdfName SymbolSetEncoding = new("/SymbolSetEncoding", false);
    public static readonly PdfName ZapfDingbatsEncoding = new("/ZapfDingbatsEncoding", false);
    public static readonly PdfName ExpertEncoding = new("/ExpertEncoding", false);
    public static readonly PdfName MacExpertEncoding = new("/MacExpertEncoding", false);

    public static implicit operator PdfName(string name)
    {
        // TODO common lookups for above?
        if (name[0] == '/')
        {
            return new(name);
        }
        else
        {
            return new("/" + name);
        }

    }

    public override string ToString()
    {
        return Value;
    }


}
