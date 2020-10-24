using System;
using PdfLexer.Objects.Parsers;

namespace PdfLexer.Objects.Nested
{
public ref struct NestedParser
    {
        private ReadOnlySpan<byte> _data;

        internal int currentStart;
        internal int currentLength;
        private int beginning;
        private bool isFinished;
        private NestedParseType runType;

        public PdfObjectType ObjectType { get; private set; }

        public NestedParser(ReadOnlySpan<byte> data)
        {
            _data = data;
            currentStart = 0;
            currentLength = 0;
            beginning = 0;
            isFinished = false;
            runType = NestedParseType.Unknown;
            ObjectType = PdfObjectType.NullObj;
        }

        public ReadOnlySpan<byte> GetCurrentTokenSpan()
        {
            return _data.Slice(currentStart, currentLength);
        }

        public bool Completed()
        {
            return isFinished;
        }

        public ReadOnlySpan<byte> GetFullSpan()
        {
            if (!isFinished)
            {
                throw new ApplicationException("Parsing was not completed, cannot get full span.");
            }
            return _data.Slice(beginning, currentLength-beginning);
        }

        public int GetFullLength()
        {
            if (!isFinished)
            {
                throw new ApplicationException("Parsing was not completed, cannot get full length.");
            }
            return currentLength;
        }

        public bool Read()
        {
            if (runType == NestedParseType.Unknown)
            {
                var whiteSpace = CommonUtil.FindNextToken(_data, out PdfTokenType initType);
                if (whiteSpace == -1)
                {
                    return false;
                }
                else if (initType != PdfTokenType.DictionaryStart && initType != PdfTokenType.ArrayStart)
                {
                    throw new ApplicationException("Non-dictionary and non-Array passed to NestedParser");
                }

                runType = initType == PdfTokenType.DictionaryStart ? NestedParseType.Dictionary : NestedParseType.Array;
                
                currentStart += whiteSpace;
                beginning = whiteSpace;
                currentStart += initType == PdfTokenType.DictionaryStart ? 2 : 1; // start bytes
            } else
            {
                // bump to next object
                currentStart += currentLength;
            }



            var used = CommonUtil.FindNextToken(_data.Slice(currentStart), out PdfTokenType type);
            if (used == -1)
            {
                return false;
            }
            else
            {
                currentStart += used;
                var rest = _data.Slice(currentStart);
                var finished = false;
                var usedBytes = 0;
                switch (type)
                {
                    case PdfTokenType.NameObj:
                        usedBytes = NameParser.CountNameBytes(rest);
                        finished = usedBytes != -1;
                        break;
                    case PdfTokenType.BooleanObj:
                        finished = BoolParser.GetBool(rest, out bool result, out usedBytes); ;
                        break;
                    case PdfTokenType.DictionaryStart:
                    case PdfTokenType.ArrayStart:
                        // TODO: determine approach for nested objects, this setup requires two passes
                        usedBytes = NestedUtils.CountNestedBytes(rest);
                        finished = usedBytes != -1;
                        break;
                    case PdfTokenType.NumericObj:
                        usedBytes = NumberParser.CountNumberBytes(rest);
                        finished = usedBytes != -1;
                        break;
                    case PdfTokenType.NullObj:
                        finished = NullParser.GetNull(rest, out ReadOnlySpan<byte> nullBytes);
                        usedBytes = nullBytes.Length;
                        break;
                    case PdfTokenType.StringObj:
                        finished = StringParser.GetString(rest, out ReadOnlySpan<byte> stringBytes);
                        usedBytes = stringBytes.Length;
                        break;
                    case PdfTokenType.IndirectRef:
                        usedBytes = IndirectParser.CountIndirectRef(rest);
                        finished = usedBytes != 0;
                        break;
                    case PdfTokenType.DictionaryEnd:
                        if (runType != NestedParseType.Dictionary)
                        {
                            throw new ApplicationException("Unexpected dictionary end encountered.");
                        }
                        currentLength = currentStart + 2;
                        isFinished = true;
                        return false;
                    case PdfTokenType.ArrayEnd:
                        if (runType != NestedParseType.Array)
                        {
                            throw new ApplicationException("Unexpected array end encountered.");
                        }
                        currentLength = currentStart + 1;
                        isFinished = true;
                        return false;
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }

                if (!finished)
                {
                    return false;
                }
                ObjectType = (PdfObjectType)type;
                currentLength = usedBytes;
                return true;
            }
        }
    }
}