using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces;

public interface IColorSpace
{
    /// <summary>
    /// Number of color components per pixel.
    /// </summary>
    int Components { get; }
    /// <summary>
    /// Name of the colorspace
    /// </summary>
    PdfName Name { get; }

    /// <summary>
    /// If the colorspace is predefined by using a PdfName
    /// in the pdf spec
    /// </summary>
    bool IsPredefined { get; }

    /// <summary>
    /// Copies and converts component data to RGBA 8 bit format
    /// </summary>
    /// <param name="row">Raw component data formatted as 1 byte per component (pre-scaled to 0-255)</param>
    /// <param name="output">Buffer to write output</param>
    void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output);

    /// <summary>
    /// Copies and converts component data to RGBA 16 bit format
    /// </summary>
    /// <param name="row">Raw component data formatted as 1 byte per component (pre-scaled to 0-65535)</param>
    /// <param name="output">Buffer to write output</param>
    void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output);

    bool IsDefaultDecode(int bpc, List<float> decode) { return false; }

    IPdfObject GetPdfObject() { throw new NotImplementedException(); }

}

