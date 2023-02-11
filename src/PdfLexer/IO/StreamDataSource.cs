using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer.IO;

internal class StreamDataSource : StreamBase
{
    public override bool IsEncrypted => Document.IsEncrypted;

    public StreamDataSource(PdfDocument doc, Stream stream) : base(doc, stream, true) { }

    public StreamDataSource(PdfDocument doc, Stream stream, bool leaveOpen) : base(doc, stream, leaveOpen) { }

    public override IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref)
    {
        if (xref.MaxLength < ctx.Options.MaxMemorySegment)
        {
            var requiredBytes = xref.MaxLength;
            var startPosition = xref.Offset;
            if (requiredBytes > TotalBytes - startPosition)
            {
                throw new ApplicationException("More data requested from data source than available.");
            }

            var data = new byte[requiredBytes+1];
            _stream.Seek(startPosition, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = _stream.Read(data, total, requiredBytes - total)) > 0)
            {
                total += read;
            }
            data[requiredBytes] = (byte)'\n';
            var ims = new InMemoryDataSource(Document, data, startPosition);
            xref.CachedSource = new WeakReference<IPdfDataSource>(ims);
            try
            {
                return ims.GetIndirectObject(ctx, xref);
            } catch
            {
                xref.CachedSource = null;
                // fallback to stream
            }
        }
        return this.ReadWrappedFromStream(xref);
    }

    public override void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination)
        => this.WriteWrappedFromStream(xref, destination.Stream);
}
