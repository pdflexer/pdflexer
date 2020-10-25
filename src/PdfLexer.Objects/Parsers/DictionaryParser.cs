using System;
using System.Text;
using PdfLexer.Objects.Lazy;
using PdfLexer.Objects.Nested;

namespace PdfLexer.Objects.Parsers
{
    public class DictionaryParser
    {
        public static PdfLazyDictionary ParseLazyDictionary(ReadOnlySpan<byte> data, int offset)
        {
            var dict = new PdfLazyDictionary();
            var dictReader = new NestedParser(data.Slice(offset));
            while (dictReader.Read())
            {
                if (dictReader.ObjectType == PdfObjectType.NameObj)
                {
                    var name = Encoding.ASCII.GetString(dictReader.GetCurrentTokenSpan());
                    var valueRead = dictReader.Read();
                    if (!valueRead)
                    {
                        throw new ApplicationException("Dictionary missing value for key " + name);
                    }
                    dict.Values[name] = new PdfLazyObject(dictReader.ObjectType, offset + dictReader.currentStart, dictReader.currentLength);
                }
            }

            if (!dictReader.Completed())
            {
                throw new ApplicationException("Dictionary was not complete");
            }
            var length = dictReader.GetFullLength();
            if (data.Slice(offset + length).SequenceEqual(IndirectSequences.stream))
            {
                dict.HasStream = true;
                dict.StreamOffset = (ulong)(offset + length);
            }
            return dict;
        }

        public static bool GetDict(ReadOnlySpan<byte> bytes, out ReadOnlySpan<byte> dictBytes)
        {
            var parser = new NestedParser(bytes);
            while (parser.Read())
            {

            }
            if (parser.Completed())
            {
                dictBytes = parser.GetFullSpan();
                return true;
            }
            dictBytes = null;
            return false;
            //return Utils.GetDictOrArray(bytes, out dictBytes);
        }

        public static int CountDictBytes(ReadOnlySpan<byte> bytes)
        {
            return NestedUtils.CountNestedBytes(bytes);
        }
    }
}