using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using PdfLexer.Objects.Nested;
using PdfLexer.Objects.Parsers;

namespace PdfLexer.Objects
{
    public interface ITokenizer
    {
        IPdfObject Tokenize(IPdfDataSource source, long objectStart);
    }

    public interface IParser<T> where T : IParsedPdfObject
    {
        T ParsePdfObject(IPdfObject obj);
    }

    public class Test
    {
        public Test()
        {
            Stream stream = null;
            var reader = PipeReader.Create(stream);
            reader.TryRead(out var result);

            //result.Buffer
        }
    }

    public interface IPdfDataSource
    {
        IPdfObject RegisterObject(long startPosition, int length, PdfObjectType type, bool indirect);
        int FillData(long startPosition, int desiredBytes, out Span<byte> data);
        void CopyData(long startPosition, int desiredBytes, Stream stream);
        void CopyData(IPdfObject obj, Stream stream);
    }
    public class InMemoryDataSource : IPdfDataSource
    {
        private readonly byte[] _data;

        public InMemoryDataSource(byte[] data)
        {
            _data = data;
        }

        private Dictionary<IPdfObject, (long, int)> objects = new Dictionary<IPdfObject, (long, int)>();
        public IPdfObject RegisterObject(long startPosition, int length, PdfObjectType type, bool indirect)
        {
            var obj = new PdfObject
            {
                Source = this,
                IsIndirect = indirect,
                Parsed = null,
                Type = type
            };
            objects[obj] = (startPosition, length);
            return obj;
        }

        public int FillData(long startPosition, int desiredBytes, out Span<byte> data)
        {
            // TODO
            var start = (int) startPosition;
            if (desiredBytes > _data.Length - start)
            {
                desiredBytes = _data.Length - start;
            }
            data = new Span<byte>(_data, start, desiredBytes);
            return desiredBytes;
        }
        public void CopyData(long startPosition, int desiredBytes, Stream stream)
        {
            var start = (int) startPosition;
            if (desiredBytes > _data.Length - start)
            {
                desiredBytes = _data.Length - start;
            }
            stream.Write(_data, (int) startPosition, desiredBytes); // TODO
        }

        public void CopyData(IPdfObject obj, Stream stream)
        {
            var data = objects[obj];
            CopyData(data.Item1, data.Item2, stream);
        }
    }

    internal enum ReadResult
    {
        NotEnoughData,
        FoundObject,
        CompletedNested,
    }
    public ref struct NestedTokenizer
    {
        private ReadOnlySpan<byte> _data;
        private long currentDataStart;
        private IPdfDataSource _source;
        internal int currentStart;
        internal int currentLength;
        internal int beginningOffset;
        internal long startPosition;
        internal bool isFinished;
        internal NestedParseType runType;


        public PdfObjectType ObjectType { get; private set; }

        public NestedTokenizer(IPdfDataSource source, long objectStart, int initialBytes=250)
        {
            _source = source;
            currentDataStart = objectStart;
            source.FillData(objectStart, initialBytes, out var span);
            _data = span;
            startPosition = objectStart;
            currentStart = 0;
            beginningOffset = 0;
            currentLength = 0;
            isFinished = false;
            runType = NestedParseType.Unknown;
            ObjectType = PdfObjectType.NullObj;
        }

        public NestedTokenizer(IPdfDataSource source, long objectStart, ReadOnlySpan<byte> initialBytes)
        {
            _source = source;
            currentDataStart = objectStart;
            _data = initialBytes;
            startPosition = objectStart;
            currentStart = 0;
            beginningOffset = 0;
            currentLength = 0;
            isFinished = false;
            runType = NestedParseType.Unknown;
            ObjectType = PdfObjectType.NullObj;
        }

        public (long StartPos, int Length) GetCurrentInfo()
        {
            return (startPosition + currentStart, currentLength);
        }

        public (long StartPos, int Length) GetTotalInfo()
        {
            if (!isFinished)
            {
                throw new ApplicationException();
            }
            return (startPosition, (int)(currentDataStart-startPosition) + currentLength);
        }

        public bool Read()
        {
            var lastSuccess = true;
            while (true)
            {
                var result = ReadInternal(lastSuccess);
                switch (result)
                {
                    case ReadResult.FoundObject:
                        return true;
                    case ReadResult.CompletedNested:
                        isFinished = true;
                        return false;
                    default:
                        break;
                }
                
                _source.FillData(currentDataStart + currentStart, 2*(_data.Length - currentStart), out var span);
                currentDataStart = startPosition + currentDataStart + currentStart;
                _data = span;
                currentStart = 0;
                currentLength = 0;
                lastSuccess = false;
            }
        }

        private ReadResult ReadInternal(bool lastSuccess=true)
        {
            if (runType == NestedParseType.Unknown)
            {
                var whiteSpace = CommonUtil.FindNextToken(_data, out PdfTokenType initType);
                if (whiteSpace == -1)
                {
                    return ReadResult.NotEnoughData;
                }
                else if (initType != PdfTokenType.DictionaryStart && initType != PdfTokenType.ArrayStart)
                {
                    throw new ApplicationException("Non-dictionary and non-Array passed to NestedParser");
                }

                runType = initType == PdfTokenType.DictionaryStart ? NestedParseType.Dictionary : NestedParseType.Array;
                
                currentStart += whiteSpace;
                beginningOffset = whiteSpace;
                currentStart += initType == PdfTokenType.DictionaryStart ? 2 : 1; // start bytes
            } else
            {
                if (lastSuccess)
                {
                    // bump to next object
                    currentStart += currentLength;
                }
                
            }

            var startPos = PdfTokenizer.ReadNextToken(_data, out PdfTokenType type, currentStart, out int length);
            if (startPos == -1)
            {
                return ReadResult.NotEnoughData;
            }
            else
            {
                currentStart = startPos;
                currentLength = length;

                switch (type)
                {
                    case PdfTokenType.NameObj:
                    case PdfTokenType.BooleanObj:
                    case PdfTokenType.NumericObj:
                    case PdfTokenType.NullObj:
                    case PdfTokenType.StringObj:
                    case PdfTokenType.IndirectRef:
                        break;
                    case PdfTokenType.DictionaryStart:
                    case PdfTokenType.ArrayStart:
                        // TODO: determine approach for nested objects, this setup requires two passes
                        var rest = _data.Slice(currentStart);
                        currentLength = NestedUtils.CountNestedBytes(rest);
                        if (currentLength == -1)
                        {
                            return ReadResult.NotEnoughData;
                        }
                        break;
                    case PdfTokenType.DictionaryEnd:
                        if (runType != NestedParseType.Dictionary)
                        {
                            throw new ApplicationException("Unexpected dictionary end encountered.");
                        }
                        return ReadResult.CompletedNested;
                    case PdfTokenType.ArrayEnd:
                        if (runType != NestedParseType.Array)
                        {
                            throw new ApplicationException("Unexpected array end encountered.");
                        }
                        return ReadResult.CompletedNested;
                    default:
                        throw new ApplicationException("Unknown object encountered.");
                }


                ObjectType = (PdfObjectType)type;
                return ReadResult.FoundObject;
            }
        }
    }
    
}
