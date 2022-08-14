using PdfLexer.DOM;
using PdfLexer.Filters;

namespace PdfLexer.Writing;

public sealed class PageWriter : ContentWriter, IDisposable
{
    private readonly PageWriteMode Mode;
    private PdfPage Page;
    public PageWriter(PdfPage page, PageWriteMode mode = PageWriteMode.Append, PageUnit unit = PageUnit.Points) : base(page.Resources, unit)
    {
        Page = page;
        Mode = mode;
    }

    public void Dispose()
    {
        if (Page == null) { return; }
        var data = base.Complete();
        switch (Mode)
        {
            case PageWriteMode.Replace:
                Page.NativeObject[PdfName.Contents] = PdfIndirectRef.Create(new PdfStream(data));
                break;
            case PageWriteMode.Pre:
                {
                    var arr = new PdfArray();
                    arr.Add(PdfIndirectRef.Create(new PdfStream(data)));
                    foreach (var existing in Page.Contents)
                    {
                        arr.Add(existing);
                    }
                    Page.NativeObject[PdfName.Contents] = arr;
                    break; ;
                }
            case PageWriteMode.Append:
                {
                    // have to copy to ensure q/Q
                    var fw = new FlateWriter();
                    q_Op.WriteLn(fw);
                    foreach (var existing in Page.Contents)
                    {
                        using var es = existing.Contents.GetDecodedStream();
                        es.CopyTo(fw);
                    }
                    Q_Op.WriteLn(fw);
                    var arr = new PdfArray();
                    arr.Add(PdfIndirectRef.Create(new PdfStream(fw.Complete())));
                    arr.Add(PdfIndirectRef.Create(new PdfStream(data)));
                    Page.NativeObject[PdfName.Contents] = arr;
                    break;
                }
        }
        Page = null!;
    }
}

public enum PageWriteMode
{
    Replace,
    Pre,
    Append
}
