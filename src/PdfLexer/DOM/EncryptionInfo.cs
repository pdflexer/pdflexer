namespace PdfLexer.DOM;

internal class EncryptionInfo
{
    public PdfDictionary NativeObject { get; }

    public EncryptionInfo(PdfDictionary info)
	{
        NativeObject = info;
	}


    [return: NotNullIfNotNull("dict")]
    public static implicit operator EncryptionInfo?(PdfDictionary? dict) => dict == null ? null : new EncryptionInfo(dict);
    [return: NotNullIfNotNull("info")]
    public static implicit operator PdfDictionary?(EncryptionInfo? info) => info?.NativeObject;


    public PdfName? Filter
    {
        get => NativeObject.Get<PdfName>(PdfName.Filter);
        set => NativeObject.Set(PdfName.Filter, value);
    }

    public PdfName? SubFilter
    {
        get => NativeObject.Get<PdfName>(PdfName.SubFilter);
        set => NativeObject.Set(PdfName.SubFilter, value);
    }

    public EncryptionAlgorithm V
    {
        get {
            var v = NativeObject.Get<PdfNumber>(PdfName.V);
            if (v == null) return EncryptionAlgorithm.Undocumented;
            return (EncryptionAlgorithm)(int)v;
        }
        set => NativeObject.Set(PdfName.V, new PdfIntNumber((int)value));
    }

    /// <summary>
    /// Length of encryption key, shall be multiple of 8.
    /// </summary>
    public int Length
    {
        get => NativeObject.Get<PdfNumber>(PdfName.Length) ?? 40;
        set => NativeObject.Set(PdfName.Length, new PdfIntNumber(value));
    }

    /// <summary>
    /// Crypt filter dictionaries
    /// Only meaningful with V == 4
    /// </summary>
    public PdfDictionary? CF
    {
        get => NativeObject.Get<PdfDictionary>(PdfName.CF);
        set => NativeObject.Set(PdfName.CF, value);
    }

    /// <summary>
    /// Name of crypt filter to use for decrypting streams in the document
    /// Only meaningful with V == 4
    /// Name shall be key in CF dict
    /// </summary>
    public PdfName? StmF
    {
        get => NativeObject.Get<PdfName>(PdfName.StmF);
        set => NativeObject.Set(PdfName.StmF, value);
    }

    /// <summary>
    /// Name of crypt filter to use for decrypting strings in the document
    /// Only meaningful with V == 4
    /// Name shall be key in CF dict
    /// </summary>
    public PdfName? StrF
    {
        get => NativeObject.Get<PdfName>(PdfName.StrF);
        set => NativeObject.Set(PdfName.StrF, value);
    }

    /// <summary>
    /// Name of crypt filter to use for embedded file streams that do not have their own crypt filter specified.
    /// Only meaningful with V == 4
    /// Name shall be key in CF dict
    /// </summary>
    public PdfName? EFF
    {
        get => NativeObject.Get<PdfName>(PdfName.EFF);
        set => NativeObject.Set(PdfName.EFF, value);
    }
}

public enum EncryptionAlgorithm
{
    Undocumented = 0,
    RC4OrAES40bit = 1,
    RC4OrAESLargeKey = 2,
    UpublishedLargeKey = 3,
    InDocument = 4,
}
