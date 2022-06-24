using System;
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
        private ParseState currentState = ParseState.None;
        private bool completed = false;

        private PdfName currentKey;
        private IParsedLazyObj obj = null;

        internal IParsedLazyObj GetCompletedObject()
        {
            if (!completed)
            {
                throw new InvalidOperationException("GetCompletedObject() called before parsing was finished.");
            }
            currentState = ParseState.None;
            currentKey = null;
            startPos = default;
            currentLength = default;
            var toReturn = obj;
            obj = null;
            completed = false;
            return toReturn;
        }

        public IParsedLazyObj ParseNestedItem(ReadOnlySpan<byte> buffer, int startAt)
        {
            while (ParseNestedItemPart(buffer, startAt)) { }

            return GetCompletedObject();
        }

        private int startPos;
        private int currentLength;
        internal bool ParseNestedItemPart(ReadOnlySpan<byte> buffer, int startAt)
        {
            startPos = startAt;
            var lastStart = 0;
            while ((startPos = PdfSpanLexer.TryReadNextToken(buffer, out var tokenType, lastStart=startPos, out currentLength)) != -1)
            {
                switch (tokenType)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.StringObj:
                        AddValue(buffer, tokenType);
                        startPos += currentLength;
                        continue;
                    case PdfTokenType.DictionaryStart:
                        if (obj != null)
                        {
                            startPos += currentLength;
                            var end = startPos;
                            if (!NestedUtil.AdvanceToDictEnd(buffer, ref end, out _))
                            {
                                return false;
                            }

                            currentLength = end - startPos;
                            AddValue(buffer, PdfTokenType.DictionaryStart);
                            startPos += currentLength;
                            continue;
                        }
                        currentState = ParseState.ReadDictKey;
                        obj = new PdfDictionary();
                        startPos += currentLength;
                        continue;
                    case PdfTokenType.ArrayStart:
                        if (obj != null)
                        {
                            startPos += currentLength;
                            var end = startPos;
                            if (!NestedUtil.AdvanceToArrayEnd(buffer, ref end, out _))
                            {
                                return false;
                            }

                            currentLength = end - startPos;
                            AddValue(buffer, PdfTokenType.ArrayStart);
                            startPos += currentLength;
                            continue;
                        }
                        obj = new PdfArray();
                        currentState = ParseState.ReadArray;
                        startPos += currentLength;
                        continue;
                    case PdfTokenType.NumericObj:
                        var current = PdfSpanLexer.TryReadNextToken(buffer, out var secondType, startPos + currentLength,
                            out int secondLength);
                        if (current == -1)
                        {
                            return false;
                        }
   
                        if (secondType != PdfTokenType.NumericObj)
                        {
                            AddValue(buffer, tokenType);
                            startPos += currentLength;
                            continue;
                        }

                        current = PdfSpanLexer.TryReadNextToken(buffer, out var thirdType, current + secondLength,
                            out int thirdLength);
                        if (current == -1)
                        {
                            return false;
                        }

                        if (thirdType != PdfTokenType.IndirectRef)
                        {
                            AddValue(buffer, tokenType);
                            startPos += currentLength;
                            continue;
                        }
                        currentLength = current + thirdLength - startPos;
                        AddValue(buffer, PdfTokenType.IndirectRef);
                        startPos += currentLength;
                        continue;
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

            if (!completed)
            {
                throw CommonUtil.DisplayDataErrorException(buffer, lastStart,
                    $"Parsing ended unexpectedly for looking for {currentState.ToString()}, remaining data");
            }

            return false;
        }

        void AddValue(ReadOnlySpan<byte> buffer, PdfTokenType type)
        {
            switch (currentState)
            {
                case ParseState.ReadDictKey:
                    currentKey = _ctx.NameParser.Parse(buffer, startPos, currentLength);
                    currentState = ParseState.ReadDictValue;
                    return;
                case ParseState.ReadDictValue:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, buffer, startPos, currentLength);
                    (obj as PdfDictionary)[currentKey] = item;
                    currentKey = null;
                    currentState = ParseState.ReadDictKey;
                    return;
                }
                case ParseState.ReadArray:
                {
                    var item = _ctx.GetPdfItem((PdfObjectType) type, buffer, startPos, currentLength);
                    (obj as PdfArray).Add(item);
                    return;
                }
            }
        }
    }
}
