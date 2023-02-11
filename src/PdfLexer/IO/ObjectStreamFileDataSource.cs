using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System.Buffers;
using System.Diagnostics;

namespace PdfLexer.IO;


/// <summary>
/// Wrapper for pdf 1.5 object streams based on streams
/// </summary>
internal class ObjectStreamFileDataSource : StreamBase
{
    private List<int> _offsets;
    private int _start;
    private int SourceObject;

    public override bool IsEncrypted => false;

    public ObjectStreamFileDataSource(PdfDocument doc, int sourceObject, Stream data, List<int> oss, int start) : base(doc, data, false)
    {
        SourceObject = sourceObject;
        _offsets = oss;
        _start = start;
    }

    public override IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref)
    {
        // todo use sequence?
        Debug.Assert(xref.ObjectStreamNumber == SourceObject);
        var data = GetRented(ctx, xref);
        var orig = ctx.Options.Eagerness;
        ctx.Options.Eagerness = Eagerness.FullEager; // TODO fix so this works lazy
        var obj = ctx.GetPdfItem(data, 0, out _);
        ctx.Options.Eagerness = orig;
        ArrayPool<byte>.Shared.Return(data);
        return obj;
    }

    public override void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination)
    {
        // todo use sequence?
        var data = GetRented(ctx, xref);
        var scanner = new Scanner(ctx, data, 0);
        this.CopyRawObjFromSpan(ref scanner, destination);
        ArrayPool<byte>.Shared.Return(data);
    }

    private byte[] GetRented(ParsingContext ctx, XRefEntry xref)
    {
        var os = _offsets[xref.ObjectIndex] + _start;
        var length = (int)(_stream.Length - os);
        if (_offsets.Count > xref.ObjectIndex + 1)
        {
            length = _offsets[xref.ObjectIndex + 1] + _start - os;
        }
        ctx.CurrentSource = this;
        ctx.CurrentOffset = os;
        var buffer = ArrayPool<byte>.Shared.Rent(length+1);
        _stream.Seek(os, SeekOrigin.Begin);
        int total = 0;
        int read;
        while ((read = _stream.Read(buffer, total, length - total)) > 0)
        {
            total += read;
        }
        buffer[length] = 0;
        return buffer;
    }
}
