
#if NET7_0_OR_GREATER
global using b_Op = PdfLexer.Operators.b_Op<double>;
global using B_Op = PdfLexer.Operators.B_Op<double>;
global using b_Star_Op = PdfLexer.Operators.b_Star_Op<double>;
global using B_Star_Op = PdfLexer.Operators.B_Star_Op<double>;
global using BDC_Op = PdfLexer.Operators.BDC_Op<double>;
global using BI_Op = PdfLexer.Operators.BI_Op<double>;
global using BMC_Op = PdfLexer.Operators.BMC_Op<double>;
global using BT_Op = PdfLexer.Operators.BT_Op<double>;
global using BX_Op = PdfLexer.Operators.BX_Op<double>;
global using c_Op = PdfLexer.Operators.c_Op<double>;
global using cm_Op = PdfLexer.Operators.cm_Op<double>;
global using CS_Op = PdfLexer.Operators.CS_Op<double>;
global using cs_Op = PdfLexer.Operators.cs_Op<double>;
global using d_Op = PdfLexer.Operators.d_Op<double>;
global using d0_Op = PdfLexer.Operators.d0_Op<double>;
global using d1_Op = PdfLexer.Operators.d1_Op<double>;
global using Do_Op = PdfLexer.Operators.Do_Op<double>;
global using DP_Op = PdfLexer.Operators.DP_Op<double>;
global using EI_Op = PdfLexer.Operators.EI_Op<double>;
global using EMC_Op = PdfLexer.Operators.EMC_Op<double>;
global using ET_Op = PdfLexer.Operators.ET_Op<double>;
global using EX_Op = PdfLexer.Operators.EX_Op<double>;
global using f_Op = PdfLexer.Operators.f_Op<double>;
global using F_Op = PdfLexer.Operators.F_Op<double>;
global using f_Star_Op = PdfLexer.Operators.f_Star_Op<double>;
global using G_Op = PdfLexer.Operators.G_Op<double>;
global using g_Op = PdfLexer.Operators.g_Op<double>;
global using gs_Op = PdfLexer.Operators.gs_Op<double>;
global using h_Op = PdfLexer.Operators.h_Op<double>;
global using i_Op = PdfLexer.Operators.i_Op<double>;
global using ID_Op = PdfLexer.Operators.ID_Op<double>;
global using j_Op = PdfLexer.Operators.j_Op<double>;
global using J_Op = PdfLexer.Operators.J_Op<double>;
global using K_Op = PdfLexer.Operators.K_Op<double>;
global using k_Op = PdfLexer.Operators.k_Op<double>;
global using l_Op = PdfLexer.Operators.l_Op<double>;
global using m_Op = PdfLexer.Operators.m_Op<double>;
global using M_Op = PdfLexer.Operators.M_Op<double>;
global using MP_Op = PdfLexer.Operators.MP_Op<double>;
global using n_Op = PdfLexer.Operators.n_Op<double>;
global using q_Op = PdfLexer.Operators.q_Op<double>;
global using Q_Op = PdfLexer.Operators.Q_Op<double>;
global using re_Op = PdfLexer.Operators.re_Op<double>;
global using RG_Op = PdfLexer.Operators.RG_Op<double>;
global using rg_Op = PdfLexer.Operators.rg_Op<double>;
global using ri_Op = PdfLexer.Operators.ri_Op<double>;
global using s_Op = PdfLexer.Operators.s_Op<double>;
global using S_Op = PdfLexer.Operators.S_Op<double>;
global using SC_Op = PdfLexer.Operators.SC_Op<double>;
global using sc_Op = PdfLexer.Operators.sc_Op<double>;
global using SCN_Op = PdfLexer.Operators.SCN_Op<double>;
global using scn_Op = PdfLexer.Operators.scn_Op<double>;
global using sh_Op = PdfLexer.Operators.sh_Op<double>;
global using T_Star_Op = PdfLexer.Operators.T_Star_Op<double>;
global using Tc_Op = PdfLexer.Operators.Tc_Op<double>;
global using Td_Op = PdfLexer.Operators.Td_Op<double>;
global using TD_Op = PdfLexer.Operators.TD_Op<double>;
global using Tf_Op = PdfLexer.Operators.Tf_Op<double>;
global using Tj_Op = PdfLexer.Operators.Tj_Op<double>;
global using TJ_Op = PdfLexer.Operators.TJ_Op<double>;
global using TL_Op = PdfLexer.Operators.TL_Op<double>;
global using Tm_Op = PdfLexer.Operators.Tm_Op<double>;
global using Tr_Op = PdfLexer.Operators.Tr_Op<double>;
global using Ts_Op = PdfLexer.Operators.Ts_Op<double>;
global using Tw_Op = PdfLexer.Operators.Tw_Op<double>;
global using Tz_Op = PdfLexer.Operators.Tz_Op<double>;
global using v_Op = PdfLexer.Operators.v_Op<double>;
global using w_Op = PdfLexer.Operators.w_Op<double>;
global using W_Op = PdfLexer.Operators.W_Op<double>;
global using W_Star_Op = PdfLexer.Operators.W_Star_Op<double>;
global using y_Op = PdfLexer.Operators.y_Op<double>;
global using singlequote_Op = PdfLexer.Operators.singlequote_Op<double>;
global using doublequote_Op = PdfLexer.Operators.doublequote_Op<double>;



