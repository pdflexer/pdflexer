﻿namespace PdfLexer;

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
    /// <param name="value">Pdf name excluding '/'</param>
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
    /// Value of the PDF Name excluding '/'
    /// </summary>
    public string Value { get; }

    public override PdfObjectType Type => PdfObjectType.NameObj;
    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        if (obj is string str)
        {
            if (str.Length > 0 && str[0] == '/')
            {
                if (Value.Length > 0 && Value[0] != '/')
                {
                    return Value.AsSpan().SequenceEqual(str.AsSpan().Slice(1));
                }
            }
            return Value.Equals(str);
        }
        return obj is PdfName name && Value.Equals(name?.Value);
    }

    public virtual bool Equals(PdfName? other)
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


    public static readonly PdfName a = new("a", false);
    public static readonly PdfName A = new("A", false);
    public static readonly PdfName Absolute = new("Absolute", false);
    public static readonly PdfName AbsoluteColorimetric = new("AbsoluteColorimetric", false);
    public static readonly PdfName AcroFormInteract = new("AcroFormInteract", false);
    public static readonly PdfName Action = new("Action", false);
    public static readonly PdfName AD3 = new("AD3", false);
    public static readonly PdfName ADBE_Extn3 = new("ADBE_Extn3", false);
    public static readonly PdfName adbepkcs7detached = new("adbe.pkcs7.detached", false);
    public static readonly PdfName adbepkcs7s3 = new("adbe.pkcs7.s3", false);
    public static readonly PdfName adbepkcs7s4 = new("adbe.pkcs7.s4", false);
    public static readonly PdfName adbepkcs7s5 = new("adbe.pkcs7.s5", false);
    public static readonly PdfName adbepkcs7sha1 = new("adbe.pkcs7.sha1", false);
    public static readonly PdfName adbex509rsa_sha1 = new("adbe.x509.rsa_sha1", false);
    public static readonly PdfName Add = new("Add", false);
    public static readonly PdfName AddRKSJH = new("Add-RKSJ-H", false);
    public static readonly PdfName AddRKSJV = new("Add-RKSJ-V", false);
    public static readonly PdfName AdobePPKLite = new("Adobe.PPKLite", false);
    public static readonly PdfName AdobePubSec = new("Adobe.PubSec", false);
    public static readonly PdfName AESV2 = new("AESV2", false);
    public static readonly PdfName AESV3 = new("AESV3", false);
    public static readonly PdfName AESV4 = new("AESV4", false);
    public static readonly PdfName After = new("After", false);
    public static readonly PdfName ALaw = new("ALaw", false);
    public static readonly PdfName All = new("All", false);
    public static readonly PdfName AllOff = new("AllOff", false);
    public static readonly PdfName AllOn = new("AllOn", false);
    public static readonly PdfName AllPages = new("AllPages", false);
    public static readonly PdfName Alpha = new("Alpha", false);
    public static readonly PdfName ALT = new("ALT", false);
    public static readonly PdfName Alternative = new("Alternative", false);
    public static readonly PdfName And = new("And", false);
    public static readonly PdfName ANF = new("ANF", false);
    public static readonly PdfName Annot = new("Annot", false);
    public static readonly PdfName Annots = new("Annots", false);
    public static readonly PdfName AnnotStates = new("AnnotStates", false);
    public static readonly PdfName AnyOff = new("AnyOff", false);
    public static readonly PdfName AnyOn = new("AnyOn", false);
    public static readonly PdfName AppDefault = new("AppDefault", false);
    public static readonly PdfName ArtBox = new("ArtBox", false);
    public static readonly PdfName Artifact = new("Artifact", false);
    public static readonly PdfName Artwork = new("Artwork", false);
    public static readonly PdfName ASCII85Decode = new("ASCII85Decode", false);
    public static readonly PdfName ASCIIHexDecode = new("ASCIIHexDecode", false);
    public static readonly PdfName AttachedToSig = new("AttachedToSig", false);
    public static readonly PdfName Attachment = new("Attachment", false);
    public static readonly PdfName AttachmentEditing = new("AttachmentEditing", false);
    public static readonly PdfName auto = new("auto", false);
    public static readonly PdfName Auto = new("Auto", false);
    public static readonly PdfName B = new("B", false);
    public static readonly PdfName B5pcH = new("B5pc-H", false);
    public static readonly PdfName B5pcV = new("B5pc-V", false);
    public static readonly PdfName Background = new("Background", false);
    public static readonly PdfName BarcodePlaintext = new("BarcodePlaintext", false);
    public static readonly PdfName Bates = new("Bates", false);
    public static readonly PdfName BatesN = new("BatesN", false);
    public static readonly PdfName Bead = new("Bead", false);
    public static readonly PdfName Before = new("Before", false);
    public static readonly PdfName BG = new("BG", false);
    public static readonly PdfName BleedBox = new("BleedBox", false);
    public static readonly PdfName Blinds = new("Blinds", false);
    public static readonly PdfName Block = new("Block", false);
    public static readonly PdfName Blue = new("Blue", false);
    public static readonly PdfName Border = new("Border", false);
    public static readonly PdfName Both = new("Both", false);
    public static readonly PdfName BoundingBox = new("BoundingBox", false);
    public static readonly PdfName Box = new("Box", false);
    public static readonly PdfName BtLr = new("BtLr", false);
    public static readonly PdfName Btn = new("Btn", false);
    public static readonly PdfName BtRl = new("BtRl", false);
    public static readonly PdfName Butt = new("Butt", false);
    public static readonly PdfName C = new("C", false);
    public static readonly PdfName CAD = new("CAD", false);
    public static readonly PdfName CalGray = new("CalGray", false);
    public static readonly PdfName CalRGB = new("CalRGB", false);
    public static readonly PdfName Caret = new("Caret", false);
    public static readonly PdfName Catalog = new("Catalog", false);
    public static readonly PdfName cb = new("cb", false);
    public static readonly PdfName CCITTFaxDecode = new("CCITTFaxDecode", false);
    public static readonly PdfName Center = new("Center", false);
    public static readonly PdfName Ch = new("Ch", false);
    public static readonly PdfName CICISignIt = new("CICI.SignIt", false);
    public static readonly PdfName CIDFontType0 = new("CIDFontType0", false);
    public static readonly PdfName CIDFontType0C = new("CIDFontType0C", false);
    public static readonly PdfName CIDFontType2 = new("CIDFontType2", false);
    public static readonly PdfName Circle = new("Circle", false);
    public static readonly PdfName ClosedArrow = new("ClosedArrow", false);
    public static readonly PdfName Cloud = new("Cloud", false);
    public static readonly PdfName CMap = new("CMap", false);
    public static readonly PdfName CNSEUCH = new("CNS-EUC-H", false);
    public static readonly PdfName CNSEUCV = new("CNS-EUC-V", false);
    public static readonly PdfName Collection = new("Collection", false);
    public static readonly PdfName CollectionColors = new("CollectionColors", false);
    public static readonly PdfName CollectionEditing = new("CollectionEditing", false);
    public static readonly PdfName CollectionField = new("CollectionField", false);
    public static readonly PdfName CollectionItem = new("CollectionItem", false);
    public static readonly PdfName CollectionSchema = new("CollectionSchema", false);
    public static readonly PdfName CollectionSort = new("CollectionSort", false);
    public static readonly PdfName CollectionSplit = new("CollectionSplit", false);
    public static readonly PdfName CollectionSubitem = new("CollectionSubitem", false);
    public static readonly PdfName Color = new("Color", false);
    public static readonly PdfName ColorBurn = new("ColorBurn", false);
    public static readonly PdfName ColorDodge = new("ColorDodge", false);
    public static readonly PdfName ColorSpace = new("ColorSpace", false);
    public static readonly PdfName Column = new("Column", false);
    public static readonly PdfName Comment = new("Comment", false);
    public static readonly PdfName Compatible = new("Compatible", false);
    public static readonly PdfName CompressedSize = new("CompressedSize", false);
    public static readonly PdfName Condensed = new("Condensed", false);
    public static readonly PdfName Copy = new("Copy", false);
    public static readonly PdfName CosineDot = new("CosineDot", false);
    public static readonly PdfName Cover = new("Cover", false);
    public static readonly PdfName Create = new("Create", false);
    public static readonly PdfName CreationDate = new("CreationDate", false);
    public static readonly PdfName CropBox = new("CropBox", false);
    public static readonly PdfName CropRect = new("CropRect", false);
    public static readonly PdfName Cross = new("Cross", false);
    public static readonly PdfName Crypt = new("Crypt", false);
    public static readonly PdfName CryptFilter = new("CryptFilter", false);
    public static readonly PdfName CryptFilterDecodeParms = new("CryptFilterDecodeParms", false);
    public static readonly PdfName Cube = new("Cube", false);
    public static readonly PdfName CuePoint = new("CuePoint", false);
    public static readonly PdfName D = new("D", false);
    public static readonly PdfName Darken = new("Darken", false);
    public static readonly PdfName Dashed = new("Dashed", false);
    public static readonly PdfName Data = new("Data", false);
    public static readonly PdfName DataMatrix = new("DataMatrix", false);
    public static readonly PdfName Day = new("Day", false);
    public static readonly PdfName DCTDecode = new("DCTDecode", false);
    public static readonly PdfName Decimal = new("Decimal", false);
    public static readonly PdfName Default = new("Default", false);
    public static readonly PdfName DefaultCryptFilter = new("DefaultCryptFilter", false);
    public static readonly PdfName DefEmbeddedFile = new("DefEmbeddedFile", false);
    public static readonly PdfName DEG = new("DEG", false);
    public static readonly PdfName Delete = new("Delete", false);
    public static readonly PdfName Desc = new("Desc", false);
    public static readonly PdfName Description = new("Description", false);
    public static readonly PdfName Design = new("Design", false);
    public static readonly PdfName Dest = new("Dest", false);
    public static readonly PdfName DeveloperExtensions = new("DeveloperExtensions", false);
    public static readonly PdfName DeviceCMY = new("DeviceCMY", false);
    public static readonly PdfName DeviceCMYK = new("DeviceCMYK", false);
    public static readonly PdfName DeviceGray = new("DeviceGray", false);
    public static readonly PdfName DeviceN = new("DeviceN", false);
    public static readonly PdfName DeviceRGB = new("DeviceRGB", false);
    public static readonly PdfName DeviceRGBK = new("DeviceRGBK", false);
    public static readonly PdfName Diamond = new("Diamond", false);
    public static readonly PdfName Difference = new("Difference", false);
    public static readonly PdfName DigSig = new("DigSig", false);
    public static readonly PdfName DigSigMDP = new("DigSigMDP", false);
    public static readonly PdfName DigSigValidation = new("DigSigValidation", false);
    public static readonly PdfName Disc = new("Disc", false);
    public static readonly PdfName Dissolve = new("Dissolve", false);
    public static readonly PdfName Distribute = new("Distribute", false);
    public static readonly PdfName DocMDP = new("DocMDP", false);
    public static readonly PdfName DocOpen = new("DocOpen", false);
    public static readonly PdfName DocTimeStamp = new("DocTimeStamp", false);
    public static readonly PdfName DOS = new("DOS", false);
    public static readonly PdfName Dotted = new("Dotted", false);
    public static readonly PdfName Double = new("Double", false);
    public static readonly PdfName DoubleDot = new("DoubleDot", false);
    public static readonly PdfName DPart = new("DPart", false);
    public static readonly PdfName DPartInteract = new("DPartInteract", false);
    public static readonly PdfName DPartRoot = new("DPartRoot", false);
    public static readonly PdfName DSm = new("DSm", false);
    public static readonly PdfName DSS = new("DSS", false);
    public static readonly PdfName DuplexFlipLongEdge = new("DuplexFlipLongEdge", false);
    public static readonly PdfName DuplexFlipShortEdge = new("DuplexFlipShortEdge", false);
    public static readonly PdfName EF = new("EF", false);
    public static readonly PdfName EFOpen = new("EFOpen", false);
    public static readonly PdfName Ellipse = new("Ellipse", false);
    public static readonly PdfName EllipseA = new("EllipseA", false);
    public static readonly PdfName EllipseB = new("EllipseB", false);
    public static readonly PdfName EllipseC = new("EllipseC", false);
    public static readonly PdfName Embedded = new("Embedded", false);
    public static readonly PdfName EmbeddedFile = new("EmbeddedFile", false);
    public static readonly PdfName EnableJavaScripts = new("EnableJavaScripts", false);
    public static readonly PdfName Encoding = new("Encoding", false);
    public static readonly PdfName Encrypt = new("Encrypt", false);
    public static readonly PdfName EncryptedPayload = new("EncryptedPayload", false);
    public static readonly PdfName Encryption = new("Encryption", false);
    public static readonly PdfName End = new("End", false);
    public static readonly PdfName EntrustPPKEF = new("Entrust.PPKEF", false);
    public static readonly PdfName EP = new("EP", false);
    public static readonly PdfName EPSG = new("EPSG", false);
    public static readonly PdfName ETenB5H = new("ETen-B5-H", false);
    public static readonly PdfName ETenB5V = new("ETen-B5-V", false);
    public static readonly PdfName ETenmsB5H = new("ETenms-B5-H", false);
    public static readonly PdfName ETenmsB5V = new("ETenms-B5-V", false);
    public static readonly PdfName ETSICAdESdetached = new("ETSI.CAdES.detached", false);
    public static readonly PdfName ETSIRFC3161 = new("ETSI.RFC3161", false);
    public static readonly PdfName EUCH = new("EUC-H", false);
    public static readonly PdfName EUCV = new("EUC-V", false);
    public static readonly PdfName Event = new("Event", false);
    public static readonly PdfName Exclude = new("Exclude", false);
    public static readonly PdfName Exclusion = new("Exclusion", false);
    public static readonly PdfName ExData = new("ExData", false);
    public static readonly PdfName Expanded = new("Expanded", false);
    public static readonly PdfName Export = new("Export", false);
    public static readonly PdfName Extensions = new("Extensions", false);
    public static readonly PdfName ExtGState = new("ExtGState", false);
    public static readonly PdfName ExtraCondensed = new("ExtraCondensed", false);
    public static readonly PdfName ExtraExpanded = new("ExtraExpanded", false);
    public static readonly PdfName ExtRKSJH = new("Ext-RKSJ-H", false);
    public static readonly PdfName ExtRKSJV = new("Ext-RKSJ-V", false);
    public static readonly PdfName F = new("F", false);
    public static readonly PdfName Fade = new("Fade", false);
    public static readonly PdfName False = new("False", false);
    public static readonly PdfName Far = new("Far", false);
    public static readonly PdfName FG = new("FG", false);
    public static readonly PdfName FieldMDP = new("FieldMDP", false);
    public static readonly PdfName File = new("File", false);
    public static readonly PdfName FileAttachment = new("FileAttachment", false);
    public static readonly PdfName Filespec = new("Filespec", false);
    public static readonly PdfName FillIn = new("FillIn", false);
    public static readonly PdfName FilmStrip = new("FilmStrip", false);
    public static readonly PdfName Fit = new("Fit", false);
    public static readonly PdfName FitB = new("FitB", false);
    public static readonly PdfName FitBH = new("FitBH", false);
    public static readonly PdfName FitBV = new("FitBV", false);
    public static readonly PdfName FitH = new("FitH", false);
    public static readonly PdfName FitR = new("FitR", false);
    public static readonly PdfName FitV = new("FitV", false);
    public static readonly PdfName FixedPrint = new("FixedPrint", false);
    public static readonly PdfName Flash = new("Flash", false);
    public static readonly PdfName FlateDecode = new("FlateDecode", false);
    public static readonly PdfName Fly = new("Fly", false);
    public static readonly PdfName Folder = new("Folder", false);
    public static readonly PdfName Font = new("Font", false);
    public static readonly PdfName FontDescriptor = new("FontDescriptor", false);
    public static readonly PdfName FontFile = new("FontFile", false);
    public static readonly PdfName FontFile2 = new("FontFile2", false);
    public static readonly PdfName FontFile3 = new("FontFile3", false);
    public static readonly PdfName Footer = new("Footer", false);
    public static readonly PdfName Foreground = new("Foreground", false);
    public static readonly PdfName Form = new("Form", false);
    public static readonly PdfName FormData = new("FormData", false);
    public static readonly PdfName FreeForm = new("FreeForm", false);
    public static readonly PdfName FreeText = new("FreeText", false);
    public static readonly PdfName FreeTextCallout = new("FreeTextCallout", false);
    public static readonly PdfName FreeTextTypeWriter = new("FreeTextTypeWriter", false);
    public static readonly PdfName FT = new("FT", false);
    public static readonly PdfName full_color = new("full_color", false);
    public static readonly PdfName FullSave = new("FullSave", false);
    public static readonly PdfName FullScreen = new("FullScreen", false);
    public static readonly PdfName FWParams = new("FWParams", false);
    public static readonly PdfName GBEUCH = new("GB-EUC-H", false);
    public static readonly PdfName GBEUCV = new("GB-EUC-V", false);
    public static readonly PdfName GBK2KH = new("GBK2K-H", false);
    public static readonly PdfName GBK2KV = new("GBK2K-V", false);
    public static readonly PdfName GBKEUCH = new("GBK-EUC-H", false);
    public static readonly PdfName GBKEUCV = new("GBK-EUC-V", false);
    public static readonly PdfName GBKpEUCH = new("GBKp-EUC-H", false);
    public static readonly PdfName GBKpEUCV = new("GBKp-EUC-V", false);
    public static readonly PdfName GBpcEUCH = new("GBpc-EUC-H", false);
    public static readonly PdfName GBpcEUCV = new("GBpc-EUC-V", false);
    public static readonly PdfName GEO = new("GEO", false);
    public static readonly PdfName GEOGCS = new("GEOGCS", false);
    public static readonly PdfName Geospatial2D = new("Geospatial2D", false);
    public static readonly PdfName Geospatial3D = new("Geospatial3D", false);
    public static readonly PdfName Glitter = new("Glitter", false);
    public static readonly PdfName Global = new("Global", false);
    public static readonly PdfName glTF = new("glTF", false);
    public static readonly PdfName GoTo = new("GoTo", false);
    public static readonly PdfName GoTo3DView = new("GoTo3DView", false);
    public static readonly PdfName GoToDp = new("GoToDp", false);
    public static readonly PdfName GoToE = new("GoToE", false);
    public static readonly PdfName GoToR = new("GoToR", false);
    public static readonly PdfName GRD = new("GRD", false);
    public static readonly PdfName Groove = new("Groove", false);
    public static readonly PdfName Group = new("Group", false);
    public static readonly PdfName GTS_PDFA1 = new("GTS_PDFA1", false);
    public static readonly PdfName GTS_PDFX = new("GTS_PDFX", false);
    public static readonly PdfName H = new("H", false);
    public static readonly PdfName HA = new("HA", false);
    public static readonly PdfName Halftone = new("Halftone", false);
    public static readonly PdfName Hard = new("Hard", false);
    public static readonly PdfName HardLight = new("HardLight", false);
    public static readonly PdfName Header = new("Header", false);
    public static readonly PdfName Headlamp = new("Headlamp", false);
    public static readonly PdfName Help = new("Help", false);
    public static readonly PdfName HF = new("HF", false);
    public static readonly PdfName Hidden = new("Hidden", false);
    public static readonly PdfName HiddenWireframe = new("HiddenWireframe", false);
    public static readonly PdfName Hide = new("Hide", false);
    public static readonly PdfName Highlight = new("Highlight", false);
    public static readonly PdfName HKscsB5H = new("HKscs-B5-H", false);
    public static readonly PdfName HKscsB5V = new("HKscs-B5-V", false);
    public static readonly PdfName Hue = new("Hue", false);
    public static readonly PdfName I = new("I", false);
    public static readonly PdfName ICCBased = new("ICCBased", false);
    public static readonly PdfName Identity = new("Identity", false);
    public static readonly PdfName IdentityH = new("Identity-H", false);
    public static readonly PdfName IdentityV = new("Identity-V", false);
    public static readonly PdfName Illustration = new("Illustration", false);
    public static readonly PdfName Image = new("Image", false);
    public static readonly PdfName ImageB = new("ImageB", false);
    public static readonly PdfName ImageC = new("ImageC", false);
    public static readonly PdfName ImageI = new("ImageI", false);
    public static readonly PdfName Import = new("Import", false);
    public static readonly PdfName ImportData = new("ImportData", false);
    public static readonly PdfName Include = new("Include", false);
    public static readonly PdfName Ind = new("Ind", false);
    public static readonly PdfName Indexed = new("Indexed", false);
    public static readonly PdfName Ink = new("Ink", false);
    public static readonly PdfName Inline = new("Inline", false);
    public static readonly PdfName Insert = new("Insert", false);
    public static readonly PdfName Inset = new("Inset", false);
    public static readonly PdfName InvertedDouble = new("InvertedDouble", false);
    public static readonly PdfName InvertedDoubleDot = new("InvertedDoubleDot", false);
    public static readonly PdfName InvertedEllipseA = new("InvertedEllipseA", false);
    public static readonly PdfName InvertedEllipseC = new("InvertedEllipseC", false);
    public static readonly PdfName InvertedSimpleDot = new("InvertedSimpleDot", false);
    public static readonly PdfName ISO_PDFE1 = new("ISO_PDFE1", false);
    public static readonly PdfName ISO_TS_24064 = new("ISO_TS_24064", false);
    public static readonly PdfName ISO_TS_32001 = new("ISO_TS_32001", false);
    public static readonly PdfName ISO_TS_32003 = new("ISO_TS_32003", false);
    public static readonly PdfName ISO_TS_32004 = new("ISO_TS_32004", false);
    public static readonly PdfName IT = new("IT", false);
    public static readonly PdfName JavaScript = new("JavaScript", false);
    public static readonly PdfName JBIG2Decode = new("JBIG2Decode", false);
    public static readonly PdfName JPXDecode = new("JPXDecode", false);
    public static readonly PdfName JS = new("JS", false);
    public static readonly PdfName Justify = new("Justify", false);
    public static readonly PdfName Key = new("Key", false);
    public static readonly PdfName KM = new("KM", false);
    public static readonly PdfName KSCEUCH = new("KSC-EUC-H", false);
    public static readonly PdfName KSCEUCV = new("KSC-EUC-V", false);
    public static readonly PdfName KSCmsUHCH = new("KSCms-UHC-H", false);
    public static readonly PdfName KSCmsUHCHWH = new("KSCms-UHC-HW-H", false);
    public static readonly PdfName KSCmsUHCHWV = new("KSCms-UHC-HW-V", false);
    public static readonly PdfName KSCmsUHCV = new("KSCms-UHC-V", false);
    public static readonly PdfName KSCpcEUCH = new("KSCpc-EUC-H", false);
    public static readonly PdfName L = new("L", false);
    public static readonly PdfName L2R = new("L2R", false);
    public static readonly PdfName Lab = new("Lab", false);
    public static readonly PdfName LastModified = new("LastModified", false);
    public static readonly PdfName LAT = new("LAT", false);
    public static readonly PdfName Launch = new("Launch", false);
    public static readonly PdfName Layout = new("Layout", false);
    public static readonly PdfName lb = new("lb", false);
    public static readonly PdfName LD3 = new("LD3", false);
    public static readonly PdfName Lighten = new("Lighten", false);
    public static readonly PdfName Line = new("Line", false);
    public static readonly PdfName Linear = new("Linear", false);
    public static readonly PdfName LineArrow = new("LineArrow", false);
    public static readonly PdfName LineDimension = new("LineDimension", false);
    public static readonly PdfName LineNum = new("LineNum", false);
    public static readonly PdfName LineThrough = new("LineThrough", false);
    public static readonly PdfName LineX = new("LineX", false);
    public static readonly PdfName LineY = new("LineY", false);
    public static readonly PdfName Link = new("Link", false);
    public static readonly PdfName List = new("List", false);
    public static readonly PdfName LLE = new("LLE", false);
    public static readonly PdfName LON = new("LON", false);
    public static readonly PdfName LowerAlpha = new("LowerAlpha", false);
    public static readonly PdfName LowerRoman = new("LowerRoman", false);
    public static readonly PdfName LrBt = new("LrBt", false);
    public static readonly PdfName LrTb = new("LrTb", false);
    public static readonly PdfName Luminosity = new("Luminosity", false);
    public static readonly PdfName LZWDecode = new("LZWDecode", false);
    public static readonly PdfName M = new("M", false);
    public static readonly PdfName Mac = new("Mac", false);
    public static readonly PdfName MacExpertEncoding = new("MacExpertEncoding", false);
    public static readonly PdfName MacRomanEncoding = new("MacRomanEncoding", false);
    public static readonly PdfName Markup = new("Markup", false);
    public static readonly PdfName Markup3D = new("Markup3D", false);
    public static readonly PdfName MarkupGeo = new("MarkupGeo", false);
    public static readonly PdfName Mask = new("Mask", false);
    public static readonly PdfName Material = new("Material", false);
    public static readonly PdfName Matte = new("Matte", false);
    public static readonly PdfName max = new("max", false);
    public static readonly PdfName Max = new("Max", false);
    public static readonly PdfName MCD = new("MCD", false);
    public static readonly PdfName MCR = new("MCR", false);
    public static readonly PdfName MCS = new("MCS", false);
    public static readonly PdfName MD5 = new("MD5", false);
    public static readonly PdfName Measure = new("Measure", false);
    public static readonly PdfName MediaBox = new("MediaBox", false);
    public static readonly PdfName MediaClip = new("MediaClip", false);
    public static readonly PdfName MediaCriteria = new("MediaCriteria", false);
    public static readonly PdfName MediaDuration = new("MediaDuration", false);
    public static readonly PdfName MediaOffset = new("MediaOffset", false);
    public static readonly PdfName MediaPermissions = new("MediaPermissions", false);
    public static readonly PdfName MediaPlayerInfo = new("MediaPlayerInfo", false);
    public static readonly PdfName MediaPlayers = new("MediaPlayers", false);
    public static readonly PdfName MediaPlayParams = new("MediaPlayParams", false);
    public static readonly PdfName MediaScreenParams = new("MediaScreenParams", false);
    public static readonly PdfName Metadata = new("Metadata", false);
    public static readonly PdfName MI = new("MI", false);
    public static readonly PdfName Middle = new("Middle", false);
    public static readonly PdfName Min = new("Min", false);
    public static readonly PdfName MinBitDepth = new("MinBitDepth", false);
    public static readonly PdfName MinScreenSize = new("MinScreenSize", false);
    public static readonly PdfName MMType1 = new("MMType1", false);
    public static readonly PdfName ModDate = new("ModDate", false);
    public static readonly PdfName Modify = new("Modify", false);
    public static readonly PdfName Module = new("Module", false);
    public static readonly PdfName monochrome = new("monochrome", false);
    public static readonly PdfName Movie = new("Movie", false);
    public static readonly PdfName MR = new("MR", false);
    public static readonly PdfName muLaw = new("muLaw", false);
    public static readonly PdfName Multimedia = new("Multimedia", false);
    public static readonly PdfName Multiply = new("Multiply", false);
    public static readonly PdfName N = new("N", false);

    public static readonly PdfName N10 = new("1.0", false);
    public static readonly PdfName N11 = new("1.1", false);
    public static readonly PdfName N12 = new("1.2", false);
    public static readonly PdfName N13 = new("1.3", false);
    public static readonly PdfName N14 = new("1.4", false);
    public static readonly PdfName N15 = new("1.5", false);
    public static readonly PdfName N16 = new("1.6", false);
    public static readonly PdfName N17 = new("1.7", false);
    public static readonly PdfName N20 = new("2.0", false);
    public static readonly PdfName N3D = new("3D", false);
    public static readonly PdfName N3DAnimationStyle = new("3DAnimationStyle", false);
    public static readonly PdfName N3DBG = new("3DBG", false);
    public static readonly PdfName N3DC = new("3DC", false);
    public static readonly PdfName N3DCrossSection = new("3DCrossSection", false);
    public static readonly PdfName N3DLightingScheme = new("3DLightingScheme", false);
    public static readonly PdfName N3DM = new("3DM", false);
    public static readonly PdfName N3DMarkup = new("3DMarkup", false);
    public static readonly PdfName N3DMeasure = new("3DMeasure", false);
    public static readonly PdfName N3DNode = new("3DNode", false);
    public static readonly PdfName N3DRef = new("3DRef", false);
    public static readonly PdfName N3DRenderMode = new("3DRenderMode", false);
    public static readonly PdfName N3DView = new("3DView", false);
    public static readonly PdfName N83pvRKSJH = new("83pv-RKSJ-H", false);
    public static readonly PdfName N90mspRKSJH = new("90msp-RKSJ-H", false);
    public static readonly PdfName N90mspRKSJV = new("90msp-RKSJ-V", false);
    public static readonly PdfName N90msRKSJH = new("90ms-RKSJ-H", false);
    public static readonly PdfName N90msRKSJV = new("90ms-RKSJ-V", false);
    public static readonly PdfName N90pvRKSJH = new("90pv-RKSJ-H", false);
    public static readonly PdfName Named = new("Named", false);
    public static readonly PdfName Namespace = new("Namespace", false);
    public static readonly PdfName Navigation = new("Navigation", false);
    public static readonly PdfName Navigator = new("Navigator", false);
    public static readonly PdfName NavNode = new("NavNode", false);
    public static readonly PdfName NChannel = new("NChannel", false);
    public static readonly PdfName Near = new("Near", false);
    public static readonly PdfName neutral = new("neutral", false);
    public static readonly PdfName NewParagraph = new("NewParagraph", false);
    public static readonly PdfName Night = new("Night", false);
    public static readonly PdfName NM = new("NM", false);
    public static readonly PdfName None = new("None", false);
    public static readonly PdfName NoOp = new("NoOp", false);
    public static readonly PdfName NOP = new("NOP", false);
    public static readonly PdfName Normal = new("Normal", false);
    public static readonly PdfName Not = new("Not", false);
    public static readonly PdfName Note = new("Note", false);
    public static readonly PdfName NSO = new("NSO", false);
    public static readonly PdfName NumberFormat = new("NumberFormat", false);
    public static readonly PdfName O = new("O", false);
    public static readonly PdfName OBJR = new("OBJR", false);
    public static readonly PdfName ObjStm = new("ObjStm", false);
    public static readonly PdfName OCAutoStates = new("OCAutoStates", false);
    public static readonly PdfName OCG = new("OCG", false);
    public static readonly PdfName OCInteract = new("OCInteract", false);
    public static readonly PdfName OCMD = new("OCMD", false);
    public static readonly PdfName off = new("off", false);
    public static readonly PdfName OFF = new("OFF", false);
    public static readonly PdfName on = new("on", false);
    public static readonly PdfName ON = new("ON", false);
    public static readonly PdfName Once = new("Once", false);
    public static readonly PdfName OneColumn = new("OneColumn", false);
    public static readonly PdfName Online = new("Online", false);
    public static readonly PdfName Open = new("Open", false);
    public static readonly PdfName OpenArrow = new("OpenArrow", false);
    public static readonly PdfName OpenType = new("OpenType", false);
    public static readonly PdfName OPI = new("OPI", false);
    public static readonly PdfName Or = new("Or", false);
    public static readonly PdfName Ordered = new("Ordered", false);
    public static readonly PdfName Org = new("Org", false);
    public static readonly PdfName Oscillating = new("Oscillating", false);
    public static readonly PdfName Outlines = new("Outlines", false);
    public static readonly PdfName OutputIntent = new("OutputIntent", false);
    public static readonly PdfName Outset = new("Outset", false);
    public static readonly PdfName Overlay = new("Overlay", false);
    public static readonly PdfName Overline = new("Overline", false);
    public static readonly PdfName P = new("P", false);
    public static readonly PdfName Page = new("Page", false);
    public static readonly PdfName PageLabel = new("PageLabel", false);
    public static readonly PdfName PageNum = new("PageNum", false);
    public static readonly PdfName Pages = new("Pages", false);
    public static readonly PdfName Pagination = new("Pagination", false);
    public static readonly PdfName Palindrome = new("Palindrome", false);
    public static readonly PdfName PaperMetaData = new("PaperMetaData", false);
    public static readonly PdfName Paragraph = new("Paragraph", false);
    public static readonly PdfName Path = new("Path", false);
    public static readonly PdfName Pattern = new("Pattern", false);
    public static readonly PdfName Pause = new("Pause", false);
    public static readonly PdfName pb = new("pb", false);
    public static readonly PdfName PC = new("PC", false);
    public static readonly PdfName PD3 = new("PD3", false);
    public static readonly PdfName PDF = new("PDF", false);
    public static readonly PdfName PDF_VT2 = new("PDF_VT2", false);
    public static readonly PdfName PDF417 = new("PDF417", false);
    public static readonly PdfName Perceptual = new("Perceptual", false);
    public static readonly PdfName PI = new("PI", false);
    public static readonly PdfName PieceInfo = new("PieceInfo", false);
    public static readonly PdfName Play = new("Play", false);
    public static readonly PdfName PO = new("PO", false);
    public static readonly PdfName Polygon = new("Polygon", false);
    public static readonly PdfName PolygonCloud = new("PolygonCloud", false);
    public static readonly PdfName PolygonDimension = new("PolygonDimension", false);
    public static readonly PdfName PolyLine = new("PolyLine", false);
    public static readonly PdfName PolyLineDimension = new("PolyLineDimension", false);
    public static readonly PdfName Popup = new("Popup", false);
    public static readonly PdfName PRC = new("PRC", false);
    public static readonly PdfName Primary = new("Primary", false);
    public static readonly PdfName Print = new("Print", false);
    public static readonly PdfName PrinterMark = new("PrinterMark", false);
    public static readonly PdfName PrintField = new("PrintField", false);
    public static readonly PdfName PrintScaling = new("PrintScaling", false);
    public static readonly PdfName PROJCS = new("PROJCS", false);
    public static readonly PdfName Projection = new("Projection", false);
    public static readonly PdfName PS = new("PS", false);
    public static readonly PdfName PtData = new("PtData", false);
    public static readonly PdfName Push = new("Push", false);
    public static readonly PdfName PV = new("PV", false);
    public static readonly PdfName QRCode = new("QRCode", false);
    public static readonly PdfName r = new("r", false);
    public static readonly PdfName R = new("R", false);
    public static readonly PdfName R2L = new("R2L", false);
    public static readonly PdfName Raw = new("Raw", false);
    public static readonly PdfName rb = new("rb", false);
    public static readonly PdfName RClosedArrow = new("RClosedArrow", false);
    public static readonly PdfName RD3 = new("RD3", false);
    public static readonly PdfName Record = new("Record", false);
    public static readonly PdfName Red = new("Red", false);
    public static readonly PdfName Redact = new("Redact", false);
    public static readonly PdfName Redaction = new("Redaction", false);
    public static readonly PdfName registration = new("registration", false);
    public static readonly PdfName RelativeColorimetric = new("RelativeColorimetric", false);
    public static readonly PdfName Rendition = new("Rendition", false);
    public static readonly PdfName Repeat = new("Repeat", false);
    public static readonly PdfName ReqHandler = new("ReqHandler", false);
    public static readonly PdfName Requirement = new("Requirement", false);
    public static readonly PdfName ResetForm = new("ResetForm", false);
    public static readonly PdfName Resume = new("Resume", false);
    public static readonly PdfName RF = new("RF", false);
    public static readonly PdfName Rhomboid = new("Rhomboid", false);
    public static readonly PdfName RichMedia = new("RichMedia", false);
    public static readonly PdfName RichMediaActivation = new("RichMediaActivation", false);
    public static readonly PdfName RichMediaAnimation = new("RichMediaAnimation", false);
    public static readonly PdfName RichMediaCommand = new("RichMediaCommand", false);
    public static readonly PdfName RichMediaConfiguration = new("RichMediaConfiguration", false);
    public static readonly PdfName RichMediaContent = new("RichMediaContent", false);
    public static readonly PdfName RichMediaDeactivation = new("RichMediaDeactivation", false);
    public static readonly PdfName RichMediaExecute = new("RichMediaExecute", false);
    public static readonly PdfName RichMediaInstance = new("RichMediaInstance", false);
    public static readonly PdfName RichMediaParams = new("RichMediaParams", false);
    public static readonly PdfName RichMediaPosition = new("RichMediaPosition", false);
    public static readonly PdfName RichMediaPresentation = new("RichMediaPresentation", false);
    public static readonly PdfName RichMediaSettings = new("RichMediaSettings", false);
    public static readonly PdfName RichMediaWindow = new("RichMediaWindow", false);
    public static readonly PdfName Ridge = new("Ridge", false);
    public static readonly PdfName RIPEMD160 = new("RIPEMD160", false);
    public static readonly PdfName RL = new("RL", false);
    public static readonly PdfName RlBt = new("RlBt", false);
    public static readonly PdfName RlTb = new("RlTb", false);
    public static readonly PdfName ROpenArrow = new("ROpenArrow", false);
    public static readonly PdfName Round = new("Round", false);
    public static readonly PdfName Row = new("Row", false);
    public static readonly PdfName RT = new("RT", false);
    public static readonly PdfName RunLengthDecode = new("RunLengthDecode", false);
    public static readonly PdfName S = new("S", false);
    public static readonly PdfName Saturation = new("Saturation", false);
    public static readonly PdfName SC = new("SC", false);
    public static readonly PdfName Schema = new("Schema", false);
    public static readonly PdfName Screen = new("Screen", false);
    public static readonly PdfName SemiCondensed = new("SemiCondensed", false);
    public static readonly PdfName SemiExpanded = new("SemiExpanded", false);
    public static readonly PdfName Separation = new("Separation", false);
    public static readonly PdfName SeparationSimulation = new("SeparationSimulation", false);
    public static readonly PdfName SetOCGState = new("SetOCGState", false);
    public static readonly PdfName SetState = new("SetState", false);
    public static readonly PdfName SHA1 = new("SHA1", false);
    public static readonly PdfName SHA256 = new("SHA256", false);
    public static readonly PdfName SHA3256 = new("SHA3-256", false);
    public static readonly PdfName SHA3384 = new("SHA3-384", false);
    public static readonly PdfName SHA3512 = new("SHA3-512", false);
    public static readonly PdfName SHA384 = new("SHA384", false);
    public static readonly PdfName SHA512 = new("SHA512", false);
    public static readonly PdfName ShadedIllustration = new("ShadedIllustration", false);
    public static readonly PdfName ShadedVertices = new("ShadedVertices", false);
    public static readonly PdfName ShadedWireframe = new("ShadedWireframe", false);
    public static readonly PdfName Shadow = new("Shadow", false);
    public static readonly PdfName SHAKE256 = new("SHAKE256", false);
    public static readonly PdfName Sig = new("Sig", false);
    public static readonly PdfName SigFieldLock = new("SigFieldLock", false);
    public static readonly PdfName Signed = new("Signed", false);
    public static readonly PdfName SigRef = new("SigRef", false);
    public static readonly PdfName SimpleDot = new("SimpleDot", false);
    public static readonly PdfName Simplex = new("Simplex", false);
    public static readonly PdfName SinglePage = new("SinglePage", false);
    public static readonly PdfName SingleUse = new("SingleUse", false);
    public static readonly PdfName SIS = new("SIS", false);
    public static readonly PdfName Size = new("Size", false);
    public static readonly PdfName Slash = new("Slash", false);
    public static readonly PdfName SlideShow = new("SlideShow", false);
    public static readonly PdfName SoftLight = new("SoftLight", false);
    public static readonly PdfName SoftwareIdentifier = new("SoftwareIdentifier", false);
    public static readonly PdfName Solid = new("Solid", false);
    public static readonly PdfName SolidOutline = new("SolidOutline", false);
    public static readonly PdfName SolidWireframe = new("SolidWireframe", false);
    public static readonly PdfName Sound = new("Sound", false);
    public static readonly PdfName Source = new("Source", false);
    public static readonly PdfName SpawnTemplate = new("SpawnTemplate", false);
    public static readonly PdfName SpiderContentSet = new("SpiderContentSet", false);
    public static readonly PdfName Split = new("Split", false);
    public static readonly PdfName SPS = new("SPS", false);
    public static readonly PdfName SQFT = new("SQFT", false);
    public static readonly PdfName SQKM = new("SQKM", false);
    public static readonly PdfName SQM = new("SQM", false);
    public static readonly PdfName SQMI = new("SQMI", false);
    public static readonly PdfName Square = new("Square", false);
    public static readonly PdfName Squiggly = new("Squiggly", false);
    public static readonly PdfName SR = new("SR", false);
    public static readonly PdfName Stamp = new("Stamp", false);
    public static readonly PdfName StampImage = new("StampImage", false);
    public static readonly PdfName StampSnapshot = new("StampSnapshot", false);
    public static readonly PdfName Standalone = new("Standalone", false);
    public static readonly PdfName Standard = new("Standard", false);
    public static readonly PdfName Start = new("Start", false);
    public static readonly PdfName StdCF = new("StdCF", false);
    public static readonly PdfName STEP = new("STEP", false);
    public static readonly PdfName Stop = new("Stop", false);
    public static readonly PdfName Stream = new("Stream", false);
    public static readonly PdfName StrikeOut = new("StrikeOut", false);
    public static readonly PdfName StructElem = new("StructElem", false);
    public static readonly PdfName StructParent = new("StructParent", false);
    public static readonly PdfName StructParents = new("StructParents", false);
    public static readonly PdfName StructTreeRoot = new("StructTreeRoot", false);
    public static readonly PdfName Style = new("Style", false);
    public static readonly PdfName Sub = new("Sub", false);
    public static readonly PdfName SubmitForm = new("SubmitForm", false);
    public static readonly PdfName SubmitStandalone = new("SubmitStandalone", false);
    public static readonly PdfName SummaryView = new("SummaryView", false);
    public static readonly PdfName Sup = new("Sup", false);
    public static readonly PdfName Supplement = new("Supplement", false);
    public static readonly PdfName SV = new("SV", false);
    public static readonly PdfName SVCert = new("SVCert", false);
    public static readonly PdfName T = new("T", false);
    public static readonly PdfName Table = new("Table", false);
    public static readonly PdfName TbLr = new("TbLr", false);
    public static readonly PdfName TbRl = new("TbRl", false);
    public static readonly PdfName Template = new("Template", false);
    public static readonly PdfName Text = new("Text", false);
    public static readonly PdfName Thread = new("Thread", false);
    public static readonly PdfName Timespan = new("Timespan", false);
    public static readonly PdfName Toggle = new("Toggle", false);
    public static readonly PdfName Top = new("Top", false);
    public static readonly PdfName Trans = new("Trans", false);
    public static readonly PdfName TransformParams = new("TransformParams", false);
    public static readonly PdfName Transitions = new("Transitions", false);
    public static readonly PdfName Transparency = new("Transparency", false);
    public static readonly PdfName Transparent = new("Transparent", false);
    public static readonly PdfName TransparentBoundingBox = new("TransparentBoundingBox", false);
    public static readonly PdfName TransparentBoundingBoxOutline = new("TransparentBoundingBoxOutline", false);
    public static readonly PdfName TransparentWireframe = new("TransparentWireframe", false);
    public static readonly PdfName TrapNet = new("TrapNet", false);
    public static readonly PdfName Tree = new("Tree", false);
    public static readonly PdfName TrimBox = new("TrimBox", false);
    public static readonly PdfName True = new("True", false);
    public static readonly PdfName TrueType = new("TrueType", false);
    public static readonly PdfName TSm = new("TSm", false);
    public static readonly PdfName Ttl = new("Ttl", false);
    public static readonly PdfName tv = new("tv", false);
    public static readonly PdfName TwoColumnLeft = new("TwoColumnLeft", false);
    public static readonly PdfName TwoColumnRight = new("TwoColumnRight", false);
    public static readonly PdfName TwoPageLeft = new("TwoPageLeft", false);
    public static readonly PdfName TwoPageRight = new("TwoPageRight", false);
    public static readonly PdfName Tx = new("Tx", false);
    public static readonly PdfName Type0 = new("Type0", false);
    public static readonly PdfName Type1 = new("Type1", false);
    public static readonly PdfName Type1C = new("Type1C", false);
    public static readonly PdfName Type3 = new("Type3", false);
    public static readonly PdfName U = new("U", false);
    public static readonly PdfName U3D = new("U3D", false);
    public static readonly PdfName UltraCondensed = new("UltraCondensed", false);
    public static readonly PdfName UltraExpanded = new("UltraExpanded", false);
    public static readonly PdfName Unchanged = new("Unchanged", false);
    public static readonly PdfName Uncover = new("Uncover", false);
    public static readonly PdfName Underline = new("Underline", false);
    public static readonly PdfName UniCNSUCS2H = new("UniCNS-UCS2-H", false);
    public static readonly PdfName UniCNSUCS2V = new("UniCNS-UCS2-V", false);
    public static readonly PdfName UniCNSUTF16H = new("UniCNS-UTF16-H", false);
    public static readonly PdfName UniCNSUTF16V = new("UniCNS-UTF16-V", false);
    public static readonly PdfName UniGBUCS2H = new("UniGB-UCS2-H", false);
    public static readonly PdfName UniGBUCS2V = new("UniGB-UCS2-V", false);
    public static readonly PdfName UniGBUTF16H = new("UniGB-UTF16-H", false);
    public static readonly PdfName UniGBUTF16V = new("UniGB-UTF16-V", false);
    public static readonly PdfName UniJISUCS2H = new("UniJIS-UCS2-H", false);
    public static readonly PdfName UniJISUCS2HWH = new("UniJIS-UCS2-HW-H", false);
    public static readonly PdfName UniJISUCS2HWV = new("UniJIS-UCS2-HW-V", false);
    public static readonly PdfName UniJISUCS2V = new("UniJIS-UCS2-V", false);
    public static readonly PdfName UniJISUTF16H = new("UniJIS-UTF16-H", false);
    public static readonly PdfName UniJISUTF16V = new("UniJIS-UTF16-V", false);
    public static readonly PdfName UniKSUCS2H = new("UniKS-UCS2-H", false);
    public static readonly PdfName UniKSUCS2V = new("UniKS-UCS2-V", false);
    public static readonly PdfName UniKSUTF16H = new("UniKS-UTF16-H", false);
    public static readonly PdfName UniKSUTF16V = new("UniKS-UTF16-V", false);
    public static readonly PdfName Unix = new("Unix", false);
    public static readonly PdfName Unknown = new("Unknown", false);
    public static readonly PdfName Unordered = new("Unordered", false);
    public static readonly PdfName Unspecified = new("Unspecified", false);
    public static readonly PdfName UpperAlpha = new("UpperAlpha", false);
    public static readonly PdfName UpperRoman = new("UpperRoman", false);
    public static readonly PdfName UR = new("UR", false);
    public static readonly PdfName UR3 = new("UR3", false);
    public static readonly PdfName URI = new("URI", false);
    public static readonly PdfName UseAttachments = new("UseAttachments", false);
    public static readonly PdfName UseNone = new("UseNone", false);
    public static readonly PdfName UseOC = new("UseOC", false);
    public static readonly PdfName UseOutlines = new("UseOutlines", false);
    public static readonly PdfName UserProperties = new("UserProperties", false);
    public static readonly PdfName UseThumbs = new("UseThumbs", false);
    public static readonly PdfName USFT = new("USFT", false);
    public static readonly PdfName USm = new("USm", false);
    public static readonly PdfName V = new("V", false);
    public static readonly PdfName V2 = new("V2", false);
    public static readonly PdfName VeriSignPPKVS = new("VeriSign.PPKVS", false);
    public static readonly PdfName Version = new("Version", false);
    public static readonly PdfName Vertices = new("Vertices", false);
    public static readonly PdfName Video = new("Video", false);
    public static readonly PdfName View = new("View", false);
    public static readonly PdfName Viewport = new("Viewport", false);
    public static readonly PdfName VisiblePages = new("VisiblePages", false);
    public static readonly PdfName VRI = new("VRI", false);
    public static readonly PdfName W = new("W", false);
    public static readonly PdfName Warichu = new("Warichu", false);
    public static readonly PdfName Watermark = new("Watermark", false);
    public static readonly PdfName White = new("White", false);
    public static readonly PdfName Widget = new("Widget", false);
    public static readonly PdfName Win = new("Win", false);
    public static readonly PdfName WinAnsiEncoding = new("WinAnsiEncoding", false);
    public static readonly PdfName Windowed = new("Windowed", false);
    public static readonly PdfName Wipe = new("Wipe", false);
    public static readonly PdfName Wireframe = new("Wireframe", false);
    public static readonly PdfName WKT = new("WKT", false);
    public static readonly PdfName XA = new("XA", false);
    public static readonly PdfName XD = new("XD", false);
    public static readonly PdfName XML = new("XML", false);
    public static readonly PdfName XNF = new("XNF", false);
    public static readonly PdfName XObject = new("XObject", false);
    public static readonly PdfName XRef = new("XRef", false);
    public static readonly PdfName XYZ = new("XYZ", false);

    public static readonly PdfName Name = new("Name", false);
    public static readonly PdfName CharProcs = new("CharProcs", false);
    public static readonly PdfName Prev = new("Prev", false);
    public static readonly PdfName Length = new("Length", false);
    public static readonly PdfName Root = new("Root", false);
    public static readonly PdfName TYPE = new("Type", false);
    public static readonly PdfName TypeName = new("Type", false);
    public static readonly PdfName Count = new("Count", false);
    public static readonly PdfName Kids = new("Kids", false);
    public static readonly PdfName Contents = new("Contents", false);
    public static readonly PdfName Resources = new("Resources", false);
    public static readonly PdfName Rotate = new("Rotate", false);
    public static readonly PdfName Filter = new("Filter", false);
    public static readonly PdfName Predictor = new("Predictor", false);
    public static readonly PdfName Columns = new("Columns", false);
    public static readonly PdfName Colors = new("Colors", false);
    public static readonly PdfName BitsPerComponent = new("BitsPerComponent", false);
    public static readonly PdfName EarlyChange = new("EarlyChange", false);
    public static readonly PdfName DecodeParms = new("DecodeParms", false);
    public static readonly PdfName First = new("First", false);
    public static readonly PdfName Index = new("Index", false);
    public static readonly PdfName XRefStm = new("XRefStm", false);
    public static readonly PdfName Parent = new("Parent", false);
    public static readonly PdfName Subtype = new("Subtype", false);
    public static readonly PdfName ProcSet = new("ProcSet", false);
    public static readonly PdfName Shading = new("Shading", false);
    public static readonly PdfName BBox = new("BBox", false);

    public static readonly PdfName Widths = new("Widths", false);
    public static readonly PdfName FontName = new("FontName", false);
    public static readonly PdfName FontFamily = new("FontFamily", false);
    public static readonly PdfName FontWeight = new("FontWeight", false);
    public static readonly PdfName Leading = new("Leading", false);
    public static readonly PdfName StemV = new("StemV", false);

    public static readonly PdfName StemH = new("StemH", false);
    public static readonly PdfName MissingWidth = new("MissingWidth", false);
    public static readonly PdfName CharSet = new("CharSet", false);


    public static readonly PdfName FontBBox = new("FontBBox", false);

    public static readonly PdfName DW = new("DW", false);
    public static readonly PdfName DW2 = new("DW2", false);
    public static readonly PdfName W2 = new("W2", false);
    public static readonly PdfName CIDToGIDMap = new("CIDToGIDMap", false);
    public static readonly PdfName CIDSystemInfo = new("CIDSystemInfo", false);
    public static readonly PdfName Registry = new("Registry", false);
    public static readonly PdfName Ordering = new("Ordering", false);

    public static readonly PdfName Matrix = new("Matrix", false);
    public static readonly PdfName DescendantFonts = new("DescendantFonts", false);
    public static readonly PdfName BaseFont = new("BaseFont", false);
    public static readonly PdfName Length1 = new("Length1", false);
    public static readonly PdfName Length2 = new("Length2", false);
    public static readonly PdfName Length3 = new("Length3", false);
    public static readonly PdfName FontMatrix = new("FontMatrix", false);
    public static readonly PdfName Ascent = new("Ascent", false);
    public static readonly PdfName Descent = new("Descent", false);
    public static readonly PdfName XHeight = new("XHeight", false);
    public static readonly PdfName CapHeight = new("CapHeight", false);
    public static readonly PdfName Flags = new("Flags", false);
    public static readonly PdfName ItalicAngle = new("ItalicAngle", false);

    public static readonly PdfName FirstChar = new("FirstChar", false);
    public static readonly PdfName LastChar = new("LastChar", false);
    public static readonly PdfName BaseEncoding = new("BaseEncoding", false);
    public static readonly PdfName ToUnicode = new("ToUnicode", false);
    public static readonly PdfName Differences = new("Differences", false);

    public static readonly PdfName Decode = new("Decode", false);


    public static readonly PdfName Height = new("Height", false);
    public static readonly PdfName Width = new("Width", false);
    public static readonly PdfName Interpolate = new("Interpolate", false);
    public static readonly PdfName ImageMask = new("ImageMask", false);
    public static readonly PdfName SMask = new("SMask", false);

    public static readonly PdfName StandardEncoding = new("StandardEncoding", false);
    public static readonly PdfName SymbolSetEncoding = new("SymbolSetEncoding", false);
    public static readonly PdfName ZapfDingbatsEncoding = new("ZapfDingbatsEncoding", false);
    public static readonly PdfName ExpertEncoding = new("ExpertEncoding", false);

    public static readonly PdfName OE = new("OE", false);
    public static readonly PdfName UE = new("UE", false);
    public static readonly PdfName CF = new("CF", false);
    public static readonly PdfName CFM = new("CFM", false);
    public static readonly PdfName StmF = new("StmF", false);
    public static readonly PdfName StrF = new("StrF", false);
    public static readonly PdfName EFF = new("EFF", false);
    public static readonly PdfName SubFilter = new("SubFilter", false);
    public static readonly PdfName EncryptMetadata = new("EncryptMetadata", false);
    public static readonly PdfName ID = new("ID", false);




    [return: NotNullIfNotNull("name")]
    public static implicit operator PdfName?(string? name)
    {
        if (name == null) { return null; }
        // TODO common lookups for above?
        if (name[0] == '/')
        {
            return new(name.Substring(1));
        }
        return new(name);
    }

    [return: NotNullIfNotNull("name")]
    public static implicit operator string?(PdfName? name)
    {
        return name?.Value;
    }

    public override string ToString()
    {
        return Value;
    }


}
