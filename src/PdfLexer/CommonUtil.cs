
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using PdfLexer.Parsers;

[assembly: InternalsVisibleTo("PdfLexer.Tests")]
[assembly: InternalsVisibleTo("PdfLexer.Benchmarks")]

namespace PdfLexer
{

    public class CommonUtil
    {
        internal static byte[] Terminators = new byte[16] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20,
            (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };

        internal static byte[] WhiteSpaces = new byte[6] { 0x00, 0x09, 0x0A, 0x0C, 0x0D, 0x20 };

        internal static byte[] EOLs = new byte[2] { (byte)'\r', (byte)'n' };

        // internal static byte[] numeric = new byte[13] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6',
        // (byte)'7', (byte)'8', (byte)'9', (byte)'.', (byte)'-', (byte)'+'};
        internal static byte[] ints = new byte[12] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6',
         (byte)'7', (byte)'8', (byte)'9', (byte)'-', (byte)'+'};
        // special characters that allow us to stop scanning and confirm the token is a numeric
        // internal static byte[] numTers = new byte[11] { (byte)'.',
        //     (byte)'(', (byte)')', (byte)'<', (byte)'>', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'/', (byte)'%' };
        public static bool IsWhiteSpace(ReadOnlySpan<char> chars, int location)
        {
            var c = chars[location];
            return c == 0x00
                   || c == 0x09
                   || c == 0x0A
                   || c == 0x0C
                   || c == 0x0D
                   || c == 0x20;
        }
        public static bool IsWhiteSpace(byte b)
        {
            return b == 0x00
                   || b == 0x09
                   || b == 0x0A
                   || b == 0x0C
                   || b == 0x0D
                   || b == 0x20;
        }

        public static bool IsNonBinary(byte b)
        {
            return (b < 123 && b > 31)
                   || b == 0x00
                   || b == 0x00
                   || b == 0x0A
                   || b == 0x0C
                   || b == 0x0D
                   || b == 0x20;
        }

        /// <summary>
        /// Enumerates the PDF page tree.
        /// </summary>
        /// <param name="dict">Pdf catalog.</param>
        /// <returns>Page dictionaries.</returns>
        public static IEnumerable<PdfDictionary> EnumeratePageTree(PdfDictionary dict) => EnumeratePages(dict, null, null, null, null, new HashSet<object>());
        internal static IEnumerable<PdfDictionary> EnumeratePages(PdfDictionary dict, PdfDictionary resources, PdfArray mediabox, PdfArray cropbox, PdfNumber rotate, HashSet<object> read)
        {
            // Inheritible items if not provided in page dict:
            // Resources required (dictionary)
            // MediaBox required (rectangle)
            // CropBox => default to MediaBox (rectangle)
            // Rotate (integer)
            read.Add(dict);
            var type = dict.GetRequiredValue<PdfName>(PdfName.TypeName);
            switch (type.Value)
            {
                case "/Pages":
                    if (dict.TryGetValue<PdfDictionary>(PdfName.Resources, out var next))
                    {
                        resources = next;
                    }
                    if (dict.TryGetValue<PdfArray>(PdfName.MediaBox, out var thisMediaBox))
                    {
                        mediabox = thisMediaBox;
                    }
                    if (dict.TryGetValue<PdfArray>(PdfName.CropBox, out var thisCropBox))
                    {
                        cropbox = thisCropBox;
                    }
                    if (dict.TryGetValue<PdfNumber>(PdfName.Rotate, out var thisRotate))
                    {
                        rotate = thisRotate;
                    }


                    var kids = dict.GetRequiredValue<PdfArray>(PdfName.Kids);
                    foreach (var child in kids)
                    {
                        var instance = child.GetValue<PdfDictionary>();
                        if (read.Contains(instance))
                        {
                            // TODO warn
                            continue;
                        }
                        
                        foreach (var pg in EnumeratePages(instance, resources, mediabox, cropbox, rotate, read)) 
                        {
                            yield return pg;
                        }
                    }
                    break;
                case "/Page":
                    if (!dict.ContainsKey(PdfName.Resources) && resources != null)
                    {
                        dict[PdfName.Resources] = resources;
                    }
                    if (!dict.ContainsKey(PdfName.MediaBox) && mediabox != null)
                    {
                        dict[PdfName.MediaBox] = mediabox;
                    }
                    if (!dict.ContainsKey(PdfName.CropBox) && cropbox != null)
                    {
                        dict[PdfName.CropBox] = cropbox;
                    }
                    if (!dict.ContainsKey(PdfName.Rotate) && rotate != null)
                    {
                        dict[PdfName.Rotate] = rotate;
                    }
                    yield return dict;
                    break;
            }
        }

