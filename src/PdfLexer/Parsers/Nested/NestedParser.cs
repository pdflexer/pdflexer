using System;
using System.Collections.Generic;
using PdfLexer.IO;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Nested
{
    internal class NestedParser
    {
        private ObjParseState CurrentState = default;
        private List<ObjParseState> StateStack = new List<ObjParseState>(10);

        // TODO re-use bags
        // private List<List<IPdfObject>> Bags = new List<List<IPdfObject>>(10);
        // private List<IPdfObject> CurrentBag = default;
        
        private readonly ParsingContext _ctx;
        public NestedParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public IPdfObject ParseNestedItem(ReadOnlySpan<byte> buffer, int startAt, out int itemEnd)
        {
            bool completed = false;
            var lastStart = 0;
            itemEnd = 0;
            while ((startAt = PdfSpanLexer.TryReadNextToken(buffer, out var tokenType, lastStart = startAt, out var currentLength)) != -1)
            {
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.DecimalObj:
                    case PdfTokenType.NumericObj:
                        CurrentState.Bag.Add(_ctx.GetKnownPdfItem((PdfObjectType)tokenType, buffer, startAt, currentLength));
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.StringStart:
                        if (!_ctx.Options.LazyStrings)
                        {
                            CurrentState.Bag.Add(_ctx.StringParser.Parse(buffer, startAt, currentLength));
                        } else
                        {
                            AddLazyValue(startAt, startAt, PdfTokenType.StringStart, false);
                        }
                        startAt += currentLength;
                        break;
                    case PdfTokenType.DictionaryStart:
                        if (CurrentState.IsParsing())
                        {
                            var eager = _ctx.Options.Eagerness;
                            if (_ctx.CurrentSource == null) { eager = Eagerness.FullEager; }
                            switch (eager)
                            {
                                case Eagerness.Lazy:
                                    var originalStart = startAt;
                                    startAt += currentLength;
                                    var end = startAt;
                                    if (!NestedUtil.AdvanceToDictEnd(buffer, ref end, out var hadIndirect))
                                    {
                                        goto Done;
                                    }

                                    AddLazyValue(originalStart, end - originalStart, PdfTokenType.DictionaryStart, hadIndirect);
                                    startAt = end;
                                    continue;
                                case Eagerness.FullEager:
                                default:
                                    StateStack.Add(CurrentState);
                                    CurrentState = default;
                                    CurrentState.State = ParseState.ReadDict;
                                    CurrentState.Bag ??= new List<IPdfObject>(10);
                                    CurrentState.Bag.Clear();
                                    startAt += currentLength;
                                    continue;
                            }
                        }
                        CurrentState.State = ParseState.ReadDict;
                        CurrentState.Bag ??= new List<IPdfObject>(10);
                        CurrentState.Bag.Clear();
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (CurrentState.IsParsing())
                        {
                            var eager = _ctx.Options.Eagerness;
                            if (_ctx.CurrentSource == null) { eager = Eagerness.FullEager; }
                            switch (eager)
                            {
                                case Eagerness.Lazy:
                                    var originalStart = startAt;
                                    startAt += currentLength;
                                    var end = startAt;
                                    if (!NestedUtil.AdvanceToArrayEnd(buffer, ref end, out var hadIndirect))
                                    {
                                        goto Done;
                                    }

                                    AddLazyValue(originalStart, end - originalStart, PdfTokenType.ArrayStart, hadIndirect);
                                    startAt = end;
                                    continue;
                                case Eagerness.FullEager:
                                default:
                                    StateStack.Add(CurrentState);
                                    CurrentState = default;
                                    CurrentState.State = ParseState.ReadArray;
                                    CurrentState.Bag ??= new List<IPdfObject>(10);
                                    CurrentState.Bag.Clear();
                                    startAt += currentLength;
                                    continue;
                            }
                        }
                        CurrentState.State = ParseState.ReadArray;
                        CurrentState.Bag ??= new List<IPdfObject>(10);
                        CurrentState.Bag.Clear();
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.IndirectRef:
                        CurrentState.Bag.Add(IndirectRefToken.Value);
                        startAt += 1;
                        continue;
                    case PdfTokenType.DictionaryEnd:
                        if (CurrentState.State != ParseState.ReadDict)
                        {
                            // TODO more advanced repair
                            startAt += currentLength;
                            continue;
                        }
                        CurrentState.Dict = CurrentState.GetDictionaryFromBag(_ctx);
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Dict);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count - 1);
                            startAt += currentLength;
                            break;
                        }
                        else
                        {
                            itemEnd = startAt + 2;
                            completed = true;
                            goto Done;
                        }
                    case PdfTokenType.ArrayEnd:
                        if (CurrentState.State != ParseState.ReadArray)
                        {
                            // TODO more advanced repair
                            startAt += currentLength;
                            continue;
                        }
                        CurrentState.Array = CurrentState.GetArrayFromBag(_ctx);
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Array);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count - 1);
                            startAt += currentLength;
                            break;
                        }
                        else
                        {
                            completed = true;
                            goto Done;
                        }

                    default:
                        var info = CommonUtil.GetDataErrorInfo(buffer, lastStart);
                        _ctx.Error("Unknown token encountered parsing nested item: " + info);
                        if (tokenType == PdfTokenType.StartObj)
                        {
                            // special case.. we known we've over read
                            goto Done;
                        }
                        if (currentLength <= 0)
                        {
                            startAt += 1;
                        }
                        else
                        {
                            startAt += currentLength;
                        }
                        continue;
                }


            }

        Done:

            if (!completed)
            {
                return AttemptRepairCurrentState(buffer, lastStart);
            }

            IPdfObject obj = (IPdfObject)CurrentState.Dict ?? CurrentState.Array;
            CurrentState = default;

            return obj;

            void AddLazyValue(int startPos, int length, PdfTokenType type, bool hadIndirect)
            {
                var lazy = new PdfLazyObject
                {
                    Source = _ctx.CurrentSource,
                    Offset = _ctx.CurrentOffset + startPos,
                    Length = length,
                    HasLazyIndirect = hadIndirect,
                    LazyObjectType = (PdfObjectType)type
                };
                CurrentState.Bag.Add(lazy);
            }

        }

        private IPdfObject AttemptRepairCurrentState(ReadOnlySpan<byte> buffer, int lastStart)
        {
            var info = CommonUtil.GetDataErrorInfo(buffer, lastStart);
            _ctx.Error("Parsing ended unexpectedly for looking nested item end: " + info);
            if (CurrentState.State == ParseState.ReadDict)
            {
                CurrentState.Dict = CurrentState.GetDictionaryFromBag(_ctx);
            }
            else
            {
                CurrentState.Array = CurrentState.GetArrayFromBag(_ctx);
            }

            while (StateStack.Count > 0)
            {
                var last = StateStack[^1];
                last.Bag.Add(CurrentState.Array == null ? (IPdfObject)CurrentState.Dict : (IPdfObject)CurrentState.Array);
                CurrentState = last;
                StateStack.RemoveAt(StateStack.Count - 1);

                if (CurrentState.State == ParseState.ReadDict)
                {
                    CurrentState.Dict = CurrentState.GetDictionaryFromBag(_ctx);
                }
                else
                {
                    CurrentState.Array = CurrentState.GetArrayFromBag(_ctx);
                }
            }
            IPdfObject obj = (IPdfObject)CurrentState.Dict ?? CurrentState.Array;
            CurrentState = default;

            return obj;
        }
    }
}
