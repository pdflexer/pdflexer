using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Buffers;
using System.IO;

namespace PdfLexer.IO
{
    internal class StreamDataSource : StreamBase
    {
        public StreamDataSource(ParsingContext ctx, Stream stream) : base(ctx, stream, true)
        {

        }

        public override IPdfObject GetIndirectObject(XRefEntry xref)
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

        public override void CopyIndirectObject(XRefEntry xref, WritingContext destination)
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

        public override IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            throw new NotImplementedException("Repairs not implemented for FileStream.");
        }
    }
}
