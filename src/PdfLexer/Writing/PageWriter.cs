using PdfLexer.DOM;
using PdfLexer.Filters;
using System.Numerics;

namespace PdfLexer.Writing;

public sealed class PageWriter<T> : ContentWriter<T>, IDisposable where T : struct, IFloatingPoint<T>
{
    private readonly PageWriteMode Mode;
    private PdfPage Page;
    public PageWriter(PdfPage page, PageWriteMode mode = PageWriteMode.Append, PageUnit unit = PageUnit.Points) : base(page.Resources, unit)
    // append without encode / decode ->
    // need to first make sure can turn page into a form (no nested forms with resources on page)
    // mode == PageWriteMode.Append && page.NativeObject?.Get(PdfName.Contents)?.Resolve()?.Type == PdfObjectType.StreamObj ? new PdfDictionary() : 
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
                    var content = Page.NativeObject?.Get(PdfName.Contents)?.Resolve();
                    if (content == null)
                    {
                        Page.NativeObject[PdfName.Contents] = new PdfStream(data).Indirect();
                    } else // if (content.Type == PdfObjectType.ArrayObj)
                    {
                        // can't make form without rewritting to single stream so just write
                        // with Q q
                        var fw = new ZLibLexerStream();
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
                    } 
                    // else
                    // {
                    //     var fm = XObjForm.FromPage(Page);
                    //     var fm2 = new XObjForm(new PdfStream(data));
                    //     fm2.Resources = this.Resources;
                    //     var fw = new FlateWriter();
                    //     Do_Op.WriteLn("F1", fw);
                    //     Do_Op.WriteLn("F2", fw);
                    //     Page.Resources = new PdfDictionary();
                    //     Page.AddXObj("F1", fm.NativeObject);
                    //     Page.AddXObj("F1", fm2.NativeObject);
                    //     Page.NativeObject[PdfName.Contents] = new PdfStream(fw.Complete()).Indirect();
                    // }
                   
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
