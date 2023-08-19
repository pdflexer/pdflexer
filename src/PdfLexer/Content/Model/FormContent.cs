using PdfLexer.DOM;
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
        var form = (XObjForm)Stream;
        if (form.BBox == null)
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
        var bb = form.BBox.ToContentModel<T>();
        return GraphicsState.CTM.GetTransformedBoundingBox(bb);
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
        GraphicsState = GraphicsState with
        {
            CTM = transformation * GraphicsState.CTM,
            Clipping = GraphicsState.Clipping == null ? null :
                GraphicsState.Clipping.Select(x => { var c = x.ShallowClone(); c.TM = transformation * c.TM; return c; }).ToList()

        };
    }

    public void TransformInitial(GfxMatrix<T> transformation)
    {
        GraphicsState = GraphicsState with
        {
            CTM = GraphicsState.CTM * transformation,
            Clipping = GraphicsState.Clipping == null ? null :
                GraphicsState.Clipping.Select(x => { var c = x.ShallowClone(); c.TM = c.TM * transformation; return c; }).ToList()

        };
    }


    public void ClipExcept(PdfRect<T> rect)
    {
        throw new NotImplementedException("ClipExcept not supported for forms. Complete action on individual content items inside form.");
    }

    public void ClipFrom(GfxState<T> other)
    {
        throw new NotImplementedException("ClipFrom not supported for forms. Complete action on individual content items inside form.");
    }
}