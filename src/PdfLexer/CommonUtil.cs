using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using PdfLexer.DOM;

namespace PdfLexer;

internal static class CommonUtil
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

    public static void FillArray(this Stream stream, byte[] array, int requiredBytes = -1)
    {
        if (!TryFillArray(stream, array, requiredBytes))
        {
            throw new PdfLexerException("unable to fill array from stream");
        }
    }

    public static int Fill(this Stream stream, byte[] array)
    {
        var l = array.Length;
        int total = 0;
        int read;
        while ((read = stream.Read(array, total, l - total)) > 0)
        {
            total += read;
        }
        return total;
    }

    public static bool TryFillArray(this Stream stream, byte[] array, int requiredBytes = -1)
    {
        if (requiredBytes < 0) { requiredBytes = array.Length; }
        int total = 0;
        int read;
        while ((read = stream.Read(array, total, requiredBytes - total)) > 0)
        {
            total += read;
        }
        if (total != requiredBytes)
        {
            // for (var i=total; i < requiredBytes; i++)
            // {
            //     array[i] = 0;
            // }
            return false;
        }
        return true;
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

    public static PdfName? GetFirstFilter(PdfDictionary streamDict)
    {
        PdfName? filterName = null;
        var filter = streamDict.Get(PdfName.Filter);
        switch (filter)
        {
            case PdfArray arr:
                filterName = arr.FirstOrDefault()?.GetValue<PdfName>();
                break;
            case PdfName nm:
                filterName = nm;
                break;
        }
        return filterName;
    }

    public static PdfName? GetFirstFilterFromList(IPdfObject? filters)
    {
        if (filters == null) { return null; }
        PdfName? filterName = null;
        switch (filters)
        {
            case PdfArray arr:
                filterName = arr.FirstOrDefault()?.GetValue<PdfName>();
                break;
            case PdfName nm:
                filterName = nm;
                break;
        }
        return filterName;
    }

    public static PdfPage RecursePage(PdfPage obj)
    {
        obj = obj.NativeObject.CloneShallow();
        obj.Parent = null;
        RecursiveLoad(obj.NativeObject);
        return obj;
    }
    public static void RecursiveLoad(IPdfObject obj) => RecursiveLoad(obj, new HashSet<object>());
    internal static void RecursiveLoad(IPdfObject obj, HashSet<object> refStack)
    {
        if (obj is PdfIndirectRef ir)
        {
            obj = ir.GetObject();
        }

        if (refStack.Contains(obj))
        {
            return;
        }

        switch (obj)
        {
            case PdfLazyObject lz:
                obj = lz.Resolve();
                RecursiveLoad(obj, refStack);
                break;
            case PdfStream str:
                {
                    refStack.Add(obj);
                    RecursiveLoad(str.Dictionary, refStack);
                    var ms = new MemoryStream();
                    using var data = str.Contents.GetEncodedData();
                    data.CopyTo(ms);
                    var f = str.Contents.Filters;
                    var fp = str.Contents.DecodeParams;
                    str.Contents = new PdfByteArrayStreamContents(ms.ToArray());
                    str.Contents.Filters = f;
                    str.Contents.DecodeParams = fp;
                    break;
                }
            case PdfDictionary dict:
                refStack.Add(obj);
                dict.TryGetValue<PdfName>(PdfName.TypeName, out var type, false);
                foreach (var (k, v) in dict)
                {
                    if (type == PdfName.Page && k == PdfName.Parent)
                    {
                        // continue;
                    }
                    RecursiveLoad(v, refStack);
                }
                break;
            case PdfArray array:
                refStack.Add(obj);
                foreach (var v in array)
                {
                    RecursiveLoad(v, refStack);
                }
                break;
            default:
                break;
        }
    }


    internal static void Recurse(IPdfObject obj, HashSet<PdfIndirectRef> refStack, Action<IPdfObject, PdfIndirectRef?> pre, Action<IPdfObject, PdfIndirectRef?> post, bool ordered = false, PdfIndirectRef? irRef = null)
    {
        if (refStack.Contains(obj))
        {
            return;
        }

        pre(obj, irRef);

        switch (obj)
        {
            case PdfIndirectRef ir:
                refStack.Add(ir);
                obj = ir.GetObject();
                Recurse(obj, refStack, pre, post, ordered, ir);
                return;
            case PdfLazyObject lz:
                obj = lz.Resolve();
                Recurse(obj, refStack, pre, post, ordered);
                break;
            case PdfStream str:
                DoDict(str.Dictionary);
                break;
            case PdfDictionary dict:
                DoDict(dict);
                break;
            case PdfArray array:
                foreach (var v in array)
                {
                    Recurse(v, refStack, pre, post, ordered);
                }
                break;
            default:
                break;
        }

        post(obj, irRef);

        void DoDict(PdfDictionary d)
        {
            if (ordered)
            {
                foreach (var k in d.Keys.OrderBy(x => x.Value))
                {
                    Recurse(d[k], refStack, pre, post, ordered);
                }

            }
            else
            {
                foreach (var v in d.Values)
                {
                    Recurse(v, refStack, pre, post, ordered);
                }

            }
        }
    }

    internal static void Recurse(IPdfObject obj, HashSet<PdfIndirectRef> refStack, Func<IPdfObject, bool> skip, Action<IPdfObject, PdfIndirectRef?> objWork, bool ordered = false, PdfIndirectRef? irRef = null)
    {
        if (refStack.Contains(obj) || skip(obj))
        {
            return;
        }

        switch (obj)
        {
            case PdfIndirectRef ir:
                refStack.Add(ir);
                obj = ir.GetObject();
                Recurse(obj, refStack, skip, objWork, ordered, ir);
                return;
            case PdfLazyObject lz:
                obj = lz.Resolve();
                Recurse(obj, refStack, skip, objWork, ordered);
                break;
            case PdfStream str:
                DoDict(str.Dictionary);
                break;
            case PdfDictionary dict:
                DoDict(dict);
                break;
            case PdfArray array:
                foreach (var v in array)
                {
                    Recurse(v, refStack, skip, objWork, ordered);
                }
                break;
            default:
                break;
        }

        objWork(obj, irRef);

        void DoDict(PdfDictionary d)
        {
            if (ordered)
            {
                foreach (var k in d.Keys.OrderBy(x => x.Value))
                {
                    Recurse(d[k], refStack, skip, objWork, ordered);
                }

            }
            else
            {
                foreach (var v in d.Values)
                {
                    Recurse(v, refStack, skip, objWork, ordered);
                }

            }
        }
    }

    /// <summary>
    /// Enumerates the PDF page tree.
    /// </summary>
    /// <param name="dict">Pdf catalog.</param>
    /// <returns>Page dictionaries.</returns>
    public static IEnumerable<PdfPage> EnumeratePageTree(PdfDictionary dict) => EnumeratePages(dict, null, null, null, null, null, new HashSet<object>());
    internal static IEnumerable<PdfPage> EnumeratePages(PdfDictionary dict,
        PdfDictionary? resources, PdfArray? mediabox, PdfArray? cropbox, PdfNumber? rotate, ExistingIndirectRef? irf,
        HashSet<object> read)
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
                    var ir = child as ExistingIndirectRef;
                    var pg = child.Resolve();
                    if (pg.Type != PdfObjectType.DictionaryObj)
                    {
                        // TODO warn
                        continue;
                    }
                    var instance = pg.GetValue<PdfDictionary>();
                    if (read.Contains(instance))
                    {
                        // TODO warn
                        continue;
                    }

                    foreach (var npg in EnumeratePages(instance, resources, mediabox, cropbox, rotate, ir, read))
                    {
                        yield return npg;
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
                var pgg = irf == null ? new PdfPage(dict) : new PdfPage(dict, irf);
                yield return pgg;
                break;
        }
    }

    internal static Exception DisplayDataErrorException(ReadOnlySpan<byte> data, int i, string prefixInfo)
    {
        var count = data.Length > i + 25 ? 25 : data.Length - i;
        return new PdfLexerException(prefixInfo + ": '" + Encoding.ASCII.GetString(data.Slice(i, count)) + "'");
    }

    internal static string DisplayDataError(ReadOnlySpan<byte> data, int i, string prefixInfo)
    {
        var count = data.Length > i + 25 ? 25 : data.Length - i;
        return prefixInfo + ": '" + Encoding.ASCII.GetString(data.Slice(i, count)) + "'";
    }

    internal static string GetDataErrorInfo(ReadOnlySpan<byte> data, int i)
    {
        if (i + 1 > data.Length || i < 0)
        {
            i = Math.Max(data.Length - 10, 0);
        }
        var count = data.Length > i + 25 ? 25 : data.Length - i;
        return Encoding.ASCII.GetString(data.Slice(i, count));
    }

    internal static Exception DisplayDataErrorException(ref SequenceReader<byte> reader, string prefixInfo)
    {
        var count = reader.Remaining > 25 ? 25 : reader.Remaining;
        return new PdfLexerException(prefixInfo + ": '" + Encoding.ASCII.GetString(reader.Sequence.Slice(reader.Position, count).ToArray()) + "'");
    }

    internal static string GetDataErrorInfo(ref SequenceReader<byte> reader)
    {
        var count = reader.Remaining > 25 ? 25 : reader.Remaining;
        return Encoding.ASCII.GetString(reader.Sequence.Slice(reader.Position, count).ToArray());
    }

    internal static Exception DisplayDataErrorException(ReadOnlySequence<byte> sequence, SequencePosition position, string prefixInfo)
    {
        var count = sequence.Length > 25 ? 25 : sequence.Length;

        return new PdfLexerException(prefixInfo + ": '" + Encoding.ASCII.GetString(sequence.Slice(position, count).ToArray()) + "'");
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
            // TODO rebench with extra number chars
            var b = local[pos];
            if (
                b == 0x00 || b == 0x09 || b == 0x0A || b == 0x0C || b == 0x0D || b == 0x20
                || b == (byte)'(' || b == (byte)')' || b == (byte)'<' || b == (byte)'>'
                || b == (byte)'[' || b == (byte)']' || b == (byte)'{' || b == (byte)'}'
                || b == (byte)'/' || b == (byte)'%'
                || (b > 47 && b < 58) // number start
                || b == (byte)'+' || b == (byte)'-' || b == (byte)'.'
                )
            {
                return;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEndOfToken(byte b)
    {
        // ugly but benched better than Span IndexOf / IndexOfAny alternatives
        // TODO rebench with extra number chars
        if (
            b == 0x00 || b == 0x09 || b == 0x0A || b == 0x0C || b == 0x0D || b == 0x20
            || b == (byte)'(' || b == (byte)')' || b == (byte)'<' || b == (byte)'>'
            || b == (byte)'[' || b == (byte)']' || b == (byte)'{' || b == (byte)'}'
            || b == (byte)'/' || b == (byte)'%'
            || (b > 47 && b < 58) // number start
            || b == (byte)'+' || b == (byte)'-' || b == (byte)'.')
        {
            return true;
        }
        return false;
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
