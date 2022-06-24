using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Nested
{
    internal enum ParseState
    {
        None,
        ReadDictKey,
        ReadDictValue,
        ReadArray,
        SkipDict,
        SkipArray,
        ReadString
    }

    internal struct ObjParseState
    {
        public ParseState State { get; set; }
        public PdfName CurrentKey { get; set; }
        public IParsedLazyObj Object { get; set; }
    }

    /// <summary>
    /// TODO: re-entrant eager parsing
    /// TODO: re-entrant string parsing
    /// </summary>
    internal class LazyNestedSeqParser
    {
        private readonly ParsingContext _ctx;
        public LazyNestedSeqParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        private List<ObjParseState> StateStack = new List<ObjParseState>(10);
        private ObjParseState CurrentState = default;
        internal bool completed = false;

        internal SequencePosition startPos = default;
        internal SequencePosition endPos = default;

        public IParsedLazyObj GetCompletedObject()
        {
            if (!completed)
            {
                throw new InvalidOperationException("GetCompletedObject() called before parsing was finished.");
            }
            
            startPos = default;
            endPos = default;
            var toReturn = CurrentState.Object;
            CurrentState = default;
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
            switch (CurrentState.State)
            {
                case ParseState.None:
                case ParseState.ReadArray:
                case ParseState.ReadDictKey:
                case ParseState.ReadDictValue:
                    break;
                case ParseState.ReadString:
                    if (!_ctx.StringParser.TryReadString(ref reader))
                    {
                        return false;
                    }

                    CurrentState = StateStack[^1];
                    StateStack.RemoveAt(StateStack.Count-1);
                    return ParseNestedItem(ref reader, isCompleted);
                case ParseState.SkipDict:
                    if (!_ctx.Skipper.TryScanToEndOfDict(ref reader))
                    {
                        return false;
                    }
                    CurrentState = StateStack[^1];
                    StateStack.RemoveAt(StateStack.Count-1);
                    return ParseNestedItem(ref reader, isCompleted);
                case ParseState.SkipArray:
                    if (!_ctx.Skipper.TryScanToEndOfArray(ref reader))
                    {
                        return false;
                    }
                    CurrentState = StateStack[^1];
                    StateStack.RemoveAt(StateStack.Count-1);
                    return ParseNestedItem(ref reader, isCompleted);
            }

            var sequence = reader.Sequence;
            while (reader.TryReadNextToken(isCompleted, out var tokenType, out startPos))
            {
                endPos = reader.Position;
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                        AddValue(in sequence, tokenType);
                        break;
                    case PdfTokenType.StringObj:
                        reader.Rewind(1);
                        if (!_ctx.StringParser.TryReadString(ref reader))
                        {
                            StateStack.Add(CurrentState);
                            CurrentState = default;
                            CurrentState.State = ParseState.ReadString;
                            return false;
                        }
                        AddCurrentString();
                        break;
                    case PdfTokenType.DictionaryStart:
                        if (CurrentState.Object != null)
                        {
                            if (_ctx.IsEager)
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.State = ParseState.ReadDictKey;
                                CurrentState.Object = new PdfDictionary();
                                continue;
                            }
                            else
                            {
                                if (!reader.AdvanceToDictEnd(out _))
                                {
                                    StateStack.Add(CurrentState);
                                    CurrentState.State = ParseState.SkipDict;
                                    return false;
                                }
                                endPos = reader.Position;
                                AddLazyValue(in sequence, tokenType);
                                continue;
                            }
                        }
                        Debug.Assert(CurrentState.State == ParseState.None);
                        CurrentState.State = ParseState.ReadDictKey;
                        CurrentState.Object = new PdfDictionary();
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (CurrentState.Object != null)
                        {
                            if (!reader.AdvanceToArrayEnd(out _))
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.Object = new PdfArray();
                                CurrentState.State = ParseState.SkipArray;
                                return false;
                            }
                            endPos = reader.Position;
                            AddLazyValue(in sequence, tokenType);
                            continue;
                        }
                        CurrentState.Object = new PdfArray();
                        Debug.Assert(CurrentState.State == ParseState.None);
                        CurrentState.State = ParseState.ReadArray;
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
                            reader.Rewind(reader.Consumed - current);
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
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            if (last.State == ParseState.ReadDictValue)
                            {
                                (last.Object as PdfDictionary)[last.CurrentKey] = CurrentState.Object;
                                last.State = ParseState.ReadDictKey;
                            } else if (last.State == ParseState.ReadArray)
                            {
                                (last.Object as PdfArray).Add(CurrentState.Object);
                            }
                            else
                            {
                                throw new ApplicationException("Todo");
                            }
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            break;
                        }
                        else
                        {
                            var dict = CurrentState.Object as PdfDictionary;
                            endPos = reader.Position;
                            dict.IsModified = false;
                            completed = true;
                            return false;
                        }
                    case PdfTokenType.ArrayEnd:
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            if (last.State == ParseState.ReadDictValue)
                            {
                                (last.Object as PdfDictionary)[last.CurrentKey] = CurrentState.Object;
                                last.State = ParseState.ReadDictKey;
                            } else if (last.State == ParseState.ReadArray)
                            {
                                (last.Object as PdfArray).Add(CurrentState.Object);
                            }
                            else
                            {
                                throw new ApplicationException("Todo");
                            }
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            break;
                        }
                        else
                        {
                            var arr = CurrentState.Object as PdfArray;
                            endPos = reader.Position;
                            arr.IsModified = false;
                            completed = true;
                            return false;
                        }

                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }

            }

            return false;
        }

        void AddCurrentString()
        {
            switch (CurrentState.State)
            {
                case ParseState.ReadDictValue:
                {
                    (CurrentState.Object as PdfDictionary)[CurrentState.CurrentKey] = new PdfString(_ctx.StringParser.GetCurrentString());
                    CurrentState.CurrentKey = null;
                    CurrentState.State = ParseState.ReadDictKey;
                    return;
                }
                case ParseState.ReadArray:
                {
                    (CurrentState.Object as PdfArray).Add(new PdfString(_ctx.StringParser.GetCurrentString()));
                    return;
                }
            }
        }

        void AddLazyValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (CurrentState.State)
            {
                case ParseState.ReadDictValue:
                {
                    var item = _ctx.CreateLazy((PdfObjectType) type, sequence, startPos, endPos);
                    (CurrentState.Object as PdfDictionary)[CurrentState.CurrentKey] = item;
                    CurrentState.CurrentKey = null;
                    CurrentState.State = ParseState.ReadDictKey;
                    return;
                }
                case ParseState.ReadArray:
                {
                    var item = _ctx.CreateLazy((PdfObjectType) type, sequence, startPos, endPos);
                    (CurrentState.Object as PdfArray).Add(item);
                    return;
                }
            }
        }
        void AddValue(in ReadOnlySequence<byte> sequence, PdfTokenType type)
        {
            switch (CurrentState.State)
            {
                case ParseState.ReadDictKey:
                    var seg = sequence.Slice(startPos, endPos);
                    CurrentState.CurrentKey = _ctx.NameParser.Parse(in seg);
                    CurrentState.State = ParseState.ReadDictValue;
                    return;
                case ParseState.ReadDictValue:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, sequence, startPos, endPos);
                    (CurrentState.Object as PdfDictionary)[CurrentState.CurrentKey] = item;
                    CurrentState.CurrentKey = null;
                    CurrentState.State = ParseState.ReadDictKey;
                    return;
                }
                case ParseState.ReadArray:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, sequence, startPos, endPos);
                    (CurrentState.Object as PdfArray).Add(item);
                    return;
                }
            }
        }
    }
}
