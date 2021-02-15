using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Nested
{
    internal class LazyNestedSeqParser
    {
        private readonly ParsingContext _ctx;
        public LazyNestedSeqParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }
        private ParseState currentState = ParseState.Unknown;
        internal bool completed = false;

        private PdfName currentKey;
        private IParsedLazyObj obj = null;
        internal SequencePosition startPos = default;
        internal SequencePosition endPos = default;

        public IParsedLazyObj GetCompletedObject()
        {
            if (!completed)
            {
                throw new InvalidOperationException("GetCompletedObject() called before parsing was finished.");
            }
            currentState = ParseState.Unknown;
            currentKey = null;
            startPos = default;
            endPos = default;
            var toReturn = obj;
            obj = null;
            completed = false;
            return toReturn;
        }

        public bool ParseNestedItem(in ReadOnlySequence<byte> sequence, bool isCompleted)
        {
            var reader = new SequenceReader<byte>(sequence);
            return ParseNestedItem(ref reader, isCompleted);
        }

        public bool ParseNestedItem(ref SequenceReader<byte> reader, bool isCompleted)
        {
            var sequence = reader.Sequence;
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

                            AddLazyValue(in sequence, tokenType);
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
                            AddLazyValue(in sequence, tokenType);
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
                        endPos = reader.Position;
                        dict.IsModified = false;
                        completed = true;
                        return false;
                    case PdfTokenType.ArrayEnd:
                        var arr = obj as PdfArray;
                        endPos = reader.Position;
                        arr.IsModified = false;
                        completed = true;
                        return false;
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }

            }

            return false;
        }

        void AddLazyValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (currentState)
            {
                case ParseState.DictValue:
                {
                    var item = _ctx.CreateLazy((PdfObjectType) type, sequence, startPos, endPos);
                    (obj as PdfDictionary)[currentKey] = item;
                    currentKey = null;
                    currentState = ParseState.DictKey;
                    return;
                }
                case ParseState.Array:
                {
                    var item = _ctx.CreateLazy((PdfObjectType) type, sequence, startPos, endPos);
                    (obj as PdfArray).Add(item);
                    return;
                }
            }
        }
        void AddValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (currentState)
            {
                case ParseState.DictKey:
                    var seg = sequence.Slice(startPos, endPos);
                    currentKey = _ctx.NameParser.Parse(in seg);
                    currentState = ParseState.DictValue;
                    return;
                case ParseState.DictValue:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, sequence, startPos, endPos);
                    (obj as PdfDictionary)[currentKey] = item;
                    currentKey = null;
                    currentState = ParseState.DictKey;
                    return;
                }
                case ParseState.Array:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, sequence, startPos, endPos);
                    (obj as PdfArray).Add(item);
                    return;
                }
            }
        }
    }
}
