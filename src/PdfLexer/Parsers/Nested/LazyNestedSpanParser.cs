﻿using System;
using System.Collections.Generic;
using PdfLexer.IO;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Nested
{
    internal class LazyNestedSpanParser
    {
        private List<ObjParseState> StateStack = new List<ObjParseState>(10);
        private ObjParseState CurrentState = default;
        private readonly ParsingContext _ctx;
        public LazyNestedSpanParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public IParsedLazyObj ParseNestedItem(IPdfDataSource source, long offset, ReadOnlySpan<byte> buffer, int startAt)
        {
            bool completed = false;
            var lastStart = 0;
            while ((startAt = PdfSpanLexer.TryReadNextToken(buffer, out var tokenType, lastStart=startAt, out var currentLength)) != -1)
            {
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.DecimalObj:
                    case PdfTokenType.NumericObj:
                        CurrentState.Bag.Add(_ctx.GetKnownPdfItem((PdfObjectType) tokenType, buffer, startAt, currentLength));
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.StringStart:
                        CurrentState.Bag.Add(_ctx.StringParser.Parse(buffer, startAt, out int used));
                        startAt += used;
                        break;
                    case PdfTokenType.DictionaryStart:
                        if (CurrentState.IsParsing())
                        {
                            if (_ctx.IsEager)
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.State = ParseState.ReadDictKey;
                                CurrentState.Dict = new PdfDictionary();
                                CurrentState.Bag ??= new List<IPdfObject>();
                                CurrentState.Bag.Clear();
                                startAt += currentLength;
                                continue;
                            }
                            else
                            {
                                var originalStart = startAt;
                                startAt += currentLength;
                                var end = startAt;
                                if (!NestedUtil.AdvanceToDictEnd(buffer, ref end, out var hadIndirect))
                                {
                                    goto Done;
                                }

                                currentLength = end - startAt;
                                AddLazyValue(source, offset, originalStart, currentLength, PdfTokenType.DictionaryStart, hadIndirect);
                                startAt += currentLength;
                                continue;
                            }
                        }
                        CurrentState.State = ParseState.ReadDictKey;
                        CurrentState.Dict = new PdfDictionary();
                        CurrentState.Bag ??= new List<IPdfObject>();
                        CurrentState.Bag.Clear();
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (CurrentState.IsParsing())
                        {
                            if (_ctx.IsEager)
                            {
                                StateStack.Add(CurrentState);
                                CurrentState = default;
                                CurrentState.State = ParseState.ReadArray;
                                CurrentState.Array = new PdfArray();
                                CurrentState.Bag ??= new List<IPdfObject>();
                                CurrentState.Bag.Clear();
                                startAt += currentLength;
                                continue;
                            }
                            else
                            {
                                var originalStart = startAt;
                                startAt += currentLength;
                                var end = startAt;
                                if (!NestedUtil.AdvanceToArrayEnd(buffer, ref end, out var hadIndirect))
                                {
                                   goto Done;
                                }

                                currentLength = end - startAt;
                                AddLazyValue(source, offset, originalStart, currentLength, PdfTokenType.ArrayStart, hadIndirect);
                                startAt += currentLength;
                                continue;
                            }
                        }
                        CurrentState.Array = new PdfArray();
                        CurrentState.State = ParseState.ReadArray;
                        CurrentState.Bag ??= new List<IPdfObject>();
                        CurrentState.Bag.Clear();
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.IndirectRef:
                        CurrentState.Bag.Add(IndirectRefToken.Value);
                        startAt += 1;
                        continue;
                    case PdfTokenType.DictionaryEnd:
                        CurrentState.Dict = CurrentState.GetDictionaryFromBag();
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Dict);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            startAt += currentLength;
                            break;
                        }
                        else
                        {
                            completed = true;
                            goto Done;
                        }
                    case PdfTokenType.ArrayEnd:
                        CurrentState.Array = CurrentState.GetArrayFromBag();
                        if (StateStack.Count > 0)
                        {
                            var last = StateStack[^1];
                            last.Bag.Add(CurrentState.Array);
                            CurrentState = last;
                            StateStack.RemoveAt(StateStack.Count-1);
                            startAt += currentLength;
                            break;
                        }
                        else
                        {
                            completed = true;
                            goto Done;
                        }
                        
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }


            }

            Done:
            var obj = (IParsedLazyObj) CurrentState.Dict ?? CurrentState.Array;
            CurrentState = default;
            if (!completed)
            {
                throw CommonUtil.DisplayDataErrorException(buffer, lastStart,
                    $"Parsing ended unexpectedly for looking for {CurrentState.State.ToString()} or Dict End, remaining data");
            }
            
            return obj;

            void AddLazyValue(IPdfDataSource source, long offset, int startPos, int length, PdfTokenType type, bool hadIndirect)
            {
                var lazy = new PdfLazyObject
                {
                    Source = source,
                    Offset = offset + startPos,
                    Length = length,
                    HasLazyIndirect = hadIndirect,
                    Type = (PdfObjectType) type
                };
                CurrentState.Bag.Add(lazy);
            }

        }
    }
}
