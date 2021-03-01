﻿using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.IO
{
    public class WritableInMemorySource : IPdfDataSource
    {
        public Stream Stream = new MemoryStream();
        public WritableInMemorySource(ParsingContext ctx)
        {
            Context = ctx;

        }
        public long TotalBytes => throw new NotSupportedException();

        public bool ReturnsCompleteData => false;

        public bool SupportsCloning =>  false;

        public ParsingContext Context {get;}

        public IPdfDataSource Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void FillData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(long startPosition)
        {
            throw new NotImplementedException();
        }

        private Dictionary<PdfIndirectRef, PdfIndirectReference> sharedStack = new Dictionary<PdfIndirectRef, PdfIndirectReference>();
        public IPdfObject AddItem(IPdfObject obj)
        {
            sharedStack.Clear();
            return AddItem(obj, sharedStack);
        }

        internal IPdfObject AddItem(IPdfObject obj, Dictionary<PdfIndirectRef, PdfIndirectReference> refStack)
        {
            if (obj is PdfLazyObject lz)
            {
                if (lz.HasLazyIndirect || !lz.IsModified())
                {
                    var rz = lz.Resolve();
                    if (rz is PdfDictionary dict)
                    {
                        return AddItem(dict, refStack);
                    } else if (rz is PdfArray arr)
                    {
                        return AddItem(arr, refStack);
                    } else
                    {
                        throw new NotImplementedException($"PdfLazyObject of type {rz.GetType()} not implemented for copy");
                    }
                } else
                {
                    var init = Stream.Position;
                    lz.Source.CopyData(lz.Offset, lz.Length, Stream);
                    var copy = new PdfLazyObject
                    {
                        LazyObjectType = lz.LazyObjectType,
                        HasLazyIndirect = false,
                        Source = this,
                        Offset = init,
                        Length = lz.Length,
                    };
                    return copy;
                }
            } else if (obj is PdfIndirectRef ir)
            {
                if (ir.IsOwned(Context.SourceId)) { return ir; }
                if (refStack.TryGetValue(ir, out var copied))
                {
                    return copied;
                }
                var newRef = new PdfIndirectReference();
                refStack[ir] = newRef;
                var newObj = AddItem(ir.GetObject(), refStack);
                newRef.Object = newObj;
                return newObj;
            } else if (obj is PdfDictionary dict)
            {
                var copy = new PdfDictionary();
                foreach (var kvp in dict)
                {
                    copy[kvp.Key] = AddItem(kvp.Value, refStack);
                }
                return copy;
            } else if (obj is PdfArray arr)
            {
                var copy = new PdfArray();
                foreach (var item in arr)
                {
                    copy.Add(AddItem(item, refStack));
                }
                return copy;
            }
            // rest are immutable
            return obj;
        }
    }
}