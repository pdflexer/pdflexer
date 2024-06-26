﻿global using InlineImage_Op = PdfLexer.Operators.InlineImage_Op<double>;

using System.IO;
using System.Numerics;

namespace PdfLexer.Operators;


public class InlineImage_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
{
    public PdfOperatorType Type => PdfOperatorType.Unknown;

    public PdfArray header { get; }
    public byte[] allData { get; }
    public InlineImage_Op(PdfArray header, byte[] allData)
    {
        this.header = header;
        this.allData = allData;
    }
    public void Serialize(Stream stream)
    {
        stream.Write(BI_Op.OpData);
        stream.WriteByte((byte)'\n');
        for (var i = 0; i < header.Count; i++)
        {
            var item = header[i];
            PdfOperator.Shared.SerializeObject(stream, item, x => x);
            if (i < header.Count - 1)
            {
                stream.WriteByte((byte)' ');
            }
        }
        stream.WriteByte((byte)'\n');
        stream.Write(ID_Op.OpData);
        stream.WriteByte((byte)'\n');
        stream.Write(allData);
        stream.WriteByte((byte)'\n');
        stream.Write(EI_Op.OpData);
    }

    public PdfStream ConvertToStream(PdfDictionary resources)
    {
        var dict = new PdfDictionary();
        PdfName? key = null;
        foreach (var item in header)
        {
            if (key == null && item.Type == PdfObjectType.NameObj)
            {
                key = GetReplacedName((PdfName)item);
                continue;
            }
            if (key != null)
            {
                if (key == PdfName.ColorSpace)
                {
                    var v = GetReplacement(item, true);

                    if (v is PdfName nm &&
                        resources.TryGetValue<PdfDictionary>(PdfName.ColorSpace, out var css) && css.TryGetValue(nm, out var obj))
                    {
                        obj = obj.Resolve();
                        dict[key] = obj.Indirect();
                    } else 
                    {
                        dict[key] = v;
                    }
                } else
                {
                    var v = GetReplacement(item);
                    dict[key] = v;
                }
                key = null;
                
            }
        }
        dict[PdfName.Subtype] = PdfName.Image;
        var ba = new PdfByteArrayStreamContents(allData);
        ba.Filters = dict.Get(PdfName.Filter);
        ba.DecodeParams = dict.Get(PdfName.DecodeParms);
        return new PdfStream(dict, ba);
    }
    private static IPdfObject GetReplacement(IPdfObject obj, bool cs = false)
    {
        switch (obj)
        {
            case PdfName nm:
                return GetReplacedName(nm, cs);
            case PdfArray arr:
                var copy = new PdfArray();
                foreach (var item in arr)
                {
                    copy.Add(GetReplacement(item, cs));
                }
                return copy;
            default:
                return obj;
        }
    }
    private static PdfName GetReplacedName(PdfName name, bool cs = false)
    {
        switch (name.Value)
        {
            case "BPC":
                return PdfName.BitsPerComponent;
            case "CS":
                return PdfName.ColorSpace;
            case "G":
                return PdfName.DeviceGray;
            case "RGB":
                return PdfName.DeviceRGB;
            case "CMYK":
                return PdfName.DeviceCMYK;
            case "D":
                return PdfName.Decode;
            case "F":
                return PdfName.Filter;
            case "AHx":
                return PdfName.ASCIIHexDecode;
            case "A85":
                return PdfName.ASCII85Decode;
            case "LZW":
                return PdfName.LZWDecode;
            case "Fl":
                return PdfName.FlateDecode;
            case "RL":
                return PdfName.RunLengthDecode;
            case "CCF":
                return PdfName.CCITTFaxDecode;
            case "DCT":
                return PdfName.DCTDecode;
            case "DP":
                return PdfName.DecodeParms;
            case "W":
                return PdfName.Width;
            case "H":
                return PdfName.Height;
            case "I":
                return cs ? PdfName.Indexed : PdfName.Interpolate;
            case "IM":
                return PdfName.ImageMask;
        }
        return name;
    }
}