namespace PdfLexer.Validation;

internal static partial class MathUtil
{
    public static decimal mult(int val, int? val2)
    {
        if (val2 == null) return 0;
        return val * val2.Value;
    }
    public static bool eq(int? val, int? val2)
    {
        if (val == null && val2 == null) { return true; }
        if (val == null || val2 == null) { return false; }
        return val.Value == val2.Value;
    }

    public static IPdfObject? Get(this IPdfObject? obj, string name)
    {
        if (obj == null) return null;
        if (obj is PdfDictionary dict)
        {
            return dict.Get(name);
        }
        return null;
    }
    public static IPdfObject? Get(this IPdfObject? obj, int index)
    {
        if (obj == null) return null;
        if (obj is PdfArray arr)
        {
            return arr.Get(index);
        }
        return null;
    }

    public static bool ContainsKey(this PdfDictionary dict, IPdfObject? obj)
    {
        if (obj == null) return false;
        if (obj is PdfArray arr)
        {
            foreach (var val in arr)
            {
                var item = val.Resolve();
                if (item is PdfName nm)
                {
                    if (dict.ContainsKey(nm))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if (obj is PdfName nm)
        {
            return dict.ContainsKey(nm);
        }
        return false;
    }

    public static bool IsAssociatedFile(IPdfObject? obj)
    {
        throw new NotImplementedException("IsAssociatedFile");
    }

    private static HashSet<string> Base14Names = new HashSet<string>
    {
        "Times-Roman",
        "TimesNewRoman",
        "TimesNewRomanPS",
        "TimesNewRomanPSMT",
        "Helvetica",
        "ArialNarrow",
        "ArialBlack",
        "Arial-Black",
        "Arial",
        "ArialMT",
        "ArialUnicodeMS",

        "Courier",
        "CourierNew",
        "CourierNewPSMT",

        "Symbol",
        "Symbol-Bold",
        "Symbol-BoldItalic",
        "Symbol-Italic",

        "Times-Bold",
        "TimesNewRoman-Bold",
        "TimesNewRomanPS-Bold",
        "TimesNewRomanPSMT-Bold",

        "Helvetica-Bold",
        "ArialNarrow-Bold",
        "ArialBlack-Bold",
        "Arial-Black-Bold",
        "Arial-Bold",
        "Arial-BoldMT",
        "ArialUnicodeMS-Bold",

        "Courier-Bold",
        "CourierNew-Bold",
        "CourierNewPS-BoldMT",

        "ZapfDingbats",
        "Times-Italic",
        "TimesNewRoman-Italic",
        "TimesNewRomanPS-ItalicMT",
        "TimesNewRomanPS-Italic",
        "TimesNewRomanPSMT-Italic",

        "Helvetica-Oblique",
        "Helvetica-Italic",
        "ArialUnicodeMS-Italic",
        "Arial-ItalicMT",
        "Arial-Italic",
        "Arial-Black-Italic",
        "ArialBlack-Italic",
        "ArialNarrow-Italic",

        "Courier-Oblique",
        "Courier-Italic",
        "CourierNew-Italic",
        "CourierNewPS-ItalicMT",

        "Times-BoldItalic",
        "TimesNewRoman-BoldItalic",
        "TimesNewRomanPS-BoldItalic",
        "TimesNewRomanPS-BoldItalicMT",
        "TimesNewRomanPSMT-BoldItalic",

        "Helvetica-BoldOblique",
        "Helvetica-BoldItalic",
        "ArialUnicodeMS-BoldItalic",
        "Arial-BoldItalicMT",
        "Arial-BoldItalic",
        "Arial,Bold",
        "Arial-Black-BoldItalic",
        "ArialBlack-BoldItalic",
        "ArialNarrow-BoldItalic",

        "Courier-BoldOblique",
        "Courier-BoldItalic",
        "CourierNew-BoldItalic",
        "CourierNewPS-BoldItalicMT",
    };
    public static bool NotStandard14Font(IPdfObject? obj)
    {
        var bf = obj?.Get(PdfName.BaseFont);
        if (bf == null) { return true; }
        if (bf is PdfName nm)
        {
            return Base14Names.Contains(nm.Value);
        }
        return true;
    }

    public static bool IsHexString(IPdfObject? obj)
    {
        if (obj == null) return false;
        if (obj is PdfString str)
        {
            return str.StringType == PdfStringType.Hex;
        }
        return false;
    }

    public static bool BitsClear(IPdfObject? obj, uint mask)
    {
        if (obj == null) return false;
        if (obj is PdfNumber num)
        {
            var i = (uint)(int)num;
            return (i & mask) == 0;
        }
        return false;
    }

    public static bool BitsSet(IPdfObject? obj, uint mask)
    {
        if (obj == null) return false;
        if (obj is PdfNumber num)
        {
            var i = (uint)(int)num;
            return (i & mask) == mask;
        }
        return false;
    }

    public static int? StringLength(IPdfObject? obj)
    {
        if (obj == null) return null;
        if (obj is PdfString str)
        {
            return str.Value.Length;
        }
        return null;
    }

    public static bool IsFieldName(IPdfObject? obj)
    {
        return false;
    }

    public static bool MustBeDirect(IPdfObject? obj)
    {
        return false;
    }

    public static bool AlwaysUnencrypted(IPdfObject? obj)
    {
        return false;
    }

    public static bool ArraySortAscending(IPdfObject? obj, int i)
    {
        return false;
    }


    public static bool FontHasLatinChars(IPdfObject? obj)
    {
        throw new NotImplementedException("FontHasLatinChars");
    }

    public static bool IsPDFTagged(IPdfObject? obj)
    {
        throw new NotImplementedException("IsPDFTagged");
    }

    public static bool IsEncryptedWrapper(IPdfObject? obj)
    {
        throw new NotImplementedException("IsEncryptedWrapper");
    }

    public static bool ImageIsStructContentItem(IPdfObject? obj)
    {
        throw new NotImplementedException("ImageIsStructContentItem");
    }

    public static bool HasProcessColorants(IPdfObject? obj)
    {
        throw new NotImplementedException("HasProcessColorants");
    }

    public static bool HasSpotColorants(IPdfObject? obj)
    {
        throw new NotImplementedException("HasProcessColorants");
    }

    public static decimal RectWidth(IPdfObject? obj)
    {
        if (obj == null) { return 0; }
        if (obj is PdfArray arr)
        {
            if (arr.Count < 4)
            {
                return 0;
            }
            var p1 = arr[0] as PdfNumber;
            var p2 = arr[2] as PdfNumber;
            if (p1 == null || p2 == null) { return 0; }
            return Math.Abs((decimal)p1 - (decimal)p2);

        }
        return 0;
    }

    public static decimal RectHeight(IPdfObject? obj)
    {
        if (obj == null) { return 0; }
        if (obj is PdfArray arr)
        {
            if (arr.Count < 4)
            {
                return 0;
            }
            var p1 = arr[1] as PdfNumber;
            var p2 = arr[3] as PdfNumber;
            if (p1 == null || p2 == null) { return 0; }
            return Math.Abs((decimal)p1 - (decimal)p2);

        }
        return 0;
    }

    public static bool PageContainsStructContentItems(IPdfObject? obj)
    {
        return false; // TODO
        // throw new NotImplementedException("PageContainsStructContentItems");
    }

    public static bool InNameTree(IPdfObject? val)
    {
        throw new NotImplementedException("InNameTree");
    }

    public static bool Contains(IPdfObject? obj, string val)
    {
        if (obj == null)
        {
            return false;
        }
        var nm = new PdfName(val);
        if (obj is PdfArray arr)
        {

            return arr.Any(x => { var o = x.Resolve(); return o.Equals(nm); });
        }
        else
        {
            return obj.Equals(nm);
        }
    }
    public static int? mod(IPdfObject? obj, int v)
    {
        if (obj == null) { return null; }
        var n = obj as PdfNumber;
        if (n == null) { return null; }
        return n % v;
    }

    public static int? mod(IPdfObject? obj, int? v)
    {
        if (v == null) return null;
        return mod(obj, v.Value);
    }
    public static int? mod(int? v1, int? v2)
    {
        if (v1 == null || v2 == null) return null;
        return v1.Value % v2.Value;
    }
}
