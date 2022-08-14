using PdfLexer.Filters;

namespace PdfLexer.DOM;

public class XObjForm
{
    public PdfStream NativeObject { get; }
    public XObjForm(PdfStream dict)
    {
        NativeObject = dict;
    }

    public XObjForm()
    {
        NativeObject = new PdfStream();
        NativeObject.Dictionary[PdfName.Subtype] = PdfName.Form;
    }

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
            var flate = new FlateWriter();
            foreach(var stream in contents)
            {
                using var str = stream.Contents.GetDecodedStream();
                str.CopyTo(flate.Stream);
            }
            form.Contents = flate.Complete();
        }

        form.BBox = page.MediaBox.NativeObject.CloneShallow();
        form.Resources = page.Resources;
        return form;
    }
}
