using System;
using PdfLexer.IO;
using PdfLexer.Lexing;

namespace PdfLexer.Parsers.Nested
{
    /// <summary>
    /// Todo: clean this up.. mirrors the re-entrant sequence setup but doesn't need to as all data
    /// is supposed to be in memory for this
    /// </summary>
    internal class LazyNestedSpanParser
    {
        private readonly ParsingContext _ctx;
        public LazyNestedSpanParser(ParsingContext ctx)
        {
            _ctx = ctx;
        }

        public IParsedLazyObj ParseNestedItem(IPdfDataSource source, long offset, ReadOnlySpan<byte> buffer, int startAt)
        {
            ParseState state = default;
            IParsedLazyObj obj = null;
            PdfName currentKey = null;
            bool completed = false;
            var lastStart = 0;
            while ((startAt = PdfSpanLexer.TryReadNextToken(buffer, out var tokenType, lastStart=startAt, out var currentLength)) != -1)
            {
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.StringObj:
                        AddValue(buffer, startAt, currentLength, tokenType);
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.DictionaryStart:
                        if (obj != null)
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
                        state = ParseState.ReadDictKey;
                        obj = new PdfDictionary();
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (obj != null)
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
                        obj = new PdfArray();
                        state = ParseState.ReadArray;
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.NumericObj:
                        var current = PdfSpanLexer.TryReadNextToken(buffer, out var secondType, startAt + currentLength,
                            out int secondLength);
                        if (current == -1)
                        {
                             goto Done;
                        }
   
                        if (secondType != PdfTokenType.NumericObj)
                        {
                            AddValue(buffer, startAt, currentLength, tokenType);
                            startAt += currentLength;
                            continue;
                        }

                        current = PdfSpanLexer.TryReadNextToken(buffer, out var thirdType, current + secondLength,
                            out int thirdLength);
                        if (current == -1)
                        {
                             goto Done;
                        }

                        if (thirdType != PdfTokenType.IndirectRef)
                        {
                            AddValue(buffer, startAt, currentLength, tokenType);
                            startAt += currentLength;
                            continue;
                        }
                        currentLength = current + thirdLength - startAt;
                        AddValue(buffer, startAt, currentLength, PdfTokenType.IndirectRef);
                        startAt += currentLength;
                        continue;
                    case PdfTokenType.IndirectRef:
                        throw new ApplicationException("Unexpected indirect Ref token found.");
                    case PdfTokenType.DictionaryEnd:
                        var dict = obj as PdfDictionary;
                        dict.IsModified = false;
                        completed = true;
                        goto Done;
                    case PdfTokenType.ArrayEnd:
                        var arr = obj as PdfArray;
                        arr.IsModified = false;
                        completed = true;
                        goto Done;
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }


            }

            Done:

            if (!completed)
            {
                throw CommonUtil.DisplayDataErrorException(buffer, lastStart,
                    $"Parsing ended unexpectedly for looking for {state.ToString()} or Dict End, remaining data");
            }

            return obj;

            void AddLazyValue(IPdfDataSource source, long offset, int startPos, int length, PdfTokenType type, bool hadIndirect)
            {
                // TODO lazy support with span... should we allow? would need to track offset
                switch (state)
                {
                    case ParseState.ReadDictValue:
                    {
                        var lazy = new PdfLazyObject
                        {
                            Source = source,
                            Offset = offset + startPos,
                            Length = length,
                            HasLazyIndirect = hadIndirect,
                            Type = (PdfObjectType) type
                        };
                        (obj as PdfDictionary)[currentKey] = lazy;
                        currentKey = null;
                        state = ParseState.ReadDictKey;
                        return;
                    }
                    case ParseState.ReadArray:
                    {
                        var lazy = new PdfLazyObject
                        {
                            Source = source,
                            Offset = offset + startPos,
                            Length = length,
                            HasLazyIndirect = hadIndirect,
                            Type = (PdfObjectType) type
                        };
                        (obj as PdfArray).Add(lazy);
                        return;
                    }
                }
            }

            void AddValue(ReadOnlySpan<byte> buffer, int startPos, int length, PdfTokenType type)
            {
                switch (state)
                {
                    case ParseState.ReadDictKey:
                        currentKey = _ctx.NameParser.Parse(buffer, startPos, length);
                        state = ParseState.ReadDictValue;
                        return;
                    case ParseState.ReadDictValue:
                    {
                        var item = _ctx.GetKnownPdfItem((PdfObjectType) type, buffer, startPos, length);
                        (obj as PdfDictionary)[currentKey] = item;
                        currentKey = null;
                        state = ParseState.ReadDictKey;
                        return;
                    }
                    case ParseState.ReadArray:
                    {
                        var item = _ctx.GetKnownPdfItem((PdfObjectType) type, buffer, startPos, length);
                        (obj as PdfArray).Add(item);
                        return;
                    }
                }
            }


        }
    }
}
