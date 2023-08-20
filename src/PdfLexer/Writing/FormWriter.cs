using PdfLexer.Content;
using PdfLexer.DOM;
using System.Numerics;

namespace PdfLexer.Writing;

public sealed class FormWriter : FormWriter<double> 
{
    public FormWriter(PageSize size, PageUnit unit = PageUnit.Points) : base(size, unit)
    {

    }

    public FormWriter(PdfRect<double> bbox, PageUnit unit = PageUnit.Points) : base(bbox, unit)
    {

    }

    public FormWriter(double width, double height, PageUnit unit = PageUnit.Points) : base(width, height, unit)
    {
    }
}

public class FormWriter<T> : ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    private PdfArray BBox;
    public FormWriter(PageSize size, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = PageSizeHelpers.GetMediaBox(size);
    }

    public FormWriter(PdfRect<T> bbox, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = PdfRectangle.FromContentModel(bbox).NativeObject;
    }

    public FormWriter(T width, T height, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = new PdfArray {
            PdfCommonNumbers.Zero, PdfCommonNumbers.Zero,
            FPC<T>.Util.ToPdfNumber<T>(width), FPC<T>.Util.ToPdfNumber<T>(height)
        };
    }

    public new XObjForm Complete()
    {
        var contents = base.Complete();
        var form = new XObjForm();
        form.NativeObject.Dictionary[PdfName.Resources] = Resources;
        form.NativeObject.Dictionary[PdfName.BBox] = BBox;
        form.NativeObject.Contents = contents;
        return form;
    }
}
