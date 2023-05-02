using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

internal class FormContent<T> : IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Form;
    public required GfxState<T> GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }

    public void Write(ContentWriter<T> writer)
    {
        writer.Form(Stream);
    }

    public List<IContentGroup<T>> Parse(PdfDictionary parentPage)
    {
        var parser = new ContentModelParser<T>(ParsingContext.Current, parentPage, Stream, GraphicsState);
        return parser.Parse();
    }

    public PdfRect<T> GetBoundingBox()
    {
        var x = GraphicsState.CTM.E;
        var y = GraphicsState.CTM.F;
        return new PdfRect<T>
        {
            LLx = x,
            LLy = y,
            URx = x + GraphicsState.CTM.A,
            URy = y + GraphicsState.CTM.D
        };
    }

    // not able to accurately get bounding box for form
    // without fully processing it


    IContentGroup<T>? IContentGroup<T>.CopyArea(PdfRect<T> rect)
    {
        throw new NotImplementedException();
    }

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect)
    {
        throw new NotImplementedException();
    }
}