using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using PdfLexer.Content;

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

    public static class ParseOpMapping<T> where T: struct, IFloatingPoint<T>
    {
        public static Dictionary<int, PdfOperator.ParseOp<T>> Parsers = new () {
            [98] =  b_Op<T>.Parse, 
            [66] =  B_Op<T>.Parse, 
            [10850] =  b_Star_Op<T>.Parse, 
            [10818] =  B_Star_Op<T>.Parse, 
            [4408386] =  PdfOperator.ParseBDC<T>, 
            [18754] =  BI_Op<T>.Parse, 
            [4410690] =  BMC_Op<T>.Parse, 
            [21570] =  BT_Op<T>.Parse, 
            [22594] =  BX_Op<T>.Parse, 
            [99] =  c_Op<T>.Parse, 
            [28003] =  cm_Op<T>.Parse, 
            [21315] =  CS_Op<T>.Parse, 
            [29539] =  cs_Op<T>.Parse, 
            [100] =  PdfOperator.Parsed<T>, 
            [12388] =  d0_Op<T>.Parse, 
            [12644] =  d1_Op<T>.Parse, 
            [28484] =  Do_Op<T>.Parse, 
            [20548] =  PdfOperator.ParseDP<T>, 
            [18757] =  EI_Op<T>.Parse, 
            [4410693] =  EMC_Op<T>.Parse, 
            [21573] =  ET_Op<T>.Parse, 
            [22597] =  EX_Op<T>.Parse, 
            [102] =  f_Op<T>.Parse, 
            [70] =  F_Op<T>.Parse, 
            [10854] =  f_Star_Op<T>.Parse, 
            [71] =  G_Op<T>.Parse, 
            [103] =  g_Op<T>.Parse, 
            [29543] =  gs_Op<T>.Parse, 
            [104] =  h_Op<T>.Parse, 
            [105] =  i_Op<T>.Parse, 
            [17481] =  ID_Op<T>.Parse, 
            [106] =  j_Op<T>.Parse, 
            [74] =  J_Op<T>.Parse, 
            [75] =  K_Op<T>.Parse, 
            [107] =  k_Op<T>.Parse, 
            [108] =  l_Op<T>.Parse, 
            [109] =  m_Op<T>.Parse, 
            [77] =  M_Op<T>.Parse, 
            [20557] =  MP_Op<T>.Parse, 
            [110] =  n_Op<T>.Parse, 
            [113] =  q_Op<T>.Parse, 
            [81] =  Q_Op<T>.Parse, 
            [25970] =  re_Op<T>.Parse, 
            [18258] =  RG_Op<T>.Parse, 
            [26482] =  rg_Op<T>.Parse, 
            [26994] =  ri_Op<T>.Parse, 
            [115] =  s_Op<T>.Parse, 
            [83] =  S_Op<T>.Parse, 
            [17235] =  PdfOperator.ParseSC<T>, 
            [25459] =  PdfOperator.Parsesc<T>, 
            [5129043] =  PdfOperator.ParseSCN<T>, 
            [7234419] =  PdfOperator.Parsescn<T>, 
            [26739] =  sh_Op<T>.Parse, 
            [10836] =  T_Star_Op<T>.Parse, 
            [25428] =  Tc_Op<T>.Parse, 
            [25684] =  Td_Op<T>.Parse, 
            [17492] =  TD_Op<T>.Parse, 
            [26196] =  Tf_Op<T>.Parse, 
            [27220] =  PdfOperator.ParseTj<T>, 
            [19028] =  PdfOperator.ParseTJ<T>, 
            [19540] =  TL_Op<T>.Parse, 
            [27988] =  Tm_Op<T>.Parse, 
            [29268] =  Tr_Op<T>.Parse, 
            [29524] =  Ts_Op<T>.Parse, 
            [30548] =  Tw_Op<T>.Parse, 
            [31316] =  Tz_Op<T>.Parse, 
            [118] =  v_Op<T>.Parse, 
            [119] =  w_Op<T>.Parse, 
            [87] =  W_Op<T>.Parse, 
            [10839] =  W_Star_Op<T>.Parse, 
            [121] =  y_Op<T>.Parse, 
            [39] =  PdfOperator.Parsesinglequote<T>, 
            [34] =  PdfOperator.Parsedoublequote<T>, 
        };

                public static PdfOperator.ParseOp<T>?[] SingleByteParsers = new PdfOperator.ParseOp<T>?[] {
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
            PdfOperator.Parsedoublequote<T>,
            null,
            null,
            null,
            null,
            PdfOperator.Parsesinglequote<T>,
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
            B_Op<T>.Parse,
            null,
            null,
            null,
            F_Op<T>.Parse,
            G_Op<T>.Parse,
            null,
            null,
            J_Op<T>.Parse,
            K_Op<T>.Parse,
            null,
            M_Op<T>.Parse,
            null,
            null,
            null,
            Q_Op<T>.Parse,
            null,
            S_Op<T>.Parse,
            null,
            null,
            null,
            W_Op<T>.Parse,
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
            b_Op<T>.Parse,
            c_Op<T>.Parse,
            PdfOperator.Parsed<T>,
            null,
            f_Op<T>.Parse,
            g_Op<T>.Parse,
            h_Op<T>.Parse,
            i_Op<T>.Parse,
            j_Op<T>.Parse,
            k_Op<T>.Parse,
            l_Op<T>.Parse,
            m_Op<T>.Parse,
            n_Op<T>.Parse,
            null,
            null,
            q_Op<T>.Parse,
            null,
            s_Op<T>.Parse,
            null,
            null,
            v_Op<T>.Parse,
            w_Op<T>.Parse,
            null,
            y_Op<T>.Parse,
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
    }


    /// <summary>
    /// Close, fill, and stroke path using nonzero winding number rule
    /// </summary>
    public partial class b_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'b' };
        public PdfOperatorType Type => PdfOperatorType.b;
        public static readonly b_Op<T> Value = new ();

        // Close, fill, and stroke path using nonzero winding number rule
        public b_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is b_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static b_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill and stroke path using nonzero winding number rule
    /// </summary>
    public partial class B_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B' };
        public PdfOperatorType Type => PdfOperatorType.B;
        public static readonly B_Op<T> Value = new ();

        // Fill and stroke path using nonzero winding number rule
        public B_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is B_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static B_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Close, fill, and stroke path using even-odd rule
    /// </summary>
    public partial class b_Star_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'b', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.b_Star;
        public static readonly b_Star_Op<T> Value = new ();

        // Close, fill, and stroke path using even-odd rule
        public b_Star_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is b_Star_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static b_Star_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill and stroke path using even-odd rule
    /// </summary>
    public partial class B_Star_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.B_Star;
        public static readonly B_Star_Op<T> Value = new ();

        // Fill and stroke path using even-odd rule
        public B_Star_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is B_Star_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static B_Star_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin marked-content sequence with property list
    /// </summary>
    public partial class BDC_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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
    public partial class BI_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.BI;
        public static readonly BI_Op<T> Value = new ();

        // Begin inline image object
        public BI_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is BI_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static BI_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin marked-content sequence
    /// </summary>
    public partial class BMC_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static BMC_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new BMC_Op<T>(a0);
        }
    }

    /// <summary>
    /// Begin text object
    /// </summary>
    public partial class BT_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.BT;
        public static readonly BT_Op<T> Value = new ();

        // Begin text object
        public BT_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is BT_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static BT_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Begin compatibility section
    /// </summary>
    public partial class BX_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'B', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.BX;
        public static readonly BX_Op<T> Value = new ();

        // Begin compatibility section
        public BX_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is BX_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static BX_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append curved segment to path (three control points)
    /// </summary>
    public partial class c_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.c;
        public T x1 { get; }
        public T y1 { get; }
        public T x2 { get; }
        public T y2 { get; }
        public T x3 { get; }
        public T y3 { get; }
        public c_Op(T x1, T y1, T x2, T y2, T x3, T y3)
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
            FPC<T>.Util.Write(x1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x1, T y1, T x2, T y2, T x3, T y3, Stream stream) 
        {
            Write(x1, y1, x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x1, T y1, T x2, T y2, T x3, T y3, Stream stream) 
        {
            FPC<T>.Util.Write(x1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T", "T", "T"  };

        public static c_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
            var a4 = FPC<T>.Util.Parse<T>(ctx, data, operands[4]);
            var a5 = FPC<T>.Util.Parse<T>(ctx, data, operands[5]);
    
            return new c_Op<T>(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Concatenate matrix to current transformation matrix
    /// </summary>
    public partial class cm_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'c', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.cm;
        public T a { get; }
        public T b { get; }
        public T c { get; }
        public T d { get; }
        public T e { get; }
        public T f { get; }
        public cm_Op(T a, T b, T c, T d, T e, T f)
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
            FPC<T>.Util.Write(a, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(d, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(e, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T a, T b, T c, T d, T e, T f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T a, T b, T c, T d, T e, T f, Stream stream) 
        {
            FPC<T>.Util.Write(a, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(d, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(e, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T", "T", "T"  };

        public static cm_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
            var a4 = FPC<T>.Util.Parse<T>(ctx, data, operands[4]);
            var a5 = FPC<T>.Util.Parse<T>(ctx, data, operands[5]);
    
            return new cm_Op<T>(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Set color space for stroking operations
    /// </summary>
    public partial class CS_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static CS_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new CS_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set color space for nonstroking operations
    /// </summary>
    public partial class cs_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static cs_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new cs_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set line dash pattern
    /// </summary>
    public partial class d_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.d;
        public PdfArray dashArray { get; }
        public T dashPhase { get; }
        public d_Op(PdfArray dashArray, T dashPhase)
        {
            this.dashArray = dashArray;
            this.dashPhase = dashPhase;
        }
    }

    /// <summary>
    /// Set glyph width in Type 3 font
    /// </summary>
    public partial class d0_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd', (byte) '0' };
        public PdfOperatorType Type => PdfOperatorType.d0;
        public T wx { get; }
        public T wy { get; }
        public d0_Op(T wx, T wy)
        {
            this.wx = wx;
            this.wy = wy;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(wx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T wx, T wy, Stream stream) 
        {
            Write(wx, wy, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T wx, T wy, Stream stream) 
        {
            FPC<T>.Util.Write(wx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T"  };

        public static d0_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new d0_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Set glyph width and bounding box in Type 3 font
    /// </summary>
    public partial class d1_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'd', (byte) '1' };
        public PdfOperatorType Type => PdfOperatorType.d1;
        public T wx { get; }
        public T wy { get; }
        public T llx { get; }
        public T lly { get; }
        public T urx { get; }
        public T ury { get; }
        public d1_Op(T wx, T wy, T llx, T lly, T urx, T ury)
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
            FPC<T>.Util.Write(wx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(wy, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(llx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(lly, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(urx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T wx, T wy, T llx, T lly, T urx, T ury, Stream stream) 
        {
            Write(wx, wy, llx, lly, urx, ury, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T wx, T wy, T llx, T lly, T urx, T ury, Stream stream) 
        {
            FPC<T>.Util.Write(wx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(wy, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(llx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(lly, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(urx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T", "T", "T"  };

        public static d1_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
            var a4 = FPC<T>.Util.Parse<T>(ctx, data, operands[4]);
            var a5 = FPC<T>.Util.Parse<T>(ctx, data, operands[5]);
    
            return new d1_Op<T>(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Invoke named XObject
    /// </summary>
    public partial class Do_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static Do_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new Do_Op<T>(a0);
        }
    }

    /// <summary>
    /// Define marked-content point with property list
    /// </summary>
    public partial class DP_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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
    public partial class EI_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.EI;
        public static readonly EI_Op<T> Value = new ();

        // End inline image object
        public EI_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is EI_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static EI_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End marked-content sequence
    /// </summary>
    public partial class EMC_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'M', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.EMC;
        public static readonly EMC_Op<T> Value = new ();

        // End marked-content sequence
        public EMC_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is EMC_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static EMC_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End text object
    /// </summary>
    public partial class ET_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.ET;
        public static readonly ET_Op<T> Value = new ();

        // End text object
        public ET_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is ET_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static ET_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// End compatibility section
    /// </summary>
    public partial class EX_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'E', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.EX;
        public static readonly EX_Op<T> Value = new ();

        // End compatibility section
        public EX_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is EX_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static EX_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using nonzero winding number rule
    /// </summary>
    public partial class f_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.f;
        public static readonly f_Op<T> Value = new ();

        // Fill path using nonzero winding number rule
        public f_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is f_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static f_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using nonzero winding number rule (obsolete)
    /// </summary>
    public partial class F_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'F' };
        public PdfOperatorType Type => PdfOperatorType.F;
        public static readonly F_Op<T> Value = new ();

        // Fill path using nonzero winding number rule (obsolete)
        public F_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is F_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static F_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Fill path using even-odd rule
    /// </summary>
    public partial class f_Star_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'f', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.f_Star;
        public static readonly f_Star_Op<T> Value = new ();

        // Fill path using even-odd rule
        public f_Star_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is f_Star_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static f_Star_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set gray level for stroking operations
    /// </summary>
    public partial class G_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.G;
        public T gray { get; }
        public G_Op(T gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T gray, Stream stream) 
        {
            FPC<T>.Util.Write(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static G_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new G_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set gray level for nonstroking operations
    /// </summary>
    public partial class g_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.g;
        public T gray { get; }
        public g_Op(T gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T gray, Stream stream) 
        {
            FPC<T>.Util.Write(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static g_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new g_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set parameters from graphics state parameter dictionary
    /// </summary>
    public partial class gs_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static gs_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new gs_Op<T>(a0);
        }
    }

    /// <summary>
    /// Close subpath
    /// </summary>
    public partial class h_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'h' };
        public PdfOperatorType Type => PdfOperatorType.h;
        public static readonly h_Op<T> Value = new ();

        // Close subpath
        public h_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is h_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static h_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set flatness tolerance
    /// </summary>
    public partial class i_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'i' };
        public PdfOperatorType Type => PdfOperatorType.i;
        public T flatness { get; }
        public i_Op(T flatness)
        {
            this.flatness = flatness;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T flatness, Stream stream) 
        {
            Write(flatness, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T flatness, Stream stream) 
        {
            FPC<T>.Util.Write(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static i_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new i_Op<T>(a0);
        }
    }

    /// <summary>
    /// Begin inline image data
    /// </summary>
    public partial class ID_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'I', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.ID;
        public static readonly ID_Op<T> Value = new ();

        // Begin inline image data
        public ID_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is ID_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static ID_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set line join style
    /// </summary>
    public partial class j_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static j_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new j_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set line capstyle
    /// </summary>
    public partial class J_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static J_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new J_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set CMYK color for stroking operations
    /// </summary>
    public partial class K_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'K' };
        public PdfOperatorType Type => PdfOperatorType.K;
        public T c { get; }
        public T m { get; }
        public T y { get; }
        public T k { get; }
        public K_Op(T c, T m, T y, T k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(m, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T c, T m, T y, T k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T c, T m, T y, T k, Stream stream) 
        {
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(m, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T"  };

        public static K_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
    
            return new K_Op<T>(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set CMYK color for nonstroking operations
    /// </summary>
    public partial class k_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'k' };
        public PdfOperatorType Type => PdfOperatorType.k;
        public T c { get; }
        public T m { get; }
        public T y { get; }
        public T k { get; }
        public k_Op(T c, T m, T y, T k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(m, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T c, T m, T y, T k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T c, T m, T y, T k, Stream stream) 
        {
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(m, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T"  };

        public static k_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
    
            return new k_Op<T>(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Append straight line segment to path
    /// </summary>
    public partial class l_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'l' };
        public PdfOperatorType Type => PdfOperatorType.l;
        public T x { get; }
        public T y { get; }
        public l_Op(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x, T y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x, T y, Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T"  };

        public static l_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new l_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Begin new subpath
    /// </summary>
    public partial class m_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.m;
        public T x { get; }
        public T y { get; }
        public m_Op(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x, T y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x, T y, Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T"  };

        public static m_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new m_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Set miter limit
    /// </summary>
    public partial class M_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'M' };
        public PdfOperatorType Type => PdfOperatorType.M;
        public T miterLimit { get; }
        public M_Op(T miterLimit)
        {
            this.miterLimit = miterLimit;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T miterLimit, Stream stream) 
        {
            Write(miterLimit, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T miterLimit, Stream stream) 
        {
            FPC<T>.Util.Write(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static M_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new M_Op<T>(a0);
        }
    }

    /// <summary>
    /// Define marked-content point
    /// </summary>
    public partial class MP_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static MP_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new MP_Op<T>(a0);
        }
    }

    /// <summary>
    /// End path without filling or stroking
    /// </summary>
    public partial class n_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.n;
        public static readonly n_Op<T> Value = new ();

        // End path without filling or stroking
        public n_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is n_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static n_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Save graphics state
    /// </summary>
    public partial class q_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'q' };
        public PdfOperatorType Type => PdfOperatorType.q;
        public static readonly q_Op<T> Value = new ();

        // Save graphics state
        public q_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is q_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static q_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Restore graphics state
    /// </summary>
    public partial class Q_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'Q' };
        public PdfOperatorType Type => PdfOperatorType.Q;
        public static readonly Q_Op<T> Value = new ();

        // Restore graphics state
        public Q_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is Q_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static Q_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append rectangle to path
    /// </summary>
    public partial class re_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'r', (byte) 'e' };
        public PdfOperatorType Type => PdfOperatorType.re;
        public T x { get; }
        public T y { get; }
        public T width { get; }
        public T height { get; }
        public re_Op(T x, T y, T width, T height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(width, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x, T y, T width, T height, Stream stream) 
        {
            Write(x, y, width, height, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x, T y, T width, T height, Stream stream) 
        {
            FPC<T>.Util.Write(x, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(width, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T"  };

        public static re_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
    
            return new re_Op<T>(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set RGB color for stroking operations
    /// </summary>
    public partial class RG_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'R', (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.RG;
        public T r { get; }
        public T g { get; }
        public T b { get; }
        public RG_Op(T r, T g, T b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(r, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(g, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T r, T g, T b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T r, T g, T b, Stream stream) 
        {
            FPC<T>.Util.Write(r, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(g, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T"  };

        public static RG_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
    
            return new RG_Op<T>(a0, a1, a2);
        }
    }

    /// <summary>
    /// Set RGB color for nonstroking operations
    /// </summary>
    public partial class rg_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'r', (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.rg;
        public T r { get; }
        public T g { get; }
        public T b { get; }
        public rg_Op(T r, T g, T b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(r, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(g, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T r, T g, T b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T r, T g, T b, Stream stream) 
        {
            FPC<T>.Util.Write(r, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(g, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T"  };

        public static rg_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
    
            return new rg_Op<T>(a0, a1, a2);
        }
    }

    /// <summary>
    /// Set color rendering intent
    /// </summary>
    public partial class ri_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static ri_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new ri_Op<T>(a0);
        }
    }

    /// <summary>
    /// Close and stroke path
    /// </summary>
    public partial class s_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.s;
        public static readonly s_Op<T> Value = new ();

        // Close and stroke path
        public s_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is s_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static s_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Stroke path
    /// </summary>
    public partial class S_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S' };
        public PdfOperatorType Type => PdfOperatorType.S;
        public static readonly S_Op<T> Value = new ();

        // Stroke path
        public S_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is S_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static S_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set color for stroking operations
    /// </summary>
    public partial class SC_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.SC;
        public List<T> colorInfo { get; }
        public SC_Op(List<T> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    /// <summary>
    /// Set color for nonstroking operations
    /// </summary>
    public partial class sc_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.sc;
        public List<T> colorInfo { get; }
        public sc_Op(List<T> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    /// <summary>
    /// Set color for stroking operations (ICCBased and special color spaces)
    /// </summary>
    public partial class SCN_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'S', (byte) 'C', (byte) 'N' };
        public PdfOperatorType Type => PdfOperatorType.SCN;
        public List<T> colorInfo { get; }
        public PdfName? name { get; }
        public SCN_Op(List<T> colorInfo, PdfName? name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    /// <summary>
    /// Set color for nonstroking operations (ICCBased and special color spaces)
    /// </summary>
    public partial class scn_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 's', (byte) 'c', (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.scn;
        public List<T> colorInfo { get; }
        public PdfName? name { get; }
        public scn_Op(List<T> colorInfo, PdfName? name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    /// <summary>
    /// Paint area defined by shading pattern
    /// </summary>
    public partial class sh_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static sh_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new sh_Op<T>(a0);
        }
    }

    /// <summary>
    /// Move to start of next text line
    /// </summary>
    public partial class T_Star_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.T_Star;
        public static readonly T_Star_Op<T> Value = new ();

        // Move to start of next text line
        public T_Star_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is T_Star_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static T_Star_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set character spacing
    /// </summary>
    public partial class Tc_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.Tc;
        public T charSpace { get; }
        public Tc_Op(T charSpace)
        {
            this.charSpace = charSpace;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T charSpace, Stream stream) 
        {
            Write(charSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T charSpace, Stream stream) 
        {
            FPC<T>.Util.Write(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static Tc_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new Tc_Op<T>(a0);
        }
    }

    /// <summary>
    /// Move text position
    /// </summary>
    public partial class Td_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.Td;
        public T tx { get; }
        public T ty { get; }
        public Td_Op(T tx, T ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(tx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T tx, T ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T tx, T ty, Stream stream) 
        {
            FPC<T>.Util.Write(tx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T"  };

        public static Td_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new Td_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Move text position and set leading
    /// </summary>
    public partial class TD_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.TD;
        public T tx { get; }
        public T ty { get; }
        public TD_Op(T tx, T ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(tx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T tx, T ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T tx, T ty, Stream stream) 
        {
            FPC<T>.Util.Write(tx, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T"  };

        public static TD_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new TD_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Set text font and size
    /// </summary>
    public partial class Tf_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.Tf;
        public PdfName font { get; }
        public T size { get; }
        public Tf_Op(PdfName font, T size)
        {
            this.font = font;
            this.size = size;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName font, T size, Stream stream) 
        {
            Write(font, size, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName font, T size, Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "PdfName", "T"  };

        public static Tf_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
    
            return new Tf_Op<T>(a0, a1);
        }
    }

    /// <summary>
    /// Show text
    /// </summary>
    public partial class Tj_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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
    public partial class TJ_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'J' };
        public PdfOperatorType Type => PdfOperatorType.TJ;
        public List<TJ_Item<T>> info { get; }
        public TJ_Op(List<TJ_Item<T>> info)
        {
            this.info = info;
        }
    }

    /// <summary>
    /// Set text leading
    /// </summary>
    public partial class TL_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'L' };
        public PdfOperatorType Type => PdfOperatorType.TL;
        public T leading { get; }
        public TL_Op(T leading)
        {
            this.leading = leading;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T leading, Stream stream) 
        {
            Write(leading, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T leading, Stream stream) 
        {
            FPC<T>.Util.Write(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static TL_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new TL_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set text matrix and text line matrix
    /// </summary>
    public partial class Tm_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.Tm;
        public T a { get; }
        public T b { get; }
        public T c { get; }
        public T d { get; }
        public T e { get; }
        public T f { get; }
        public Tm_Op(T a, T b, T c, T d, T e, T f)
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
            FPC<T>.Util.Write(a, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(d, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(e, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T a, T b, T c, T d, T e, T f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T a, T b, T c, T d, T e, T f, Stream stream) 
        {
            FPC<T>.Util.Write(a, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(b, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(c, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(d, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(e, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T", "T", "T"  };

        public static Tm_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
            var a4 = FPC<T>.Util.Parse<T>(ctx, data, operands[4]);
            var a5 = FPC<T>.Util.Parse<T>(ctx, data, operands[5]);
    
            return new Tm_Op<T>(a0, a1, a2, a3, a4, a5);
        }
    }

    /// <summary>
    /// Set text rendering mode
    /// </summary>
    public partial class Tr_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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

        public static Tr_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
    
            return new Tr_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set text rise
    /// </summary>
    public partial class Ts_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.Ts;
        public T rise { get; }
        public Ts_Op(T rise)
        {
            this.rise = rise;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T rise, Stream stream) 
        {
            Write(rise, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T rise, Stream stream) 
        {
            FPC<T>.Util.Write(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static Ts_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new Ts_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set word spacing
    /// </summary>
    public partial class Tw_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.Tw;
        public T wordSpace { get; }
        public Tw_Op(T wordSpace)
        {
            this.wordSpace = wordSpace;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T wordSpace, Stream stream) 
        {
            Write(wordSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T wordSpace, Stream stream) 
        {
            FPC<T>.Util.Write(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static Tw_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new Tw_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set horizontal text scaling
    /// </summary>
    public partial class Tz_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'T', (byte) 'z' };
        public PdfOperatorType Type => PdfOperatorType.Tz;
        public T scale { get; }
        public Tz_Op(T scale)
        {
            this.scale = scale;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T scale, Stream stream) 
        {
            Write(scale, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T scale, Stream stream) 
        {
            FPC<T>.Util.Write(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static Tz_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new Tz_Op<T>(a0);
        }
    }

    /// <summary>
    /// Append curved segment to path (initial point replicated)
    /// </summary>
    public partial class v_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'v' };
        public PdfOperatorType Type => PdfOperatorType.v;
        public T x2 { get; }
        public T y2 { get; }
        public T x3 { get; }
        public T y3 { get; }
        public v_Op(T x2, T y2, T x3, T y3)
        {
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(x2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x2, T y2, T x3, T y3, Stream stream) 
        {
            Write(x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x2, T y2, T x3, T y3, Stream stream) 
        {
            FPC<T>.Util.Write(x2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y2, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T"  };

        public static v_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
    
            return new v_Op<T>(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Set line width
    /// </summary>
    public partial class w_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.w;
        public T lineWidth { get; }
        public w_Op(T lineWidth)
        {
            this.lineWidth = lineWidth;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T lineWidth, Stream stream) 
        {
            Write(lineWidth, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T lineWidth, Stream stream) 
        {
            FPC<T>.Util.Write(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T"  };

        public static w_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
    
            return new w_Op<T>(a0);
        }
    }

    /// <summary>
    /// Set clipping path using nonzero winding number rule
    /// </summary>
    public partial class W_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'W' };
        public PdfOperatorType Type => PdfOperatorType.W;
        public static readonly W_Op<T> Value = new ();

        // Set clipping path using nonzero winding number rule
        public W_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is W_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static W_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Set clipping path using even-odd rule
    /// </summary>
    public partial class W_Star_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'W', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.W_Star;
        public static readonly W_Star_Op<T> Value = new ();

        // Set clipping path using even-odd rule
        public W_Star_Op()
        {

        }

        public override bool Equals(object? obj) 
        {
            if (obj == null) return false;
            if (obj is W_Star_Op<T>) 
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode() => 1;

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
        public static W_Star_Op<T> Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;

    }

    /// <summary>
    /// Append curved segment to path (final point replicated)
    /// </summary>
    public partial class y_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) 'y' };
        public PdfOperatorType Type => PdfOperatorType.y;
        public T x1 { get; }
        public T y1 { get; }
        public T x3 { get; }
        public T y3 { get; }
        public y_Op(T x1, T y1, T x3, T y3)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            FPC<T>.Util.Write(x1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(T x1, T y1, T x3, T y3, Stream stream) 
        {
            Write(x1, y1, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(T x1, T y1, T x3, T y3, Stream stream) 
        {
            FPC<T>.Util.Write(x1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y1, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(x3, stream);
            stream.WriteByte((byte)' ');
            FPC<T>.Util.Write(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static readonly List<string> OpTypes = new () { "T", "T", "T", "T"  };

        public static y_Op<T>? Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = FPC<T>.Util.Parse<T>(ctx, data, operands[0]);
            var a1 = FPC<T>.Util.Parse<T>(ctx, data, operands[1]);
            var a2 = FPC<T>.Util.Parse<T>(ctx, data, operands[2]);
            var a3 = FPC<T>.Util.Parse<T>(ctx, data, operands[3]);
    
            return new y_Op<T>(a0, a1, a2, a3);
        }
    }

    /// <summary>
    /// Move to next line and show text
    /// </summary>
    public partial class singlequote_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
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
    public partial class doublequote_Op<T> : IPdfOperation<T> where T : struct, IFloatingPoint<T>
    {
        public static byte[] OpData { get; } = new byte[] { (byte) '"' };
        public PdfOperatorType Type => PdfOperatorType.doublequote;
        public T aw { get; }
        public T ac { get; }
        public byte[] text { get; }
        public doublequote_Op(T aw, T ac, byte[] text)
        {
            this.aw = aw;
            this.ac = ac;
            this.text = text;
        }
    }
}

#endif