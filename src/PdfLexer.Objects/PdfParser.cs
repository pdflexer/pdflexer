using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfLexer.Objects.Parsers;

namespace PdfLexer.Objects
{
    public class PdfParser
    {
        public IPdfObject GetPdfObject(IPdfDataSource source, long startPosition)
        {
            source.FillData(startPosition, 250, out var data);
            var start = PdfTokenizer.ReadNextToken(data, out var tokenType, 0, out var length);
            // TODO fill more data
            long startObj = startPosition + start;

            PdfObjectType objType = PdfObjectType.NullObj;
            IParsedPdfObject parsed = null;
            switch (tokenType)
            {
                case PdfTokenType.ArrayStart:
                case PdfTokenType.DictionaryStart:
                    break;
                case PdfTokenType.BooleanObj:
                    objType = PdfObjectType.BooleanObj;
                    BoolParser.GetBool(data, out bool result, out int used);
                    length = used;
                    break;
                case PdfTokenType.NameObj:
                    objType = PdfObjectType.NameObj;
                    length = NameParser.CountNameBytes(data);
                    break;
                case PdfTokenType.NullObj:
                    objType = PdfObjectType.NullObj;
                    length = NullParser.nullbytes.Length;
                    break;
                case PdfTokenType.NumericObj:
                    // can be indirect, need multiple tokens
                    var secondStart = PdfTokenizer.ReadNextToken(data, out var secondType, length + start, out var secondLength);
                    // TODO fill more data
                    if (secondType != PdfTokenType.NumericObj)
                    {
                        objType = PdfObjectType.NumericObj;
                        break;
                    }
                    var third = PdfTokenizer.ReadNextToken(data, out var thirdType, secondStart + secondLength, out var _);
                    // TODO fill more data
                    if (thirdType == PdfTokenType.IndirectRef)
                    {
                        objType = PdfObjectType.IndirectRefObj;
                        length = third + 1 - start;
                    }
                    break;
                case PdfTokenType.StringObj:
                    StringParser.GetString(data, out var stringData);
                    length = stringData.Length;
                    break;
                default:
                    throw new NotImplementedException();
            }
            var obj = source.RegisterObject(startObj, length, objType, true);
            obj.Parsed = parsed;
            return obj;
        }

        private static IParsedPdfObject GetNestedObject(IPdfDataSource source, long startPosition, out int skipped,
            out int length)
        {
            skipped = 0;
            length = 0;
            var nested = new NestedTokenizer(source, startPosition);
            var items = new List<KeyValuePair<string, IPdfObject>>();
            while (nested.Read())
            {
                var type = nested.ObjectType;
                var info = nested.GetCurrentInfo();
                var used = source.FillData(info.StartPos, info.Length, out var nameData);
                var key = NameParser.ParseName(nameData);
                nested.Read();
                type = nested.ObjectType;
                info = nested.GetCurrentInfo();
                var obj = source.RegisterObject(info.StartPos, info.Length, type, false);
                items.Add(new KeyValuePair<string, IPdfObject>(key, obj));
            }

            var dictInfo = nested.GetTotalInfo();

            var read = source.FillData(dictInfo.StartPos + dictInfo.Length, 6, out var stream);
            if (read == 6 && stream.SequenceEqual(IndirectSequences.stream))
            {
                // stream
                throw new NotImplementedException();
            }
            else
            {
                var wrapper = source.RegisterObject(dictInfo.StartPos, dictInfo.Length, PdfObjectType.DictionaryObj, false);
                var dict = new PdfDictionary(wrapper, items);
                return dict;
            }

            return null;
        }
    }
}
