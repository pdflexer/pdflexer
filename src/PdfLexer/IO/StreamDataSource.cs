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
    internal class StreamDataSource : IPdfDataSource
    {
        private ParsingContext _ctx;
        private Stream _stream;

        public StreamDataSource(ParsingContext ctx, Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("Streams must be seekable.");
            }
            TotalBytes = stream.Length;
            _ctx = ctx;
            _stream = stream;
        }
        public long TotalBytes { get; }

        public bool SupportsCloning => false;

        public ParsingContext Context => _ctx;

        public IPdfDataSource Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            if (requiredBytes > TotalBytes - startPosition)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            // can manually copy without substream at some point if need to
            using var sub = new SubStream(_stream, startPosition, requiredBytes, false);
            sub.CopyTo(stream);
        }

        public void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
        {
            if (requiredBytes > TotalBytes - startPosition)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }

            var data = new byte[requiredBytes];
            _stream.Seek(startPosition, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = _stream.Read(data, total, requiredBytes-total)) > 0)
            {
                total += read;
            }
            buffer = data;
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            if (desiredBytes > TotalBytes - startPosition)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new SubStream(_stream, startPosition, desiredBytes, false);
        }

        public Stream GetStream(long startPosition)
        {
            if (startPosition > TotalBytes-1)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }
            Context.CurrentSource = this;
            Context.CurrentOffset = startPosition; // TODO move this somewhere else
            return new SubStream(_stream, startPosition, TotalBytes-startPosition, false);
        }



        public IPdfObject GetIndirectObject(XRefEntry xref)
        {
            var buffer = GetRented(xref);
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                var result = Context.GetWrappedIndirectObject(xref, buffer);
                ArrayPool<byte>.Shared.Return(buffer);
                return result;
            }
            catch (PdfLexerTokenMismatchException e)
            {
                throw new NotImplementedException("Repairs not implemented for FileStream.", e);
                // Context.Error($"XRef offset for {xref.Reference} was not valid.");
                // if (!TryRepairXRef(xref, out var repaired))
                // {
                //     throw;
                // }
                // Context.CurrentOffset = repaired.Offset;
                // Context.Error("XRef offset repairs to " + repaired.Offset);
                // UpdateXref(repaired);
                // return Context.GetWrappedIndirectObject(xref, buffer.Slice((int)repaired.Offset, repaired.MaxLength));
            }
        }

        private byte[] GetRented(XRefEntry xref)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(xref.MaxLength);
            _stream.Seek(xref.Offset, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = _stream.Read(buffer, total, xref.MaxLength - total)) > 0)
            {
                total += read;
            }
            return buffer;
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            var buffer = GetRented(xref);
            Context.CurrentSource = this;
            Context.CurrentOffset = xref.Offset;
            try
            {
                Context.UnwrapAndCopyObjData(buffer, destination);
                ArrayPool<byte>.Shared.Return(buffer);
            }
            catch (PdfLexerTokenMismatchException e)
            {
                throw new NotImplementedException("Repairs not implemented for FileStream.", e);
                // Context.Error($"XRef offset for {xref.Reference} was not valid.");
                // if (!TryRepairXRef(xref, out var repaired))
                // {
                //     throw;
                // }
                // Context.CurrentOffset = repaired.Offset;
                // Context.Error("XRef offset repairs to " + repaired.Offset);
                // UpdateXref(repaired);
                // Context.UnwrapAndCopyObjData(buffer.Slice((int)xref.Offset, xref.MaxLength), destination);
            }
        }

        public IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            throw new NotImplementedException("Repairs not implemented for FileStream.");
        }
    }
}
