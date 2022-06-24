using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using PdfLexer.IO;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers
{
    public class XRefParser
    {
        private const int XrefBackScan = 250;
        private static byte[] startxref = new byte[9] {
            (byte)'s',(byte)'t',(byte)'a',(byte)'r',(byte)'t',(byte)'x',(byte)'r',(byte)'e',(byte)'f'
        };
        public static byte[] trailer = new byte[7] {
            (byte)'t',(byte)'r',(byte)'a',(byte)'i',(byte)'l',(byte)'e',(byte)'r'
        };
        public static byte[] xref = new byte[4] {
            (byte)'x',(byte)'r',(byte)'e',(byte)'f'
        };
        private readonly ParsingContext _ctx;


        public XRefParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public async Task LoadCrossReference(IPdfDataSource source)
        {
            var readStart = source.TotalBytes - XrefBackScan;
            if (readStart < 0)
            {
                readStart = 0;
            }

            long offset = 0;
            var stream = source.GetStream(readStart);
            int stage = 0;
            var pipe = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
            var item = await pipe.ReadNextObject(_ctx);
            // pipe.ReadTokenSequence(_ctx, _ctx.ObjectBag, )
            while (true)
            {
                var result = await pipe.ReadAsync();
                if (TryGetOffset(result.Buffer, result.IsCompleted, ref stage, out offset, out var pos))
                {
                    break;
                }
                pipe.AdvanceTo(pos);

                if (result.IsCompleted || result.IsCanceled)
                {
                    throw new ApplicationException("Unable to find xref offset.");
                }
            }
            pipe.Complete();

            stream = source.GetStream(offset);
            pipe = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
            var peak = await pipe.ReadAsync();
            var start = PdfSpanLexer.TryReadNextToken(peak.Buffer.FirstSpan, out var type, 0, out int length);
            if (start == -1)
            {
                throw new ApplicationException("Unable to read data to determine Xref catalog type");
            }

            if (type == PdfTokenType.NumericObj)
            {
                throw new NotImplementedException("PDF 1.5 Cross-Reference Streams not yet implemented.");
            } else if (type == PdfTokenType.Xref)
            {
                var sequence = await pipe.LocateXrefTableAndTrailer();
                if (sequence.IsSingleSegment)
                {

                }
            }
        }

        internal List<XRefEntry> GetEntries(ReadOnlySpan<byte> data)
        {
            var entries = new List<XRefEntry>();
            var i = 0;
            while (data.Length > i)
            {
                var info = GetSectionHeader(data, ref i);
                GetSection(info.firtObjectNum, info.totalCount, entries, data, ref i);
                CommonUtil.SkipWhiteSpace(data, ref i);
            }

            return entries;
        }

        private (int firtObjectNum, int totalCount) GetSectionHeader(ReadOnlySpan<byte> data, ref int position)
        {
            var first = PdfSpanLexer.TryReadNextToken(data, out var type, position, out var length);
            if (first == -1 || type != PdfTokenType.NumericObj)
            {
                throw CommonUtil.DisplayDataErrorException(data, position, "Bad xref header entry");
            }
            var num1 = _ctx.NumberParser.Parse(data, first, length);

            var second = PdfSpanLexer.TryReadNextToken(data, out type, first+length, out length);
            if (second == -1 || type != PdfTokenType.NumericObj)
            {
                throw CommonUtil.DisplayDataErrorException(data, position, "Bad xref header entry");
            }

            var num2 = _ctx.NumberParser.Parse(data, second, length);

            if (!(num1 is PdfIntNumber fn) || !(num2 is PdfIntNumber sn))
            {
                throw CommonUtil.DisplayDataErrorException(data, position, "Bad xref entry header, items not int");
            }

            position = second + length;
            return (fn.Value, sn.Value);
        }

        private void GetSection(int firtObjectNum, int totalCount, List<XRefEntry> entries, ReadOnlySpan<byte> data, ref int position)
        {
            for (var i=0;i<totalCount;i++)
            {
                var entry = GetSingleEntry(data, position, out int nextPos);
                entry.ObjectNumber = firtObjectNum + i;
                position = nextPos;
                entries.Add(entry);
            }
        }

        private XRefEntry GetSingleEntry(ReadOnlySpan<byte> data, int startPos, out int nextPos)
        {
            var first = PdfSpanLexer.TryReadNextToken(data, out var type, startPos, out var length);
            if (first == -1 || type != PdfTokenType.NumericObj)
            {
                throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
            }
            Debug.Assert(length == 10, "Xref offset is 10 bytes");
            var num1 = _ctx.NumberParser.Parse(data, first, length);

            var second = PdfSpanLexer.TryReadNextToken(data, out type, first+length, out length);
            if (second == -1 || type != PdfTokenType.NumericObj)
            {
                throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
            }
            Debug.Assert(length == 5, "Xref gen number is 5 bytes");
            var num2 = _ctx.NumberParser.Parse(data, second, length);

            var pos = second + length;
            CommonUtil.SkipWhiteSpace(data, ref pos);

            bool isFree = false;
            var b = data[pos];
            if (b == (byte) 'f')
            {
                isFree = true;
            } else if (b == (byte) 'n')
            {
                isFree = false;
            } else
            {
                throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry");
            }

            if (!(num1 is PdfLongNumber ln))
            {
                throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry, offset not long");
            }
            if (!(num2 is PdfIntNumber intNum))
            {
                throw CommonUtil.DisplayDataErrorException(data, startPos, "Bad xref entry, generation not int");
            }

            nextPos = pos + 1;
            CommonUtil.SkipWhiteSpace(data, ref nextPos);
            Debug.Assert(nextPos - pos == 3, "nextPos - pos == 3, 2 EOL chars");

            return new XRefEntry
            {
                Offset = ln.Value,
                Generation = intNum.Value,
                IsFree = isFree
            };
        }

        private bool TryGetOffset(ReadOnlySequence<byte> buffer, bool isCompleted, ref int stage, out long offset, out SequencePosition pos)
        {
            offset = 0;
            var reader = new SequenceReader<byte>(buffer);
            if (stage == 0)
            {
                if (!reader.TryAdvanceTo((byte) 's'))
                {
                    pos = reader.Position;
                    return false;
                }

                if (!reader.IsNext(startxref))
                {
                    pos = reader.Position;
                    return false;
                }

                stage++;
            } else if (stage == 1)
            {
                if (!reader.TryReadNextToken(isCompleted, out var type, out var position))
                {
                    pos = reader.Position;
                    return false;
                }

                if (type != PdfTokenType.NumericObj)
                {
                    throw new ApplicationException("Startxref not followed by offset number.");
                }

                var numData = buffer.Slice(reader.Position, position);
                var offsetNumber = _ctx.NumberParser.Parse(in numData);
                if (offsetNumber is PdfLongNumber ln)
                {
                    pos = reader.Position;
                    offset = ln.Value;
                    return true;
                } else if (offsetNumber is PdfIntNumber nm)
                {
                    pos = reader.Position;
                    offset = nm.Value;
                    return true;
                } else
                {
                    throw new ApplicationException("Did not get long or int for xref offset");
                }
            }

            throw new ApplicationException("Unknown stage for XRef parsing.");
        }

        private long GetXrefEasy(Stream stream)
        {
            // stream.Seek(-scanback, SeekOrigin.End);
            // var read = stream.Read(_ctx.Buffer, 0, scanback);
            // Debug.Assert(read == scanback, "read == scanback for GettingXrefOffset");
            // var buff = new Span<byte>(_ctx.Buffer, 0, scanback);
            // var start = buff.LastIndexOf(startxref);
            // if (start == -1)
            // {
            //     throw new ApplicationException("Unable to find startxref.");
            // }
            // 
            // buff = buff.Slice(start + 9);
            // var loc = PdfSpanLexer.TryReadNextToken(buff, out PdfTokenType type, out int length);
            // if (loc == -1 || type != PdfTokenType.NumericObj)
            // {
            //     throw new ApplicationException("Startxref not followed by offset number.");
            // }
            // 
            // var offsetNumber = _ctx.NumberParser.Parse(buff, loc, length);
            // if (offsetNumber is PdfLongNumber ln)
            // {
            //     return ln.Value;
            // } else if (offsetNumber is PdfIntNumber nm)
            // {
            //     return nm.Value;
            // } else
            // {
            //     throw new ApplicationException("Did not get long or int for xref offset");
            // }
            return 0;
        }

        public PdfDictionary LoadCrossReference(Stream stream, long offset)
        {

            // for non streamed
            // starts with xref then lew line
            // section started with two numbers (first obj number in section, total objects in section)
            // 20 byte entries including eol 
            // 10 digits offset, 5 digit gen, n or f, 2 char eol
            // always starts with object 0, gen 65,535, f
            return null;
        }

        public static int GetXrefEnd(ReadOnlySpan<byte> bytes)
        {
            var start = bytes.LastIndexOf(startxref);
            if (start == -1)
            {
                return start;
            }
            else
            {
                return start + 9;
            }
        }

        public static int GetTrailerEnd(ReadOnlySpan<byte> bytes)
        {
            var start = bytes.LastIndexOf(trailer);
            if (start == -1)
            {
                return start;
            }
            else
            {
                return start + 7;
            }
        }

        public static int GetTrailerDict(ReadOnlySpan<byte> bytes)
        {
            var start = bytes.LastIndexOf(trailer);
            if (start == -1)
            {
                return start;
            }
            else
            {
                return start + 7;
            }
        }

        /// <summary>
        /// Basic lookup to get byte offset of XRef table.
        /// Not performance critical, once per file.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ulong GetXrefTableOffset(ReadOnlySpan<byte> bytes)
        {
            var end = GetXrefEnd(bytes);
            if (end == -1)
            {
                throw new ApplicationException("No startxref statement found.");
            }

            var offset = bytes.Slice(end);
            var loc = PdfSpanLexer.TryReadNextToken(offset, out PdfTokenType type, out int length);
            if (loc == -1 || type != PdfTokenType.NumericObj)
            {
                throw new ApplicationException("Startxref not followed by offset number.");
            }
            if (loc > 0)
            {
                offset = offset.Slice(loc);
            }

            throw new NotImplementedException();
            // var numFound = NumberParser.GetNumber(offset, out ReadOnlySpan<byte> numberBytes, out NumberType numberType);
            // if (numFound == false)
            // {
            //     throw new ApplicationException("Incomplete file, xref offset not finished.");
            // }
            // else if (numberType != NumberType.Integer)
            // {
            //     throw new ApplicationException("Non-integer byte offset given for xref.");
            // }
            // return NumberParser.ParseUInt64(numberBytes);
        }


        public static (int FirstObj, int LastObj) ParseHeader(ReadOnlySpan<char> chars)
        {
            int first = 0;
            int secondStart = 0;
            bool readingFirst = true;
            bool middlespace = false;
            bool readingSecond = false;
            for (var i = 0; i < chars.Length; i++)
            {
                if (readingFirst && CommonUtil.IsWhiteSpace(chars, i))
                {
                    readingFirst = false;
                    middlespace = true;
                    first = int.Parse(chars.Slice(0, i));
                }
                else if (middlespace && !CommonUtil.IsWhiteSpace(chars, i))
                {
                    middlespace = false;
                    readingSecond = true;
                    secondStart = i;
                }
                else if (readingSecond && CommonUtil.IsWhiteSpace(chars, i))
                {
                    return (first, int.Parse(chars.Slice(secondStart, i - secondStart)));
                }
            }
            return (first, int.Parse(chars.Slice(secondStart)));
        }

        public static (uint FirstObj, uint ObjectCount) ParseHeader(ReadOnlySpan<byte> bytes)
        {
            uint first = 0;
            int secondStart = 0;
            bool readingFirst = true;
            bool middlespace = false;
            bool readingSecond = false;
            var buffer = new Span<char>(new char[bytes.Length]);
            Encoding.ASCII.GetChars(bytes, buffer);
            for (var i = 0; i < bytes.Length; i++)
            {
                if (readingFirst && CommonUtil.IsWhiteSpace(bytes, i))
                {
                    readingFirst = false;
                    middlespace = true;
                    first = uint.Parse(buffer.Slice(0, i));
                }
                else if (middlespace && !CommonUtil.IsWhiteSpace(bytes, i))
                {
                    middlespace = false;
                    readingSecond = true;
                    secondStart = i;
                }
                else if (readingSecond && CommonUtil.IsWhiteSpace(bytes, i))
                {
                    return (first, uint.Parse(buffer.Slice(secondStart, i - secondStart)));
                }
            }
            return (first, uint.Parse(buffer.Slice(secondStart)));
        }

        public static XRefEntry ParseXrefRecord(ReadOnlySpan<byte> bytes, Span<char> buffer)
        {
            if (!CommonUtil.IsWhiteSpace(bytes[10]) || !CommonUtil.IsWhiteSpace(bytes[16]))
            {
                throw new ApplicationException("Invalid XREF entry.");
            };
            Encoding.ASCII.GetChars(bytes.Slice(0, 10), buffer);
            var offset = UInt64.Parse(buffer.Slice(0, 10));
            Encoding.ASCII.GetChars(bytes.Slice(11, 5), buffer);
            var gen = uint.Parse(buffer.Slice(0, 5));
            return null;
            // return new XRefEntry
            // {
            //     Offset = offset,
            //     Generation = gen,
            //     IsFree = bytes[17] == 'f'
            // };
        }
    }

    internal enum XRefParseStage
    {
        Start,
        Records,
        Finished
    }
    internal ref struct XRefTableReader
    {

        private ReadOnlySpan<byte> _data;

        private XRefParseStage stage;
        private Span<char> _buffer;
        private uint currentObject;
        private uint endObject;

        // current xref entry values
        private ulong xrefOffset;
        private uint xrefGen;
        private bool xrefIsFree;
        private uint xrefObjNum;

        public bool IsFinished;
        public int CurrentPosition;

        public XRefTableReader(ReadOnlySpan<byte> data)
        {
            _data = data;
            CurrentPosition = 0;
            currentObject = 0;
            endObject = 0;
            IsFinished = false;
            stage = XRefParseStage.Start;
            _buffer = new Span<char>(new char[20]);
            xrefOffset = 0;
            xrefGen = 0;
            xrefObjNum = 0;
            xrefIsFree = false;
        }

        public XRefEntry GetCurrentEntry()
        {
            return null;
            //return new XRefEntry
            //{
            //    ObjectNumber = xrefObjNum,
            //    Offset = xrefOffset,
            //    Generation = xrefGen,
            //    IsFree = xrefIsFree
            //};
        }

        public bool Read()
        {
            if (stage == XRefParseStage.Start)
            {
                if (_data.Length < 32) // minimum for xref + record + trailer
                {
                    return false;
                }
                if (_data.IndexOf(XRefTableParser.xref) != 0)
                {
                    throw new ApplicationException("xref not found at start of xreftable");
                }
                stage = XRefParseStage.Records;
                CurrentPosition += 4;

                if (!SkipWhiteSpace())
                {
                    return false;
                }

                if (!ParseHeader())
                {
                    return false;
                }
            }

            if (stage == XRefParseStage.Records)
            {
                if (currentObject == endObject)
                {
                    if (!SkipWhiteSpace())
                    {
                        return false;
                    }
                    if (IsAtTrailer())
                    {
                        IsFinished = true;
                        stage = XRefParseStage.Finished;
                        return false;
                    }
                    else if (!ParseHeader()) // new section
                    {
                        return false;
                    }
                }

                if (!SkipWhiteSpace())
                {
                    return false;
                }

                ParseRecord();
                return true;
            }

            return false;
        }

        private bool IsAtTrailer()
        {
            return _data.Slice(CurrentPosition).IndexOf(XRefTableParser.trailer) == 0;
        }

        private bool ParseHeader()
        {
            var eoc = _data.Slice(CurrentPosition).IndexOfAny(CommonUtil.whiteSpaces);
            if (eoc == -1)
            {
                return false;
            }

            Encoding.ASCII.GetChars(_data.Slice(CurrentPosition, eoc), _buffer);
            currentObject = uint.Parse(_buffer.Slice(0, eoc));
            CurrentPosition += eoc;

            if (!SkipWhiteSpace())
            {
                return false;
            }

            eoc = _data.Slice(CurrentPosition).IndexOfAny(CommonUtil.whiteSpaces);
            if (eoc == -1)
            {
                return false;
            }

            Encoding.ASCII.GetChars(_data.Slice(CurrentPosition, eoc), _buffer);
            endObject = currentObject + uint.Parse(_buffer.Slice(0, eoc));
            CurrentPosition += eoc;
            return true;
        }

        private void ParseRecord()
        {
            if (!CommonUtil.IsWhiteSpace(_data[CurrentPosition + 10]) || !CommonUtil.IsWhiteSpace(_data[CurrentPosition + 16]))
            {
                throw new ApplicationException("Invalid XREF entry.");
            };
            Encoding.ASCII.GetChars(_data.Slice(CurrentPosition, 10), _buffer);
            xrefOffset = UInt64.Parse(_buffer.Slice(0, 10));
            Encoding.ASCII.GetChars(_data.Slice(CurrentPosition + 11, 5), _buffer);
            if (!uint.TryParse(_buffer.Slice(0, 5), out xrefGen))
            {
                throw new ApplicationException($"Xref gen not parsable for offset {xrefOffset}: " + _buffer.Slice(0, 5).ToString());
            }
            // xrefGen = uint.Parse(_buffer.Slice(0, 5));
            xrefIsFree = _data[CurrentPosition + 17] == 'f';
            xrefObjNum = currentObject;
            currentObject++;
            CurrentPosition += 20;
        }

        private bool SkipWhiteSpace()
        {
            for (var i = CurrentPosition; i < _data.Length; i++)
            {
                if (!CommonUtil.IsWhiteSpace(_data[i]))
                {
                    CurrentPosition = i;
                    return true;
                }
            }
            return false;
        }
    }
    public class XRefTableParser
    {
        internal static byte[] xref = new byte[4] { (byte)'x', (byte)'r', (byte)'e', (byte)'f' };
        internal static byte[] trailer = new byte[7] { (byte)'t', (byte)'r', (byte)'a', (byte)'i', (byte)'l', (byte)'e', (byte)'r' };
        internal static byte[] newline = new byte[2] { (byte)'\r', (byte)'\n' };
        internal static byte[] prevEntry = new byte[5] { (byte)'/', (byte)'P', (byte)'r', (byte)'e', (byte)'v' };
        public static Dictionary<int, XRefEntry> GetEntries(ReadOnlySpan<byte> bytes, ulong xrefOffset, out PdfDictionary trailer)
        {
            throw new NotSupportedException();
            trailer = null;
            var tableOffset = (int)xrefOffset;
            var tableStart = bytes.Slice(tableOffset);
            var entries = new Dictionary<int, XRefEntry>();
            var hasTable = true;
            while (hasTable)
            {
                var reader = new XRefTableReader(tableStart);
                while (reader.Read())
                {
                    var entry = reader.GetCurrentEntry();
                    entries[(int)entry.ObjectNumber] = entry;
                }
                if (!reader.IsFinished)
                {
                    throw new ApplicationException("Ahhh");
                }

                // var thisTrailer = null; NestedParser.ParseLazyDictionary(bytes, tableOffset + reader.CurrentPosition + 7); //7 for trailer
                // if (thisTrailer.Values.ContainsKey("/Prev"))
                // {
                //     var value = thisTrailer.Values["/Prev"];
                //     if (value.Type != PdfObjectType.NumericObj)
                //     {
                //         throw new ApplicationException("/Prev entry is non-numeric.");
                //     }
                //     // can optimize
                //     var stringOffset = Encoding.ASCII.GetString(bytes.Slice(value.Offset, value.Length));
                //     var offset = ulong.Parse(stringOffset);
                //     tableStart = bytes.Slice((int)offset);
                //     tableOffset = (int)offset;
                //     hasTable = true;
                // }
                // else
                // {
                //     hasTable = false;
                // }
                // trailer = MergeTrailers(trailer, thisTrailer);
            }
            return entries;
        }

        // private static PdfLazyDictionary MergeTrailers(PdfLazyDictionary newer, PdfLazyDictionary current)
        // {
        //     if (newer == null) // first go round there is no newer
        //     {
        //         return current;
        //     }
        //     foreach (var item in current.Values)
        //     {
        //         if (!newer.Values.ContainsKey(item.Key))
        //         {
        //             newer.Values[item.Key] = item.Value;
        //         }
        //     }
        //     return newer;
        // }

        private static int SeekToObj(ReadOnlySpan<byte> bytes)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                if (!CommonUtil.IsWhiteSpace(bytes[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
    public class XRefEntry
    {
        public int ObjectNumber { get; set; }
        public long Offset { get; set; }
        public int Generation { get; set; }
        public bool IsFree { get; set; }
    }
}