using System.Text;

namespace PdfLexer.Fonts.Files;

// CFFReader PORTED FROM PDF.JS, PDF.JS is licensed as follows:
/* Copyright 2012 Mozilla Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/// <summary>
/// 
/// </summary>
internal ref struct CFFReader
{
    private ReadOnlySpan<byte> Data;
    private int Position;

    private ParsingContext Ctx;
    private CFFIndex NameIndex;
    private List<string> Names;

    private CFFIndex TopDictIndex;
    private CFFIndex StringIndex;
    private CFFIndex GlobalSubrIndex;

    public CFFReader(ParsingContext ctx, ReadOnlySpan<byte> data)
    {
        Data = data;
        Position = 0;
        Ctx = ctx;
        NameIndex = null!;
        Names = null!;
        TopDictIndex = null!;
        StringIndex = null!;
        GlobalSubrIndex = null!;

        var header = ParseHeader();
        NameIndex = ParseIndex().obj;
        Names = ReadStrings(NameIndex);
        if (Names.Count == 0) { throw new PdfLexerException("No fonts found in cff font"); } // switch to checking topdict?
        TopDictIndex = ParseIndex().obj;
        StringIndex = ParseIndex().obj;
        GlobalSubrIndex = ParseIndex().obj;
    }


    public string[] GetBaseSimpleEncoding(PdfName fontName)
    {
        var i = GetFontNumber(fontName);
        var osi = TopDictIndex.objects[i];
        var topDictParse = ParseDict(Data.Slice(osi.start, osi.length));
        var os = GetTopLevelDictOffsets(topDictParse);
        Position = os.charStringos;
        var charStringIndex = ParseIndex().obj;

        // todo
        var lookup = ReadStrings(StringIndex);

        var cset = ParseCharSet(os.charSetOs, charStringIndex.objects.Count, false);
        // todo, if we only want names, don't think we need to pass in like this and then map
        return GetSimpleEncoding(os.encodingOs, cset, lookup);
    }

    public void AddCharactersToCid(PdfName fontName, Dictionary<uint, uint>? cidLu, Dictionary<uint, Glyph> all, Glyph?[] b1g)
    {
        // TODO
        // it may be useful to extract more info in case PDF hasn't 
        // included it in pdf dictionaries (eg. ROS info for char set, font matrix, fontbbox)
        // 
        // can potentially grab other nice to have info like metrics

        // should only have single for opentype but check anyway in case multiple
        var set = GetCharSet(fontName, true);
        for (var c = 0; c < set.Count; c++)
        {
            var gid = (uint)set.GetId(c);
            var cid = gid;
            if (cidLu != null)
            {
                cidLu.TryGetValue(gid, out cid);
            }
            if (all.ContainsKey(cid))
            {
                continue;
            }
            var g = new Glyph
            {
                GuessedUnicode = true,
                Char = (char)cid,
                CID = cid
            };
            all[cid] = g;
            if (cid < b1g.Length)
            {
                b1g[cid] = g;
            }
        }
    }


    private int GetFontNumber(PdfName fontName)
    {
        var match = -1;
        for (var i = 0; i < Names.Count; i++)
        {
            var name = Names[i];
            if (name.Equals(fontName.Value))
            {
                match = i;
                break;
            }
        }
        if (match == -1)
        {
            match = 0;
        }
        return match;
    }

    private ICFFCharSet GetCharSet(PdfName fontName, bool isComposite)
    {
        var i = GetFontNumber(fontName);

        var osi = TopDictIndex.objects[i];
        var topDictParse = ParseDict(Data.Slice(osi.start, osi.length));
        var os = GetTopLevelDictOffsets(topDictParse);
        Position = os.charStringos;
        var charStringIndex = ParseIndex().obj;

        return ParseCharSet(os.charSetOs, charStringIndex.objects.Count, isComposite);
    }

    public static bool IsCFFfile(ReadOnlySpan<byte> data)
    {
        if (
          /* major version, [1, 255] */ data[0] >= 1 &&
          /* minor version, [0, 255]; header[1] */
          /* header size, [0, 255]; header[2] */
          /* offset(0) size, [1, 4] */ data[3] >= 1 &&
          data[3] <= 4
        )
        {
            return true;
        }
        return false;
    }

    private (int charSetOs, int encodingOs, int charStringos)
        GetTopLevelDictOffsets(List<(int, List<IorF>)> dict)
    {
        int charSet = 0, encoding = 0, charStrings = 0;
        for (var i = 0; i < dict.Count; ++i)
        {
            var pair = dict[i];
            var key = pair.Item1;
            var value = pair.Item2;
            switch (key)
            {
                case 15: // CharSetOffset
                    charSet = value.Count > 0 ? value[0].i ?? 0 : 0;
                    break;
                case 16: // EncodingOffset
                    encoding = value.Count > 0 ? value[0].i ?? 0 : 0;
                    break;
                case 17: // CharStringsOffset
                    charStrings = value.Count > 0 ? value[0].i ?? 0 : 0;
                    break;

            }
        }
        return (charSet, encoding, charStrings);
    }

    private ICFFCharSet ParseCharSet(int pos, int length, bool cid)
    {
        if (pos == 0)
        {
            return new CFFSet(CharSets.ISOAdobeCharset);
        }
        else if (pos == 1)
        {
            return new CFFSet(CharSets.ExpertCharset);
        }
        else if (pos == 2)
        {
            return new CFFSet(CharSets.ExpertSubsetCharset);
        }

        var start = pos;
        var format = Data[pos++];

        int id, count, i;

        ICFFCharSet charset = cid ? new CFFCidCharSet(length) : new CFFNameCharSet(length);

        // subtract 1 for the .notdef glyph
        length -= 1;

        switch (format)
        {
            case 0:
                for (i = 0; i < length; i++)
                {
                    id = (Data[pos++] << 8) | Data[pos++];
                    charset.Add(id);
                }
                break;
            case 1:
                while (charset.Count <= length)
                {
                    id = (Data[pos++] << 8) | Data[pos++];
                    count = Data[pos++];
                    for (i = 0; i <= count; i++)
                    {
                        charset.Add(id++);
                    }
                }
                break;
            case 2:
                while (charset.Count <= length)
                {
                    id = (Data[pos++] << 8) | Data[pos++];
                    count = (Data[pos++] << 8) | Data[pos++];
                    for (i = 0; i <= count; i++)
                    {
                        charset.Add(id++);
                    }
                }
                break;
            default:
                throw new PdfLexerException("Unknown charset format");
        }
        // Raw won't be needed if we actually compile the charset.
        var end = pos;

        return charset;
        // var raw = bytes.subarray(start, end);

        // return new CFFCharset(false, format, charset, raw);

    }

    private string[] GetSimpleEncoding(int pos, ICFFCharSet charset, List<string> strings)
    {
        // var predefined = false;
        int format, i, ii;
        // string raw = null;
        var encoding = new string[256];

        if (pos == 0 || pos == 1)
        {
            // predefined = true;
            format = pos;
            var baseEncoding = pos != 0 ? Encodings.ExpertEncoding : Encodings.StandardEncoding;
            for (i = 0, ii = charset.Count; i < ii; i++)
            {
                var name = charset.GetName(i);
                var index = Array.IndexOf(baseEncoding, name);
                if (index != -1 && index < 256) // simple only
                {
                    encoding[index] = name;
                }
            }
        }
        else
        {
            // var dataStart = pos;
            format = Data[pos++];
            switch (format & 0x7f)
            {
                case 0:
                    var glyphsCount = Data[pos++];
                    for (i = 1; i <= glyphsCount; i++)
                    {
                        encoding[Data[pos++]] = charset.GetName(i); // ?? ReadString(i, strings);
                    }
                    break;

                case 1:
                    var rangesCount = Data[pos++];
                    var gid = 1;
                    for (i = 0; i < rangesCount; i++)
                    {
                        int start = Data[pos++];
                        int left = Data[pos++];
                        for (var j = start; j <= start + left; j++)
                        {
                            encoding[j] = charset.GetName(gid++); // ?? ReadString(gid++, strings);
                        }
                    }
                    break;

                default:
                    throw new PdfLexerException($"Unknown encoding format: { format } in CFF");
            }
            // var dataEnd = pos;
            // if ((format & 0x80) != 0)
            // {
            //     // hasSupplement
            //     // The font sanitizer does not support CFF encoding with a
            //     // supplement, since the encoding is not really used to map
            //     // between gid to glyph, let's overwrite what is declared in
            //     // the top dictionary to let the sanitizer think the font use
            //     // StandardEncoding, that's a lie but that's ok.
            //     Data[dataStart] &= 0x7f;
            //     readSupplement();
            // }
            // raw = bytes.subarray(dataStart, dataEnd);
        }
        // format &= 0x7f;

        return encoding;

        // void readSupplement(ReadOnlySpan<byte> bytes)
        // {
        //     var supplementsCount = bytes[pos++];
        //     for (i = 0; i < supplementsCount; i++)
        //     {
        //         var code = bytes[pos++];
        //         var sid = (bytes[pos++] << 8) + (bytes[pos++] & 0xff);
        //         // encoding[code] = charset.indexOf(strings.get(sid));
        //     }
        // }
    }

    // private static string ReadString(int index, List<string> stringIndex)
    // {
    //     if (index >= 0 && index <= 390)
    //     {
    //         return CFFStandardStrings[index];
    //     }
    //     if (index - 391 < stringIndex.Count)
    //     {
    //         return stringIndex[index - 391];
    //     }
    //     return null;
    // }
    // 
    private Sized<CFFHeader> ParseHeader()
    {
        Position = 0;

        // Prevent an infinite loop, by checking that the offset is within the
        // bounds of the bytes array. Necessary in empty, or invalid, font files.
        while (Position < Data.Length && Data[Position] != 1)
        {
            ++Position;
        }
        if (Position >= Data.Length)
        {
            throw new PdfLexerException("Invalid CFF header");
        }
        if (Position != 0)
        {
            // info("cff data is shifted");
            Data = Data.Slice(Position);
            Position = 0;
        }
        var major = Data[0];
        var minor = Data[1];
        var hdrSize = Data[2];
        var offSize = Data[3];
        var header = new CFFHeader(major, minor, hdrSize, offSize);
        Position = hdrSize;
        return new Sized<CFFHeader>(header, hdrSize);
    }

    private Sized<CFFIndex> ParseIndex()
    {
        var cffIndex = new CFFIndex(new List<(int start, int length)>());
        var count = (Data[Position++] << 8) | Data[Position++];
        var end = Position;
        int i, ii;
        var offsets = new List<int>();
        if (count != 0)
        {
            var offsetSize = Data[Position++];
            // add 1 for offset to determine size of last object
            var startPos = Position + (count + 1) * offsetSize - 1;

            for (i = 0, ii = count + 1; i < ii; ++i)
            {
                var offset = 0;
                for (var j = 0; j < offsetSize; ++j)
                {
                    offset <<= 8;
                    offset += Data[Position++];
                }
                offsets.Add(startPos + offset);
            }
            end = offsets[count];
        }
        for (i = 0, ii = offsets.Count - 1; i < ii; ++i)
        {
            var offsetStart = offsets[i];
            var offsetEnd = offsets[i + 1];
            cffIndex.objects.Add((offsetStart, offsetEnd - offsetStart));
        }
        Position = end;
        return new Sized<CFFIndex>(cffIndex, end);
    }

    private List<string> ReadStrings(CFFIndex index)
    {
        var s = new List<string>();
        foreach (var item in index.objects)
        {
            s.Add(Iso88591.GetString(Data.Slice(item.start, item.length)));
        }

        return s;
    }
    public static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");
    private List<(int, List<IorF>)> ParseDict(ReadOnlySpan<byte> dict)
    {
        var ctx = Ctx;
        var operands = new List<IorF>();
        var entries = new List<(int, List<IorF>)>();
        var pos = 0;
        var end = dict.Length;
        while (pos < end)
        {
            int b = dict[pos];
            if (b <= 21)
            {
                if (b == 12)
                {
                    b = (b << 8) | dict[++pos];
                }
                entries.Add((b, operands));
                operands = new List<IorF>();
                ++pos;
            }
            else
            {
                operands.Add(ParseOperand(dict));
            }
        }
        return entries;

        IorF ParseOperand(ReadOnlySpan<byte> dict)
        {
            int value = dict[pos++];
            if (value == 30)
            {
                return new IorF { f = ParseFloatOperand(dict) };
            }
            else if (value == 28)
            {
                value = dict[pos++];
                value = ((value << 24) | (dict[pos++] << 16)) >> 16;
                return new IorF { i = value };
            }
            else if (value == 29)
            {
                value = dict[pos++];
                value = (value << 8) | dict[pos++];
                value = (value << 8) | dict[pos++];
                value = (value << 8) | dict[pos++];
                return new IorF { i = value };
            }
            else if (value >= 32 && value <= 246)
            {
                return new IorF { i = value - 139 };
            }
            else if (value >= 247 && value <= 250)
            {
                return new IorF { i = (value - 247) * 256 + dict[pos++] + 108 };
            }
            else if (value >= 251 && value <= 254)
            {
                return new IorF { i = -((value - 251) * 256) - dict[pos++] - 108 };
            }
            ctx.Error($"CFFParser_parseDict: ${ value } is a reserved command.");
            return new IorF();
        }

        float? ParseFloatOperand(ReadOnlySpan<byte> dict)
        {
            var str = "";
            var eof = 15;
            // prettier-ignore
            var lookup = new List<string?> { "0", "1", "2", "3", "4", "5", "6", "7", "8",
                            "9", ".", "E", "E-", null, "-" };
            var length = dict.Length;
            while (pos < length)
            {
                var b = dict[pos++];
                var b1 = b >> 4;
                var b2 = b & 15;

                if (b1 == eof)
                {
                    break;
                }
                str += lookup[b1];

                if (b2 == eof)
                {
                    break;
                }
                str += lookup[b2];
            }
            return float.Parse(str);
        }
    }

    internal static string[] CFFStandardStrings = new string[] {
".notdef", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent",
"ampersand", "quoteright", "parenleft", "parenright", "asterisk", "plus",
"comma", "hyphen", "period", "slash", "zero", "one", "two", "three", "four",
"five", "six", "seven", "eight", "nine", "colon", "semicolon", "less",
"equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F", "G", "H",
"I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W",
"X", "Y", "Z", "bracketleft", "backslash", "bracketright", "asciicircum",
"underscore", "quoteleft", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j",
"k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y",
"z", "braceleft", "bar", "braceright", "asciitilde", "exclamdown", "cent",
"sterling", "fraction", "yen", "florin", "section", "currency",
"quotesingle", "quotedblleft", "guillemotleft", "guilsinglleft",
"guilsinglright", "fi", "fl", "endash", "dagger", "daggerdbl",
"periodcentered", "paragraph", "bullet", "quotesinglbase", "quotedblbase",
"quotedblright", "guillemotright", "ellipsis", "perthousand", "questiondown",
"grave", "acute", "circumflex", "tilde", "macron", "breve", "dotaccent",
"dieresis", "ring", "cedilla", "hungarumlaut", "ogonek", "caron", "emdash",
"AE", "ordfeminine", "Lslash", "Oslash", "OE", "ordmasculine", "ae",
"dotlessi", "lslash", "oslash", "oe", "germandbls", "onesuperior",
"logicalnot", "mu", "trademark", "Eth", "onehalf", "plusminus", "Thorn",
"onequarter", "divide", "brokenbar", "degree", "thorn", "threequarters",
"twosuperior", "registered", "minus", "eth", "multiply", "threesuperior",
"copyright", "Aacute", "Acircumflex", "Adieresis", "Agrave", "Aring",
"Atilde", "Ccedilla", "Eacute", "Ecircumflex", "Edieresis", "Egrave",
"Iacute", "Icircumflex", "Idieresis", "Igrave", "Ntilde", "Oacute",
"Ocircumflex", "Odieresis", "Ograve", "Otilde", "Scaron", "Uacute",
"Ucircumflex", "Udieresis", "Ugrave", "Yacute", "Ydieresis", "Zcaron",
"aacute", "acircumflex", "adieresis", "agrave", "aring", "atilde",
"ccedilla", "eacute", "ecircumflex", "edieresis", "egrave", "iacute",
"icircumflex", "idieresis", "igrave", "ntilde", "oacute", "ocircumflex",
"odieresis", "ograve", "otilde", "scaron", "uacute", "ucircumflex",
"udieresis", "ugrave", "yacute", "ydieresis", "zcaron", "exclamsmall",
"Hungarumlautsmall", "dollaroldstyle", "dollarsuperior", "ampersandsmall",
"Acutesmall", "parenleftsuperior", "parenrightsuperior", "twodotenleader",
"onedotenleader", "zerooldstyle", "oneoldstyle", "twooldstyle",
"threeoldstyle", "fouroldstyle", "fiveoldstyle", "sixoldstyle",
"sevenoldstyle", "eightoldstyle", "nineoldstyle", "commasuperior",
"threequartersemdash", "periodsuperior", "questionsmall", "asuperior",
"bsuperior", "centsuperior", "dsuperior", "esuperior", "isuperior",
"lsuperior", "msuperior", "nsuperior", "osuperior", "rsuperior", "ssuperior",
"tsuperior", "ff", "ffi", "ffl", "parenleftinferior", "parenrightinferior",
"Circumflexsmall", "hyphensuperior", "Gravesmall", "Asmall", "Bsmall",
"Csmall", "Dsmall", "Esmall", "Fsmall", "Gsmall", "Hsmall", "Ismall",
"Jsmall", "Ksmall", "Lsmall", "Msmall", "Nsmall", "Osmall", "Psmall",
"Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall",
"Xsmall", "Ysmall", "Zsmall", "colonmonetary", "onefitted", "rupiah",
"Tildesmall", "exclamdownsmall", "centoldstyle", "Lslashsmall",
"Scaronsmall", "Zcaronsmall", "Dieresissmall", "Brevesmall", "Caronsmall",
"Dotaccentsmall", "Macronsmall", "figuredash", "hypheninferior",
"Ogoneksmall", "Ringsmall", "Cedillasmall", "questiondownsmall", "oneeighth",
"threeeighths", "fiveeighths", "seveneighths", "onethird", "twothirds",
"zerosuperior", "foursuperior", "fivesuperior", "sixsuperior",
"sevensuperior", "eightsuperior", "ninesuperior", "zeroinferior",
"oneinferior", "twoinferior", "threeinferior", "fourinferior",
"fiveinferior", "sixinferior", "seveninferior", "eightinferior",
"nineinferior", "centinferior", "dollarinferior", "periodinferior",
"commainferior", "Agravesmall", "Aacutesmall", "Acircumflexsmall",
"Atildesmall", "Adieresissmall", "Aringsmall", "AEsmall", "Ccedillasmall",
"Egravesmall", "Eacutesmall", "Ecircumflexsmall", "Edieresissmall",
"Igravesmall", "Iacutesmall", "Icircumflexsmall", "Idieresissmall",
"Ethsmall", "Ntildesmall", "Ogravesmall", "Oacutesmall", "Ocircumflexsmall",
"Otildesmall", "Odieresissmall", "OEsmall", "Oslashsmall", "Ugravesmall",
"Uacutesmall", "Ucircumflexsmall", "Udieresissmall", "Yacutesmall",
"Thornsmall", "Ydieresissmall", "001.000", "001.001", "001.002", "001.003",
"Black", "Bold", "Book", "Light", "Medium", "Regular", "Roman", "Semibold"
};

    internal const int NUM_STANDARD_CFF_STRINGS = 391;
}

