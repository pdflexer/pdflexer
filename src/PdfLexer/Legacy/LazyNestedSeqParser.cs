using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PdfLexer.Legacy
{
    [Obsolete("No longer in use", false)]
    [ExcludeFromCodeCoverage]
    internal struct ObjParseState
    {
        public ParseState State { get; set; }
        public PdfName CurrentKey { get; set; }
        public PdfDictionary Dict { get; set; }
        public PdfArray Array { get; set; }
        public List<IPdfObject> Bag { get; set; }

        public bool IsParsing()
        {
            return Dict != null || Array != null;
        }

        public PdfArray GetArrayFromBag()
        {
            for (var i=0;i<Bag.Count;i++)
            {
                var item = Bag[i];
                if (item is PdfNumber num 
                    && i + 2 < Bag.Count
                    && Bag[i+1] is PdfIntNumber num2
                    && Bag[i+2] is IndirectRefToken)
                {
                    Array.Add(new ExistingIndirectRef(null, (long)num, num2.Value));
                    i+=2;
                } else
                {
                    Array.Add(item);
                }
            }
            return Array;
        }

        public PdfDictionary GetDictionaryFromBag()
        {
            bool key = true;
            PdfName name = null;
            for (var i=0;i<Bag.Count;i++)
            {
                var item = Bag[i];
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
                        && i + 2 < Bag.Count
                        && Bag[i+1] is PdfIntNumber num2
                        && Bag[i+2] is IndirectRefToken)
                    {
                        Dict[name] = new ExistingIndirectRef(null, (long)num, num2.Value);
                        i+=2;
                    } else
                    {
                        Dict[name] = item;
                    }
                    
                }
                key = !key;
            }
            Dict.DictionaryModified = false;
            return Dict;
        }
    }
    [Obsolete("No longer in use", false)]
    internal enum ParseState
    {
        None,
        ReadDictKey,
        ReadDictValue,
        ReadArray,
        SkipDict,
        SkipArray
    }

    [Obsolete("No longer in use", false)]
    [ExcludeFromCodeCoverage]
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

        // public IParsedLazyObj GetCompletedObject()
        // {
        //     if (!completed)
        //     {
        //         throw new InvalidOperationException("GetCompletedObject() called before parsing was finished.");
        //     }
        //     
        //     startPos = default;
        //     endPos = default;
        //     var toReturn = (IParsedLazyObj) CurrentState.Dict ?? CurrentState.Array;
        //     CurrentState = default;
        //     completed = false;
        //     return toReturn;
        // }
        // 
        // public bool ParseNestedItem(in ReadOnlySequence<byte> sequence, bool isCompleted)
        // {
        //     var reader = new SequenceReader<byte>(sequence);
        //     return false;
        //     //return ParseNestedItem(ref reader, isCompleted);
        // }

        // public bool ParseNestedItem(ref SequenceReader<byte> reader, bool isCompleted)
        // {
        //     switch (CurrentState.State)
        //     {
        //         case ParseState.None:
        //         case ParseState.ReadArray:
        //         case ParseState.ReadDictKey:
        //         case ParseState.ReadDictValue:
        //             break;
        //         // case ParseState.ReadString:
        //         //     if (!_ctx.StringParser.TryReadString(ref reader))
        //         //     {
        //         //         return false;
        //         //     }
        //         // 
        //         //     CurrentState = StateStack[^1];
        //         //     StateStack.RemoveAt(StateStack.Count-1);
        //         //     return ParseNestedItem(ref reader, isCompleted);
        //         case ParseState.SkipDict:
        //             // if (!_ctx.Skipper.TryScanToEndOfDict(ref reader))
        //             // {
        //             //     return false;
        //             // }
        //             CurrentState = StateStack[^1];
        //             StateStack.RemoveAt(StateStack.Count-1);
        //             return ParseNestedItem(ref reader, isCompleted);
        //         case ParseState.SkipArray:
        //             // if (!_ctx.Skipper.TryScanToEndOfArray(ref reader))
        //             // {
        //             //     return false;
        //             // }
        //             CurrentState = StateStack[^1];
        //             StateStack.RemoveAt(StateStack.Count-1);
        //             return ParseNestedItem(ref reader, isCompleted);
        //     }
        // 
        //     var sequence = reader.Sequence;
        //     while (reader.TryReadNextToken(isCompleted, out var tokenType, out startPos))
        //     {
        //         endPos = reader.Position;
        //         switch (tokenType)
        //         {
        //             case PdfTokenType.NameObj:
        //             case PdfTokenType.BooleanObj:
        //             case PdfTokenType.NullObj:
        //             case PdfTokenType.NumericObj:
        //             case PdfTokenType.DecimalObj:
        //                 CurrentState.Bag.Add(_ctx.GetPdfItem((PdfObjectType) tokenType, sequence, startPos, endPos));
        //                 break;
        //             case PdfTokenType.StringStart:
        //                 // reader.Rewind(1);
        //                 var item = _ctx.StringParser.Parse(reader.Sequence.Slice(startPos, endPos));
        //                 // if (!_ctx.StringParser.TryReadString(ref reader))
        //                 // {
        //                 //     StateStack.Add(CurrentState);
        //                 //     CurrentState = default;
        //                 //     CurrentState.State = ParseState.ReadString;
        //                 //     return false;
        //                 // }
        //                 CurrentState.Bag.Add(item);
        //                 break;
        //             case PdfTokenType.DictionaryStart:
        //                 if (CurrentState.IsParsing())
        //                 {
        //                     if (_ctx.IsEager)
        //                     {
        //                         StateStack.Add(CurrentState);
        //                         CurrentState = default;
        //                         CurrentState.State = ParseState.ReadDictValue;
        //                         CurrentState.Dict = new PdfDictionary();
        //                         CurrentState.Bag ??= new List<IPdfObject>();
        //                         CurrentState.Bag.Clear();
        //                         continue;
        //                     }
        //                     else
        //                     {
        //                         if (!PdfLexer.Parsers.Nested.NestedUtil.AdvanceToDictEnd(ref reader, out _))
        //                         {
        //                             StateStack.Add(CurrentState);
        //                             CurrentState.State = ParseState.SkipDict;
        //                             return false;
        //                         }
        //                         endPos = reader.Position;
        //                         //CurrentState.Bag.Add(_ctx.CreateLazy((PdfObjectType) tokenType, sequence, startPos, endPos));
        //                         continue;
        //                     }
        //                 }
        //                 Debug.Assert(CurrentState.State == ParseState.None);
        //                 CurrentState.State = ParseState.ReadDictValue;
        //                 CurrentState.Dict = new PdfDictionary();
        //                 CurrentState.Bag ??= new List<IPdfObject>();
        //                 CurrentState.Bag.Clear();
        //                 continue;
        //             case PdfTokenType.ArrayStart:
        //                 if (CurrentState.IsParsing())
        //                 {
        //                     if (_ctx.Options.IsEager)
        //                     {
        //                         StateStack.Add(CurrentState);
        //                         CurrentState = default;
        //                         CurrentState.Array = new PdfArray();
        //                         CurrentState.State = ParseState.ReadArray;
        //                         CurrentState.Bag ??= new List<IPdfObject>();
        //                         CurrentState.Bag.Clear();
        //                         continue;
        //                     }
        //                     else
        //                     {
        //                         if (!PdfLexer.Parsers.Nested.NestedUtil.AdvanceToArrayEnd(ref reader, out _))
        //                         {
        //                             StateStack.Add(CurrentState);
        //                             CurrentState.State = ParseState.SkipArray;
        //                             return false;
        //                         }
        //                         endPos = reader.Position;
        //                         //CurrentState.Bag.Add(_ctx.CreateLazy((PdfObjectType) tokenType, sequence, startPos, endPos));
        //                         continue;
        //                     }
        //                 }
        //                 CurrentState.Array = new PdfArray();
        //                 Debug.Assert(CurrentState.State == ParseState.None);
        //                 CurrentState.State = ParseState.ReadArray;
        //                 CurrentState.Bag ??= new List<IPdfObject>();
        //                 CurrentState.Bag.Clear();
        //                 break;
        //             case PdfTokenType.IndirectRef:
        //                 CurrentState.Bag.Add(IndirectRefToken.Value);
        //                 endPos = reader.Position;
        //                 break;
        //             case PdfTokenType.DictionaryEnd:
        //                 CurrentState.Dict = CurrentState.GetDictionaryFromBag();
        //                 if (StateStack.Count > 0)
        //                 {
        //                     var last = StateStack[^1];
        //                     last.Bag.Add(CurrentState.Dict);
        //                     CurrentState = last;
        //                     StateStack.RemoveAt(StateStack.Count-1);
        //                     break;
        //                 }
        //                 else
        //                 {
        //                     endPos = reader.Position;
        //                     completed = true;
        //                     return false;
        //                 }
        //             case PdfTokenType.ArrayEnd:
        //                 CurrentState.Array = CurrentState.GetArrayFromBag();
        //                 if (StateStack.Count > 0)
        //                 {
        //                     var last = StateStack[^1];
        //                     last.Bag.Add(CurrentState.Array);
        //                     CurrentState = last;
        //                     StateStack.RemoveAt(StateStack.Count-1);
        //                     break;
        //                 }
        //                 else
        //                 {
        //                     endPos = reader.Position;
        //                     completed = true;
        //                     return false;
        //                 }
        // 
        //             default:
        //                 throw new ApplicationException("Unknown object encountered.");
        //         }
        // 
        //     }
        // 
        //     return false;
        // }
    }
}
