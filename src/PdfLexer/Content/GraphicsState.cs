using PdfLexer.Content;
using System.IO;
using System.Numerics;

namespace PdfLexer.Content
{
    public class GraphicsState
    {
        public GraphicsState()
        {
            CTM = Matrix4x4.Identity;
        }
        public GraphicsState? Prev { get; set; }
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

        public Matrix4x4 GetTranslation(Matrix4x4 desired)
        {
            Matrix4x4.Invert(CTM, out var iv);
            return desired * iv;
        }

        public void Apply(Matrix4x4 cm)
        {
            CTM = cm * CTM;
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
            state = new GraphicsState();
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