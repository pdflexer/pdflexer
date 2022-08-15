using System.Text;

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}

namespace PdfLexer.CMaps
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Keeping pdfjs naming")]
    internal class BCMapReader
    {
        internal enum ToUnicodeState
        {
            None,
            ReadChars,
            ReadRange,
            ReadSpace
        }

        
        private static void incHex(byte[] a, int size)
        {
            var c = 1;
            for (var i = size; i >= 0 && c > 0; i--)
            {
                c += a[i];
                a[i] = (byte)(c & 255);
                c >>= 8;
            }
        }
        private static void addHex(byte[] a, byte[] b, int size)
        {
            var c = 0;
            for (var i = size; i >= 0; i--)
            {
                c += a[i] + b[i];
                a[i] = (byte)(c & 255);
                c >>= 8;
            }
        }

        private static void readHex(ReadOnlySpan<byte> data, ref int i, byte[] num, int size)
        {
            data.Slice(i, size + 1).CopyTo(num);
            i += size + 1;
        }

        private static void readHexNumber(ReadOnlySpan<byte> data, ref int i, byte[] tmp, byte[] num, int size)
        {
            bool last;
            var sp = 0;
            do
            {
                var b = data[i++];
                last = (b & 0x80) == 0;
                tmp[sp++] = (byte)(b & 0x7f);
            } while (!last);
            var k = size;
            var buffer = 0;
            var bufferSize = 0;
            while (k >= 0)
            {
                while (bufferSize < 8 && tmp.Length > 0)
                {
                    var loc = --sp;
                    if (loc > -1)
                    {
                        buffer |= tmp[loc] << bufferSize;
                    }
                    
                    bufferSize += 7;
                }
                num[k] = (byte)(buffer & 255);
                k--;
                buffer >>= 8;
                bufferSize -= 8;
            }
        }

        private static int readSigned(ReadOnlySpan<byte> data, ref int i)
        {
            var n = readNumber(data, ref i);
            return (n & 1) != 0 ? (int)~(n >> 1) : (int)(n >> 1);
        }

        private static uint hexToInt(byte[] a, int size)
        {
            uint n = 0;
            for (var i = 0; i <= size; i++)
            {
                n = (n << 8) | a[i];
            }
            return n >> 0;
        }

        private static void readHexSigned(ReadOnlySpan<byte> data, ref int i, byte[] tmp, byte[] num, int size)
        {
            readHexNumber(data, ref i, tmp, num, size);
            var sign = (num[size] & 1) != 0 ? 255 : 0;
            var c = 0;
            for (var k = 0; k <= size; k++)
            {
                c = ((c & 1) << 8) | num[k];
                num[k] = (byte)((c >> 1) ^ sign);
            }
        }

        private static CResult hexToStr(byte[] a, int size)
        {
            ReadOnlySpan<byte> data = a;
            if (size == 1)
            {
                return new CResult { Code = hexToInt(a, size) };
            } else
            {
                return new CResult { MultiChar = UnicodeEncoding.BigEndianUnicode.GetString(data.Slice(0, size+1)) };
            }
        }



        private const int MAX_NUM_SIZE = 16;
        private const int MAX_ENCODED_NUM_SIZE = 19; // ceil(MAX_NUM_SIZE * 7 / 8)
        public static (List<CRange> Ranges, Dictionary<uint, CResult> Mapping) GetGlyphsFromToUnicode(ReadOnlySpan<byte> data)
        {
            var ranges = new List<CRange>();
            var mapping = new Dictionary<uint, CResult>();

            string useCMap;

            var start = new byte[MAX_NUM_SIZE];
            var end = new byte[MAX_NUM_SIZE];
            var c = new byte[MAX_NUM_SIZE];
            var charCode = new byte[MAX_NUM_SIZE];
            var tmp = new byte[MAX_NUM_SIZE];
            var tmpc = new byte[MAX_NUM_SIZE];

            var header = data[0];
            var vertical = (header & 1) != 0;

            int i = 1;
            while (i < data.Length)
            {
                var b = data[i++];
                var type = b >> 5;
                if (type == 7)
                {
                    // metadata, e.g. comment or usecmap
                    switch (b & 0x1f)
                    {
                        case 0:
                            readString(data, ref i); // skipping comment
                            break;
                        case 1:
                            useCMap = readString(data, ref i);
                            break;
                    }
                    continue;
                }
                var sequence = (b & 0x10) != 0;
                var dataSize = b & 15;

                if (dataSize + 1 > MAX_NUM_SIZE)
                {
                    throw new ApplicationException("BinaryCMapReader.process: Invalid dataSize.");
                }


                uint code;
                var ucs2DataSize = 1;
                var subitemsCount = readNumber(data, ref i);
                switch (type)
                {
                    case 0: // codespacerange
                        readHex(data, ref i, start, dataSize);
                        readHexNumber(data, ref i, tmp, end, dataSize);
                        addHex(end, start, dataSize);
                        ranges.Add(new CRange
                        {
                            Bytes = dataSize + 1,
                            Start = hexToInt(start, dataSize),
                            End = hexToInt(end, dataSize),
                        });
                        for (var k = 1; k < subitemsCount; k++)
                        {
                            incHex(end, dataSize);
                            readHexNumber(data, ref i, tmp, start, dataSize);
                            addHex(start, end, dataSize);
                            readHexNumber(data, ref i, tmp, end, dataSize);
                            addHex(end, start, dataSize);
                            ranges.Add(new CRange
                            {
                                Bytes = dataSize + 1,
                                Start = hexToInt(start, dataSize),
                                End = hexToInt(end, dataSize),
                            });
                        }
                        break;
                    case 1: // notdefrange
                        readHex(data, ref i, start, dataSize);
                        readHexNumber(data, ref i, tmp, end, dataSize);
                        addHex(end, start, dataSize);
                        readNumber(data, ref i); // code
                                                 // undefined range, skipping
                        for (var k = 1; k < subitemsCount; k++)
                        {
                            incHex(end, dataSize);
                            readHexNumber(data, ref i, tmp, start, dataSize);
                            addHex(start, end, dataSize);
                            readHexNumber(data, ref i, tmp, end, dataSize);
                            addHex(end, start, dataSize);
                            readNumber(data, ref i); // code
                                                     // nop
                        }
                        break;
                    case 2: // cidchar
                        readHex(data, ref i, c, dataSize);
                        code = readNumber(data, ref i);
                        mapping[hexToInt(c, dataSize)] = new CResult { Code = code };
                        for (var k = 1; k < subitemsCount; k++)
                        {
                            incHex(c, dataSize);
                            if (!sequence)
                            {
                                readHexNumber(data, ref i, tmp, tmpc, dataSize);
                                addHex(c, tmpc, dataSize);
                            }
                            code = (uint)(readSigned(data, ref i) + (code + 1));
                            mapping[hexToInt(c, dataSize)] = new CResult { Code = code };
                        }
                        break;
                    case 3: // cidrange
                        {
                            readHex(data, ref i, start, dataSize);
                            readHexNumber(data, ref i, tmp, end, dataSize);
                            addHex(end, start, dataSize);
                            code = readNumber(data, ref i);
                            var low = hexToInt(start, dataSize);
                            var high = hexToInt(end, dataSize);
                            while (low <= high)
                            {
                                mapping[low++] = new CResult { Code = code++ };
                            }
                            for (var k = 1; k < subitemsCount; k++)
                            {
                                incHex(end, dataSize);
                                if (!sequence)
                                {
                                    readHexNumber(data, ref i, tmp, start, dataSize);
                                    addHex(start, end, dataSize);
                                }
                                else
                                {
                                    end.CopyTo(start, 0);
                                }
                                readHexNumber(data, ref i, tmp, end, dataSize);
                                addHex(end, start, dataSize);
                                code = readNumber(data, ref i);
                                low = hexToInt(start, dataSize);
                                high = hexToInt(end, dataSize);
                                while (low <= high)
                                {
                                    mapping[low++] = new CResult { Code = code++ };
                                }
                            }
                            break;
                        }
                    case 4: // bfchar
                        {
                            readHex(data, ref i, c, ucs2DataSize);
                            readHex(data, ref i, charCode, dataSize);
                            var cp = hexToInt(c, ucs2DataSize);
                            mapping[cp] = hexToStr(charCode, dataSize);
                            for (var k = 1; k < subitemsCount; k++)
                            {
                                incHex(c, ucs2DataSize);
                                if (!sequence)
                                {
                                    readHexNumber(data, ref i, tmp, tmpc, ucs2DataSize);
                                    addHex(c, tmpc, ucs2DataSize);
                                }
                                incHex(charCode, dataSize);
                                readHexSigned(data, ref i, tmp, tmpc, dataSize);
                                addHex(charCode, tmpc, dataSize);

                                cp = hexToInt(c, ucs2DataSize);
                                mapping[cp] = hexToStr(charCode, dataSize);
                            }
                            break;
                        }
                    case 5: // bfrange
                        {
                            readHex(data, ref i, start, ucs2DataSize);
                            readHexNumber(data, ref i, tmp, end, ucs2DataSize);
                            addHex(end, start, ucs2DataSize);
                            readHex(data, ref i, charCode, dataSize);
                            var low = hexToInt(start, ucs2DataSize);
                            var high = hexToInt(end, ucs2DataSize);
                            var lb = dataSize - 1;
                            while (low <= high)
                            {
                                mapping[low++] = hexToStr(charCode, dataSize);
                                charCode[lb + 1] = (byte)(charCode[lb + 1] + 1);
                                // todo edge case
                            }
                            for (var k = 1; k < subitemsCount; k++)
                            {
                                incHex(end, ucs2DataSize);
                                if (!sequence)
                                {
                                    readHexNumber(data, ref i, tmp, start, ucs2DataSize);
                                    addHex(start, end, ucs2DataSize);
                                }
                                else
                                {
                                    end.CopyTo(start, 0);
                                }
                                readHexNumber(data, ref i, tmp, end, ucs2DataSize);
                                addHex(end, start, ucs2DataSize);
                                readHex(data, ref i, charCode, dataSize);
                                low = hexToInt(start, ucs2DataSize);
                                high = hexToInt(end, ucs2DataSize);
                                lb = dataSize - 1;
                                while (low <= high)
                                {
                                    mapping[low++] = hexToStr(charCode, dataSize);
                                    charCode[lb+1] = (byte)(charCode[lb+1] + 1);
                                    // todo edge case
                                }

                            }
                            break;
                        }
                    default:
                        throw new ApplicationException($"BinaryCMapReader.process - unknown type: ${ type }");
                }
            }

            return (ranges, mapping);
        }

        private static string readString(ReadOnlySpan<byte> data, ref int i)
        {
            var len = readNumber(data, ref i);
            var s = new StringBuilder();
            for (var c = 0; c < len; c++)
            {
                s.Append((char)readNumber(data, ref i));
            }
            return s.ToString();
        }

        private static uint readNumber(ReadOnlySpan<byte> data, ref int i)
        {
            uint n = 0;
            bool last;
            do
            {
                var b = data[i++];
                last = (b & 0x80) == 0;
                n = ((n << 7) | (uint)(b & 0x7f));
            } while (!last);
            return n;
        }
    }
}
