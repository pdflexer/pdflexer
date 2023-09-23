using PdfLexer.DOM;
using PdfLexer.Filters;
using System;
using System.Net.Http.Headers;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PdfLexer.Fonts.Files;

// TrueTypeReader PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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
public ref struct TrueTypeReader
{
    private readonly ReadOnlySpan<byte> Data;
    private readonly Dictionary<string, Tab> Headers;
    public IReadOnlyDictionary<string, Tab> Tables
    {
        get => Headers;
    }
    private int Position;
    public TrueTypeReader(ParsingContext ctx, ReadOnlySpan<byte> data)
    {
        Data = data;
        Position = 0;
        Headers = null!;
        Headers = ReadHeader();
    }

    public static bool IsTTFile(ReadOnlySpan<byte> data)
    {
        var header = data.Slice(0, 4);
        uint b0 = data[0], b1 = data[1], b2 = data[2], b3 = data[3];
        var v = (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
        if (v == 0x00010000) { return true; }

        var s = Encoding.UTF8.GetString(header);
        if (s == "true" || s == "typ1") { return true; }
        return false;
    }

    public static bool IsTTCollectionFile(ReadOnlySpan<byte> data)
    {
        var s = Encoding.UTF8.GetString(data.Slice(0, 4));
        if (s == "ttcf") { return true; }
        return false;
    }

    public static bool IsOpenTypeFile(ReadOnlySpan<byte> data)
    {
        var s = Encoding.UTF8.GetString(data.Slice(0, 4));
        if (s == "OTTO") { return true; }
        return false;
    }

    private Dictionary<string, Tab> ReadHeader()
    {
        var ttcTag = GetString(4);
        if (ttcTag == "ttcf")
        {
            var (numFonts, offsetTable) = GetFromTTCF();

            var parts = "".Split("+");
            for (var i = 0; i < numFonts; i++)
            {
                Position = (int)offsetTable[i];
                var numTables = ReadOpenTypeHeader();

                var tables = new Dictionary<string, Tab>();
                for (var t = 0; t < numTables; t++)
                {
                    var tab = ReadOTTableEntry();
                    tables[tab.Name] = tab;
                }

                if (!tables.TryGetValue("name", out var nameTabInfo))
                {
                    // err ?
                    continue;
                }


                // TODO complete name table parsing
                var nameTable = ReadNameTable(nameTabInfo.Offset, nameTabInfo.Length);

                // TODO match to correct font based on name or partial name.
            }
            throw new NotImplementedException("true type collections not implemented.");
        }
        else
        {
            Position -= 4; // unread header
            var numTables = ReadOpenTypeHeader();
            var tables = new Dictionary<string, Tab>();
            for (var i = 0; i < numTables; i++)
            {
                var t = ReadOTTableEntry();
                tables[t.Name] = t;
            }

            return tables;
        }

    }

    // GetPdfFontInfo table reading ported from pdfcpu (https://github.com/pdfcpu/pdfcpu)
    // pdfcpu is licensed as follows:
    /* Copyright 2019 The pdfcpu Authors.
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

    internal TrueTypeEmbeddedFont GetPdfFontInfo()
    {
        var info = new TrueTypeEmbeddedFont();

        // "head", "OS/2", "post", "name", "hhea", "maxp", "hmtx", "cmap"
        AddFontHeaderTable(info);
        AddWindowsMetricsTable(info);
        AddPostTable(info);
        info.PostScriptName = GetPostscriptName() ?? "Unknown";
        AddHorizontalHeaderTable(info);
        AddMaxProfileTable(info);
        AddHorizontalMetricsTable(info);

        var cmaps = ReadCMapTables();
        var cmap = GetPdfCmap(cmaps, false, false);

        info.DefaultEncoding = cmap.PlatformId == 3 ? PdfName.WinAnsiEncoding : PdfName.MacRomanEncoding;
        var glyphs = cmap.PlatformId == 3 ? Encodings.GetPartialGlyphs(Encodings.WinAnsiEncoding, cmap.Mappings!)
                : Encodings.GetPartialGlyphs(Encodings.MacRomanEncoding, cmap.Mappings!);

        foreach (var glyph in glyphs.Values)
        {
            var gid = cmap.Mappings![glyph.CodePoint!.Value];
            glyph.w0 = info.GlyphWidths[gid] / 1000f;
        }

        info.Glyphs = glyphs;

        var str = new PdfStream();
        {
            var flate = new ZLibLexerStream();
            flate.Write(Data);
            str.Contents = flate.Complete();
        }
        str.Dictionary[PdfName.Length1] = new PdfIntNumber(Data.Length);

        var fd = new FontDescriptor
        {
            FontName = info.PostScriptName,
            Flags = FontFlags.Nonsymbolic,
            FontBBox = new PdfRectangle
            {
                LLx = new PdfDoubleNumber(info.LLx),
                LLy = new PdfDoubleNumber(info.LLy),
                URx = new PdfDoubleNumber(info.URx),
                URy = new PdfDoubleNumber(info.URy),
            },
            ItalicAngle = new PdfDoubleNumber(info.ItalicAngle),
            CapHeight = info.CapHeight,
            Ascent = info.Ascent,
            Descent = info.Descent,
            StemV = info.ApproxStemV,
            FontFile2 = str,
        };
        if (info.Bold)
        {
            fd.Flags = fd.Flags | FontFlags.ForceBold;
        }
        info.Descriptor = fd;

        return info;
    }

    private void AddHorizontalMetricsTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("hmtx", out var table))
        {
            throw new PdfLexerException("No hmtx table in TTF font");
        }

        var t = new DataView(Data.Slice(table.Offset));

        fd.GlyphWidths = new int[fd.GlyphCount];
        for (var i = 0; i < fd.HorizontalMetricsCount; i++)
        {
            fd.GlyphWidths[i] = (int)fd.ToPDFGlyphSpace(t.GetUInt16(i * 4));
        }

        for (var i = fd.HorizontalMetricsCount; i < fd.GlyphCount; i++)
        {
            fd.GlyphWidths[i] = fd.GlyphWidths[fd.HorizontalMetricsCount - 1];
        }
    }

    private void AddMaxProfileTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("maxp", out var table))
        {
            throw new PdfLexerException("No maxp table in TTF font");
        }

        var t = new DataView(Data.Slice(table.Offset));

        fd.GlyphCount = t.GetUInt16(4);
    }

    private void AddHorizontalHeaderTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("hhea", out var table))
        {
            throw new PdfLexerException("No hhea table in TTF font");
        }

        var t = new DataView(Data.Slice(table.Offset));

        if (fd.Ascent == 0)
        {
            fd.Ascent = (int)fd.ToPDFGlyphSpace(t.GetInt16(4));
        }
        if (fd.Descent == 0)
        {
            fd.Descent = (int)fd.ToPDFGlyphSpace(t.GetInt16(6));
        }

        fd.HorizontalMetricsCount = t.GetUInt16(34);
    }

    private void AddPostTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("post", out var table))
        {
            throw new PdfLexerException("No post table in TTF font");
        }

        var t = new DataView(Data.Slice(table.Offset));
        fd.ItalicAngle = t.GetDouble(4);
        fd.FixedPitch = t.GetUInt16(16) != 0;
    }

    private void AddFontHeaderTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("head", out var table))
        {
            throw new PdfLexerException("No head table in TTF font");
        }

        var t = new DataView(Data.Slice(table.Offset));
        fd.UnitsPerEm = t.GetUInt16(18);
        var xMin = t.GetInt16(36);
        fd.LLx = fd.ToPDFGlyphSpace(xMin);
        var yMin = t.GetInt16(38);
        fd.LLy = fd.ToPDFGlyphSpace(yMin);
        var xMax = t.GetInt16(40);
        fd.URx = fd.ToPDFGlyphSpace(xMax);
        var yMax = t.GetInt16(42);
        fd.URy = fd.ToPDFGlyphSpace(yMax);
    }

    private void AddWindowsMetricsTable(TrueTypeEmbeddedFont fd)
    {
        if (!Headers.TryGetValue("OS/2", out var table))
        {
            return;
        }

        var t = new DataView(Data.Slice(table.Offset));

        var version = t.GetUInt16(0);

        var weightClass = t.GetUInt16(4);
        var w = weightClass / 65.0;
        fd.ApproxStemV = (int)(50 + w * w + 0.5);

        var fsType = t.GetUInt16(8);

        var prot = (fsType & 2) > 0;

        var uniCodeRange1 = t.GetUInt32(42);
        var fdUnicodeRange0 = uniCodeRange1;
        var uniCodeRange2 = t.GetUInt32(46);
        var fdUnicodeRange1 = uniCodeRange2;
        var uniCodeRange3 = t.GetUInt32(50);
        var fdUnicodeRange2 = uniCodeRange3;
        var uniCodeRange4 = t.GetUInt32(54);
        var fdUnicodeRange3 = uniCodeRange4;

        var sTypoAscender = t.GetInt16(68);
        fd.Ascent = (int)fd.ToPDFGlyphSpace(sTypoAscender);
        var sTypoDescender = t.GetInt16(70);
        fd.Descent = (int)fd.ToPDFGlyphSpace(sTypoDescender);

        if (version >= 2)
        {
            var sCapHeight = t.GetInt16(88);
            fd.CapHeight = (int)fd.ToPDFGlyphSpace(sCapHeight);
        }
        else
        {
            fd.CapHeight = fd.Ascent;
        }

        var fsSelection = t.GetUInt16(62);
        fd.Bold = (fsSelection & 0x40) > 0;


        var fsFirstCharIndex = t.GetUInt16(64);
        fd.FirstChar = fsFirstCharIndex;
        var fsLastCharIndex = t.GetUInt16(66);
        fd.LastChar = fsLastCharIndex;
    }


    private string? GetPostscriptName()
    {
        if (!Headers.TryGetValue("name", out var table))
        {
            return null;
        }

        var t = new DataView(Data.Slice(table.Offset));
        var count = t.GetUInt16(2);
        var stringOffset = t.GetUInt16(4);
        var baseOff = 6;
        for (var i = 0; i < count; i++)
        {
            var recOff = baseOff + i * 12;
            var pf = t.GetUInt16(recOff);
            var enc = t.GetUInt16(recOff + 2);
            var lang = t.GetUInt16(recOff + 4);
            var nameID = t.GetUInt16(recOff + 6);
            var l = t.GetUInt16(recOff + 8);
            var o = t.GetUInt16(recOff + 10);
            var soff = stringOffset + o;
            var s = t.Data.Slice(soff, l);
            if (nameID == 6)
            {
                if (pf == 3 && enc == 1 && lang == 0x0409)
                {
                    return Encoding.BigEndianUnicode.GetString(s);
                }
                if (pf == 1 && enc == 0 && lang == 0)
                {
                    return Encoding.UTF8.GetString(s);
                }
            }
        }
        return null;
    }

    private int ReadOpenTypeHeader()
    {
        var version = GetString(4);
        var numTables = GetUint16();
        var searchRange = GetUint16();
        var entrySelector = GetUint16();
        var rangeShift = GetUint16();
        return numTables;
    }

    private (uint numFonts, List<uint> offsetTable) GetFromTTCF()
    {
        // 4 byte already read
        var majorVersion = GetUint16();
        var minorVersion = GetUint16();
        var numFonts = GetUInt32();
        var offsetTable = new List<uint> { };
        for (var i = 0; i < numFonts; i++)
        {
            offsetTable.Add(GetUInt32());
        }

        switch (majorVersion)
        {
            case 1:
                // return new { ttcTag };
                return (numFonts, offsetTable);
            case 2:
                {
                    var dsigTag = GetInt32();
                    var dsigLength = GetInt32();
                    var dsigOffset = GetInt32();
                    return (numFonts, offsetTable);
                }
        }
        throw new PdfLexerException("Unknown truetype format: " + majorVersion);
    }

    public struct Tab
    {
        public string Name;
        public int Offset;
        public int Length;
    }


    public bool HasCMapTable()
    {
        return Headers.ContainsKey("cmap");
    }

    public List<TTCMap> ReadCMapTables()
    {
        var table = Headers["cmap"];
        Position = table.Offset;
        _ = GetUint16(); // version
        var numTables = GetUint16();
        var tables = new List<TTCMap>(numTables);
        for (var i = 0; i < numTables; i++)
        {
            var platformId = GetUint16();
            var encodingId = GetUint16();
            var offset = GetUInt32();
            tables.Add(new TTCMap
            {
                PlatformId = platformId,
                EncodingId = encodingId,
                Offset = table.Offset + (int)offset
            });
        }
        return tables;
    }

    public bool TryGetNameMap(List<TTCMap> maps, [NotNullWhen(true)] out Dictionary<uint, string>? gidToUnicode)
    {
        foreach (var map in maps)
        {
            if (map.PlatformId == 3 && map.EncodingId == 1)
            {
                var mapping = GetSingleMap(map);
                // map winansi to glyph id
                var enc = Encodings.WinAnsiEncoding;
                gidToUnicode = GetDict(mapping, enc);
                return true;
            }
            else if (map.PlatformId == 1)
            {
                var mapping = GetSingleMap(map);
                // map macroman to glyph id
                var enc = Encodings.MacRomanEncoding;
                gidToUnicode = GetDict(mapping, enc);
                return true;
            }
        }

        gidToUnicode = null;
        return false;

        Dictionary<uint, string> GetDict(Dictionary<uint, uint> cmap, string?[] enc)
        {
            var dict = new Dictionary<uint, string>();
            foreach (var item in cmap)
            {
                var winansi = item.Key;
                var gid = item.Value;
                if (winansi < 256)
                {
                    var v = enc[winansi];
                    if (v != null)
                    {
                        dict[gid] = v;
                    }
                }
            }
            return dict;
        }
    }

    public TTCMap GetPdfCmap(List<TTCMap> maps, bool hasEncoding, bool isSymbolicFont)
    {

        var canBreak = false;
        TTCMap? potentialTable = null;
        // There's an order of preference in terms of which cmap subtable to
        // use:
        // - non-symbolic fonts the preference is a 3,1 table then a 1,0 table
        // - symbolic fonts the preference is a 3,0 table then a 1,0 table
        // The following takes advantage of the fact that the tables are sorted
        // to work.
        for (var i = 0; i < maps.Count; i++)
        {
            var platformId = maps[i].PlatformId;
            var encodingId = maps[i].EncodingId;
            var useTable = false;

            // Sometimes there are multiple of the same type of table. Default
            // to choosing the first table and skip the rest.
            if (potentialTable != null
                && potentialTable.PlatformId == platformId
                && potentialTable.EncodingId == encodingId)
            {
                continue;
            }

            if (
                  platformId == 0 &&
                  (encodingId == /* Unicode Default */ 0 ||
                    encodingId == /* Unicode 1.1 */ 1 ||
                    encodingId == /* Unicode BMP */ 3)
                )
            {
                useTable = true;
                // Continue the loop since there still may be a higher priority
                // table.
            }
            else if (platformId == 1 && encodingId == 0)
            {
                useTable = true;
                // Continue the loop since there still may be a higher priority
                // table.
            }
            else if (
              platformId == 3 &&
              encodingId == 1 &&
              (hasEncoding || potentialTable == null)
          )
            {
                useTable = true;
                if (!isSymbolicFont)
                {
                    canBreak = true;
                }
            }
            else if (isSymbolicFont && platformId == 3 && encodingId == 0)
            {
                useTable = true;

                var correctlySorted = true;
                if (i < maps.Count - 1)
                {
                    // var nextBytes = file.peekBytes(2),
                    var nextPlatformId = GetUint16();
                    Position -= 2; // peek
                    if (nextPlatformId < platformId)
                    {
                        correctlySorted = false;
                    }
                }
                if (correctlySorted)
                {
                    canBreak = true;
                }
            }

            if (useTable)
            {
                potentialTable = maps[i];
            }
            if (canBreak)
            {
                break;
            }
        }

        if (potentialTable == null || potentialTable.Offset >= Data.Length - 1)
        {
            // warn("Could not find a preferred cmap table.");
            return new TTCMap
            {
                PlatformId = -1,
                EncodingId = -1,
                Mappings = new Dictionary<uint, uint>()
                // mappings: [],
                // hasShortCmap: false
            };
        }
        potentialTable.Mappings = GetSingleMap(potentialTable);
        return potentialTable;
    }

    public Dictionary<uint, uint> GetSingleMap(TTCMap map)
    {
        Position = map.Offset;
        var format = GetUint16();
        var mappings = new Dictionary<uint, uint>();
        int j = 0, glyphId = 0;
        if (format == 0)
        {
            Position += 4; // length + language
            for (j = 0; j < 256; j++)
            {
                var index = Data[Position++];
                if (index == 0) // (!index)
                {
                    continue;
                }
                mappings[(uint)j] = index;
            }
        }
        else if (format == 2)
        {
            Position += 4; // length + language
            var subHeaderKeys = new List<int>();
            var maxSubHeaderKey = 0;
            // Read subHeaderKeys. If subHeaderKeys[i] === 0, then i is a
            // single-byte character. Otherwise, i is the first byte of a
            // multi-byte character, and the value is 8*index into
            // subHeaders.
            for (var i = 0; i < 256; i++)
            {
                var subHeaderKey = GetUint16() >> 3;
                subHeaderKeys.Add(subHeaderKey);
                maxSubHeaderKey = Math.Max(subHeaderKey, maxSubHeaderKey);
            }

            // Read subHeaders. The number of entries is determined
            // dynamically based on the subHeaderKeys found above.
            var subHeaders = new List<TTSubHeader>();
            for (var i = 0; i <= maxSubHeaderKey; i++)
            {
                subHeaders.Add(new TTSubHeader
                {
                    FirstCode = GetUint16(),
                    EntryCount = GetUint16(),
                    IdDelta = GetInt16(),
                    IdRangePos = Position + GetUint16()
                });
            }
            for (var i = 0; i < 256; i++)
            {
                if (subHeaderKeys[i] == 0)
                {
                    // i is a single-byte code.
                    Position = subHeaders[0].IdRangePos + 2 * i;
                    glyphId = GetUint16();
                    mappings[(uint)i] = (uint)glyphId;
                }
                else
                {
                    // i is the first byte of a two-byte code.
                    var s = subHeaders[subHeaderKeys[i]];
                    for (j = 0; j < s.EntryCount; j++)
                    {
                        var charCode = (i << 8) + j + s.FirstCode;
                        Position = s.IdRangePos + 2 * j;
                        glyphId = GetUint16();
                        if (glyphId != 0)
                        {
                            glyphId = (glyphId + s.IdDelta) % 65536;
                        }

                        mappings[(uint)charCode] = (uint)glyphId;
                    }
                }
            }
        }
        else if (format == 4)
        {
            Position += 2 + 2; // length + language

            // re-creating the table in format 4 since the encoding
            // might be changed
            var segCount = GetUint16() >> 1;
            Position += 6; // skipping range fields
            var segments = new List<TTSegMent>();
            var segIndex = 0;
            for (segIndex = 0; segIndex < segCount; segIndex++)
            {
                segments.Add(new TTSegMent { End = GetUint16() });
            }
            Position += 2;
            for (segIndex = 0; segIndex < segCount; segIndex++)
            {
                segments[segIndex].Start = GetUint16();
            }

            for (segIndex = 0; segIndex < segCount; segIndex++)
            {
                segments[segIndex].Delta = GetUint16();
            }

            int offsetsCount = 0, offsetIndex = 0;
            for (segIndex = 0; segIndex < segCount; segIndex++)
            {
                var segment = segments[segIndex];
                var rangeOffset = GetUint16();
                if (rangeOffset == 0)
                {
                    segment.OffsetIndex = -1;
                    continue;
                }

                offsetIndex = (rangeOffset >> 1) - (segCount - segIndex);
                segment.OffsetIndex = offsetIndex;
                offsetsCount = Math.Max(
                  offsetsCount,
                  offsetIndex + segment.End - segment.Start + 1
                );
            }

            var offsets = new List<int>();
            for (j = 0; j < offsetsCount; j++)
            {
                offsets.Add(GetUint16());
            }

            for (segIndex = 0; segIndex < segCount; segIndex++)
            {
                var segment = segments[segIndex];
                var start = segment.Start;
                var end = segment.End;
                var delta = segment.Delta;
                offsetIndex = segment.OffsetIndex;

                for (j = start; j <= end; j++)
                {
                    if (j == 0xffff)
                    {
                        continue;
                    }

                    glyphId = offsetIndex < 0 ? j : offsets[offsetIndex + j - start];
                    glyphId = (glyphId + delta) & 0xffff;
                    mappings[(uint)j] = (uint)glyphId;
                }
            }
        }
        else if (format == 6)
        {
            Position += 2 + 2; // length + language

            // Format 6 is a 2-bytes dense mapping, which means the font data
            // lives glue together even if they are pretty far in the unicode
            // table. (This looks weird, so I can have missed something), this
            // works on Linux but seems to fails on Mac so let's rewrite the
            // cmap table to a 3-1-4 style
            var firstCode = GetUint16();
            var entryCount = GetUint16();

            for (j = 0; j < entryCount; j++)
            {
                glyphId = GetUint16();
                var charCode = firstCode + j;

                mappings[(uint)charCode] = (uint)glyphId;

            }
        }
        else if (format == 12)
        {
            Position += 2 + 4 + 4; // reserved + length + language
            var nGroups = GetUInt32();
            for (j = 0; j < nGroups; j++)
            {
                var startCharCode = GetUInt32();
                var endCharCode = GetUInt32();
                var glyphCode = GetUInt32();

                for (
                  var charCode = startCharCode;
                  charCode <= endCharCode;
                  charCode++
                )
                {
                    mappings[charCode] = glyphCode++;
                }
            }
        }
        else
        {
            // warn
        }

        return mappings;
    }

    private Tab ReadOTTableEntry()
    {
        var tag = GetString(4);

        var checksum = GetUInt32();
        var offset = GetInt32();
        var length = GetInt32();

        // Read the table associated data
        var data = new byte[length];
        Data.Slice(offset, length).CopyTo(data);

        if (tag == "head")
        {
            // clearing checksum adjustment
            data[8] = data[9] = data[10] = data[11] = 0;
            data[17] |= 0x20; // Set font optimized for cleartype flag.
        }
        return new Tab { Name = tag, Length = length, Offset = offset };
    }

    private string GetString(int length)
    {
        var s = Encoding.UTF8.GetString(Data.Slice(Position, length));
        Position += length;
        return s;
    }

    private int GetUint16()
    {
        return Data[Position++] << 8 | Data[Position++];
    }

    private int GetInt32()
    {
        var b0 = Data[Position++];
        var b1 = Data[Position++];
        var b2 = Data[Position++];
        var b3 = Data[Position++];
        return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
    }

    private uint GetUInt32()
    {
        uint b0 = Data[Position++];
        uint b1 = Data[Position++];
        uint b2 = Data[Position++];
        uint b3 = Data[Position++];
        return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
    }

    private int GetInt16()
    {
        var b0 = Data[Position++];
        var b1 = Data[Position++];
        var value = (b0 << 8) + b1;
        return (value & (1 << 15)) != 0 ? value - 0x10000 : value;
    }

    private object? ReadNameTable(int o, int l)
    {
        //const names = [[], []];
        Position = o;
        var format = GetUint16();
        var FORMAT_0_HEADER_LENGTH = 6;
        if (format != 0 || l < FORMAT_0_HEADER_LENGTH)
        {
            // unsupported name table format or table "too" small
            return null;
        }
        var numRecords = GetUint16();
        var stringsStart = GetUint16();
        var records = new List<Tab>();
        var NAME_RECORD_LENGTH = 12;
        int i = 0, ii = 0;

        for (i = 0; i < numRecords && Position + NAME_RECORD_LENGTH <= Data.Length; i++)
        {
            var platform = GetUint16();
            var encoding = GetUint16();
            var language = GetUint16();
            var name = GetUint16();
            var length = GetUint16();
            var offset = GetUint16();
            if (
              (platform == 1 && encoding == 0 && language == 0) ||
              (platform == 3 && encoding == 1 && language == 0x409)
            )
            {
                var start = o + stringsStart + offset;
                var end = start + length;
                if (end > o + l)
                {
                    continue;
                }
                var nm = Encoding.UTF8.GetString(Data.Slice(start, length));
                // records.Add(new Tab { Name = $"{(char)name}", Length = length, Offset = offset });
            }
        };

        for (i = 0, ii = records.Count; i < ii; i++)
        {
            var record = records[i];
            // if (record.length <= 0)
            // {
            //     continue; // Nothing to process, ignoring.
            // }
            // var pos = start + stringsStart + record.offset;
            // if (pos + record.length > end)
            // {
            //     continue; // outside of name table, ignoring
            // }
            // font.pos = pos;
            // var nameIndex = record.name;
            // if (record.encoding)
            // {
            //     // unicode
            //     var str = "";
            //     for (let j = 0, jj = record.length; j < jj; j += 2)
            //     {
            //         str += String.fromCharCode(font.getUint16());
            //     }
            //     names[1][nameIndex] = str;
            // }
            // else
            // {
            //     names[0][nameIndex] = font.getString(record.length);
            // }
        }
        return null;
    }
    public bool HasPostTable()
    {
        return Headers.ContainsKey("post");
    }
    public (bool, string?[]) ReadPostScriptTable(int maxpNumGlyphs)
    {
        var table = Headers["post"];
        Position = table.Offset;
        var length = table.Length;
        int end = Position + length;
        var version = GetInt32();

        // skip rest to the tables
        Position += 28;

        var valid = true;
        var i = 0;
        switch (version)
        {
            case 0x00010000:
                return (true, Encodings.MacRomanEncoding);
            case 0x00020000:
                var numGlyphs = GetUint16();
                if (numGlyphs != maxpNumGlyphs)
                {
                    valid = false;
                    break;
                }
                var glyphNameIndexes = new List<int>();
                for (i = 0; i < numGlyphs; ++i)
                {
                    var index = GetUint16();
                    if (index >= 32768)
                    {
                        valid = false;
                        break;
                    }
                    glyphNameIndexes.Add(index);
                }
                if (!valid)
                {
                    break;
                }

                var customNames = new List<string>();
                var strBuf = new StringBuilder();
                while (Position < end)
                {
                    var stringLength = (int)Data[Position++];
                    for (i = 0; i < stringLength; ++i)
                    {
                        strBuf.Append((char)Data[Position++]);
                    }
                    customNames.Add(strBuf.ToString());
                    strBuf.Length = 0;
                }

                var glyphNames = new List<string?>();

                for (i = 0; i < numGlyphs; ++i)
                {
                    var j = glyphNameIndexes[i];
                    if (j < 258)
                    {
                        glyphNames.Add(Encodings.MacStandardGlyphOrdering[j]);
                        continue;
                    }
                    glyphNames.Add(customNames[j - 258]);
                }
                return (valid, glyphNames.ToArray());
            case 0x00030000:
                return (true, empty);
            default:
                valid = false;
                break;
        }
        return (valid, empty);
    }
    private static string[] empty = new string[0];

    public bool HasGlyfInfo()
    {
        return Headers.ContainsKey("loca");
    }

    public bool HasCFFData()
    {
        return Headers.ContainsKey("CFF ");
    }

    public ReadOnlySpan<byte> GetCFFData()
    {
        var table = Headers["CFF "];
        return Data.Slice(table.Offset, table.Length);
    }

    public bool TryGetMaxpGlyphs(out int count)
    {
        count = 0;
        if (!Headers.TryGetValue("maxp", out var maxp))
        {
            return false;
        }

        Position = maxp.Offset;
        _ = GetInt32(); // version
        count = GetUint16();
        return true;
    }

    public bool[] ReadGlyfInfo(int numGlyphs)
    {
        // get glyph size
        var header = Headers["head"];
        Position = header.Offset + 50;
        var gs = GetInt16();
        var isLong = gs != 0;


        var locaTab = Headers["loca"];

        var results = new bool[numGlyphs];
        var loca = new DataView(Data.Slice(locaTab.Offset, locaTab.Length));
        var offsetSize = isLong ? 4 : 2;
        var prev = isLong ? loca.GetUInt32(0) : 2 * (uint)loca.GetUInt16(0);
        var pos = 0;
        for (var i = 0; i < numGlyphs; i++)
        {
            pos += offsetSize;
            var next = isLong
              ? loca.GetUInt32(pos)
              : 2 * (uint)loca.GetUInt16(pos);
            if (next == prev)
            {
                results[i] = false;
                continue;
            }

            results[i] = true;
            prev = next;
        }
        return results;
    }

    public class TTCMap
    {
        public int PlatformId { get; set; }
        public int EncodingId { get; set; }
        public int Offset { get; set; }
        public Dictionary<uint, uint>? Mappings { get; set; }
    }

    public class TTSubHeader
    {
        public int FirstCode { get; set; }
        public int EntryCount { get; set; }
        public int IdDelta { get; set; }
        public int IdRangePos { get; set; }
    }
    public class TTSegMent
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Delta { get; set; }
        public int OffsetIndex { get; set; }
    }
}

internal ref struct DataView
{
    internal ReadOnlySpan<byte> Data;
    private int Pos;
    public DataView(ReadOnlySpan<byte> data)
    {
        Data = data;
        Pos = 0;
    }

    public int GetUInt16(int pos)
    {
        Pos = pos;
        return Data[Pos++] << 8 | Data[Pos++];
    }

    public int GetInt16(int pos)
    {
        Pos = pos;
        var b0 = Data[Pos++];
        var b1 = Data[Pos++];
        var value = (b0 << 8) + b1;
        return (value & (1 << 15)) != 0 ? value - 0x10000 : value;
    }

    public uint GetUInt32(int pos)
    {
        Pos = pos;
        uint b0 = Data[Pos++];
        uint b1 = Data[Pos++];
        uint b2 = Data[Pos++];
        uint b3 = Data[Pos++];
        return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
    }

    public double GetDouble(int pos)
    {
        return ((double)GetUInt32(pos)) / 65536.0;
    }

}
