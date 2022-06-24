using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
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

        public IPdfObject AddItem(IPdfObject obj)
        {
            if (obj is PdfLazyObject lz)
            {
                if (lz.HasLazyIndirect || !lz.IsModified())
                {
                    var rz = lz.Resolve();
                    if (rz is PdfDictionary dict)
                    {
                        return AddItem(dict);
                    } else if (rz is PdfArray arr)
                    {
                        return AddItem(arr);
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
                    if (lz.IsIndirect)
                    {
                        copy.IndirectRef = new Parsers.Structure.XRef(-1, 0);
                    }
                    return copy;
                }
            } else if (obj is PdfDictionary dict)
            {
                var copy = new PdfDictionary();
                foreach (var kvp in dict)
                {
                    // kvp.Value.
                }
            } else if (obj is PdfArray arr)
            {

            } else if (obj.IsIndirect)
            {
                // TODO copy
            }
            return obj;
        }
    }
}
