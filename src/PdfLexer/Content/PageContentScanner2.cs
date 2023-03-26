﻿using System.ComponentModel.Design;
using System.Text;

namespace PdfLexer.Content;

public ref struct PageContentScanner2
{
    private readonly bool FlattenForms;
    private readonly PdfDictionary Page;
    private readonly ParsingContext Context;
    private readonly List<PdfStream> NextStreams;
    internal MultiPageState State;
    private PdfStream? CurrentStream;
    private PdfStream? NextForm;
    private string? NextFormName;
    private string? CurrentFormName;

    private int SkipOps;
    private DecodedStreamContents? CurrentBuffer;

    internal List<ScannerInfo> stack;
    internal ulong CurrentStreamId;

    public ContentStreamScanner Scanner;
    public PdfDictionary? CurrentForm;
    public IReadOnlyList<ScannerInfo> ReadStack { get => stack; }
    public string? FormName { get => CurrentFormName; }

    public PdfDictionary Resources;

    /// <summary>
    /// Reads content from a PDF Page
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="page"></param>
    /// <param name="flattenForms"></param>
    public PageContentScanner2(ParsingContext ctx, PdfDictionary page, bool flattenForms = false)
    {
        CurrentOperator = PdfOperatorType.Unknown;
        CurrentBuffer = null;
        FlattenForms = flattenForms;
        State = MultiPageState.Reading;
        Page = page;
        Context = ctx;
        stack = new List<ScannerInfo>();
        NextStreams = new List<PdfStream>();
        CurrentForm = null;
        NextForm = null;
        NextFormName = null;
        CurrentStream = null;
        CurrentFormName = null;
        SkipOps = -1;
        CurrentStreamId = 0;
        Resources = page.Get<PdfDictionary>(PdfName.Resources) ?? new PdfDictionary();
        if (!page.TryGetValue(PdfName.Contents, out var contents))
        {
            Scanner = new ContentStreamScanner(ctx, Array.Empty<byte>());
            return;
        }

        if (contents.GetPdfObjType() == PdfObjectType.ArrayObj)
        {
            var array = contents.GetValue<PdfArray>();
            foreach (var item in array)
            {
                var str = item.GetValueOrNull<PdfStream>();
                if (str == null)
                {
                    ctx.Error("Page scanning encounted missing or null content reference: " + str?.Type);
                    continue;
                }
                NextStreams.Add(str);
            }
        }
        else
        {
            var str = contents.GetValueOrNull<PdfStream>();
            if (str == null)
            {
                ctx.Error("Page scanning encounted missing or null content reference: " + str?.Type);
            }
            else
            {
                NextStreams.Add(str);
            }

        }
        if (NextStreams.Count == 0)
        {
            Scanner = new ContentStreamScanner(ctx, Array.Empty<byte>());
            return;
        }

        CurrentStream = NextStreams[0];
        NextStreams.RemoveAt(0);

        CurrentBuffer = CurrentStream.Contents.GetDecodedBuffer(true);
        Scanner = CurrentBuffer.GetScanner(Context);
        UpdateCurrentStreamId();
    }


    public PageContentScanner2(ParsingContext ctx, PdfDictionary page, PdfStream form)
    {
        var pr = page.Get<PdfDictionary>(PdfName.Resources);
        var fr = form.Dictionary.Get<PdfDictionary>(PdfName.Resources);
        Resources = GetResources(pr, fr);

        CurrentOperator = PdfOperatorType.Unknown;
        CurrentBuffer = null;
        FlattenForms = false;
        State = MultiPageState.ReadingForm;
        Page = page;
        Context = ctx;
        stack = new List<ScannerInfo>();
        NextStreams = new List<PdfStream>();
        CurrentForm = null;
        NextForm = null;
        NextFormName = null;
        CurrentStream = null;
        CurrentFormName = null;
        SkipOps = -1;
        CurrentStream = form;
        CurrentForm = form.Dictionary;
        CurrentFormName = "FNA";
        CurrentBuffer = form.Contents.GetDecodedBuffer(true);
        Scanner = CurrentBuffer.GetScanner(Context);
        CurrentStreamId = 0;
        if (CurrentForm.Get<PdfArray>(PdfName.Matrix) != null)
        {
            State = MultiPageState.CTMForm;
        }
        else
        {
            State = MultiPageState.ReadingForm;
        }
        UpdateCurrentStreamId();
    }

    public bool Advance()
    {
        if (State == MultiPageState.CTMForm)
        {
            State = MultiPageState.ReadingForm;
            return Advance();
        }
        if (State == MultiPageState.StartForm)
        {
            return PushForm();
        }
        if (State == MultiPageState.EndForm) { 
            PopForm();
            return Advance();
        }
        
        if (Scanner.Advance())
        {
            if (Scanner.CurrentOperator == PdfOperatorType.Do && FlattenForms)
            {
                if (!Scanner.TryGetCurrentOperation(out var op))
                {
                    Context.Error("Bad do op, skipping");
                    return Advance();
                }
                var doOp = (Do_Op)op;
                if (!TryGetForm(doOp.name, out var form))
                {
                    CurrentOperator = Scanner.CurrentOperator;
                    return true;
                }
                if (stack.Any(x => Object.ReferenceEquals(x.Stream, form.Dictionary)))
                {
                    // cyclic
                    Context.Error("Cyclic form reference: " + doOp.name);
                    CurrentOperator = Scanner.CurrentOperator;
                    return true;
                }

                NextForm = form;
                NextFormName = doOp.name.Value;
                State = MultiPageState.StartForm;
                CurrentOperator = PdfOperatorType.q;
                return true;
            }
            CurrentOperator = Scanner.CurrentOperator;
            return true;
        } else
        {
            if (State == MultiPageState.ReadingForm && FlattenForms) // may be reading form directly not from flattened, in that case exit
            {
                State = MultiPageState.EndForm;
                CurrentOperator = PdfOperatorType.Q;
                // PopForm();
                return true;
            }
            else if (NextStreams.Count > 0)
            {
                var dc = Scanner.GetDanglingOperandCount();
                if (dc > 0)
                {
                    // hack to deal with ops split across stream
                    // - need to revisit this, could be easily solved by
                    //   Operands having reference to their data but don't think
                    //   that is good idea since data is a span
                    // - this would likely blow up if the data that was split
                    //   is a /Form Do

                    var str = NextStreams[0];
                    using var tempBuff = str.Contents.GetDecodedBuffer(true);
                    var temp = tempBuff.GetScanner(Context);
                    if (SkipOps > -1)
                    {
                        temp.SetPosition(SkipOps);
                    }
                    temp.Advance();
                    
                    var dt = temp.GetDataForCurrent();
                    var lo = Scanner.Items[Scanner.Items.Length - dc];
                    var length = dt.Length;
                    var prev = Scanner.Data.Length - lo.StartAt;
                    Span<byte> tempData = new byte[length + prev + 1];
                    Scanner.Data.Slice(lo.StartAt).CopyTo(tempData);
                    tempData[prev] = (byte)' ';
                    dt.CopyTo(tempData.Slice(prev + 1));
                    SkipOps = temp.Position;
                    Scanner = new ContentStreamScanner(Context, tempData);

                    CurrentBuffer?.Dispose();
                    CurrentBuffer = null;
                    return Advance();
                }
                else
                {
                    CurrentStream = NextStreams[0];
                    NextStreams.RemoveAt(0);
                    CurrentBuffer?.Dispose();
                    CurrentBuffer = CurrentStream.Contents.GetDecodedBuffer(true);
                    Scanner = CurrentBuffer.GetScanner(Context);
                    UpdateCurrentStreamId();

                    if (SkipOps > -1)
                    {
                        Scanner.SetPosition(SkipOps);
                        SkipOps = -1;
                    }
                    return Advance();
                }
            }
            CurrentBuffer?.Dispose();
            return false;
        }
    }

    public PdfOperatorType CurrentOperator;

    public IPdfOperation? GetCurrentOperation()
    {
        TryGetCurrentOperation(out var op);
        return op;
    }

    public bool TryGetCurrentOperation([NotNullWhen(true)] out IPdfOperation? op)
    {
        if (State == MultiPageState.CTMForm)
        {
            var mtx = CurrentForm?.Get<PdfArray>(PdfName.Matrix);
            if (mtx == null)
            {
                op = new cm_Op(1m, 0m, 0m, 1m, 0m, 0m);
            }
            else
            {
                op = new cm_Op(
                mtx[0].GetAs<PdfNumber>(), mtx[1].GetAs<PdfNumber>(),
                mtx[2].GetAs<PdfNumber>(), mtx[3].GetAs<PdfNumber>(),
                mtx[4].GetAs<PdfNumber>(), mtx[5].GetAs<PdfNumber>());
            }
            return true;
        }
        if (State == MultiPageState.StartForm) { op = q_Op.Value; return true; }
        if (State == MultiPageState.EndForm) { op = Q_Op.Value; return true; }
        return Scanner.TryGetCurrentOperation(out op);
    }

    public int GetScannerPosition()
    {
        if (State == MultiPageState.Reading || State == MultiPageState.ReadingForm)
        {
            return Scanner.Position;
        }
        throw new NotSupportedException("Scanner position not supported in form flattening when in transition.");
    }

    public ReadOnlySpan<byte> GetCurrentData()
    {
        if (State == MultiPageState.CTMForm) { throw new NotSupportedException("Get current data not currently supported in form flattening"); }
        if (State == MultiPageState.StartForm) { throw new NotSupportedException("Get current data not currently supported in form flattening"); }
        if (State == MultiPageState.EndForm) { throw new NotSupportedException("Get current data not currently supported in form flattening"); }
        return Scanner.GetDataForCurrent();
    }

    private bool PushForm()
    {
        stack.Add(new ScannerInfo
        {
            Contents = CurrentBuffer!,
            Stream = CurrentStream!,
            Position = Scanner.Position,
            Form = CurrentFormName != null,
            FormName = CurrentFormName!,
        });

        CurrentStream = NextForm;
        CurrentForm = NextForm!.Dictionary;
        CurrentFormName = NextFormName;
        CurrentBuffer = NextForm.Contents.GetDecodedBuffer(true);
        Scanner = CurrentBuffer.GetScanner(Context);
        NextForm = null;
        NextFormName = null;

        UpdateCurrentStreamId();

        var pr = Page.Get<PdfDictionary>(PdfName.Resources);
        var fr = CurrentForm.Get<PdfDictionary>(PdfName.Resources);
        Resources = GetResources(pr, fr);

        if (CurrentForm.Get<PdfArray>(PdfName.Matrix) != null)
        {
            State = MultiPageState.CTMForm;
            CurrentOperator = PdfOperatorType.cm;
            return true;
        }
        else
        {
            State = MultiPageState.ReadingForm;
            return Advance();
        }
    }

    private void PopForm()
    {
        State = MultiPageState.Reading;
        var prev = stack.Last();
        State = prev.Form ? MultiPageState.ReadingForm : MultiPageState.Reading;
        stack.Remove(prev);
        CurrentBuffer?.Dispose();
        CurrentBuffer = prev.Contents;
        CurrentStream = prev.Stream;
        CurrentForm = prev.Form ? prev.Stream.Dictionary : null;
        CurrentFormName = prev.FormName;
        Scanner = CurrentBuffer.GetScanner(Context);
        Scanner.SetPosition(prev.Position);

        var pr = Page.Get<PdfDictionary>(PdfName.Resources);
        var fr = CurrentForm?.Get<PdfDictionary>(PdfName.Resources);
        Resources = GetResources(pr, fr);

        UpdateCurrentStreamId();
    }

    private void UpdateCurrentStreamId()
    {
        if (CurrentStream == null)
        {
            CurrentStreamId = 0;
            return;
        }
    }

    internal bool TryGetForm(PdfName name, [NotNullWhen(true)] out PdfStream? found)
    {
        if (CurrentForm != null && TryGetForm(CurrentForm, out found, out var isForm))
        {
            if (isForm)
            {
                return true;
            }
            else
            {
                found = null;
                return false;
            }
        }

        if (TryGetForm(Page, out found, out isForm) && isForm)
        {
            return true;
        }

        found = null;
        return false;

        bool TryGetForm(PdfDictionary obj, [NotNullWhen(true)] out PdfStream? form, out bool isForm)
        {
            form = null;
            isForm = false;
            if (obj.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) &&
                res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj) &&
                xobj.TryGetValue(name, out var formObj)
                )
            {
                if (formObj == null) { form = null; return false; }
                formObj = formObj.Resolve();
                if (formObj.Type == PdfObjectType.DictionaryObj)
                {
                    // special handling for forms that have no contents
                    // review all the special forms and see how to handle those
                    var fd = (PdfDictionary)formObj;
                    if (fd.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                        st == PdfName.Form)
                    {
                        isForm = true;
                        form = new PdfStream(fd, new PdfByteArrayStreamContents(new byte[0]));
                    }
                    else
                    {
                        form = null;
                    }
                }
                else if (formObj.Type == PdfObjectType.StreamObj)
                {
                    form = (PdfStream)formObj;
                    if (form.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                        st == PdfName.Form)
                    {
                        isForm = true;
                    }
                }
                else { form = null!; }
                return true; // special case with isform false
            }
            return false;
        }
    }

    internal bool TryGetColorSpace(PdfName name, [NotNullWhen(true)] out IPdfObject? found)
    {
        if (
            Resources.TryGetValue<PdfDictionary>(PdfName.ColorSpace, out var cs)
            && cs.TryGetValue(name, out found)
        )
        {
            found = found.Resolve();
            return true;
        }
        found = null;
        return false;
    }

    internal bool TryGetGraphicsState(PdfName name, [NotNullWhen(true)] out PdfDictionary? found)
    {
        if (
            Resources.TryGetValue<PdfDictionary>(PdfName.ExtGState, out var gs)
            && gs.TryGetValue<PdfDictionary>(name, out found, errorOnMismatch: false)
        )
        {
            return true;
        }
        found = null;
        return false;
    }

    internal bool TryGetPropertyList(PdfName name, [NotNullWhen(true)] out PdfDictionary? found)
    {
        if (
            Resources.TryGetValue<PdfDictionary>("Properties", out var props)
            && props.TryGetValue<PdfDictionary>(name, out found, errorOnMismatch: false)
        )
        {
            return true;
        }
        found = null;
        return false;
    }

    internal bool TryGetFont(PdfName name, [NotNullWhen(true)] out PdfDictionary? found)
    {
        if (
            Resources.TryGetValue<PdfDictionary>(PdfName.Font, out var fonts)
            && fonts.TryGetValue<PdfDictionary>(name, out found, errorOnMismatch: false)
        )
        {
            return true;
        }
        found = null;
        return false;
    }

    internal bool TryGetXObject(PdfName name, [NotNullWhen(true)] out PdfStream? found, out bool isForm)
    {
        isForm = false;
        found = null;
        if (
            Resources.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj)
            && xobj.TryGetValue(name, out var formObj)
                )
        {
            if (formObj == null) { return false; }
            formObj = formObj.Resolve();
            if (formObj.Type == PdfObjectType.DictionaryObj)
            {
                // special handling for forms that have no contents
                // review all the special forms and see how to handle those
                var fd = (PdfDictionary)formObj;
                found = new PdfStream(fd, new PdfByteArrayStreamContents(new byte[0]));
                if (fd.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                    st == PdfName.Form)
                {
                    isForm = true;
                }
            }
            else if (formObj.Type == PdfObjectType.StreamObj)
            {
                found = (PdfStream)formObj;
                if (found.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                    st == PdfName.Form)
                {
                    isForm = true;
                }
            }
            else { found = null; return false; } // todo warning with type
            return true;
        }
        return false;
    }

    internal bool TryGetXObj(PdfName name, [NotNullWhen(true)] out PdfStream? found, out bool isForm)
    {
        if (CurrentForm != null && TryGetXObjInt(CurrentForm, out found, out isForm))
        {
            return true;
        }

        if (TryGetXObjInt(Page, out found, out isForm))
        {
            return true;
        }

        found = null;
        return false;

        bool TryGetXObjInt(PdfDictionary obj, [NotNullWhen(true)] out PdfStream? form, out bool isForm)
        {
            form = null;
            isForm = false;
            if (obj.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) &&
                res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj) &&
                xobj.TryGetValue(name, out var formObj)
                )
            {
                if (formObj == null) { form = null; return false; }
                formObj = formObj.Resolve();
                if (formObj.Type == PdfObjectType.DictionaryObj)
                {
                    // special handling for forms that have no contents
                    // review all the special forms and see how to handle those
                    var fd = (PdfDictionary)formObj;
                    form = new PdfStream(fd, new PdfByteArrayStreamContents(new byte[0]));
                    if (fd.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                        st == PdfName.Form)
                    {
                        isForm = true;
                    }
                }
                else if (formObj.Type == PdfObjectType.StreamObj)
                {
                    form = (PdfStream)formObj;
                    if (form.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                        st == PdfName.Form)
                    {
                        isForm = true;
                    }
                }
                else { form = null; return false; } // todo warning with type
                return true;
            }
            return false;
        }
    }


    internal PdfDictionary? GetFontObj(PdfName name)
    {
        if (CurrentForm != null 
            && CurrentForm.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) 
            && res.TryGetValue<PdfDictionary>(PdfName.Font, out var fonts)
            && fonts.TryGetValue<PdfDictionary>(name, out var fnt))
        {
            return fnt;
        }

        if (stack != null && stack.Count > 0)
        {
            for (var i = stack.Count - 1; i > -1; i--)
            {
                var dict = stack[i].Stream.Dictionary?.Get<PdfDictionary>(PdfName.Resources);
                if (dict != null && dict.TryGetValue<PdfDictionary>(PdfName.Font, out var fd)
                        && fd.TryGetValue<PdfDictionary>(name, out var f))
                {
                    return f;
                }
            }
        }

        if (
            Page.TryGetValue<PdfDictionary>(PdfName.Resources, out res)
            && res.TryGetValue<PdfDictionary>(PdfName.Font, out fonts)
            && fonts.TryGetValue<PdfDictionary>(name, out fnt))
        {
            return fnt;
        }
        Context.Error($"Unable to find font for {name.Value}, using fallback.");
        return null;
    }


    private static PdfDictionary GetResources(PdfDictionary? parentResources, PdfDictionary? childResources)
    {
        if (parentResources == null)
        {
            return childResources ?? new PdfDictionary();
        }
        else if (childResources == null)
        {
            return parentResources;
        }
        else
        {
            return MergeResources(parentResources, childResources);
        }
    }
    private static PdfDictionary MergeResources(PdfDictionary parent, PdfDictionary child)
    {
        // does not follow pdf spec, more lenient with inheritance
        var result = parent.CloneShallow();
        foreach (var kvp in child)
        {
            if (kvp.Value.Type != PdfObjectType.DictionaryObj)
            {
                continue; // warn?
            }
            if (!result.TryGetValue(kvp.Key, out var sd) || sd.Type != PdfObjectType.DictionaryObj)
            {
                sd = new PdfDictionary();
                result[kvp.Key] = sd;
            } else
            {
                var tc = (PdfDictionary)sd;
                sd = tc.CloneShallow();
                result[kvp.Key] = sd;
            }
            var psd = (PdfDictionary)sd;
            var csd = (PdfDictionary)kvp.Value;
            foreach (var skvp in csd)
            {
                psd[skvp.Key] = skvp.Value;
            }
        }
        return result;

    }
}
