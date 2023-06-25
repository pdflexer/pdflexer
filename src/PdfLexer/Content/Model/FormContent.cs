using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;

/// <summary>
/// Form content. This is analogous to a /Form Do PDF operation.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FormContent<T> : IContentGroup<T> where T : struct, IFloatingPoint<T>
{
    public ContentType Type { get; } = ContentType.Form;
    public required GfxState<T> GraphicsState { get; set; }
    public required PdfStream Stream { get; set; }
    public List<MarkedContent>? Markings { get; set; }
    public bool CompatibilitySection { get; set; }
    
    /// <summary>
    /// Parent page of this form. Required if resources are
    /// not included in form resource dictionary.
    /// </summary>
    public PdfDictionary? ParentPage { get; set; }


    public void Write(ContentWriter<T> writer)
    {
        writer.Form(Stream);
    }

    public List<IContentGroup<T>> Parse()
    {
        var parser = new ContentModelParser<T>(ParsingContext.Current, ParentPage ?? new PdfDictionary(), Stream, GraphicsState);
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
        throw new NotImplementedException("Area copying not supported for forms. Complete action on individual content items inside form.");
    }

    (IContentGroup<T>? Inside, IContentGroup<T>? Outside) IContentGroup<T>.Split(PdfRect<T> rect)
    {
        throw new NotImplementedException("Area splitting not supported for forms. Complete action on individual content items inside form.");
    }

    public void Transform(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with { CTM = transformation * GraphicsState.CTM };
        if (GraphicsState.Clipping != null)
        {
            foreach (var item in GraphicsState.Clipping)
            {
                item.TM = transformation * item.TM;
            }
        }
    }

    public void TransformInitial(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with { CTM = GraphicsState.CTM * transformation };
        if (GraphicsState.Clipping != null)
        {
            foreach (var item in GraphicsState.Clipping)
            {
                item.TM = item.TM * transformation;
            }
        }
    }
}