internal struct IorF { public int? i; public float? f; }
internal record CFFIndex(List<(int start, int length)> objects);
internal record Sized<T>(T obj, int endPos);
internal record CFFHeader(int major, int minor, int hdrSize, int offSize);

internal class CFFCidCharSet : ICFFCharSet
{
    private List<int> cids;
    public CFFCidCharSet(int total)
    {
        cids = new List<int>(total);
        cids.Add(0);
    }
    public int Count => cids.Count;

    public void Add(int id)
    {
        cids.Add(id);
    }

    public string GetName(int id)
    {
        throw new NotSupportedException("Cannot get name from CID CFF font");
    }

    public int GetId(int id)
    {
        if (id < cids.Count)
        {
            return cids[id];
        }
        return 0;
    }

    public bool HasNames() => false;
}

internal class CFFNameCharSet : ICFFCharSet
{
    private List<string> cids;
    private List<int> ids;
    public CFFNameCharSet(int total)
    {
        cids = new List<string>(total);
        cids.Add(".notdef");
        ids = new List<int>();
    }
    public int Count => cids.Count;

    public void Add(int id)
    {
        if (id < CFFReader.CFFStandardStrings.Length)
        {
            cids.Add(CFFReader.CFFStandardStrings[id]);
        }
        else
        {
            cids.Add(CFFReader.CFFStandardStrings[0]);
        }
        ids.Add(id);
    }

    public int GetId(int id)
    {
        if (id < ids.Count)
        {
            return ids[id];
        }
        return 0;
    }
    public string GetName(int id)
    {
        if (id > -1 && id < cids.Count)
        {
            return cids[id];
        }
        return ".notdef";
    }
}

internal class CFFSet : ICFFCharSet
{
    private string[] chars;
    public CFFSet(string[] chars)
    {
        this.chars = chars;
    }
    public int Count => chars.Length;

    public void Add(int id)
    {
        throw new NotSupportedException("Cannot add to predefined char set");
    }

    public string GetName(int id)
    {
        if (id < chars.Length)
        {
            return chars[id];
        }
        return ".notdef";
    }


}
internal interface ICFFCharSet
{
    void Add(int id);
    int Count { get; }
    string GetName(int i);
    int GetId(int i) { return 0; }
    bool HasNames() { return true; }
}

