using PdfLexer.Fonts;
using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content.Model;



internal record ClippingInfo<T> : IClippingSection<T> where T : struct, IFloatingPoint<T>
{
    public ClippingInfo(GfxMatrix<T> tm, List<SubPath<T>> path, bool evenOdd)
    {
        TM = tm;
        Path = path;
        EvenOdd = evenOdd;
    }
    public List<SubPath<T>> Path { get; set; }
    public GfxMatrix<T> TM { get; set; }
    public bool EvenOdd { get; set; }

    public void Apply(ContentWriter<T> writer)
    {
        foreach (var path in Path)
        {
            writer.SubPath(path);
        }

        if (EvenOdd)
        {
            W_Star_Op.WriteLn(writer.Writer.Stream);
        }
        else
        {
            W_Op.WriteLn(writer.Writer.Stream);
        }
        n_Op.WriteLn(writer.Writer.Stream);
    }
}
internal interface IClippingSection : IClippingSection<double>
{
}
internal interface IClippingSection<T> where T : struct, IFloatingPoint<T>
{
    void Apply(ContentWriter<T> writer);
    GfxMatrix<T> TM { get; }
}
internal record TextClippingInfo<T> : IClippingSection<T> where T : struct, IFloatingPoint<T>
{
    public required List<GlyphOrShift<T>> Glyphs { get; set; }
    public required GfxMatrix<T> TM { get; set; }
    public GfxMatrix<T>? LineMatrix { get; set; }
    public bool NewLine { get; set; }

    public void Apply(ContentWriter<T> writer)
    {
        if (LineMatrix.HasValue)
        {
            writer.SetTextAndLinePosition(LineMatrix.Value);
        }
        else if (NewLine)
        {
            writer.Op(T_Star_Op<T>.Value);
        }
        writer.WriteGlyphs(Glyphs);
    }
}