        internal static Exception DisplayDataErrorException(ReadOnlySpan<byte> data, int i, string prefixInfo)
        {
            var count = data.Length > i + 25 ? 25 : data.Length - i;
            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(data.Slice(i, count)) + "'");
        }

        internal static Exception DisplayDataErrorException(ref SequenceReader<byte> reader, string prefixInfo)
        {
            var count = reader.Remaining > 25 ? 25 : reader.Remaining;
            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(reader.Sequence.Slice(reader.Position, count).ToArray()) + "'");
        }

        internal static Exception DisplayDataErrorException(ReadOnlySequence<byte> sequence, SequencePosition position, string prefixInfo)
        {
            var count = sequence.Length > 25 ? 25 : sequence.Length;

            return new ApplicationException(prefixInfo + ": '" + Encoding.ASCII.GetString(sequence.Slice(position, count).ToArray()) + "'");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SkipWhiteSpace(ReadOnlySpan<byte> bytes, ref int i)
        {
            ReadOnlySpan<byte> local = bytes;
            for (; i < local.Length; i++)
            {
                byte b = local[i];
                if (b == 0x00 ||
                    b == 0x09 ||
                    b == 0x0A ||
                    b == 0x0C ||
                    b == 0x0D ||
                    b == 0x20)
                {
                    continue;
                }

                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SkipWhiteSpaceArray(ReadOnlySpan<byte> bytes, ref int i)
        {
            ReadOnlySpan<byte> local = bytes;
            ReadOnlySpan<byte> whitespaces = WhiteSpaces;
            for (; i < local.Length; i++)
            {
                if (whitespaces.IndexOf(local[i]) > -1)
                {
                    continue;
                }

                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ScanTokenEnd(ReadOnlySpan<byte> bytes, ref int pos)
        {
            ReadOnlySpan<byte> local = bytes;
            for (; pos < local.Length; pos++)
            {
                // ugly but benched better than Span IndexOf / IndexOfAny alternatives
                var b = local[pos];
                if (
                    b == 0x00 || b == 0x09 || b == 0x0A || b == 0x0C || b == 0x0D || b == 0x20
                    || b == (byte)'(' || b == (byte)')' || b == (byte)'<' || b == (byte)'>'
                    || b == (byte)'[' || b == (byte)']' || b == (byte)'{' || b == (byte)'}'
                    || b == (byte)'/' || b == (byte)'%')
                {
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SkipWhiteSpaces(ReadOnlySpan<byte> bytes, int location)
        {
            ReadOnlySpan<byte> localBuffer = bytes;
            for (var i = location; i < bytes.Length; i++)
            {
                byte val = localBuffer[i];
                if (val != 0x00 &&
                    val != 0x09 &&
                    val != 0x0A &&
                    val != 0x0C &&
                    val != 0x0D &&
                    val != 0x20)
                {
                    return i;
                }
            }
            return -1;
        }

        public static PdfObjectType GetEnumType<T>() where T : IPdfObject
        {
            var type = typeof(T);
            if (type == typeof(PdfNumber))
            {
                return PdfObjectType.NumericObj;
            }
            else if (type == typeof(PdfDictionary))
            {
                return PdfObjectType.DictionaryObj;
            }
            else if (type == typeof(PdfArray))
            {
                return PdfObjectType.ArrayObj;
            }
            throw new NotImplementedException("EnumType not implemented for " + typeof(T).Name);
        }
    }
}