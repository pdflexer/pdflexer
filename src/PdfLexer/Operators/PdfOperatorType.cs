
#if !NET7_0_OR_GREATER

using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Auto-generated, do not modify.
/// </summary>
namespace PdfLexer.Operators
{
    public enum PdfOperatorType
    {
        Unknown = 200,
        NoOp = 201,
        RawOp = 202,
        // End of content
        EOC = 203,
        // Close, fill, and stroke path using nonzero winding number rule
        b = 98,
        // Fill and stroke path using nonzero winding number rule
        B = 66,
        // Close, fill, and stroke path using even-odd rule
        b_Star = 10850,
        // Fill and stroke path using even-odd rule
        B_Star = 10818,
        // Begin marked-content sequence with property list
        BDC = 4408386,
        // Begin inline image object
        BI = 18754,
        // Begin marked-content sequence
        BMC = 4410690,
        // Begin text object
        BT = 21570,
        // Begin compatibility section
        BX = 22594,
        // Append curved segment to path (three control points)
        c = 99,
        // Concatenate matrix to current transformation matrix
        cm = 28003,
        // Set color space for stroking operations
        CS = 21315,
        // Set color space for nonstroking operations
        cs = 29539,
        // Set line dash pattern
        d = 100,
        // Set glyph width in Type 3 font
        d0 = 12388,
        // Set glyph width and bounding box in Type 3 font
        d1 = 12644,
        // Invoke named XObject
        Do = 28484,
        // Define marked-content point with property list
        DP = 20548,
        // End inline image object
        EI = 18757,
        // End marked-content sequence
        EMC = 4410693,
        // End text object
        ET = 21573,
        // End compatibility section
        EX = 22597,
        // Fill path using nonzero winding number rule
        f = 102,
        // Fill path using nonzero winding number rule (obsolete)
        F = 70,
        // Fill path using even-odd rule
        f_Star = 10854,
        // Set gray level for stroking operations
        G = 71,
        // Set gray level for nonstroking operations
        g = 103,
        // Set parameters from graphics state parameter dictionary
        gs = 29543,
        // Close subpath
        h = 104,
        // Set flatness tolerance
        i = 105,
        // Begin inline image data
        ID = 17481,
        // Set line join style
        j = 106,
        // Set line capstyle
        J = 74,
        // Set CMYK color for stroking operations
        K = 75,
        // Set CMYK color for nonstroking operations
        k = 107,
        // Append straight line segment to path
        l = 108,
        // Begin new subpath
        m = 109,
        // Set miter limit
        M = 77,
        // Define marked-content point
        MP = 20557,
        // End path without filling or stroking
        n = 110,
        // Save graphics state
        q = 113,
        // Restore graphics state
        Q = 81,
        // Append rectangle to path
        re = 25970,
        // Set RGB color for stroking operations
        RG = 18258,
        // Set RGB color for nonstroking operations
        rg = 26482,
        // Set color rendering intent
        ri = 26994,
        // Close and stroke path
        s = 115,
        // Stroke path
        S = 83,
        // Set color for stroking operations
        SC = 17235,
        // Set color for nonstroking operations
        sc = 25459,
        // Set color for stroking operations (ICCBased and special color spaces)
        SCN = 5129043,
        // Set color for nonstroking operations (ICCBased and special color spaces)
        scn = 7234419,
        // Paint area defined by shading pattern
        sh = 26739,
        // Move to start of next text line
        T_Star = 10836,
        // Set character spacing
        Tc = 25428,
        // Move text position
        Td = 25684,
        // Move text position and set leading
        TD = 17492,
        // Set text font and size
        Tf = 26196,
        // Show text
        Tj = 27220,
        // Show text, allowing individual glyph positioning
        TJ = 19028,
        // Set text leading
        TL = 19540,
        // Set text matrix and text line matrix
        Tm = 27988,
        // Set text rendering mode
        Tr = 29268,
        // Set text rise
        Ts = 29524,
        // Set word spacing
        Tw = 30548,
        // Set horizontal text scaling
        Tz = 31316,
        // Append curved segment to path (initial point replicated)
        v = 118,
        // Set line width
        w = 119,
        // Set clipping path using nonzero winding number rule
        W = 87,
        // Set clipping path using even-odd rule
        W_Star = 10839,
        // Append curved segment to path (final point replicated)
        y = 121,
        // Move to next line and show text
        singlequote = 39,
        // Set word and character spacing, move to next line, and show text
        doublequote = 34,
    }

