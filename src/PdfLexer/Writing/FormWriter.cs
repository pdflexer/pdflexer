using PdfLexer.Content;
using PdfLexer.Content.Model;
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
    private readonly XObjForm Form;
    internal int CurrentMCID { get; set; }

    public FormWriter(PageSize size, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = PageSizeHelpers.GetMediaBox(size);
        Form = new XObjForm();
    }

    public FormWriter(PdfRect<T> bbox, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = PdfRectangle.FromContentModel(bbox).NativeObject;
        Form = new XObjForm();
    }

    public FormWriter(T width, T height, PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {
        BBox = new PdfArray {
            PdfCommonNumbers.Zero, PdfCommonNumbers.Zero,
            FPC<T>.Util.ToPdfNumber<T>(width), FPC<T>.Util.ToPdfNumber<T>(height)
        };
        Form = new XObjForm();
    }

    public FormWriter<T> BeginMarkedContent(StructureNode node)
    {
        var mcid = CurrentMCID++;
        var props = new PdfDictionary();
        props[PdfName.MCID] = new PdfIntNumber(mcid);
        var mc = new MarkedContent(node.Type) { InlineProps = props };
        MarkedContent(mc, accessibilityScope: true);
        node.XObjectContentItems.Add((Form, mcid));
        return this;
    }

    public new FormWriter<T> EndMarkedContent()
    {
        base.EndMarkedContent();
        return this;
    }

    public new XObjForm Complete()
    {
        var contents = base.Complete();
        Form.NativeObject.Dictionary[PdfName.Resources] = Resources;
        Form.NativeObject.Dictionary[PdfName.BBox] = BBox;
        Form.NativeObject.Contents = contents;
        return Form;
    }
}
