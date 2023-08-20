using PdfLexer.Content;
using PdfLexer.Filters;

namespace PdfLexer.DOM;

public class XObjForm
{
    public PdfStream NativeObject { get; }
    public XObjForm(PdfStream dict)
    {
        NativeObject = dict;
    }

    internal XObjForm()
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
    }

    public XObjForm(PageSize size)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = PageSizeHelpers.GetMediaBox(size);
    }

    public XObjForm(PdfRect<double> bbox)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = PdfRectangle.FromContentModel(bbox).NativeObject;
    }

    public XObjForm(double width, double height)
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
        NativeObject.Dictionary[PdfName.BBox] = new PdfArray { 
            PdfCommonNumbers.Zero, PdfCommonNumbers.Zero,
            new PdfDoubleNumber(width), new PdfDoubleNumber(height) 
        };
    }

    public static implicit operator XObjForm(PdfStream str) => new XObjForm(str);
    public static implicit operator PdfStream(XObjForm form) => form.NativeObject;

    public PdfRectangle? BBox
    {
        get => NativeObject.Dictionary.Get<PdfArray>(PdfName.BBox);
        set => NativeObject.Dictionary.Set(PdfName.BBox, value?.NativeObject);
    }

    public PdfDictionary? Resources
    {
        get => NativeObject.Dictionary.Get<PdfDictionary>(PdfName.Resources);
        set => NativeObject.Dictionary.Set(PdfName.Resources, value);
    }

    public PdfStreamContents? Contents
    {
        get => NativeObject.Contents;
        set => NativeObject.Contents = value == null ? PdfStreamContents.Empty : value;
    }

    // Type -> XObject (optional)
    // Subtype => Form
    // FormType => 1 (optional)
    // BBox req => PdfRectangle clipping box
    // Matrix opt => transformation matrix
    // Resources => same as page
    // Group opt
    // Ref opt
    // Metadata opt
    // PieceInfo opt
    // LastModified opt
    // StructParent req if structural
    // StructParents req if structural
    // OPT opt
    // OC opt
    // Name req in pdf v1 -> not recommended
    // + stream items

    // TODO remove ctx ref
    public static XObjForm FromPage(PdfPage page)
    {
        var form = new XObjForm();
        var contents = page.Contents.ToList();
        if (contents.Count == 1)
        {
            form.Contents = contents[0].Contents;
        } else
        {
            var flate = new ZLibLexerStream();
            foreach(var stream in contents)
            {
                using var str = stream.Contents.GetDecodedStream();
                str.CopyTo(flate.Stream);
            }
            form.Contents = flate.Complete();
        }

        form.BBox = page.CropBox.NativeObject.CloneShallow();
        form.Resources = page.Resources.CloneShallow();
        return form;
    }
}
