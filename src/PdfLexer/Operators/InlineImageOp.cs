using System.IO;

namespace PdfLexer.Operators
{
    public class InlineImage_Op : IPdfOperation
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

        public PdfStream ConvertToStream()
        {
            var dict = new PdfDictionary();
            PdfName key = null;
            foreach (var item in header)
            {
                if (key == null && item.Type == PdfObjectType.NameObj)
                {
                    key = GetReplacedName((PdfName)item);
                    continue;
                }
                if (key != null)
                {
                    dict[key] = GetReplacement(item);
                    key = null;
                }
            }
            dict[PdfName.Subtype] = PdfName.Image;
            var ba = new PdfByteArrayStreamContents(allData);
            ba.Filters = dict.Get(PdfName.Filter);
            ba.DecodeParams = dict.Get(PdfName.DecodeParms);
            return new PdfStream(dict, ba);
        }
        private static IPdfObject GetReplacement(IPdfObject obj)
        {
            switch (obj)
            {
                case PdfName nm:
                    return GetReplacedName(nm);
                case PdfArray arr:
                    var copy = new PdfArray();
                    foreach (var item in arr)
                    {
                        copy.Add(GetReplacement(item));
                    }
                    return copy;
                default:
                    return obj;
            }
        }
        private static PdfName GetReplacedName(PdfName name)
        {
            switch (name.Value)
            {
                case "/BPC":
                    return PdfName.BitsPerComponent;
                case "/CS":
                    return PdfName.ColorSpace;
                case "/G":
                    return PdfName.DeviceGray;
                case "/RGB":
                    return PdfName.DeviceRGB;
                case "/CMYK":
                    return PdfName.DeviceCMYK;
                case "/D":
                    return PdfName.Decode;
                case "/F":
                    return PdfName.Filter;
                case "/AHx":
                    return PdfName.ASCIIHexDecode;
                case "/A85":
                    return PdfName.ASCII85Decode;
                case "/LZW":
                    return PdfName.LZWDecode;
                case "/Fl":
                    return PdfName.FlateDecode;
                case "/RL":
                    return PdfName.RunLengthDecode;
                case "/CCF":
                    return PdfName.CCITTFaxDecode;
                case "/DCT":
                    return PdfName.DCTDecode;
                case "/DP":
                    return PdfName.DecodeParms;
                case "/W":
                    return PdfName.Width;
                case "/H":
                    return PdfName.Height;
                case "/I":
                    return PdfName.Interpolate;
                case "/IM":
                    return PdfName.ImageMask;
            }
            return name;
        }
    }

}
