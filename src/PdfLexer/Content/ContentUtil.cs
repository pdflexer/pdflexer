using PdfLexer.DOM;
using PdfLexer.Filters;

namespace PdfLexer.Content;

public class ContentUtil
{
    public static PdfDictionary ConvertInlineImagesToXObjs(ParsingContext ctx, PdfPage page, bool clone = false) 
        => ConvertInlineImagesToXObjs(ctx, page.NativeObject, new HashSet<PdfStream>(), clone);

    private static PdfStreamContents RunSingle(ParsingContext ctx, PdfDictionary xobjs, PdfStream str)
    {
        var fw = new FlateWriter();
        var last = 0;
        using var buff = str.Contents.GetDecodedBuffer();
        var scanner = new ContentStreamScanner(ctx, buff.GetData());

        while (scanner.Advance())
        {
            if (scanner.CurrentOperator == PdfOperatorType.EI && scanner.TryGetCurrentOperation(out var op))
            {
                var im = (InlineImage_Op)op;
                var i = 1;
                var nm = "Im" + i;
                while (xobjs.ContainsKey(nm))
                {
                    i++;
                    nm = "Im" + i;
                }
                var imageStream = im.ConvertToStream();
                xobjs[nm] = imageStream.Indirect();

                var start = scanner.Items[scanner.Position - 2].StartAt;
                var end = scanner.Items[scanner.Position].StartAt + 2;
                fw.Stream.Write(scanner.Data.Slice(last, start - last)); // up to BI
                fw.Stream.WriteByte((byte)'\n');
                last = end;
                Do_Op.WriteLn(nm, fw.Stream);
            }
        }

        if (last != 0)
        {
            fw.Stream.Write(scanner.Data.Slice(last));
            return fw.Complete();
        }
        else
        {
            return str.Contents;
        }
    }
    private static PdfDictionary ConvertInlineImagesToXObjs(ParsingContext ctx, PdfDictionary page, HashSet<PdfStream> callStack, bool clone=false)
    {
        var (pu, xo) = UpdateForms(ctx, page, callStack, clone);
        page = pu;


        var content = page.Get(PdfName.Contents);
        if (content == null) { return page; }
        switch (content.Type)
        {
            case PdfObjectType.StreamObj:
                page[PdfName.Contents] = new PdfStream(RunSingle(ctx, xo, (PdfStream)content)).Indirect();
                break;
            case PdfObjectType.ArrayObj:
                var arr = (PdfArray)content;
                var newArr = new PdfArray();
                foreach (var item in arr)
                {
                    var ss = item.GetAs<PdfStream>();
                    newArr.Add(new PdfStream(RunSingle(ctx, xo, ss)).Indirect());
                }
                page[PdfName.Contents] = newArr;
                break;
        }

        return page;
    }

    private static PdfStream ConvertInlineImagesToXObjsForm(ParsingContext ctx, PdfStream form, HashSet<PdfStream> callStack, bool clone = false)
    {
        if (callStack.Contains(form)) { return form; }
        callStack.Add(form);

        var (dict, xo) = UpdateForms(ctx, form.Dictionary, callStack, clone);

        form = new PdfStream(dict, RunSingle(ctx, xo, form));

        return form;
    }

    private static (PdfDictionary Main, PdfDictionary XObjs) UpdateForms(ParsingContext ctx, PdfDictionary pageOrForm, HashSet<PdfStream> callStack, bool clone = false)
    {
        var pr = pageOrForm.GetOrCreateValue<PdfDictionary>(PdfName.Resources);
        var xo = pr.GetOrCreateValue<PdfDictionary>(PdfName.XObject);

        if (clone)
        {
            pageOrForm = pageOrForm.CloneShallow();
            pr = pr.CloneShallow();
            pageOrForm[PdfName.Resources] = pr;
            xo = xo.CloneShallow();
            pr[PdfName.XObject] = xo;
        }

        foreach (var (name, item) in xo)
        {
            var xoi = item.Resolve().GetAsOrNull<PdfStream>();
            if (xoi == null) { continue; }
            if (xoi.Dictionary.Get<PdfName>(PdfName.TYPE) == PdfName.Form)
            {
                xo[name] = ConvertInlineImagesToXObjsForm(ctx, xoi, callStack, clone);
            }
        }
        return (pageOrForm, xo);
    }
}
