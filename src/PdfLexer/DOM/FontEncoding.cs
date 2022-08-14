namespace PdfLexer.DOM;

public  class FontEncoding
{
    public PdfDictionary NativeObject { get; }

    public FontEncoding()
    {
        NativeObject = new PdfDictionary();
        NativeObject[PdfName.TypeName] = PdfName.Encoding;
    }

    public FontEncoding(PdfDictionary dict)
    {
        NativeObject = dict;
    }

    [return: NotNullIfNotNull("dict")]
    public static implicit operator FontEncoding?(PdfDictionary? dict) => dict == null ? null : new (dict);
    [return: NotNullIfNotNull("page")]
    public static implicit operator PdfDictionary?(FontEncoding? page) => page?.NativeObject;

    // optional
    // implicit base encoding -> font built in encoding
    // StandardEncoding for non symbolic font
    public PdfName? BaseEncoding
    {
        get => NativeObject.Get<PdfName>(PdfName.BaseEncoding);
        set => NativeObject.Set(PdfName.BaseEncoding, value);
    }
    // optional
    // should not be used in truetype fonts
    public PdfArray? Differences
    {
        get => NativeObject.Get<PdfArray>(PdfName.Differences);
        set => NativeObject.Set(PdfName.Differences, value);
    }

    public IEnumerable<(int code, PdfName name)> ReadDifferences()
    {
        var arr = Differences;
        if (arr == null) { yield break; }
        int charCode = 0;
        foreach (var val in arr)
        {
            switch (val)
            {
                case PdfNumber cnt:
                    charCode = cnt;
                    continue;
                case PdfName nm:
                    yield return (charCode, nm);
                    charCode++;
                    continue;
            }
        }
    }
}
