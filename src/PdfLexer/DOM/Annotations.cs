using PdfLexer.Content;

namespace PdfLexer.DOM;

public sealed class LinkAnnotation
{
    internal LinkAnnotation(
        PdfPage page,
        PdfDictionary annotation,
        StructureNode? structureDestinationTarget = null,
        PdfArray? structureDestinationTemplate = null)
    {
        Page = page;
        NativeObject = annotation;
        StructureDestinationTarget = structureDestinationTarget;
        StructureDestinationTemplate = structureDestinationTemplate;
    }

    public PdfPage Page { get; }

    public PdfDictionary NativeObject { get; }

    internal StructureNode? StructureDestinationTarget { get; }

    internal PdfArray? StructureDestinationTemplate { get; }
}

public sealed class WidgetAnnotation
{
    internal WidgetAnnotation(PdfDocument document, PdfPage page, PdfDictionary annotation, PdfDictionary field)
    {
        Document = document;
        Page = page;
        NativeObject = annotation;
        Field = field;
    }

    public PdfDocument Document { get; }

    public PdfPage Page { get; }

    public PdfDictionary NativeObject { get; }

    public PdfDictionary Field { get; }
}

public static class AnnotationFactory
{
    public static LinkAnnotation CreateLink(PdfPage page, PdfRect<double> rect, IPdfObject destination, string? contents = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        annotation[PdfName.Dest] = destination;
        page.AddAnnotation(annotation);
        return new LinkAnnotation(page, annotation);
    }

    public static LinkAnnotation CreateStructureLink(
        PdfPage page,
        PdfRect<double> rect,
        StructureNode destination,
        string contents,
        PdfArray? destinationTemplate = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        page.AddAnnotation(annotation);
        return new LinkAnnotation(
            page,
            annotation,
            destination,
            destinationTemplate ?? new PdfArray { PdfNull.Value, PdfName.Fit });
    }

    public static LinkAnnotation CreateLinkAction(PdfPage page, PdfRect<double> rect, PdfDictionary action, string? contents = null)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Link, contents);
        annotation[PdfName.A] = action;
        page.AddAnnotation(annotation);
        return new LinkAnnotation(page, annotation);
    }

    public static WidgetAnnotation CreateTextWidget(
        PdfDocument document,
        PdfPage page,
        PdfRect<double> rect,
        string fieldName,
        string? tooltip = null,
        bool print = true)
    {
        var annotation = CreateBaseAnnotation(page, rect, PdfName.Widget);
        annotation[PdfName.FT] = PdfName.Tx;
        if (print)
        {
            annotation[PdfName.F] = new PdfIntNumber(4);
        }

        var field = new PdfDictionary
        {
            [PdfName.FT] = PdfName.Tx,
            [PdfName.T] = new PdfString(fieldName),
            [PdfName.Kids] = new PdfArray { annotation.Indirect() }
        };

        if (!string.IsNullOrEmpty(tooltip))
        {
            field[(PdfName)"TU"] = new PdfString(tooltip);
            annotation[(PdfName)"TU"] = new PdfString(tooltip);
        }

        annotation[PdfName.Parent] = field.Indirect();
        page.AddAnnotation(annotation);

        var acroForm = document.Catalog.GetOrCreateValue<PdfDictionary>((PdfName)"AcroForm");
        var fields = acroForm.GetOrCreateValue<PdfArray>((PdfName)"Fields");
        fields.Add(field.Indirect());

        return new WidgetAnnotation(document, page, annotation, field);
    }

    private static PdfDictionary CreateBaseAnnotation(PdfPage page, PdfRect<double> rect, PdfName subtype, string? contents = null)
    {
        var annotation = new PdfDictionary
        {
            [PdfName.TypeName] = PdfName.Annot,
            [PdfName.Subtype] = subtype,
            [PdfName.P] = page.NativeObject.Indirect(),
            [PdfName.Rect] = PdfRectangle.FromContentModel(rect).NativeObject
        };

        if (!string.IsNullOrWhiteSpace(contents))
        {
            annotation[PdfName.Contents] = new PdfString(contents);
        }

        return annotation;
    }
}
