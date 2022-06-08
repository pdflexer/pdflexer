﻿using PdfLexer.Lexing;
using PdfLexer.Operators;
using PdfLexer.Parsers;
using System.Collections.Generic;
using System.Linq;

namespace PdfLexer.Content
{
    internal enum MultiPageState
    {
        Reading,
        ReadingForm,
        StartForm,
        EndForm,
    }
    internal struct ScannerInfo
    {
        public PdfStream Contents { get; set; }
        public int Position { get; set; }
        public bool Form { get; set; }
    }
    internal ref struct PageContentScanner
    {
        private PdfDictionary Page;
        private ParsingContext Context;
        private List<PdfStream> NextStreams;
        private MultiPageState State;
        private List<ScannerInfo> Scanners;
        private PdfStream CurrentStream;
        private PdfStream NextForm;
        public PageContentScanner(ParsingContext ctx, PdfDictionary page)
        {
            State = MultiPageState.Reading;
            Page = page;
            Context = ctx;
            Scanners =  new List<ScannerInfo>();
            NextStreams = new List<PdfStream>();
            CurrentForm = null;
            NextForm = null;
            CurrentStream = null;
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

        public ContentScanner Scanner;
        public PdfOperatorType CurrentOperator => Scanner.CurrentOperator;
        public List<OperandInfo> Operands => Scanner.Operands;
        public PdfDictionary CurrentForm;

        public PdfOperatorType Peek()
        {
            var nxt = Scanner.Peek();
            if (nxt == PdfOperatorType.Do)
            {
                var doOp = (Do_Op)Scanner.GetCurrentOperation();
                if (!TryGetForm(doOp.name, out var form))
                {
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
                    CurrentStream = NextStreams[0];
                    NextStreams.RemoveAt(0);
                    Scanner = new ContentScanner(Context, CurrentStream.Contents.GetDecodedData(Context));
                    return Scanner.Peek();
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
            Scanner.SkipCurrent(); // Do
            Scanners.Add(new ScannerInfo
            {
                Contents = CurrentStream,
                Position = Scanner.Position
            });
            CurrentStream = NextForm;
            CurrentForm = NextForm.Dictionary;
            Scanner = new ContentScanner(Context, NextForm.Contents.GetDecodedData(Context));
            NextForm = null;
        }

        private void PopForm()
        {
            State = MultiPageState.Reading;
            var prev = Scanners.Last();
            State = prev.Form ? MultiPageState.ReadingForm : MultiPageState.Reading;
            Scanners.Remove(prev);
            CurrentStream = prev.Contents;
            Scanner = new ContentScanner(Context, prev.Contents.Contents.GetDecodedData(Context), prev.Position);
        }

        private bool TryGetForm(PdfName name, out PdfStream found)
        {
            if (CurrentForm != null && TryGetForm(CurrentForm, out found))
            {
                return true;
            }

            if (TryGetForm(Page, out found))
            {
                return true;
            }

            found = null;
            return false;

            bool TryGetForm(PdfDictionary obj, out PdfStream form)
            {
                if (obj.TryGetValue<PdfDictionary>(PdfName.Resources, out var res) &&
                    res.TryGetValue<PdfDictionary>(PdfName.XObject, out var xobj) &&
                    xobj.TryGetValue<PdfStream>(name, out form, errorOnMismatch:false) &&
                    form.Dictionary.TryGetValue<PdfName>(PdfName.Subtype, out var st) &&
                          st == PdfName.Form)
                {
                    return true;
                }
                form = null;
                return false;
            }
        }
    }
}
