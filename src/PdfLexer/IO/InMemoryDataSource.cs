using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    public class InMemoryDataSource : IPdfDataSource
    {

        private readonly byte[] _data;
        // TODO in memory larger than int.maxvalue bytes
        // TODO -> Memory<byte>??
        public InMemoryDataSource(ParsingContext ctx, byte[] data)
        {
            Context = ctx;
            _data = data;
        }

        public long TotalBytes => _data.LongLength;
        public bool ReturnsCompleteData => true;
        public bool SupportsCloning => true;

        public ParsingContext Context { get; }

        public IPdfDataSource Clone() => throw new NotImplementedException(); // TODO currently setting Context.Current* on source so not sharable

        public Stream GetStream(long startPosition)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new MemoryStream(_data, (int)startPosition, _data.Length - (int)startPosition, false, true);
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new MemoryStream(_data, (int)startPosition, desiredBytes, false, true);
        }

        public void GetData(long startPosition, int desiredBytes, out ReadOnlySpan<byte> data)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }
            var start = (int)startPosition;
            if (desiredBytes > _data.Length - start)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            data = new Span<byte>(_data, start, _data.Length - start);
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            if (startPosition > int.MaxValue)
            {
                throw new NotSupportedException(
                    "In memory data source does not support offsets greater than Int32.MaxValue");
            }

            var start = (int)startPosition;
            if (requiredBytes > _data.Length - start)
            {
                throw new ApplicationException();
            }
            stream.Write(_data, (int)startPosition, requiredBytes);
        }

        public IPdfObject GetIndirectObject(XRefEntry xref)
        {
            ReadOnlySpan<byte> buffer = _data;
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                return Context.GetWrappedIndirectObject(xref, buffer.Slice((int)xref.Offset, xref.MaxLength));
            }
            catch (PdfTokenMismatchException)
            {
                Context.Error($"XRef offset for {xref.Reference} was not valid.");
                if (!TryRepairXRef(xref, out var repaired))
                {
                    throw;
                }
                Context.CurrentOffset = repaired.Offset;
                Context.Error("XRef offset repairs to " + repaired.Offset);
                UpdateXref(repaired);
                return Context.GetWrappedIndirectObject(xref, buffer.Slice((int)repaired.Offset, repaired.MaxLength));
            }
        }

        private void UpdateXref(XRefEntry repaired)
        {
            Context.Document.xrefEntries[repaired.Reference.GetId()] = repaired;
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            ReadOnlySpan<byte> buffer = _data;
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                Context.UnwrapAndCopyObjData(buffer.Slice((int)xref.Offset, xref.MaxLength), destination);
            }
            catch (PdfTokenMismatchException)
            {
                Context.Error($"XRef offset for {xref.Reference} was not valid.");
                if (!TryRepairXRef(xref, out var repaired))
                {
                    throw;
                }
                Context.CurrentOffset = repaired.Offset;
                Context.Error("XRef offset repairs to " + repaired.Offset);
                UpdateXref(repaired);
                Context.UnwrapAndCopyObjData(buffer.Slice((int)xref.Offset, xref.MaxLength), destination);
            }
        }

        public IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            var scanner = new Scanner(Context, _data, 0);
            var nextOs = 0;
            IPdfObject toReturn = null;
            var orig = Context.Options.Eagerness;
            Context.Options.Eagerness = Eagerness.FullEager;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < _data.Length - 1)
            {
                scanner.Position = nextOs;
                scanner.ScanToToken(IndirectSequences.obj);
                nextOs = scanner.Position + 1;
                if (!scanner.TryScanBackTokens(2, 20))
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                scanner.SkipCurrent();
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                scanner.SkipCurrent();
                if (scanner.Peek() != PdfTokenType.StartObj)
                {
                    continue;
                }
                scanner.SkipCurrent();

                if (scanner.Peek() != type)
                {
                    continue;
                }
                var obj = scanner.GetCurrentObject();
                if (matcher(obj))
                {
                    toReturn = obj;
                }
            }
            Context.Options.Eagerness = orig;
            return toReturn;
        }

        private bool TryRepairXRef(XRefEntry entry, out XRefEntry repaired)
        {
            var min = 0; //Math.Max((int)(entry.Offset - 200), 0);
            var max = _data.Length - 1; // Math.Min((int)(entry.Offset + 200), _data.Length)-1;
            var scanner = new Scanner(Context, _data, min);
            repaired = new XRefEntry
            {
                IsFree = entry.IsFree,
                Reference = entry.Reference,
                Type = XRefType.Normal,
                Source = this,
            };
            var nextOs = scanner.Position;
            while (scanner.CurrentTokenType != PdfTokenType.EOS && nextOs < max)
            {
                scanner.Position = nextOs;
                scanner.ScanToToken(IndirectSequences.obj);
                nextOs = scanner.Position + 1;
                if (!scanner.TryScanBackTokens(2, 20))
                {
                    continue;
                }
                var os = scanner.Position;
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                var on = scanner.GetCurrentObject().GetValue<PdfNumber>();
                if (on != entry.Reference.ObjectNumber)
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.NumericObj)
                {
                    continue;
                }
                var gen = scanner.GetCurrentObject().GetValue<PdfNumber>();
                if (gen != entry.Reference.Generation)
                {
                    continue;
                }
                if (scanner.Peek() != PdfTokenType.StartObj)
                {
                    continue;
                }

                if (repaired.Offset == 0)
                {
                    repaired.Offset = os;
                    repaired.MaxLength = _data.Length - os;
                    continue;
                }
                var currDiff = (int)Math.Abs(repaired.Offset - entry.Offset);
                var newDiff = (int)Math.Abs(os - entry.Offset);
                if (newDiff < currDiff)
                {
                    repaired.Offset = os;
                    repaired.MaxLength = _data.Length - os;
                }
            }

            return repaired.Offset != 0;
        }
    }
}
