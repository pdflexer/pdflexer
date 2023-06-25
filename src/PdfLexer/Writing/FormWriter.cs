using PdfLexer.DOM;
using System.Numerics;

namespace PdfLexer.Writing;

public sealed class FormWriter : FormWriter<double> 
{
    public FormWriter(PageUnit unit = PageUnit.Points) : base(unit)
    {

    }
}

public class FormWriter<T> : ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    public FormWriter(PageUnit unit = PageUnit.Points) : base(new PdfDictionary(), unit)
    {

    }

    public new XObjForm Complete()
    {
        var contents = base.Complete();
        var form = new XObjForm();
        form.NativeObject.Dictionary[PdfName.Resources] = Resources;
        form.NativeObject.Contents = contents;
        return form;
    }

}
