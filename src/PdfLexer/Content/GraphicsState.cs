using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using System.Numerics;

namespace PdfLexer.Content
{
    public class GraphicsState
    {
        public GraphicsState()
        {
            CTM = Matrix4x4.Identity;
        }
        internal GraphicsState? Prev { get; set; }
        public Matrix4x4 CTM { get; set; }

        public (float x, float y, float w, float h) GetCurrentSize()
        {
            return (CTM.M31, CTM.M32, CTM.M22, CTM.M11);
        }

        // a b 0  ->  0 1 - -> 1 0 (translation) -> x 0 (scale)
        // c d 0      2 3 -    0 1                  0 y
        // e f 1      4 5 -    x y                  0 0
        //
        // 
        // M11 M12 M13
        // M21 M22 M23
        // M31 M32 M33
        //

        //  cosq   sinq (rotation) | 1    tana (skew)
        //  -sinq  cosq              tanb 1
        //  0      0                 0    0 
        //

        // rotation -> 180/Math.PI * Math.Atan2(d, c) - 90
        //          -> 180/Math.PI * Math.Atan2(b, a)


        public int TextMode { get; internal set; }
        public float FontSize { get; internal set; }
        public float TextHScale { get; internal set; } = 1f;
        public float TextRise { get; internal set; }
        public float CharSpacing { get; internal set; }
        public float WordSpacing { get; internal set; }
        public float TextLeading { get; internal set; }
        public PdfName? FontResourceName { get; internal set; }
        public string? FontName { get; internal set; }
        public FontFlags FontFlags { get; internal set; }
        public PdfDictionary? FontObject { get; internal set; }
        public IReadableFont? Font { get; internal set; }

        public Matrix4x4 GetTranslation(Matrix4x4 desired)
        {
            Matrix4x4.Invert(CTM, out var iv);
            return desired * iv;
        }

        public void Apply(Matrix4x4 cm)
        {
            CTM = cm * CTM;
        }

        public GraphicsState CloneNoPrev()
        {
            return new GraphicsState
            {
                CTM = CTM,
                TextHScale = TextHScale,
                TextLeading = TextLeading,
                TextMode = TextMode,
                TextRise = TextRise,
                CharSpacing = CharSpacing,
                WordSpacing = WordSpacing,
                Font = Font,
                FontObject = FontObject,
                FontResourceName = FontResourceName,
                FontName = FontName,
                FontFlags = FontFlags,
                FontSize = FontSize,
            };
        }

    }
}

namespace PdfLexer.Operators
{
    public partial class Q_Op
    {
        public void Apply(ref GraphicsState state)
        {
            if (state.Prev == null)
            {
                // err
                state = new GraphicsState();
                return;
            }
            state = state.Prev;
        }
    }

    public partial class q_Op
    {
        public void Apply(ref GraphicsState state)
        {
            var prev = state;
            state = state.CloneNoPrev();
            state.Prev = prev;
        }
    }

    public partial class cm_Op
    {
        public void Apply(ref GraphicsState state)
        {
            // new = cm x ctm
            var val = new Matrix4x4(
                          (float)a, (float)b, 0f, 0f,
                          (float)c, (float)d, 0f, 0f,
                          (float)e, (float)f, 1f, 0f,
                          0f, 0f, 0f, 1f);
            state.CTM = val * state.CTM;
        }

        public static void WriteLn(Matrix4x4 ctm, Stream stream)
        {
            WriteLn(
                (decimal)ctm.M11, (decimal)ctm.M12,
                (decimal)ctm.M21, (decimal)ctm.M22,
                (decimal)ctm.M31, (decimal)ctm.M32, stream);
        }
    }
}