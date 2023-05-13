using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Graphics;
using PdfLexer.Writing;
using System.Numerics;

namespace PdfLexer.Content
{

    public record ExtGraphicsDict<T> where T : struct, IFloatingPoint<T>
    {
        public required GfxMatrix<T> CTM { get; init; }
        public required PdfDictionary Dict { get; init; }
    }


    internal class TxtState<T> where T : struct, IFloatingPoint<T>
    {
        public TxtState()
        {
            TextLineMatrix = GfxMatrix<T>.Identity;
            TextMatrix = GfxMatrix<T>.Identity;
        }
        public GfxMatrix<T> TextRenderingMatrix { get; internal set; }
        //      Trm = [ T_fs*T_h  0       0 ] x Tm x CTM
        //              0         T_fs    0
        //              0         T_rise  1
        internal GfxMatrix<T> TextMatrix { get; set; }
        internal GfxMatrix<T> TextLineMatrix { get; set; }
    }
}