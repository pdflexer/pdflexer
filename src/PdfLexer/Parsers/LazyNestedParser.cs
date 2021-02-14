using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers
{
    internal class LazyNestedParser
    {
        public enum ParseState
        {
            Unknown,
            DictKey,
            DictValue,
            Array
        }
        public LazyNestedParser(IPdfDataSource source)
        {
            _source = source;
        }
        private ParseState currentState = ParseState.Unknown;
        private bool completed = false;
        private IPdfDataSource _source;
        private PdfName currentKey;
        private IParsedLazyObj obj = null;

        public IParsedLazyObj GetItem()
        {
            if (!completed)
            {
                throw new ApplicationException("Bad");
            }
            return obj;
        }

        SequencePosition startPos = default;
        SequencePosition endPos = default;
        public bool ParseNestedItem(in ReadOnlySequence<byte> sequence, bool isCompleted)
        {
            var reader = new SequenceReader<byte>(sequence);
            while (reader.TryReadNextToken(isCompleted, out var tokenType, out startPos))
            {
                endPos = reader.Position;
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.StringObj:
                        AddValue(in sequence, tokenType);
                        break;
                    case PdfTokenType.DictionaryStart:
                        if (obj != null)
                        {
                            if (!reader.AdvanceToDictEnd(out _))
                            {
                                return false;
                            }
                            endPos = reader.Position;

                            AddValue(in sequence, PdfTokenType.DictionaryStart);
                            continue;
                        }
                        currentState = ParseState.DictKey;
                        obj = new PdfDictionary();
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (obj != null)
                        {

                            if (!reader.AdvanceToArrayEnd(out _))
                            {
                                return false;
                            }
                            endPos = reader.Position;
                            AddValue(in sequence, PdfTokenType.DictionaryStart);
                            continue;
                        }
                        obj = new PdfArray();
                        currentState = ParseState.Array;
                        break;
                    case PdfTokenType.NumericObj:
                        var current = reader.Consumed;
                        if (!reader.TryReadNextToken(isCompleted, out var secondType, out var secondStart))
                        {
                            return false;
                        }
                        
                        if (secondType != PdfTokenType.NumericObj)
                        {
                            reader.Rewind(reader.Consumed - current);
                            AddValue(in sequence, tokenType);
                            continue;
                        }

                        if (!reader.TryReadNextToken(isCompleted, out var thirdType, out var thirdStart))
                        {
                            return false;
                        }
                        if (thirdType != PdfTokenType.IndirectRef)
                        {
                            reader.Rewind(reader.Consumed - current);
                            AddValue(in sequence, tokenType);
                            continue;
                        }
                        endPos = reader.Position;
                        AddValue(in sequence, PdfTokenType.IndirectRef);
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

            return false;
        }
        #if NET50
        void AddValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (currentState)
            {
                case ParseState.DictKey:
                    var seg = sequence.Slice(startPos, endPos);
                    currentKey = NameParser.ParseName(in seg);
                    currentState = ParseState.DictValue;
                    return;
                case ParseState.DictValue:
                {
                    var start = sequence.GetOffset(startPos);
                    var end = sequence.GetOffset(endPos);
                    var dicObj = _source.CreateLazy(start, (int) (end - start), (PdfObjectType) type, false);
                    (obj as PdfDictionary)[currentKey] = dicObj;
                    currentKey = null;
                    currentState = ParseState.DictKey;
                    return;
                }
                case ParseState.Array:
                {
                    var start = sequence.GetOffset(startPos);
                    var end = sequence.GetOffset(endPos);
                    var arrayObj = _source.CreateLazy(start, (int) (end - start), (PdfObjectType) type, false);
                    (obj as PdfArray).Add(arrayObj);
                    return;
                }
            }
        }
#else

        void AddValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (currentState)
            {
                case ParseState.DictKey:
                    var seg = sequence.Slice(startPos, endPos);
                    currentKey = NameParser.Parse(ref seg);
                    currentState = ParseState.DictValue;
                    return;
                case ParseState.DictValue:
                {
                    var start = NetStandardSeqHelper.GetOffset(sequence, startPos);
                    var end = NetStandardSeqHelper.GetOffset(sequence, endPos);
                    var dicObj = _source.CreateLazy(start, (int) (end - start), (PdfObjectType) type, false);
                    (obj as PdfDictionary)[currentKey] = dicObj;
                    currentKey = null;
                    currentState = ParseState.DictKey;
                    return;
                }
                case ParseState.Array:
                {
                    var start = NetStandardSeqHelper.GetOffset(sequence, startPos);
                    var end = NetStandardSeqHelper.GetOffset(sequence, endPos);
                    var arrayObj = _source.CreateLazy(start, (int) (end - start), (PdfObjectType) type, false);
                    (obj as PdfArray).Add(arrayObj);
                    return;
                }
            }
        }
        #endif


        private static NameParser NameParser = new NameParser();
    }

    public static class LazyNestedExts
    {
        private static byte[] dictScanTerms = new byte[4]
        {
            (byte) '<', (byte) '>', (byte) '(', (byte) 'R'
        };

        private static byte[] arrayScanTerms = new byte[5]
        {
            (byte) '<', (byte) '(', (byte) '[', (byte) ']', (byte) 'R'
        };

        public static bool AdvanceToArrayEnd(this ref SequenceReader<byte> reader, out bool hadIndirectObjects)
        {
            hadIndirectObjects = false;
            while (reader.TryAdvanceToAny(arrayScanTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {
                    case (byte) 'R':
                    {
                        hadIndirectObjects = true;
                        continue;
                    }
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
                            if (!reader.AdvanceToDictEnd(out bool hadSubIOs))
                            {
                                return false;
                            }
                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
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
                    {
                        if (!reader.AdvanceToArrayEnd(out bool hadSubIOs))
                        {
                            return false;
                        }

                        if (hadSubIOs)
                        {
                            hadIndirectObjects = true;
                        }

                        continue;
                    }
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
        public static bool AdvanceToDictEnd(this ref SequenceReader<byte> reader, out bool hadIndirectObjects)
        {
            hadIndirectObjects = false;
            while (reader.TryAdvanceToAny(dictScanTerms, false))
            {
                if (!reader.TryRead(out byte b))
                {
                    return false;
                }

                switch (b)
                {                    
                    case (byte) 'R':
                    {
                        hadIndirectObjects = true;
                        continue;
                    }
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
                            if (!reader.AdvanceToDictEnd(out bool hadSubIOs))
                            {
                                return false;
                            }

                            if (hadSubIOs)
                            {
                                hadIndirectObjects = true;
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
