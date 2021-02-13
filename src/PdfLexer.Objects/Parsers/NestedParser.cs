using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PdfLexer.Objects.Lazy;

namespace PdfLexer.Objects.Parsers
{
    public enum NestedParseType
    {
        Unknown,
        Dictionary,
        Array
    }

    internal enum ParseState
    {
        Unknown,
        DictKey,
        DictValue,
        Array
    }
    public class NestedParser
    {
        public static byte[] startDict = new byte[2] {(byte)'<', (byte)'<'};
        public static byte[] endDict = new byte[2] {(byte)'>', (byte)'>'};
        private bool completed = false;

        public NestedParser(IPdfDataSource source)
        {
            _source = source;
        }
        private string currentKey = null;
        private List<long> stackStarts = new List<long>();
        private ParseState currentState = ParseState.Unknown;
        private IPdfDataSource _source;
        private NestedParseType currentType = NestedParseType.Unknown;
        private List<string> keys = new List<string>();
        private List<IParsedPdfObject> callStack = new List<IParsedPdfObject>();
        private List<KeyValuePair<string, IPdfObject>> items = new List<KeyValuePair<string, IPdfObject>>();
        public bool TryParseDict(ReadOnlySequence<byte> sequence, bool isCompleted)
        {
            var reader = new SequenceReader<byte>(sequence);

            while (reader.TryReadNextToken(isCompleted, out var tokenType, out var start, out var length))
            {
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.StringObj:
                        AddValue(tokenType, start, length);
                        break;
                    case PdfTokenType.DictionaryStart:
                        stackStarts.Add(start);
                        callStack.Add(new PdfDictionary());
                        currentState = ParseState.DictKey;
                        keys.Add(currentKey);
                        continue;
                    case PdfTokenType.ArrayStart:
                        stackStarts.Add(start);
                        callStack.Add(new PdfArray());
                        currentState = ParseState.Array;
                        keys.Add(currentKey);
                        break;
                    case PdfTokenType.NumericObj:
                        var current = reader.Consumed;
                        if (!reader.TryReadNextToken(isCompleted, out var secondType, out var secondStart, out var secondLength))
                        {
                            return false;
                        }

                        if (secondType != PdfTokenType.NumericObj)
                        {
                            reader.Rewind(reader.Consumed - current);
                            AddValue(tokenType, start, length);
                            continue;
                        }

                        if (!reader.TryReadNextToken(isCompleted, out var thirdType, out var thirdStart, out var thirdLength))
                        {
                            return false;
                        }
                        if (thirdType != PdfTokenType.IndirectRef)
                        {
                            reader.Rewind(reader.Consumed - current);
                            AddValue(tokenType, start, length);
                            continue;
                        }
                        AddValue(PdfTokenType.IndirectRef, start, (int)(reader.Consumed - start));
                        break;
                    case PdfTokenType.IndirectRef:
                        throw new ApplicationException("Unexpected indirect Ref token found.");
                    case PdfTokenType.DictionaryEnd:
                        Debug.Assert(callStack[^1] is PdfDictionary);
                        
                        if (callStack.Count == 1)
                        {
                            completed = true;
                            return false;
                        }
                        else
                        {
                            var thisStart = stackStarts[^1];
                            var dict = _source.RegisterObject(stackStarts[^1], (int) (start - thisStart) + length,
                                PdfObjectType.DictionaryObj, false);
                            dict.Parsed = callStack[^1];
                            var key = keys[^1];
                            PopStack();
                            var toAdd = callStack[^1];
                            if (toAdd is PdfDictionary lastDict)
                            {
                                lastDict[key] = dict;
                            } else if (toAdd is PdfArray arr)
                            {
                                arr.internalList.Add(dict);
                            }
                        }
                        continue;
                    case PdfTokenType.ArrayEnd:
                        Debug.Assert(callStack[^1] is PdfArray);
                        if (callStack.Count == 1)
                        {
                            completed = true;
                            return false;
                        } else
                        {
                            var thisStart = stackStarts[^1];
                            var array = _source.RegisterObject(stackStarts[^1], (int) (start - thisStart) + length,
                                PdfObjectType.ArrayObj, false);
                            array.Parsed = callStack[^1];
                            var key = keys[^1];
                            PopStack();
                            var toAdd = callStack[^1];
                            if (toAdd is PdfDictionary dict)
                            {
                                dict[key] = array;
                            } else if (toAdd is PdfArray arr)
                            {
                                arr.internalList.Add(array);
                            }
                        }

                        continue;
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }

            }

            void AddValue(PdfTokenType type, long start, int length)
            {
                switch (currentState)
                {
                    case ParseState.DictKey:
                        _source.FillData(start, length, out var data);
                        currentKey = NameParser.ParseName(data);
                        currentState = ParseState.DictValue;
                        return;
                    case ParseState.DictValue:
                        var dicObj = _source.RegisterObject(start, length, (PdfObjectType)type, false);
                        (callStack[^1] as PdfDictionary)[currentKey] = dicObj;
                        currentKey = null;
                        currentState = ParseState.DictKey;
                        return;
                    case ParseState.Array:
                        var obj = _source.RegisterObject(start, length, (PdfObjectType)type, false);
                        (callStack[^1] as PdfArray).Add(obj);
                        return;
                }
            }

            void PopStack()
            {
                stackStarts.RemoveAt(stackStarts.Count-1);
                callStack.RemoveAt(callStack.Count-1);
                if (callStack[^1] is PdfDictionary)
                {
                    currentState = ParseState.DictKey;
                }
                else
                {
                    currentState = ParseState.Array;
                }
            }

            return false;
        }

        public IParsedPdfObject GetValue()
        {
            if (callStack.Count != 1)
            {
                throw new ApplicationException("bad");
            }

            return callStack[0];
        }
    }

    
}