    public static class ParseOpMapping
    {
        public static Dictionary<int, PdfOperator.ParseOp> Parsers = new () {
            [98] =  b_Op.Parse, 
            [66] =  B_Op.Parse, 
            [10850] =  b_Star_Op.Parse, 
            [10818] =  B_Star_Op.Parse, 
            [4408386] =  PdfOperator.ParseBDC, 
            [18754] =  BI_Op.Parse, 
            [4410690] =  BMC_Op.Parse, 
            [21570] =  BT_Op.Parse, 
            [22594] =  BX_Op.Parse, 
            [99] =  c_Op.Parse, 
            [28003] =  cm_Op.Parse, 
            [21315] =  CS_Op.Parse, 
            [29539] =  cs_Op.Parse, 
            [100] =  PdfOperator.Parsed, 
            [12388] =  d0_Op.Parse, 
            [12644] =  d1_Op.Parse, 
            [28484] =  Do_Op.Parse, 
            [20548] =  PdfOperator.ParseDP, 
            [18757] =  EI_Op.Parse, 
            [4410693] =  EMC_Op.Parse, 
            [21573] =  ET_Op.Parse, 
            [22597] =  EX_Op.Parse, 
            [102] =  f_Op.Parse, 
            [70] =  F_Op.Parse, 
            [10854] =  f_Star_Op.Parse, 
            [71] =  G_Op.Parse, 
            [103] =  g_Op.Parse, 
            [29543] =  gs_Op.Parse, 
            [104] =  h_Op.Parse, 
            [105] =  i_Op.Parse, 
            [17481] =  ID_Op.Parse, 
            [106] =  j_Op.Parse, 
            [74] =  J_Op.Parse, 
            [75] =  K_Op.Parse, 
            [107] =  k_Op.Parse, 
            [108] =  l_Op.Parse, 
            [109] =  m_Op.Parse, 
            [77] =  M_Op.Parse, 
            [20557] =  MP_Op.Parse, 
            [110] =  n_Op.Parse, 
            [113] =  q_Op.Parse, 
            [81] =  Q_Op.Parse, 
            [25970] =  re_Op.Parse, 
            [18258] =  RG_Op.Parse, 
            [26482] =  rg_Op.Parse, 
            [26994] =  ri_Op.Parse, 
            [115] =  s_Op.Parse, 
            [83] =  S_Op.Parse, 
            [17235] =  PdfOperator.ParseSC, 
            [25459] =  PdfOperator.Parsesc, 
            [5129043] =  PdfOperator.ParseSCN, 
            [7234419] =  PdfOperator.Parsescn, 
            [26739] =  sh_Op.Parse, 
            [10836] =  T_Star_Op.Parse, 
            [25428] =  Tc_Op.Parse, 
            [25684] =  Td_Op.Parse, 
            [17492] =  TD_Op.Parse, 
            [26196] =  Tf_Op.Parse, 
            [27220] =  PdfOperator.ParseTj, 
            [19028] =  PdfOperator.ParseTJ, 
            [19540] =  TL_Op.Parse, 
            [27988] =  Tm_Op.Parse, 
            [29268] =  Tr_Op.Parse, 
            [29524] =  Ts_Op.Parse, 
            [30548] =  Tw_Op.Parse, 
            [31316] =  Tz_Op.Parse, 
            [118] =  v_Op.Parse, 
            [119] =  w_Op.Parse, 
            [87] =  W_Op.Parse, 
            [10839] =  W_Star_Op.Parse, 
            [121] =  y_Op.Parse, 
            [39] =  PdfOperator.Parsesinglequote, 
            [34] =  PdfOperator.Parsedoublequote, 
        };

                public static PdfOperator.ParseOp?[] SingleByteParsers = new PdfOperator.ParseOp?[] {
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            PdfOperator.Parsedoublequote,
            null,
            null,
            null,
            null,
            PdfOperator.Parsesinglequote,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            B_Op.Parse,
            null,
            null,
            null,
            F_Op.Parse,
            G_Op.Parse,
            null,
            null,
            J_Op.Parse,
            K_Op.Parse,
            null,
            M_Op.Parse,
            null,
            null,
            null,
            Q_Op.Parse,
            null,
            S_Op.Parse,
            null,
            null,
            null,
            W_Op.Parse,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            b_Op.Parse,
            c_Op.Parse,
            PdfOperator.Parsed,
            null,
            f_Op.Parse,
            g_Op.Parse,
            h_Op.Parse,
            i_Op.Parse,
            j_Op.Parse,
            k_Op.Parse,
            l_Op.Parse,
            m_Op.Parse,
            n_Op.Parse,
            null,
            null,
            q_Op.Parse,
            null,
            s_Op.Parse,
            null,
            null,
            v_Op.Parse,
            w_Op.Parse,
            null,
            y_Op.Parse,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
        };

        public static PdfOperator.ParseOp GetParser(ReadOnlySpan<byte> opData) 
        {
            // var test = "b, B, c, C, d, D, E, f, F, G, g, h, i, I, j, J, K, k, l, m, M, n, q, Q, r, R, s, S, T, v, w, W, y, ', "";
            
            return null;
        }
    }


