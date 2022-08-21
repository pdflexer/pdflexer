using System.Diagnostics.CodeAnalysis;

namespace PdfLexer.DOM;

public class FontType0 : IPdfFont
{
    public PdfDictionary NativeObject { get; }

    public FontType0()
    {
        NativeObject = new PdfDictionary();
        NativeObject[PdfName.TypeName] = PdfName.Font;
        NativeObject[PdfName.Subtype] = PdfName.Type0;
    }

    public FontType0(PdfDictionary dict)
    {
        NativeObject = dict;
    }

    [return: NotNullIfNotNull("dict")]
    public static implicit operator FontType0?(PdfDictionary? dict) => dict == null ? null : new FontType0(dict);
    [return: NotNullIfNotNull("page")]
    public static implicit operator PdfDictionary?(FontType0? page) => page?.NativeObject;


    /// <summary>
    /// required
    /// If type 2 cidfont name should be CIDFonts BaseFont name
    /// If type 0 concat CIDFont BaseFont name "-" and the CMap name in encoding entry
    /// </summary>
    public PdfName? BaseFont
    {
        get => NativeObject.Get<PdfName>(PdfName.BaseFont);
        set => NativeObject.Set(PdfName.BaseFont, value);
    }


    /// <summary>
    /// required
    /// either name of predefined cmap or stream 
    /// of CMap. If truetype and not embedded, this must be a 
    /// predefined CMap
    /// </summary>
    public IPdfObject? Encoding
    {
        get => NativeObject.Get(PdfName.Encoding);
        set => NativeObject.Set(PdfName.Encoding, value);
    }

    /// <summary>
    /// required
    /// One element array containing CIDFont dictionary
    /// of descendant
    /// </summary>
    public PdfArray? DescendantFonts
    {
        get => NativeObject.Get<PdfArray>(PdfName.DescendantFonts);
        set => NativeObject.Set(PdfName.DescendantFonts, value);
    }

    /// <summary>
    /// Descendant CIDFont
    /// </summary>
    public FontCID? DescendantFont
    {
        get => NativeObject.Get<PdfArray>(PdfName.DescendantFonts)?.FirstOrDefault()?.GetValueOrNull<PdfDictionary>();
        set => NativeObject.Set(PdfName.DescendantFonts, value == null ? null : new PdfArray(new List<IPdfObject> { value.NativeObject.Indirect() }));
    }

    /// <summary>
    /// Optional
    /// </summary>
    public PdfStream? ToUnicode
    {
        get => NativeObject.Get<PdfStream>(PdfName.ToUnicode);
        set => NativeObject.Set(PdfName.ToUnicode, value.Indirect());
    }
}
