using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PdfLexer.IO
{
    /// <summary>
    /// TODO
    /// will eventually be for copying tokens from one PDF to another and allowing initial pdf to be disposed.
    /// </summary>
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

        public bool SupportsXRefRepair => throw new NotImplementedException();

        public bool Disposed => throw new NotImplementedException();

        public IPdfDataSource Clone()
        {
            throw new NotImplementedException();
        }

        public void CopyData(long startPosition, int requiredBytes, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(long startPosition)
        {
            throw new NotImplementedException();
        }

        private Dictionary<PdfIndirectRef, NewIndirectRef> sharedStack = new Dictionary<PdfIndirectRef, NewIndirectRef>();
        public IPdfObject AddItem(IPdfObject obj)
        {
            sharedStack.Clear();
            return AddItem(obj, sharedStack);
        }

        internal IPdfObject AddItem(IPdfObject obj, Dictionary<PdfIndirectRef, NewIndirectRef> refStack)
        {
            if (obj is PdfLazyObject lz)
            {
                if (lz.HasLazyIndirect || !lz.IsModified)
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
                if (ir.IsOwned(Context.SourceId, true)) { return ir; }
                if (refStack.TryGetValue(ir, out var copied))
                {
                    return copied;
                }
                var newRef = new NewIndirectRef();
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

        public bool IsDataInMemory(long startPosition, int length)
        {
            throw new NotImplementedException();
        }

        public Stream GetDataAsStream(long startPosition, int desiredBytes)
        {
            throw new NotImplementedException();
        }

        public IPdfObject GetIndirectObject(XRef xref)
        {
            throw new NotImplementedException();
        }

        public void CopyIndirectObject(XRef xref, Stream destination)
        {
            throw new NotImplementedException();
        }

        public IPdfObject GetIndirectObject(XRefEntry xref)
        {
            throw new NotImplementedException();
        }

        public void CopyIndirectObject(XRefEntry xref, WritingContext destination)
        {
            throw new NotImplementedException();
        }

        public bool TryRepairXRef(XRefEntry entry, out XRefEntry repaired)
        {
            throw new NotImplementedException();
        }

        public IPdfObject RepairFindLastMatching(PdfTokenType type, Func<IPdfObject, bool> matcher)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
