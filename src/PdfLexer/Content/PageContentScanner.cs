using PdfLexer.Lexing;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfLexer.Content
{
    internal enum MultiPageState
    {
        Reading,
        ReadingForm,
        StartForm,
        EndForm,
    }
    public struct ScannerInfo
    {
        public PdfStream Contents { get; internal set; }
        public int Position { get; internal set; }
        public bool Form { get; internal set; }
        public string FormName { get; internal set; }
    }
    public ref struct PageContentScanner
    {
        private bool FlattenForms;
        private PdfDictionary Page;
        private ParsingContext Context;
        private List<PdfStream> NextStreams;
        private MultiPageState State;
        
        private PdfStream CurrentStream;
        private PdfStream NextForm;
        private string NextFormName;
        private string CurrentFormName;

        public PageContentScanner(ParsingContext ctx, PdfDictionary page, bool flattenForms=false)
        {
            FlattenForms = flattenForms;
            State = MultiPageState.Reading;
            Page = page;
            Context = ctx;
            stack =  new List<ScannerInfo>();
            NextStreams = new List<PdfStream>();
            CurrentForm = null;
            NextForm = null;
            NextFormName = null;
            CurrentStream = null;
            CurrentFormName = null;
            SkipBytes = 0;
            if (!page.TryGetValue(PdfName.Contents, out var contents))
            {
                Scanner = new ContentScanner(ctx, new byte[0]);
                return;
            }
            
            if (contents.GetPdfObjType() == PdfObjectType.ArrayObj)
            {
                var array = contents.GetValue<PdfArray>();
                foreach (var item in array)
                {
                    var str = item.GetValue<PdfStream>(false);
                    if (str == null)
                    {
                        ctx.Error("Page scanning encounted missing or null content reference: " + str?.Type);
                        continue;
                    }
                    NextStreams.Add(str);
                }
            } else
            {
                var str = contents.GetValue<PdfStream>(false);
                if (str == null)
                {
                    ctx.Error("Page scanning encounted missing or null content reference: " + str?.Type);
                } else
                {
                    NextStreams.Add(str);
                }
                
            }
            if (NextStreams.Count == 0)
            {
                Scanner = new ContentScanner(ctx, new byte[0]);
                return;
            }

            CurrentStream = NextStreams[0];
            NextStreams.RemoveAt(0);
            Scanner = new ContentScanner(ctx, CurrentStream.Contents.GetDecodedData(ctx));
        }

        private List<ScannerInfo> stack;
        public IReadOnlyList<ScannerInfo> ReadStack { get => stack; }
        public string FormName { get => CurrentFormName; }
        public ContentScanner Scanner;
        public PdfOperatorType CurrentOperator => Scanner.CurrentOperator;
        public List<OperandInfo> Operands => Scanner.Operands;
        public PdfDictionary CurrentForm;
        private int SkipBytes;
        public PdfOperatorType Peek()
        {
        start:
            var nxt = Scanner.Peek();
            if (nxt == PdfOperatorType.Do)
            {
                if (!Scanner.TryGetCurrentOperation(out var op))
                {
                    Context.Error("Bad do op, skipping");
                    Scanner.SkipCurrent();
                    goto start;
                }
                var doOp = (Do_Op)op;
                if (!FlattenForms || !TryGetForm(doOp.name, out var form))
                {
                    return nxt;
                }
                if (stack.Any(x => Object.ReferenceEquals(x.Contents, form)))
                {
                    // cyclic
                    Context.Error("Cyclic form reference: " + doOp.name);
                    return nxt;
                }
                NextForm = form;
                NextFormName = doOp.name.Value;
                State = MultiPageState.StartForm;
                return PdfOperatorType.q;
            } else if (nxt == PdfOperatorType.EOC)
            {
                if (State == MultiPageState.ReadingForm)
                {
                    State = MultiPageState.EndForm;
                    return PdfOperatorType.Q;
                } else if (NextStreams.Count > 0)
                {

                    if (Scanner.Operands.Count > 0)
                    {
                        // hack to deal with ops split across stream
                        // - need to revisit this, could be easily solved by
                        //   Operands having reference to their data but don't think
                        //   that is good idea since data is a span
                        // - this would likely blow up if the data that was split
                        //   is a /Form Do
                        var str = NextStreams[0];
                        var data = str.Contents.GetDecodedData(Context);
                        var temp = new ContentScanner(Context, data, SkipBytes);
                        temp.Peek();
                        var length = temp.Position + temp.CurrentLength;
                        var prev = Scanner.Data.Length - Scanner.Operands[0].StartAt;
                        System.Span<byte> tempData = new byte[length + prev + 1];
                        Scanner.Data.Slice(Scanner.Operands[0].StartAt).CopyTo(tempData);
                        tempData[prev] = (byte)' ';
                        temp.Data.Slice(SkipBytes, length).CopyTo(tempData.Slice(prev + 1));
                        SkipBytes = length+1;
                        Scanner = new ContentScanner(Context, tempData);
                        return Scanner.Peek();
                    } else
                    {
                        CurrentStream = NextStreams[0];
                        NextStreams.RemoveAt(0);
                        Scanner = new ContentScanner(Context, CurrentStream.Contents.GetDecodedData(Context), SkipBytes);
                        SkipBytes = 0;
                        return Scanner.Peek();
                    }
                }
            }
            return nxt;
        }

        public void SkipCurrent()
        {
            if (State == MultiPageState.StartForm) 
            {
                PushForm();
                return;
            }
            if (State == MultiPageState.EndForm) { PopForm(); return; }
            Scanner.SkipCurrent();
        }

        public IPdfOperation GetCurrentOperation()
        {
            if (State == MultiPageState.StartForm) { return q_Op.Value; }
            if (State == MultiPageState.EndForm) { return Q_Op.Value; }
            return Scanner.GetCurrentOperation();
        }

        public bool TryGetCurrentOperation(out IPdfOperation op)
        {
            if (State == MultiPageState.StartForm) { op = q_Op.Value; return true; }
            if (State == MultiPageState.EndForm) { op = Q_Op.Value; return true; }
            return Scanner.TryGetCurrentOperation(out op);
        }

        public ReadOnlySpan<byte> GetCurrentData()
        {
            if (State == MultiPageState.StartForm) { throw new NotSupportedException("Get current data not currently supported in form flattening"); }
            if (State == MultiPageState.EndForm) { throw new NotSupportedException("Get current data not currently supported in form flattening"); }
            return Scanner.GetCurrentData();
        }

        private void PushForm()
        {
            Scanner.SkipCurrent(); // Do
            stack.Add(new ScannerInfo
            {
                Contents = CurrentStream,
                Position = Scanner.Position,
                Form = CurrentFormName != null,
                FormName = CurrentFormName
            });
            State = MultiPageState.ReadingForm;
            CurrentStream = NextForm;
            CurrentForm = NextForm.Dictionary;
            CurrentFormName = NextFormName;
            Scanner = new ContentScanner(Context, NextForm.Contents.GetDecodedData(Context));
            NextForm = null;
            NextFormName = null;
        }

        private void PopForm()
        {
            State = MultiPageState.Reading;
            var prev = stack.Last();
            State = prev.Form ? MultiPageState.ReadingForm : MultiPageState.Reading;
            stack.Remove(prev);
            CurrentStream = prev.Contents;
            CurrentForm = prev.Form ? prev.Contents.Dictionary : null;
            CurrentFormName = prev.FormName;
            Scanner = new ContentScanner(Context, prev.Contents.Contents.GetDecodedData(Context), prev.Position);
        }

        private bool TryGetForm(PdfName name, out PdfStream found)
        {
            if (CurrentForm != null && TryGetForm(CurrentForm, out found, out var isForm))
            {
                if (isForm)
                {
                    return true;
                } else
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

            bool TryGetForm(PdfDictionary obj, out PdfStream form, out bool isForm)
            {
                isForm = false;
                if (obj.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) &&
                    res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj) &&
                    xobj.TryGetValue(name, out var formObj)
                    )
                {
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
                        } else
                        {
                            form = null;
                        }
                    } else if (formObj.Type == PdfObjectType.StreamObj)
                    {
                        form = (PdfStream)formObj;
                        if (form.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                            st == PdfName.Form)
                        {
                            isForm = true;
                        }
                    } else { form = null; }
                    return true;
                }
                form = null;
                return false;
            }
        }
    }
}
