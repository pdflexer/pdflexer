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
        public PdfDictionary Dict { get; set; }
        public PdfArray Array { get; set; }
        public List<IPdfObject> Bag { get; set; } 
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
            var toReturn = (IParsedLazyObj) CurrentState.Dict ?? CurrentState.Array;
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
                    case PdfTokenType.NumericObj:
                    case PdfTokenType.DecimalObj:
                        CurrentState.Bag.Add(_ctx.GetPdfItem((PdfObjectType) tokenType, sequence, startPos, endPos));
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
                        CurrentState.Bag.Add(new PdfString(_ctx.StringParser.GetCurrentString()));
                        break;
                    case PdfTokenType.DictionaryStart:
                        if (CurrentState.Dict != null || CurrentState.Array != null)
                        {
                            if (_ctx.IsEager)
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.State = ParseState.ReadDictValue;
                                CurrentState.Dict = new PdfDictionary();
                                CurrentState.Bag ??= new List<IPdfObject>();
                                CurrentState.Bag.Clear();
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
                                CurrentState.Bag.Add(_ctx.CreateLazy((PdfObjectType) tokenType, sequence, startPos, endPos));
                                continue;
                            }
                        }
                        Debug.Assert(CurrentState.State == ParseState.None);
                        CurrentState.State = ParseState.ReadDictValue;
                        CurrentState.Dict = new PdfDictionary();
                        CurrentState.Bag ??= new List<IPdfObject>();
                        CurrentState.Bag.Clear();
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (CurrentState.Dict != null || CurrentState.Array != null)
                        {
                            if (_ctx.IsEager)
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.Array = new PdfArray();
                                CurrentState.State = ParseState.ReadArray;
                                CurrentState.Bag ??= new List<IPdfObject>();
                                CurrentState.Bag.Clear();
                                continue;
                            }
                            else
                            {
                                if (!reader.AdvanceToArrayEnd(out _))
                                {
                                    StateStack.Add(CurrentState);
                                    CurrentState.State = ParseState.SkipArray;
                                    return false;
                                }
                                endPos = reader.Position;
                                CurrentState.Bag.Add(_ctx.CreateLazy((PdfObjectType) tokenType, sequence, startPos, endPos));
                                continue;
                            }
                        }
                        CurrentState.Array = new PdfArray();
                        Debug.Assert(CurrentState.State == ParseState.None);
                        CurrentState.State = ParseState.ReadArray;
                        CurrentState.Bag ??= new List<IPdfObject>();
                        CurrentState.Bag.Clear();
                        break;
                    case PdfTokenType.IndirectRef:
                        CurrentState.Bag.Add(IndirectRefToken.Value);
                        endPos = reader.Position;
                        break;
                    case PdfTokenType.DictionaryEnd:
                        CurrentState.Dict = GetDictionaryFromCurrent();
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Dict);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            break;
                        }
                        else
                        {
                            endPos = reader.Position;
                            completed = true;
                            return false;
                        }
                    case PdfTokenType.ArrayEnd:
                        CurrentState.Array = GetArrayFromCurrent();
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Array);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            break;
                        }
                        else
                        {
                            endPos = reader.Position;
                            completed = true;
                            return false;
                        }

                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }

            }

            return false;
        }

        private PdfArray GetArrayFromCurrent()
        {
            
            var array = CurrentState.Array;
            foreach (var item in CurrentState.Bag)
            {
                array.Add(item);
            }
            array.IsModified = false;
            return array;
        }

        private PdfDictionary GetDictionaryFromCurrent()
        {
            bool key = true;
            PdfName name = null;
            var dict = CurrentState.Dict;
            for (var i=0;i<CurrentState.Bag.Count;i++)
            {
                var item = CurrentState.Bag[i];
                if (key)
                {
                    if (item is PdfName nm)
                    {
                        name = nm;
                    } else
                    {
                        throw new ApplicationException("");
                    }
                } else
                {
                    if (item is PdfNumber num 
                        && i + 2 < CurrentState.Bag.Count
                        && CurrentState.Bag[i+1] is PdfIntNumber num2
                        && CurrentState.Bag[i+2] is IndirectRefToken)
                    {
                        dict[name] = new PdfIndirectRef((long)num, num2.Value);
                        i+=2;
                    } else
                    {
                        dict[name] = item;
                    }
                    
                }
                key = !key;
            }
            dict.IsModified = false;
            return dict;
        }
    }
}
