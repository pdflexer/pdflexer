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
            CurrentStream = null;
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
                    NextStreams.Add(item.GetValue<PdfStream>());
                }
            } else
            {
                NextStreams.Add(contents.GetValue<PdfStream>());
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
        public IReadOnlyList<ScannerInfo> FormStack { get => stack; }
        public ContentScanner Scanner;
        public PdfOperatorType CurrentOperator => Scanner.CurrentOperator;
        public List<OperandInfo> Operands => Scanner.Operands;
        public PdfDictionary CurrentForm;
        private int SkipBytes;
        public PdfOperatorType Peek()
        {
            var nxt = Scanner.Peek();
            if (nxt == PdfOperatorType.Do)
            {
                var doOp = (Do_Op)Scanner.GetCurrentOperation();
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

        private void PushForm()
        {
            State = MultiPageState.ReadingForm;
            var op = Scanner.GetCurrentOperation();
            Scanner.SkipCurrent(); // Do
            stack.Add(new ScannerInfo
            {
                Contents = CurrentStream,
                Position = Scanner.Position,
                Form = true,
                FormName = (op as Do_Op)?.name?.Value
            });
            CurrentStream = NextForm;
            CurrentForm = NextForm.Dictionary;
            Scanner = new ContentScanner(Context, NextForm.Contents.GetDecodedData(Context));
            NextForm = null;
        }

        private void PopForm()
        {
            State = MultiPageState.Reading;
            var prev = stack.Last();
            State = prev.Form ? MultiPageState.ReadingForm : MultiPageState.Reading;
            stack.Remove(prev);
            CurrentStream = prev.Contents;
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
                    xobj.TryGetValue<PdfStream>(name, out form, errorOnMismatch:false)
                    )
                {
                    if (form.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                          st == PdfName.Form)
                    {
                        isForm = true;
                    }
                    return true;
                }
                form = null;
                return false;
            }
        }
    }
}
