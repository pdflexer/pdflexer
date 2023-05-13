using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces;

internal class UnImplementedColorSpace : IColorSpace
{
    public int Components => throw new NotSupportedException();

    public PdfName Name { get; }

    public bool IsPredefined { get; }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        throw new NotSupportedException();
    }

    public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
    {
        throw new NotSupportedException();
    }

    PdfArray? obj;

    public UnImplementedColorSpace(PdfName name, PdfArray? cs)
    {
        Name = name;
        IsPredefined = cs == null;
        obj = cs;
    }

    public IPdfObject GetPdfObject()
    {
        if (obj == null)
        {
            throw new NotSupportedException();
        }
        return obj;
    }
}
