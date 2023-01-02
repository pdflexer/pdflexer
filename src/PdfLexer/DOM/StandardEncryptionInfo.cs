namespace PdfLexer.DOM;

internal class StandardEncryptionInfo  : EncryptionInfo
{
    public StandardEncryptionInfo(PdfDictionary info) : base(info)
	{
	}


    [return: NotNullIfNotNull("dict")]
    public static implicit operator StandardEncryptionInfo?(PdfDictionary? dict) => dict == null ? null : new StandardEncryptionInfo(dict);
    [return: NotNullIfNotNull("info")]
    public static implicit operator PdfDictionary?(StandardEncryptionInfo? info) => info?.NativeObject;


    /// <summary>
    /// Security handler version
    /// </summary>
    public int? R
    {
        get => NativeObject.Get<PdfNumber>(PdfName.R);
        set => NativeObject.Set(PdfName.R, (PdfNumber?)value);
    }

    /// <summary>
    /// 32 byte string based on owner and user passwords
    /// </summary>
    public PdfString? O
    {
        get => NativeObject.Get<PdfString>(PdfName.O);
        set => NativeObject.Set(PdfName.O, value);
    }

    /// <summary>
    /// 32 bytes based on the user password
    /// </summary>
    public byte[] O_Bytes
    {
        get
        {
            if (o_bytes != null) return o_bytes;
            var o = O;
            if (o == null) { return empty16; }
            o_bytes = o.GetRawBytes();
            return o_bytes;
        }
    }
    private byte[]? o_bytes;

    /// <summary>
    /// 32 byte string based on the user password
    /// </summary>
    public PdfString? U
    {
        get => NativeObject.Get<PdfString>(PdfName.U);
        set => NativeObject.Set(PdfName.U, value);
    }

    /// <summary>
    /// 32 bytes based on the user password
    /// </summary>
    public byte[] U_Bytes
    {
        get { 
            if (u_bytes != null) return u_bytes;
            var u = U;
            if (u == null) { return empty32; }
            u_bytes = u.GetRawBytes();
            return u_bytes;
        }

    }
    private byte[]? u_bytes;
    private static byte[] empty16 = new byte[16];
    private static byte[] empty32 = new byte[32];

    /// <summary>
    /// 32 bytes based on the owner password
    /// </summary>
    public byte[] OE_Bytes
    {
        get
        {
            if (oe_bytes != null) return oe_bytes;
            var oe = NativeObject.Get<PdfString>(PdfName.OE);
            if (oe == null) { return empty32; }
            oe_bytes = oe.GetRawBytes();
            return oe_bytes;
        }

    }
    private byte[]? oe_bytes;

    /// <summary>
    /// 32 bytes based on the owner password
    /// </summary>
    public byte[] UE_Bytes
    {
        get
        {
            if (ue_bytes != null) return ue_bytes;
            var ue = NativeObject.Get<PdfString>(PdfName.UE);
            if (ue == null) { return empty32; }
            ue_bytes = ue.GetRawBytes();
            return ue_bytes;
        }

    }
    private byte[]? ue_bytes;

    /// <summary>
    /// Set of flags specifying user permissions
    /// </summary>
    public int? P
    {
        get => (int?)NativeObject.Get<PdfNumber>(PdfName.P);
        set => NativeObject.Set(PdfName.P, (PdfNumber?)value);
    }

    /// <summary>
    /// Meaningful onwly when V == 5
    /// Indicates if metadata stream is encrypted
    /// </summary>
    public bool EncryptMetadata
    {
        get => (bool?)NativeObject.Get<PdfBoolean>(PdfName.EncryptMetadata) ?? true;
        set => NativeObject.Set(PdfName.EncryptMetadata, (PdfBoolean)value);
    }

}

