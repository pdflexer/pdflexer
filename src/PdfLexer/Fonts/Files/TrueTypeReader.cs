using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Fonts.Files
{
    public ref struct TrueTypeReader
    {
        private ReadOnlySpan<byte> Data;
        private Dictionary<string, Tab> Headers;
        private int Position;
        public TrueTypeReader(ParsingContext ctx, ReadOnlySpan<byte> data)
        {
            Data = data;
            Position = 0;
            Headers = null;
            Headers = ReadHeader();
        }

        private Dictionary<string, Tab> ReadHeader()
        {
            var ttcTag = GetString(4);
            if (ttcTag == "ttcf")
            {
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
                        return null;
                    case 2:
                        {
                            var dsigTag = GetInt32();
                            var dsigLength = GetInt32();
                            var dsigOffset = GetInt32();
                            return null;
                        }
                }

                throw new ApplicationException("Unknown truetype format: " + majorVersion);
            }
            else
            {
                var numTables = GetUint16();
                var searchRange = GetUint16();
                var entrySelector = GetUint16();
                var rangeShift = GetUint16();
                var tables = new Dictionary<string, Tab>();
                for (var i = 0; i < numTables; i++)
                {
                    var t = ReadOTTableEntry();
                    tables[t.Name] = t;
                }

                return tables;
                if (!tables.TryGetValue("name", out var nameTable))
                {
                    throw new ApplicationException("No name tables");
                }
                ReadNameTable(nameTable.Offset, nameTable.Length);
            }

        }
        public struct Tab
        {
            public string Name;
            public int Offset;
            public int Length;
        }

        public class TTCMap
        {
            public int PlatformId { get; set; }
            public int EncodingId { get; set; }
            public uint Offset { get; set; }
            public bool HasShortCmap { get; set; }
            public Dictionary<uint, uint> Mappings { get; set; }
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
        public bool HasCMapTable()
        {
            return Headers.ContainsKey("cmap");
        }     
        public TTCMap ReadCMapTable(bool hasEncoding, bool isSymbolicFont)
        {
            var table = Headers["cmap"];
            Position = table.Offset;
            var numTables = GetUint16();

            TTCMap? potentialTable = null;
            var canBreak = false;

            // There's an order of preference in terms of which cmap subtable to
            // use:
            // - non-symbolic fonts the preference is a 3,1 table then a 1,0 table
            // - symbolic fonts the preference is a 3,0 table then a 1,0 table
            // The following takes advantage of the fact that the tables are sorted
            // to work.
            for (var i = 0; i < numTables; i++)
            {
                var platformId = GetUint16();
                var encodingId = GetUint16();
                var offset = GetUInt32();
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
                    if (i < numTables - 1)
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
                    potentialTable = new TTCMap
                    {
                        PlatformId = platformId,
                        EncodingId = encodingId,
                        Offset = offset,
                    };
                }
                if (canBreak)
                {
                    break;
                }
            }

            if (potentialTable != null)
            {
                Position = table.Offset + (int)potentialTable.Offset;
            }
            if (potentialTable == null || Position >= Data.Length - 1)
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

            var format = GetUint16();
            var hasShortCmap = false;
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
                hasShortCmap = true;
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

            return new TTCMap
            {
                PlatformId = potentialTable.PlatformId,
                EncodingId = potentialTable.EncodingId,
                Mappings = mappings,
                HasShortCmap = hasShortCmap
            };
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
        private object ReadNameTable(int o, int l)
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
        public (bool, string[]) ReadPostScriptTable(int maxpNumGlyphs)
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

                    var glyphNames = new List<string>();

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
            return Headers.ContainsKey("loca") &&
                Headers.ContainsKey("loca");
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
            var prev = isLong ? loca.GetUInt32(0) : 2 * (uint)loca.GetUint16(0);
            var pos = 0;
            for (var i = 0; i < numGlyphs; i++)
            {
                pos += offsetSize;
                var next = isLong
                  ? loca.GetUInt32(pos)
                  : 2 * (uint)loca.GetUint16(pos);
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
    }

    internal ref struct DataView
    {
    private ReadOnlySpan<byte> Data;
    private int Pos;
    public DataView(ReadOnlySpan<byte> data)
    {
        Data = data;
        Pos = 0;
    }

    public int GetUint16(int pos)
    {
        Pos = pos;
        return Data[Pos++] << 8 | Data[Pos++];
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

}
}
