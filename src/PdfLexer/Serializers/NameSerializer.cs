using System.Buffers;
using System.Text;

namespace PdfLexer.Serializers;

internal class NameSerializer : ISerializer<PdfName>
{
    private static Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1"); // StandardEncoding
    public void WriteToStream(PdfName obj, Stream stream)
    {
        if (obj.Value.Length == 1) { stream.Write(Iso88591.GetBytes("/Empty")); return; }
        if (obj.Value.Length < 50)
        {
            Span<byte> bytes = stackalloc byte[obj.Value.Length * 3];
            var written = GetBytes(obj, bytes);
            stream.Write(bytes.Slice(0, written));
        }
        else
        {
            var buffer = ArrayPool<byte>.Shared.Rent(obj.Value.Length * 3);
            var written = GetBytes(obj, buffer);
            stream.Write(buffer, 0, written);
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return;
    }

    public int GetBytes(PdfName obj, Span<byte> data)
    {
        if (obj.CacheValue > 0)
        {
            return WriteCached(obj, data);
        }

        if (obj.Value.Length == 1) { Iso88591.GetBytes("/Empty").CopyTo(data); return 6; }

        if (data.Length < 150)
        {
            Span<char> chars = stackalloc char[data.Length];
            var count = ReplaceChars(obj, chars);
            return Iso88591.GetBytes(chars.Slice(0, count), data);
        } else
        {
            var array = ArrayPool<char>.Shared.Rent(data.Length);
            Span<char> buffer = array;
            var count = ReplaceChars(obj, buffer);
            var result = Iso88591.GetBytes(buffer.Slice(0, count), data);
            ArrayPool<char>.Shared.Return(array);
            return result;
        }
    }

    private int ReplaceChars(PdfName obj, Span<char> data)
    {
        data[0] = '/';
        var ci = 1; // TODO perf analysis
        for (var i = 1; i < obj.Value.Length; i++)
        {
            var cc = obj.Value[i];
            if (cc == (char)0 || cc == (char)9 || cc == (char)10 || cc == (char)12
                || cc == (char)13 || cc == (char)32 || cc == '(' || cc == ')' || cc == '<'
                || cc == '>' || cc == '[' || cc == ']' || cc == '{' || cc == '}' || cc == '/'
                || cc == '%' || cc == '#')
            {
                data[ci++] = '#';
                var hex = ((int)cc).ToString("X2");
                data[ci++] = hex[0];
                data[ci++] = hex[1];
            }
            else
            {
                data[ci++] = cc;
            }
        }

        return ci;
    }

    private int WriteCached(PdfName obj, Span<byte> data)
    {
        data[0] = (byte)'/';
        ulong val = obj.CacheValue;
        for (var i = 1; i < 9; i++)
        {
            var cv = (byte)(val & 0xFF);
            if (cv == 0)
            {
                return i;
            }
            data[i] = cv;
            val = val >> 8;
        }
        return 9;
    }

    private static char[] needsEscaping = new char[]
    {
        (char) 0, (char) 9, (char) 10, (char) 12, (char) 13, (char) 32,
        '(', ')', '<', '>', '[', ']', '{', '}', '/', '%', '#'
    };
}
