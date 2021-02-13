using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfLexer.Objects.Parsers
{
    public class LazyNestedParser
    {
        public LazyNestedParser(IPdfDataSource source)
        {
            _source = source;
        }
        private ParseState currentState = ParseState.Unknown;
        private bool completed = false;
        private IPdfDataSource _source;
        private string currentKey;
        private IParsedPdfObject obj = null;

        public IParsedPdfObject GetItem()
        {
            if (!completed)
            {
                throw new ApplicationException("Bad");
            }
            return obj;
        }
        public bool ParseNestedItem(ReadOnlySequence<byte> sequence, bool isCompleted)
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
                        if (obj != null)
                        {
                            var dictStart = reader.Consumed;
                            if (!reader.AdvanceToDictEnd())
                            {
                                return false;
                            }

                            AddValue(PdfTokenType.DictionaryStart, start, (int) (reader.Consumed - dictStart) + length);
                            continue;
                        }
                        currentState = ParseState.DictKey;
                        obj = new PdfDictionary();
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (obj != null)
                        {
                            var arrayStart = reader.Consumed;
                            if (!reader.AdvanceToArrayEnd())
                            {
                                return false;
                            }
                            AddValue(PdfTokenType.DictionaryStart, start, (int) (reader.Consumed - arrayStart) + length);
                            continue;
                        }
                        obj = new PdfArray();
                        currentState = ParseState.Array;
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
                        var dict = obj as PdfDictionary;
                        dict.IsModified = false;
                        completed = true;
                        return false;
                    case PdfTokenType.ArrayEnd:
                        var arr = obj as PdfArray;
                        arr.IsModified = false;
                        completed = true;
                        return false;
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
                        (obj as PdfDictionary)[currentKey] = dicObj;
                        currentKey = null;
                        currentState = ParseState.DictKey;
                        return;
                    case ParseState.Array:
                        var arrayObj = _source.RegisterObject(start, length, (PdfObjectType)type, false);
                        (obj as PdfArray).Add(arrayObj);
                        return;
                }
            }

            return false;
        }
    }

    public static class LazyNestedExts
    {
        private static byte[] dictScanTerms = new byte[3]
        {
            (byte) '<', (byte) '>', (byte) '('
        };

        private static byte[] arrayScanTerms = new byte[4]
        {
            (byte) '<', (byte) '(', (byte) '[', (byte) ']'
        };

        public static bool AdvanceToArrayEnd(this ref SequenceReader<byte> reader)
        {
            while (reader.TryAdvanceToAny(arrayScanTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {
                    case (byte) '<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '<')
                        {
                            // new dict
                            reader.TryRead(out byte _);
                            if (!reader.AdvanceToDictEnd())
                            {
                                return false;
                            }
                        }

                        // just hex string
                    }
                        continue;
                    case (byte) '(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
                    case (byte) '[':
                        if (!reader.AdvanceToArrayEnd())
                        {
                            return false;
                        }

                        continue;
                    case (byte) ']':
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Advances to the end of dictionary. Note: Current dictionary tokens must already have been read.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>If completed successfully. False designates incomplete data.</returns>
        public static bool AdvanceToDictEnd(this ref SequenceReader<byte> reader)
        {
            while (reader.TryAdvanceToAny(dictScanTerms, false))
            {
                
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {
                    case (byte) '<':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '<')
                        {
                            // new dict
                            reader.TryRead(out byte _);
                            if (!reader.AdvanceToDictEnd())
                            {
                                return false;
                            }
                        }

                        // just hex string
                    }
                        continue;
                    case (byte) '>':
                    {
                        if (!reader.TryPeek(out byte b2))
                        {
                            return false;
                        }

                        if (b2 == (byte) '>')
                        {
                            // ended!
                            return reader.TryRead(out _);
                        }
                        // just hex end
                    }
                        continue;
                    case (byte) '(':
                    {
                        reader.Rewind(1);
                        if (!StringParser.AdvancePastStringLiteral(ref reader))
                        {
                            return false;
                        }

                        continue;
                    }
                }
            }

            return false;
        }
    }
}