    /// <summary>
    /// Close, fill, and stroke path using nonzero winding number rule
    /// </summary>
    public partial class b_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'b' };
        public PdfOperatorType Type => PdfOperatorType.b;
        public static readonly b_Op Value = new ();

        // Close, fill, and stroke path using nonzero winding number rule
        public b_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static b_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill and stroke path using nonzero winding number rule
    /// </summary>
    public partial class B_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B' };
        public PdfOperatorType Type => PdfOperatorType.B;
        public static readonly B_Op Value = new ();

        // Fill and stroke path using nonzero winding number rule
        public B_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static B_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Close, fill, and stroke path using even-odd rule
    /// </summary>
    public partial class b_Star_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'b', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.b_Star;
        public static readonly b_Star_Op Value = new ();

        // Close, fill, and stroke path using even-odd rule
        public b_Star_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static b_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill and stroke path using even-odd rule
    /// </summary>
    public partial class B_Star_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.B_Star;
        public static readonly B_Star_Op Value = new ();

        // Fill and stroke path using even-odd rule
        public B_Star_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static B_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin marked-content sequence with property list
    /// </summary>
    public partial class BDC_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'D', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.BDC;
        public PdfName tag { get; }
        public IPdfObject props { get; }
        public BDC_Op(PdfName tag, IPdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }

    /// <summary>
    /// Begin inline image object
    /// </summary>
    public partial class BI_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.BI;
        public static readonly BI_Op Value = new ();

        // Begin inline image object
        public BI_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static BI_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin marked-content sequence
    /// </summary>
    public partial class BMC_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'M', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.BMC;
        public PdfName tag { get; }
        public BMC_Op(PdfName tag)
        {
            this.tag = tag;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(tag, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName tag, Stream stream) 
        {
            Write(tag, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName tag, Stream stream) 
        {
            PdfOperator.WritePdfName(tag, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static BMC_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for BMC, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new BMC_Op(a0);
        }
    }

    /// <summary>
    /// Begin text object
    /// </summary>
    public partial class BT_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.BT;
        public static readonly BT_Op Value = new ();

        // Begin text object
        public BT_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static BT_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin compatibility section
    /// </summary>
    public partial class BX_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.BX;
        public static readonly BX_Op Value = new ();

        // Begin compatibility section
        public BX_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static BX_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append curved segment to path (three control points)
    /// </summary>
    public partial class c_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.c;
        public double x1 { get; }
        public double y1 { get; }
        public double x2 { get; }
        public double y2 { get; }
        public double x3 { get; }
        public double y3 { get; }
        public c_Op(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x1, double y1, double x2, double y2, double x3, double y3, Stream stream) 
        {
            Write(x1, y1, x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x1, double y1, double x2, double y2, double x3, double y3, Stream stream) 
        {
            PdfOperator.Writedouble(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double", "double", "double"  };

        public static c_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 6) 
            {
                ctx.Error($"Incorrect operand count for c, got {operands.Count}, expected 6.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
            var a4 = PdfOperator.Parsedouble(ctx, data, operands[4]);
            var a5 = PdfOperator.Parsedouble(ctx, data, operands[5]);
    
            return new c_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Concatenate matrix to current transformation matrix
    /// </summary>
    public partial class cm_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'c', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.cm;
        public double a { get; }
        public double b { get; }
        public double c { get; }
        public double d { get; }
        public double e { get; }
        public double f { get; }
        public cm_Op(double a, double b, double c, double d, double e, double f)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double a, double b, double c, double d, double e, double f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double a, double b, double c, double d, double e, double f, Stream stream) 
        {
            PdfOperator.Writedouble(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double", "double", "double"  };

        public static cm_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 6) 
            {
                ctx.Error($"Incorrect operand count for cm, got {operands.Count}, expected 6.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
            var a4 = PdfOperator.Parsedouble(ctx, data, operands[4]);
            var a5 = PdfOperator.Parsedouble(ctx, data, operands[5]);
    
            return new cm_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Set color space for stroking operations
    /// </summary>
    public partial class CS_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'C', (byte) 'S' };
        public PdfOperatorType Type => PdfOperatorType.CS;
        public PdfName name { get; }
        public CS_Op(PdfName name)
        {
            this.name = name;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName name, Stream stream) 
        {
            Write(name, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName name, Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static CS_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for CS, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new CS_Op(a0);
        }
    }

    /// <summary>
    /// Set color space for nonstroking operations
    /// </summary>
    public partial class cs_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'c', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.cs;
        public PdfName name { get; }
        public cs_Op(PdfName name)
        {
            this.name = name;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName name, Stream stream) 
        {
            Write(name, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName name, Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static cs_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for cs, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new cs_Op(a0);
        }
    }

    /// <summary>
    /// Set line dash pattern
    /// </summary>
    public partial class d_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.d;
        public PdfArray dashArray { get; }
        public double dashPhase { get; }
        public d_Op(PdfArray dashArray, double dashPhase)
        {
            this.dashArray = dashArray;
            this.dashPhase = dashPhase;
        }
    }

    /// <summary>
    /// Set glyph width in Type 3 font
    /// </summary>
    public partial class d0_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd', (byte) '0' };
        public PdfOperatorType Type => PdfOperatorType.d0;
        public double wx { get; }
        public double wy { get; }
        public d0_Op(double wx, double wy)
        {
            this.wx = wx;
            this.wy = wy;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double wx, double wy, Stream stream) 
        {
            Write(wx, wy, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double wx, double wy, Stream stream) 
        {
            PdfOperator.Writedouble(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double"  };

        public static d0_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for d0, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new d0_Op(a0, a1);
        }
    }

    /// <summary>
    /// Set glyph width and bounding box in Type 3 font
    /// </summary>
    public partial class d1_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd', (byte) '1' };
        public PdfOperatorType Type => PdfOperatorType.d1;
        public double wx { get; }
        public double wy { get; }
        public double llx { get; }
        public double lly { get; }
        public double urx { get; }
        public double ury { get; }
        public d1_Op(double wx, double wy, double llx, double lly, double urx, double ury)
        {
            this.wx = wx;
            this.wy = wy;
            this.llx = llx;
            this.lly = lly;
            this.urx = urx;
            this.ury = ury;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(wy, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(llx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(lly, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(urx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double wx, double wy, double llx, double lly, double urx, double ury, Stream stream) 
        {
            Write(wx, wy, llx, lly, urx, ury, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double wx, double wy, double llx, double lly, double urx, double ury, Stream stream) 
        {
            PdfOperator.Writedouble(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(wy, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(llx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(lly, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(urx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double", "double", "double"  };

        public static d1_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 6) 
            {
                ctx.Error($"Incorrect operand count for d1, got {operands.Count}, expected 6.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
            var a4 = PdfOperator.Parsedouble(ctx, data, operands[4]);
            var a5 = PdfOperator.Parsedouble(ctx, data, operands[5]);
    
            return new d1_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Invoke named XObject
    /// </summary>
    public partial class Do_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'D', (byte) 'o' };
        public PdfOperatorType Type => PdfOperatorType.Do;
        public PdfName name { get; }
        public Do_Op(PdfName name)
        {
            this.name = name;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName name, Stream stream) 
        {
            Write(name, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName name, Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static Do_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Do, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new Do_Op(a0);
        }
    }

    /// <summary>
    /// Define marked-content point with property list
    /// </summary>
    public partial class DP_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'D', (byte) 'P' };
        public PdfOperatorType Type => PdfOperatorType.DP;
        public PdfName tag { get; }
        public IPdfObject props { get; }
        public DP_Op(PdfName tag, IPdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }

    /// <summary>
    /// End inline image object
    /// </summary>
    public partial class EI_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.EI;
        public static readonly EI_Op Value = new ();

        // End inline image object
        public EI_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static EI_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End marked-content sequence
    /// </summary>
    public partial class EMC_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'M', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.EMC;
        public static readonly EMC_Op Value = new ();

        // End marked-content sequence
        public EMC_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static EMC_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End text object
    /// </summary>
    public partial class ET_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.ET;
        public static readonly ET_Op Value = new ();

        // End text object
        public ET_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static ET_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End compatibility section
    /// </summary>
    public partial class EX_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.EX;
        public static readonly EX_Op Value = new ();

        // End compatibility section
        public EX_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static EX_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using nonzero winding number rule
    /// </summary>
    public partial class f_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.f;
        public static readonly f_Op Value = new ();

        // Fill path using nonzero winding number rule
        public f_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static f_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using nonzero winding number rule (obsolete)
    /// </summary>
    public partial class F_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'F' };
        public PdfOperatorType Type => PdfOperatorType.F;
        public static readonly F_Op Value = new ();

        // Fill path using nonzero winding number rule (obsolete)
        public F_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static F_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using even-odd rule
    /// </summary>
    public partial class f_Star_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'f', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.f_Star;
        public static readonly f_Star_Op Value = new ();

        // Fill path using even-odd rule
        public f_Star_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static f_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set gray level for stroking operations
    /// </summary>
    public partial class G_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.G;
        public double gray { get; }
        public G_Op(double gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double gray, Stream stream) 
        {
            PdfOperator.Writedouble(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static G_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for G, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new G_Op(a0);
        }
    }

    /// <summary>
    /// Set gray level for nonstroking operations
    /// </summary>
    public partial class g_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.g;
        public double gray { get; }
        public g_Op(double gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double gray, Stream stream) 
        {
            PdfOperator.Writedouble(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static g_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for g, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new g_Op(a0);
        }
    }

    /// <summary>
    /// Set parameters from graphics state parameter dictionary
    /// </summary>
    public partial class gs_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'g', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.gs;
        public PdfName name { get; }
        public gs_Op(PdfName name)
        {
            this.name = name;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName name, Stream stream) 
        {
            Write(name, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName name, Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static gs_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for gs, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new gs_Op(a0);
        }
    }

    /// <summary>
    /// Close subpath
    /// </summary>
    public partial class h_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'h' };
        public PdfOperatorType Type => PdfOperatorType.h;
        public static readonly h_Op Value = new ();

        // Close subpath
        public h_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static h_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set flatness tolerance
    /// </summary>
    public partial class i_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'i' };
        public PdfOperatorType Type => PdfOperatorType.i;
        public double flatness { get; }
        public i_Op(double flatness)
        {
            this.flatness = flatness;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double flatness, Stream stream) 
        {
            Write(flatness, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double flatness, Stream stream) 
        {
            PdfOperator.Writedouble(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static i_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for i, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new i_Op(a0);
        }
    }

    /// <summary>
    /// Begin inline image data
    /// </summary>
    public partial class ID_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'I', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.ID;
        public static readonly ID_Op Value = new ();

        // Begin inline image data
        public ID_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static ID_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set line join style
    /// </summary>
    public partial class j_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'j' };
        public PdfOperatorType Type => PdfOperatorType.j;
        public int lineJoin { get; }
        public j_Op(int lineJoin)
        {
            this.lineJoin = lineJoin;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writeint(lineJoin, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(int lineJoin, Stream stream) 
        {
            Write(lineJoin, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(int lineJoin, Stream stream) 
        {
            PdfOperator.Writeint(lineJoin, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "int"  };

        public static j_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for j, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parseint(ctx, data, operands[0]);
    
            return new j_Op(a0);
        }
    }

    /// <summary>
    /// Set line capstyle
    /// </summary>
    public partial class J_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'J' };
        public PdfOperatorType Type => PdfOperatorType.J;
        public int lineCap { get; }
        public J_Op(int lineCap)
        {
            this.lineCap = lineCap;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writeint(lineCap, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(int lineCap, Stream stream) 
        {
            Write(lineCap, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(int lineCap, Stream stream) 
        {
            PdfOperator.Writeint(lineCap, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "int"  };

        public static J_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for J, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parseint(ctx, data, operands[0]);
    
            return new J_Op(a0);
        }
    }

    /// <summary>
    /// Set CMYK color for stroking operations
    /// </summary>
    public partial class K_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'K' };
        public PdfOperatorType Type => PdfOperatorType.K;
        public double c { get; }
        public double m { get; }
        public double y { get; }
        public double k { get; }
        public K_Op(double c, double m, double y, double k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double c, double m, double y, double k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double c, double m, double y, double k, Stream stream) 
        {
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double"  };

        public static K_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 4) 
            {
                ctx.Error($"Incorrect operand count for K, got {operands.Count}, expected 4.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
    
            return new K_Op(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set CMYK color for nonstroking operations
    /// </summary>
    public partial class k_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'k' };
        public PdfOperatorType Type => PdfOperatorType.k;
        public double c { get; }
        public double m { get; }
        public double y { get; }
        public double k { get; }
        public k_Op(double c, double m, double y, double k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double c, double m, double y, double k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double c, double m, double y, double k, Stream stream) 
        {
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double"  };

        public static k_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 4) 
            {
                ctx.Error($"Incorrect operand count for k, got {operands.Count}, expected 4.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
    
            return new k_Op(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Append straight line segment to path
    /// </summary>
    public partial class l_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'l' };
        public PdfOperatorType Type => PdfOperatorType.l;
        public double x { get; }
        public double y { get; }
        public l_Op(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x, double y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x, double y, Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double"  };

        public static l_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for l, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new l_Op(a0, a1);
        }
    }

    /// <summary>
    /// Begin new subpath
    /// </summary>
    public partial class m_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.m;
        public double x { get; }
        public double y { get; }
        public m_Op(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x, double y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x, double y, Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double"  };

        public static m_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for m, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new m_Op(a0, a1);
        }
    }

    /// <summary>
    /// Set miter limit
    /// </summary>
    public partial class M_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'M' };
        public PdfOperatorType Type => PdfOperatorType.M;
        public double miterLimit { get; }
        public M_Op(double miterLimit)
        {
            this.miterLimit = miterLimit;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double miterLimit, Stream stream) 
        {
            Write(miterLimit, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double miterLimit, Stream stream) 
        {
            PdfOperator.Writedouble(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static M_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for M, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new M_Op(a0);
        }
    }

    /// <summary>
    /// Define marked-content point
    /// </summary>
    public partial class MP_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'M', (byte) 'P' };
        public PdfOperatorType Type => PdfOperatorType.MP;
        public PdfName tag { get; }
        public MP_Op(PdfName tag)
        {
            this.tag = tag;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(tag, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName tag, Stream stream) 
        {
            Write(tag, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName tag, Stream stream) 
        {
            PdfOperator.WritePdfName(tag, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static MP_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for MP, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new MP_Op(a0);
        }
    }

    /// <summary>
    /// End path without filling or stroking
    /// </summary>
    public partial class n_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.n;
        public static readonly n_Op Value = new ();

        // End path without filling or stroking
        public n_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static n_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Save graphics state
    /// </summary>
    public partial class q_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'q' };
        public PdfOperatorType Type => PdfOperatorType.q;
        public static readonly q_Op Value = new ();

        // Save graphics state
        public q_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static q_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Restore graphics state
    /// </summary>
    public partial class Q_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'Q' };
        public PdfOperatorType Type => PdfOperatorType.Q;
        public static readonly Q_Op Value = new ();

        // Restore graphics state
        public Q_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static Q_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append rectangle to path
    /// </summary>
    public partial class re_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'r', (byte) 'e' };
        public PdfOperatorType Type => PdfOperatorType.re;
        public double x { get; }
        public double y { get; }
        public double width { get; }
        public double height { get; }
        public re_Op(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(width, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x, double y, double width, double height, Stream stream) 
        {
            Write(x, y, width, height, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x, double y, double width, double height, Stream stream) 
        {
            PdfOperator.Writedouble(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(width, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double"  };

        public static re_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 4) 
            {
                ctx.Error($"Incorrect operand count for re, got {operands.Count}, expected 4.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
    
            return new re_Op(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set RGB color for stroking operations
    /// </summary>
    public partial class RG_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'R', (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.RG;
        public double r { get; }
        public double g { get; }
        public double b { get; }
        public RG_Op(double r, double g, double b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double r, double g, double b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double r, double g, double b, Stream stream) 
        {
            PdfOperator.Writedouble(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double"  };

        public static RG_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 3) 
            {
                ctx.Error($"Incorrect operand count for RG, got {operands.Count}, expected 3.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
    
            return new RG_Op(a0, a1, a2);
        }
    }

    /// <summary>
    /// Set RGB color for nonstroking operations
    /// </summary>
    public partial class rg_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'r', (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.rg;
        public double r { get; }
        public double g { get; }
        public double b { get; }
        public rg_Op(double r, double g, double b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double r, double g, double b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double r, double g, double b, Stream stream) 
        {
            PdfOperator.Writedouble(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double"  };

        public static rg_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 3) 
            {
                ctx.Error($"Incorrect operand count for rg, got {operands.Count}, expected 3.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
    
            return new rg_Op(a0, a1, a2);
        }
    }

    /// <summary>
    /// Set color rendering intent
    /// </summary>
    public partial class ri_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'r', (byte) 'i' };
        public PdfOperatorType Type => PdfOperatorType.ri;
        public PdfName intent { get; }
        public ri_Op(PdfName intent)
        {
            this.intent = intent;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(intent, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName intent, Stream stream) 
        {
            Write(intent, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName intent, Stream stream) 
        {
            PdfOperator.WritePdfName(intent, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static ri_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for ri, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new ri_Op(a0);
        }
    }

    /// <summary>
    /// Close and stroke path
    /// </summary>
    public partial class s_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.s;
        public static readonly s_Op Value = new ();

        // Close and stroke path
        public s_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static s_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Stroke path
    /// </summary>
    public partial class S_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S' };
        public PdfOperatorType Type => PdfOperatorType.S;
        public static readonly S_Op Value = new ();

        // Stroke path
        public S_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static S_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set color for stroking operations
    /// </summary>
    public partial class SC_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.SC;
        public List<double> colorInfo { get; }
        public SC_Op(List<double> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    /// <summary>
    /// Set color for nonstroking operations
    /// </summary>
    public partial class sc_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.sc;
        public List<double> colorInfo { get; }
        public sc_Op(List<double> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    /// <summary>
    /// Set color for stroking operations (ICCBased and special color spaces)
    /// </summary>
    public partial class SCN_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S', (byte) 'C', (byte) 'N' };
        public PdfOperatorType Type => PdfOperatorType.SCN;
        public List<double> colorInfo { get; }
        public PdfName? name { get; }
        public SCN_Op(List<double> colorInfo, PdfName? name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    /// <summary>
    /// Set color for nonstroking operations (ICCBased and special color spaces)
    /// </summary>
    public partial class scn_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's', (byte) 'c', (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.scn;
        public List<double> colorInfo { get; }
        public PdfName? name { get; }
        public scn_Op(List<double> colorInfo, PdfName? name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    /// <summary>
    /// Paint area defined by shading pattern
    /// </summary>
    public partial class sh_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's', (byte) 'h' };
        public PdfOperatorType Type => PdfOperatorType.sh;
        public PdfName name { get; }
        public sh_Op(PdfName name)
        {
            this.name = name;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName name, Stream stream) 
        {
            Write(name, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName name, Stream stream) 
        {
            PdfOperator.WritePdfName(name, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName"  };

        public static sh_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for sh, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new sh_Op(a0);
        }
    }

    /// <summary>
    /// Move to start of next text line
    /// </summary>
    public partial class T_Star_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.T_Star;
        public static readonly T_Star_Op Value = new ();

        // Move to start of next text line
        public T_Star_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static T_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set character spacing
    /// </summary>
    public partial class Tc_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.Tc;
        public double charSpace { get; }
        public Tc_Op(double charSpace)
        {
            this.charSpace = charSpace;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double charSpace, Stream stream) 
        {
            Write(charSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double charSpace, Stream stream) 
        {
            PdfOperator.Writedouble(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static Tc_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Tc, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new Tc_Op(a0);
        }
    }

    /// <summary>
    /// Move text position
    /// </summary>
    public partial class Td_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.Td;
        public double tx { get; }
        public double ty { get; }
        public Td_Op(double tx, double ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double tx, double ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double tx, double ty, Stream stream) 
        {
            PdfOperator.Writedouble(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double"  };

        public static Td_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for Td, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new Td_Op(a0, a1);
        }
    }

    /// <summary>
    /// Move text position and set leading
    /// </summary>
    public partial class TD_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.TD;
        public double tx { get; }
        public double ty { get; }
        public TD_Op(double tx, double ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double tx, double ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double tx, double ty, Stream stream) 
        {
            PdfOperator.Writedouble(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double"  };

        public static TD_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for TD, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new TD_Op(a0, a1);
        }
    }

    /// <summary>
    /// Set text font and size
    /// </summary>
    public partial class Tf_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.Tf;
        public PdfName font { get; }
        public double size { get; }
        public Tf_Op(PdfName font, double size)
        {
            this.font = font;
            this.size = size;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName font, double size, Stream stream) 
        {
            Write(font, size, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName font, double size, Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName", "double"  };

        public static Tf_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 2) 
            {
                ctx.Error($"Incorrect operand count for Tf, got {operands.Count}, expected 2.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
    
            return new Tf_Op(a0, a1);
        }
    }

    /// <summary>
    /// Show text
    /// </summary>
    public partial class Tj_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'j' };
        public PdfOperatorType Type => PdfOperatorType.Tj;
        public byte[] text { get; }
        public Tj_Op(byte[] text)
        {
            this.text = text;
        }
    }

    /// <summary>
    /// Show text, allowing individual glyph positioning
    /// </summary>
    public partial class TJ_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'J' };
        public PdfOperatorType Type => PdfOperatorType.TJ;
        public List<TJ_Item> info { get; }
        public TJ_Op(List<TJ_Item> info)
        {
            this.info = info;
        }
    }

    /// <summary>
    /// Set text leading
    /// </summary>
    public partial class TL_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'L' };
        public PdfOperatorType Type => PdfOperatorType.TL;
        public double leading { get; }
        public TL_Op(double leading)
        {
            this.leading = leading;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double leading, Stream stream) 
        {
            Write(leading, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double leading, Stream stream) 
        {
            PdfOperator.Writedouble(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static TL_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for TL, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new TL_Op(a0);
        }
    }

    /// <summary>
    /// Set text matrix and text line matrix
    /// </summary>
    public partial class Tm_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.Tm;
        public double a { get; }
        public double b { get; }
        public double c { get; }
        public double d { get; }
        public double e { get; }
        public double f { get; }
        public Tm_Op(double a, double b, double c, double d, double e, double f)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double a, double b, double c, double d, double e, double f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double a, double b, double c, double d, double e, double f, Stream stream) 
        {
            PdfOperator.Writedouble(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double", "double", "double"  };

        public static Tm_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 6) 
            {
                ctx.Error($"Incorrect operand count for Tm, got {operands.Count}, expected 6.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
            var a4 = PdfOperator.Parsedouble(ctx, data, operands[4]);
            var a5 = PdfOperator.Parsedouble(ctx, data, operands[5]);
    
            return new Tm_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Set text rendering mode
    /// </summary>
    public partial class Tr_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'r' };
        public PdfOperatorType Type => PdfOperatorType.Tr;
        public int render { get; }
        public Tr_Op(int render)
        {
            this.render = render;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writeint(render, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(int render, Stream stream) 
        {
            Write(render, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(int render, Stream stream) 
        {
            PdfOperator.Writeint(render, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "int"  };

        public static Tr_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Tr, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parseint(ctx, data, operands[0]);
    
            return new Tr_Op(a0);
        }
    }

    /// <summary>
    /// Set text rise
    /// </summary>
    public partial class Ts_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.Ts;
        public double rise { get; }
        public Ts_Op(double rise)
        {
            this.rise = rise;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double rise, Stream stream) 
        {
            Write(rise, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double rise, Stream stream) 
        {
            PdfOperator.Writedouble(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static Ts_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Ts, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new Ts_Op(a0);
        }
    }

    /// <summary>
    /// Set word spacing
    /// </summary>
    public partial class Tw_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.Tw;
        public double wordSpace { get; }
        public Tw_Op(double wordSpace)
        {
            this.wordSpace = wordSpace;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double wordSpace, Stream stream) 
        {
            Write(wordSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double wordSpace, Stream stream) 
        {
            PdfOperator.Writedouble(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static Tw_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Tw, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new Tw_Op(a0);
        }
    }

    /// <summary>
    /// Set horizontal text scaling
    /// </summary>
    public partial class Tz_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'z' };
        public PdfOperatorType Type => PdfOperatorType.Tz;
        public double scale { get; }
        public Tz_Op(double scale)
        {
            this.scale = scale;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double scale, Stream stream) 
        {
            Write(scale, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double scale, Stream stream) 
        {
            PdfOperator.Writedouble(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static Tz_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for Tz, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new Tz_Op(a0);
        }
    }

    /// <summary>
    /// Append curved segment to path (initial point replicated)
    /// </summary>
    public partial class v_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'v' };
        public PdfOperatorType Type => PdfOperatorType.v;
        public double x2 { get; }
        public double y2 { get; }
        public double x3 { get; }
        public double y3 { get; }
        public v_Op(double x2, double y2, double x3, double y3)
        {
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x2, double y2, double x3, double y3, Stream stream) 
        {
            Write(x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x2, double y2, double x3, double y3, Stream stream) 
        {
            PdfOperator.Writedouble(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double"  };

        public static v_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 4) 
            {
                ctx.Error($"Incorrect operand count for v, got {operands.Count}, expected 4.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
    
            return new v_Op(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set line width
    /// </summary>
    public partial class w_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.w;
        public double lineWidth { get; }
        public w_Op(double lineWidth)
        {
            this.lineWidth = lineWidth;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double lineWidth, Stream stream) 
        {
            Write(lineWidth, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double lineWidth, Stream stream) 
        {
            PdfOperator.Writedouble(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double"  };

        public static w_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 1) 
            {
                ctx.Error($"Incorrect operand count for w, got {operands.Count}, expected 1.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
    
            return new w_Op(a0);
        }
    }

    /// <summary>
    /// Set clipping path using nonzero winding number rule
    /// </summary>
    public partial class W_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'W' };
        public PdfOperatorType Type => PdfOperatorType.W;
        public static readonly W_Op Value = new ();

        // Set clipping path using nonzero winding number rule
        public W_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static W_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set clipping path using even-odd rule
    /// </summary>
    public partial class W_Star_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'W', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.W_Star;
        public static readonly W_Star_Op Value = new ();

        // Set clipping path using even-odd rule
        public W_Star_Op()
        {

        }

        public static void WriteLn(Stream stream) 
        {
            Write(stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(Stream stream) 
        {
            stream.Write(OpData);
        }

        public void Serialize(Stream stream) 
            => stream.Write(OpData);
        public static W_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append curved segment to path (final point replicated)
    /// </summary>
    public partial class y_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'y' };
        public PdfOperatorType Type => PdfOperatorType.y;
        public double x1 { get; }
        public double y1 { get; }
        public double x3 { get; }
        public double y3 { get; }
        public y_Op(double x1, double y1, double x3, double y3)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedouble(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(double x1, double y1, double x3, double y3, Stream stream) 
        {
            Write(x1, y1, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(double x1, double y1, double x3, double y3, Stream stream) 
        {
            PdfOperator.Writedouble(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedouble(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "double", "double", "double", "double"  };

        public static y_Op? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            if (operands.Count != 4) 
            {
                ctx.Error($"Incorrect operand count for y, got {operands.Count}, expected 4.");
                if (!ctx.Options.AttemptOperatorRepair || !PdfOperator.TryRepair(ctx, data, operands, OpTypes, out var fixedOps)) 
                {
                    return null;
                }
                operands = fixedOps;
            }
            var a0 = PdfOperator.Parsedouble(ctx, data, operands[0]);
            var a1 = PdfOperator.Parsedouble(ctx, data, operands[1]);
            var a2 = PdfOperator.Parsedouble(ctx, data, operands[2]);
            var a3 = PdfOperator.Parsedouble(ctx, data, operands[3]);
    
            return new y_Op(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Move to next line and show text
    /// </summary>
    public partial class singlequote_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) '\'' };
        public PdfOperatorType Type => PdfOperatorType.singlequote;
        public byte[] text { get; }
        public singlequote_Op(byte[] text)
        {
            this.text = text;
        }
    }

    /// <summary>
    /// Set word and character spacing, move to next line, and show text
    /// </summary>
    public partial class doublequote_Op : IPdfOperation
    {
        public static byte[] OpData { get; } = new byte[] { (byte) '"' };
        public PdfOperatorType Type => PdfOperatorType.doublequote;
        public double aw { get; }
        public double ac { get; }
        public byte[] text { get; }
        public doublequote_Op(double aw, double ac, byte[] text)
        {
            this.aw = aw;
            this.ac = ac;
            this.text = text;
        }
    }
}

#endif