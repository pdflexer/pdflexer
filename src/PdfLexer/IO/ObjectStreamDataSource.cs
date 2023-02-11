using PdfLexer.Lexing;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System.Diagnostics;

namespace PdfLexer.IO;

/// <summary>
/// Wrapper for pdf 1.5 object streams data
/// </summary>
internal class ObjectStreamDataSource : IPdfDataSource
{
    private byte[] _data;
    private readonly List<int> _offsets;
    private readonly int _start;
    private readonly int SourceObject;

    public ObjectStreamDataSource(PdfDocument doc, int sourceObject, byte[] data, List<int> oss, int start)
    {
        Document = doc;
        SourceObject = sourceObject;
        _data = data;
        _offsets = oss;
        _start = start;
    }
    public long TotalBytes => _data.Length;

    public bool Disposed { get; private set; }

    public bool IsEncrypted => false;

    public PdfDocument Document { get; }

    public void CopyData(long startPosition, int requiredBytes, Stream stream)
    {
        stream.Write(_data, (int)startPosition, requiredBytes);
    }

    public void GetData(ParsingContext ctx, long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
    {
        ctx.CurrentSource = this;
        ctx.CurrentOffset = startPosition;
        buffer = _data;
        buffer = buffer.Slice((int)startPosition, requiredBytes);
    }

    public Stream GetDataAsStream(ParsingContext ctx, long startPosition, int desiredBytes)
    {
        throw new NotImplementedException();
    }

    public Stream GetStream(ParsingContext ctx, long startPosition)
    {
        throw new NotImplementedException();
    }

    public IPdfObject GetIndirectObject(ParsingContext ctx, XRefEntry xref)
    {
        Debug.Assert(xref.ObjectStreamNumber == SourceObject);
        var os = _offsets[xref.ObjectIndex] + _start;
        ctx.CurrentSource = this;
        ctx.CurrentOffset = os;
        ReadOnlySpan<byte> data = _data;
        if (os > data.Length - 1)
        {
            ctx.Error($"XRef stream entry {xref} past length of data: {data.Length}");
            return PdfNull.Value;
        }
        data = data.Slice(os);
        var orig = ctx.Options.Eagerness;
        ctx.Options.Eagerness = Eagerness.FullEager; // TODO fix so this works lazy
        var obj = ctx.GetPdfItem(data, 0, out _, Document);
        ctx.Options.Eagerness = orig;
        return obj;
    }

    public void CopyIndirectObject(ParsingContext ctx, XRefEntry xref, WritingContext destination)
    {
        var os = _offsets[xref.ObjectIndex] + _start;
        ctx.CurrentSource = this;
        ctx.CurrentOffset = os;
        ReadOnlySpan<byte> data = _data;
        var scanner = new Scanner(ctx, data.Slice(os), 0);
        this.CopyRawObjFromSpan(ctx, ref scanner, destination);
    }

    public void Dispose()
    {
        _data = null!;
        Disposed = true;
    }
}
