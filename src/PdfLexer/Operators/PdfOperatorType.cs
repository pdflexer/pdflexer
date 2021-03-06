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
        Unknown,
        NoOp,
        RawOp,
        // End of content
        EOC,
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
        public static Dictionary<int, PdfOperator.ParseOp> Parsers = new Dictionary<int, PdfOperator.ParseOp> {
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
    }


    // Close, fill, and stroke path using nonzero winding number rule
    public partial class b_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'b' };
        public PdfOperatorType Type => PdfOperatorType.b;
        public static b_Op Value = new b_Op();

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

    // Fill and stroke path using nonzero winding number rule
    public partial class B_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B' };
        public PdfOperatorType Type => PdfOperatorType.B;
        public static B_Op Value = new B_Op();

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

    // Close, fill, and stroke path using even-odd rule
    public partial class b_Star_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'b', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.b_Star;
        public static b_Star_Op Value = new b_Star_Op();

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

    // Fill and stroke path using even-odd rule
    public partial class B_Star_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.B_Star;
        public static B_Star_Op Value = new B_Star_Op();

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

    // Begin marked-content sequence with property list
    public partial class BDC_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) 'D', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.BDC;
        public PdfName tag { get; set; }
        public PdfObject props { get; set; }
        public BDC_Op(PdfName tag, PdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }

    // Begin inline image object
    public partial class BI_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.BI;
        public static BI_Op Value = new BI_Op();

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

    // Begin marked-content sequence
    public partial class BMC_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) 'M', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.BMC;
        public PdfName tag { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static BMC_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Begin text object
    public partial class BT_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.BT;
        public static BT_Op Value = new BT_Op();

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

    // Begin compatibility section
    public partial class BX_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'B', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.BX;
        public static BX_Op Value = new BX_Op();

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

    // Append curved segment to path (three control points)
    public partial class c_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.c;
        public decimal x1 { get; set; }
        public decimal y1 { get; set; }
        public decimal x2 { get; set; }
        public decimal y2 { get; set; }
        public decimal x3 { get; set; }
        public decimal y3 { get; set; }
        public c_Op(decimal x1, decimal y1, decimal x2, decimal y2, decimal x3, decimal y3)
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
            PdfOperator.Writedecimal(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x1, decimal y1, decimal x2, decimal y2, decimal x3, decimal y3, Stream stream) 
        {
            Write(x1, y1, x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x1, decimal y1, decimal x2, decimal y2, decimal x3, decimal y3, Stream stream) 
        {
            PdfOperator.Writedecimal(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal", "decimal", "decimal"  };

        public static c_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new c_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    // Concatenate matrix to current transformation matrix
    public partial class cm_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'c', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.cm;
        public decimal a { get; set; }
        public decimal b { get; set; }
        public decimal c { get; set; }
        public decimal d { get; set; }
        public decimal e { get; set; }
        public decimal f { get; set; }
        public cm_Op(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f)
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
            PdfOperator.Writedecimal(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f, Stream stream) 
        {
            PdfOperator.Writedecimal(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal", "decimal", "decimal"  };

        public static cm_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new cm_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    // Set color space for stroking operations
    public partial class CS_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'C', (byte) 'S' };
        public PdfOperatorType Type => PdfOperatorType.CS;
        public PdfName name { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static CS_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Set color space for nonstroking operations
    public partial class cs_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'c', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.cs;
        public PdfName name { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static cs_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Set line dash pattern
    public partial class d_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.d;
        public PdfArray dashArray { get; set; }
        public decimal dashPhase { get; set; }
        public d_Op(PdfArray dashArray, decimal dashPhase)
        {
            this.dashArray = dashArray;
            this.dashPhase = dashPhase;
        }
    }

    // Set glyph width in Type 3 font
    public partial class d0_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'd', (byte) '0' };
        public PdfOperatorType Type => PdfOperatorType.d0;
        public decimal wx { get; set; }
        public decimal wy { get; set; }
        public d0_Op(decimal wx, decimal wy)
        {
            this.wx = wx;
            this.wy = wy;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal wx, decimal wy, Stream stream) 
        {
            Write(wx, wy, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal wx, decimal wy, Stream stream) 
        {
            PdfOperator.Writedecimal(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(wy, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal"  };

        public static d0_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new d0_Op(a0, a1);
        }
    }

    // Set glyph width and bounding box in Type 3 font
    public partial class d1_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'd', (byte) '1' };
        public PdfOperatorType Type => PdfOperatorType.d1;
        public decimal wx { get; set; }
        public decimal wy { get; set; }
        public decimal llx { get; set; }
        public decimal lly { get; set; }
        public decimal urx { get; set; }
        public decimal ury { get; set; }
        public d1_Op(decimal wx, decimal wy, decimal llx, decimal lly, decimal urx, decimal ury)
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
            PdfOperator.Writedecimal(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(wy, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(llx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(lly, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(urx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal wx, decimal wy, decimal llx, decimal lly, decimal urx, decimal ury, Stream stream) 
        {
            Write(wx, wy, llx, lly, urx, ury, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal wx, decimal wy, decimal llx, decimal lly, decimal urx, decimal ury, Stream stream) 
        {
            PdfOperator.Writedecimal(wx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(wy, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(llx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(lly, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(urx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ury, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal", "decimal", "decimal"  };

        public static d1_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new d1_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    // Invoke named XObject
    public partial class Do_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'D', (byte) 'o' };
        public PdfOperatorType Type => PdfOperatorType.Do;
        public PdfName name { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static Do_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Define marked-content point with property list
    public partial class DP_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'D', (byte) 'P' };
        public PdfOperatorType Type => PdfOperatorType.DP;
        public PdfName tag { get; set; }
        public PdfObject props { get; set; }
        public DP_Op(PdfName tag, PdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }

    // End inline image object
    public partial class EI_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'E', (byte) 'I' };
        public PdfOperatorType Type => PdfOperatorType.EI;
        public static EI_Op Value = new EI_Op();

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

    // End marked-content sequence
    public partial class EMC_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'E', (byte) 'M', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.EMC;
        public static EMC_Op Value = new EMC_Op();

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

    // End text object
    public partial class ET_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'E', (byte) 'T' };
        public PdfOperatorType Type => PdfOperatorType.ET;
        public static ET_Op Value = new ET_Op();

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

    // End compatibility section
    public partial class EX_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'E', (byte) 'X' };
        public PdfOperatorType Type => PdfOperatorType.EX;
        public static EX_Op Value = new EX_Op();

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

    // Fill path using nonzero winding number rule
    public partial class f_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.f;
        public static f_Op Value = new f_Op();

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

    // Fill path using nonzero winding number rule (obsolete)
    public partial class F_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'F' };
        public PdfOperatorType Type => PdfOperatorType.F;
        public static F_Op Value = new F_Op();

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

    // Fill path using even-odd rule
    public partial class f_Star_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'f', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.f_Star;
        public static f_Star_Op Value = new f_Star_Op();

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

    // Set gray level for stroking operations
    public partial class G_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.G;
        public decimal gray { get; set; }
        public G_Op(decimal gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal gray, Stream stream) 
        {
            PdfOperator.Writedecimal(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static G_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new G_Op(a0);
        }
    }

    // Set gray level for nonstroking operations
    public partial class g_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.g;
        public decimal gray { get; set; }
        public g_Op(decimal gray)
        {
            this.gray = gray;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal gray, Stream stream) 
        {
            Write(gray, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal gray, Stream stream) 
        {
            PdfOperator.Writedecimal(gray, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static g_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new g_Op(a0);
        }
    }

    // Set parameters from graphics state parameter dictionary
    public partial class gs_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'g', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.gs;
        public PdfName name { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static gs_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Close subpath
    public partial class h_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'h' };
        public PdfOperatorType Type => PdfOperatorType.h;
        public static h_Op Value = new h_Op();

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

    // Set flatness tolerance
    public partial class i_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'i' };
        public PdfOperatorType Type => PdfOperatorType.i;
        public decimal flatness { get; set; }
        public i_Op(decimal flatness)
        {
            this.flatness = flatness;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal flatness, Stream stream) 
        {
            Write(flatness, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal flatness, Stream stream) 
        {
            PdfOperator.Writedecimal(flatness, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static i_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new i_Op(a0);
        }
    }

    // Begin inline image data
    public partial class ID_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'I', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.ID;
        public static ID_Op Value = new ID_Op();

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

    // Set line join style
    public partial class j_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'j' };
        public PdfOperatorType Type => PdfOperatorType.j;
        public int lineJoin { get; set; }
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

        private static List<string> OpTypes = new List<string> { "int"  };

        public static j_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Set line capstyle
    public partial class J_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'J' };
        public PdfOperatorType Type => PdfOperatorType.J;
        public int lineCap { get; set; }
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

        private static List<string> OpTypes = new List<string> { "int"  };

        public static J_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Set CMYK color for stroking operations
    public partial class K_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'K' };
        public PdfOperatorType Type => PdfOperatorType.K;
        public decimal c { get; set; }
        public decimal m { get; set; }
        public decimal y { get; set; }
        public decimal k { get; set; }
        public K_Op(decimal c, decimal m, decimal y, decimal k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal c, decimal m, decimal y, decimal k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal c, decimal m, decimal y, decimal k, Stream stream) 
        {
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal"  };

        public static K_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new K_Op(a0, a1, a2, a3);
        }
    }

    // Set CMYK color for nonstroking operations
    public partial class k_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'k' };
        public PdfOperatorType Type => PdfOperatorType.k;
        public decimal c { get; set; }
        public decimal m { get; set; }
        public decimal y { get; set; }
        public decimal k { get; set; }
        public k_Op(decimal c, decimal m, decimal y, decimal k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal c, decimal m, decimal y, decimal k, Stream stream) 
        {
            Write(c, m, y, k, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal c, decimal m, decimal y, decimal k, Stream stream) 
        {
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(m, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(k, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal"  };

        public static k_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new k_Op(a0, a1, a2, a3);
        }
    }

    // Append straight line segment to path
    public partial class l_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'l' };
        public PdfOperatorType Type => PdfOperatorType.l;
        public decimal x { get; set; }
        public decimal y { get; set; }
        public l_Op(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x, decimal y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x, decimal y, Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal"  };

        public static l_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new l_Op(a0, a1);
        }
    }

    // Begin new subpath
    public partial class m_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.m;
        public decimal x { get; set; }
        public decimal y { get; set; }
        public m_Op(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x, decimal y, Stream stream) 
        {
            Write(x, y, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x, decimal y, Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal"  };

        public static m_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new m_Op(a0, a1);
        }
    }

    // Set miter limit
    public partial class M_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'M' };
        public PdfOperatorType Type => PdfOperatorType.M;
        public decimal miterLimit { get; set; }
        public M_Op(decimal miterLimit)
        {
            this.miterLimit = miterLimit;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal miterLimit, Stream stream) 
        {
            Write(miterLimit, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal miterLimit, Stream stream) 
        {
            PdfOperator.Writedecimal(miterLimit, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static M_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new M_Op(a0);
        }
    }

    // Define marked-content point
    public partial class MP_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'M', (byte) 'P' };
        public PdfOperatorType Type => PdfOperatorType.MP;
        public PdfName tag { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static MP_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // End path without filling or stroking
    public partial class n_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.n;
        public static n_Op Value = new n_Op();

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

    // Save graphics state
    public partial class q_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'q' };
        public PdfOperatorType Type => PdfOperatorType.q;
        public static q_Op Value = new q_Op();

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

    // Restore graphics state
    public partial class Q_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'Q' };
        public PdfOperatorType Type => PdfOperatorType.Q;
        public static Q_Op Value = new Q_Op();

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

    // Append rectangle to path
    public partial class re_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'r', (byte) 'e' };
        public PdfOperatorType Type => PdfOperatorType.re;
        public decimal x { get; set; }
        public decimal y { get; set; }
        public decimal width { get; set; }
        public decimal height { get; set; }
        public re_Op(decimal x, decimal y, decimal width, decimal height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(width, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x, decimal y, decimal width, decimal height, Stream stream) 
        {
            Write(x, y, width, height, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x, decimal y, decimal width, decimal height, Stream stream) 
        {
            PdfOperator.Writedecimal(x, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(width, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(height, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal"  };

        public static re_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new re_Op(a0, a1, a2, a3);
        }
    }

    // Set RGB color for stroking operations
    public partial class RG_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'R', (byte) 'G' };
        public PdfOperatorType Type => PdfOperatorType.RG;
        public decimal r { get; set; }
        public decimal g { get; set; }
        public decimal b { get; set; }
        public RG_Op(decimal r, decimal g, decimal b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal r, decimal g, decimal b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal r, decimal g, decimal b, Stream stream) 
        {
            PdfOperator.Writedecimal(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal"  };

        public static RG_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
    
            return new RG_Op(a0, a1, a2);
        }
    }

    // Set RGB color for nonstroking operations
    public partial class rg_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'r', (byte) 'g' };
        public PdfOperatorType Type => PdfOperatorType.rg;
        public decimal r { get; set; }
        public decimal g { get; set; }
        public decimal b { get; set; }
        public rg_Op(decimal r, decimal g, decimal b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal r, decimal g, decimal b, Stream stream) 
        {
            Write(r, g, b, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal r, decimal g, decimal b, Stream stream) 
        {
            PdfOperator.Writedecimal(r, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(g, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal"  };

        public static rg_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
    
            return new rg_Op(a0, a1, a2);
        }
    }

    // Set color rendering intent
    public partial class ri_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'r', (byte) 'i' };
        public PdfOperatorType Type => PdfOperatorType.ri;
        public PdfName intent { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static ri_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Close and stroke path
    public partial class s_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.s;
        public static s_Op Value = new s_Op();

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

    // Stroke path
    public partial class S_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'S' };
        public PdfOperatorType Type => PdfOperatorType.S;
        public static S_Op Value = new S_Op();

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

    // Set color for stroking operations
    public partial class SC_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'S', (byte) 'C' };
        public PdfOperatorType Type => PdfOperatorType.SC;
        public List<decimal> colorInfo { get; set; }
        public SC_Op(List<decimal> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    // Set color for nonstroking operations
    public partial class sc_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 's', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.sc;
        public List<decimal> colorInfo { get; set; }
        public sc_Op(List<decimal> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }

    // Set color for stroking operations (ICCBased and special color spaces)
    public partial class SCN_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'S', (byte) 'C', (byte) 'N' };
        public PdfOperatorType Type => PdfOperatorType.SCN;
        public List<decimal> colorInfo { get; set; }
        public PdfName name { get; set; }
        public SCN_Op(List<decimal> colorInfo, PdfName name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    // Set color for nonstroking operations (ICCBased and special color spaces)
    public partial class scn_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 's', (byte) 'c', (byte) 'n' };
        public PdfOperatorType Type => PdfOperatorType.scn;
        public List<decimal> colorInfo { get; set; }
        public PdfName name { get; set; }
        public scn_Op(List<decimal> colorInfo, PdfName name)
        {
            this.colorInfo = colorInfo;
            this.name = name;
        }
    }

    // Paint area defined by shading pattern
    public partial class sh_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 's', (byte) 'h' };
        public PdfOperatorType Type => PdfOperatorType.sh;
        public PdfName name { get; set; }
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

        private static List<string> OpTypes = new List<string> { "PdfName"  };

        public static sh_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Move to start of next text line
    public partial class T_Star_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.T_Star;
        public static T_Star_Op Value = new T_Star_Op();

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

    // Set character spacing
    public partial class Tc_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'c' };
        public PdfOperatorType Type => PdfOperatorType.Tc;
        public decimal charSpace { get; set; }
        public Tc_Op(decimal charSpace)
        {
            this.charSpace = charSpace;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal charSpace, Stream stream) 
        {
            Write(charSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal charSpace, Stream stream) 
        {
            PdfOperator.Writedecimal(charSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static Tc_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new Tc_Op(a0);
        }
    }

    // Move text position
    public partial class Td_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'd' };
        public PdfOperatorType Type => PdfOperatorType.Td;
        public decimal tx { get; set; }
        public decimal ty { get; set; }
        public Td_Op(decimal tx, decimal ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal tx, decimal ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal tx, decimal ty, Stream stream) 
        {
            PdfOperator.Writedecimal(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal"  };

        public static Td_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new Td_Op(a0, a1);
        }
    }

    // Move text position and set leading
    public partial class TD_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'D' };
        public PdfOperatorType Type => PdfOperatorType.TD;
        public decimal tx { get; set; }
        public decimal ty { get; set; }
        public TD_Op(decimal tx, decimal ty)
        {
            this.tx = tx;
            this.ty = ty;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal tx, decimal ty, Stream stream) 
        {
            Write(tx, ty, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal tx, decimal ty, Stream stream) 
        {
            PdfOperator.Writedecimal(tx, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(ty, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal"  };

        public static TD_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new TD_Op(a0, a1);
        }
    }

    // Set text font and size
    public partial class Tf_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'f' };
        public PdfOperatorType Type => PdfOperatorType.Tf;
        public PdfName font { get; set; }
        public decimal size { get; set; }
        public Tf_Op(PdfName font, decimal size)
        {
            this.font = font;
            this.size = size;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(PdfName font, decimal size, Stream stream) 
        {
            Write(font, size, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(PdfName font, decimal size, Stream stream) 
        {
            PdfOperator.WritePdfName(font, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(size, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "PdfName", "decimal"  };

        public static Tf_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new Tf_Op(a0, a1);
        }
    }

    // Show text
    public partial class Tj_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'j' };
        public PdfOperatorType Type => PdfOperatorType.Tj;
        public byte[] text { get; set; }
        public Tj_Op(byte[] text)
        {
            this.text = text;
        }
    }

    // Show text, allowing individual glyph positioning
    public partial class TJ_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'J' };
        public PdfOperatorType Type => PdfOperatorType.TJ;
        public List<TJ_Item> info { get; set; }
        public TJ_Op(List<TJ_Item> info)
        {
            this.info = info;
        }
    }

    // Set text leading
    public partial class TL_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'L' };
        public PdfOperatorType Type => PdfOperatorType.TL;
        public decimal leading { get; set; }
        public TL_Op(decimal leading)
        {
            this.leading = leading;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal leading, Stream stream) 
        {
            Write(leading, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal leading, Stream stream) 
        {
            PdfOperator.Writedecimal(leading, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static TL_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new TL_Op(a0);
        }
    }

    // Set text matrix and text line matrix
    public partial class Tm_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'm' };
        public PdfOperatorType Type => PdfOperatorType.Tm;
        public decimal a { get; set; }
        public decimal b { get; set; }
        public decimal c { get; set; }
        public decimal d { get; set; }
        public decimal e { get; set; }
        public decimal f { get; set; }
        public Tm_Op(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f)
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
            PdfOperator.Writedecimal(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f, Stream stream) 
        {
            Write(a, b, c, d, e, f, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal a, decimal b, decimal c, decimal d, decimal e, decimal f, Stream stream) 
        {
            PdfOperator.Writedecimal(a, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(b, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(c, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(d, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(e, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(f, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal", "decimal", "decimal"  };

        public static Tm_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new Tm_Op(a0, a1, a2, a3, a4, a5);
        }
    }

    // Set text rendering mode
    public partial class Tr_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'r' };
        public PdfOperatorType Type => PdfOperatorType.Tr;
        public int render { get; set; }
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

        private static List<string> OpTypes = new List<string> { "int"  };

        public static Tr_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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

    // Set text rise
    public partial class Ts_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 's' };
        public PdfOperatorType Type => PdfOperatorType.Ts;
        public decimal rise { get; set; }
        public Ts_Op(decimal rise)
        {
            this.rise = rise;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal rise, Stream stream) 
        {
            Write(rise, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal rise, Stream stream) 
        {
            PdfOperator.Writedecimal(rise, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static Ts_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new Ts_Op(a0);
        }
    }

    // Set word spacing
    public partial class Tw_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.Tw;
        public decimal wordSpace { get; set; }
        public Tw_Op(decimal wordSpace)
        {
            this.wordSpace = wordSpace;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal wordSpace, Stream stream) 
        {
            Write(wordSpace, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal wordSpace, Stream stream) 
        {
            PdfOperator.Writedecimal(wordSpace, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static Tw_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new Tw_Op(a0);
        }
    }

    // Set horizontal text scaling
    public partial class Tz_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'T', (byte) 'z' };
        public PdfOperatorType Type => PdfOperatorType.Tz;
        public decimal scale { get; set; }
        public Tz_Op(decimal scale)
        {
            this.scale = scale;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal scale, Stream stream) 
        {
            Write(scale, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal scale, Stream stream) 
        {
            PdfOperator.Writedecimal(scale, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static Tz_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new Tz_Op(a0);
        }
    }

    // Append curved segment to path (initial point replicated)
    public partial class v_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'v' };
        public PdfOperatorType Type => PdfOperatorType.v;
        public decimal x2 { get; set; }
        public decimal y2 { get; set; }
        public decimal x3 { get; set; }
        public decimal y3 { get; set; }
        public v_Op(decimal x2, decimal y2, decimal x3, decimal y3)
        {
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x2, decimal y2, decimal x3, decimal y3, Stream stream) 
        {
            Write(x2, y2, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x2, decimal y2, decimal x3, decimal y3, Stream stream) 
        {
            PdfOperator.Writedecimal(x2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y2, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal"  };

        public static v_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new v_Op(a0, a1, a2, a3);
        }
    }

    // Set line width
    public partial class w_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'w' };
        public PdfOperatorType Type => PdfOperatorType.w;
        public decimal lineWidth { get; set; }
        public w_Op(decimal lineWidth)
        {
            this.lineWidth = lineWidth;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal lineWidth, Stream stream) 
        {
            Write(lineWidth, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal lineWidth, Stream stream) 
        {
            PdfOperator.Writedecimal(lineWidth, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal"  };

        public static w_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
    
            return new w_Op(a0);
        }
    }

    // Set clipping path using nonzero winding number rule
    public partial class W_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'W' };
        public PdfOperatorType Type => PdfOperatorType.W;
        public static W_Op Value = new W_Op();

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

    // Set clipping path using even-odd rule
    public partial class W_Star_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'W', (byte) '*' };
        public PdfOperatorType Type => PdfOperatorType.W_Star;
        public static W_Star_Op Value = new W_Star_Op();

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

    // Append curved segment to path (final point replicated)
    public partial class y_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) 'y' };
        public PdfOperatorType Type => PdfOperatorType.y;
        public decimal x1 { get; set; }
        public decimal y1 { get; set; }
        public decimal x3 { get; set; }
        public decimal y3 { get; set; }
        public y_Op(decimal x1, decimal y1, decimal x3, decimal y3)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x3 = x3;
            this.y3 = y3;
        }

        public void Serialize(Stream stream) 
        {
            PdfOperator.Writedecimal(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        public static void WriteLn(decimal x1, decimal y1, decimal x3, decimal y3, Stream stream) 
        {
            Write(x1, y1, x3, y3, stream);
            stream.WriteByte((byte)'\n');
        }

        public static void Write(decimal x1, decimal y1, decimal x3, decimal y3, Stream stream) 
        {
            PdfOperator.Writedecimal(x1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y1, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(x3, stream);
            stream.WriteByte((byte)' ');
            PdfOperator.Writedecimal(y3, stream);
            stream.WriteByte((byte)' ');
            stream.Write(OpData);
        }

        private static List<string> OpTypes = new List<string> { "decimal", "decimal", "decimal", "decimal"  };

        public static y_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
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
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new y_Op(a0, a1, a2, a3);
        }
    }

    // Move to next line and show text
    public partial class singlequote_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) '\'' };
        public PdfOperatorType Type => PdfOperatorType.singlequote;
        public byte[] text { get; set; }
        public singlequote_Op(byte[] text)
        {
            this.text = text;
        }
    }

    // Set word and character spacing, move to next line, and show text
    public partial class doublequote_Op : IPdfOperation
    {
        public static byte[] OpData = new byte[] { (byte) '"' };
        public PdfOperatorType Type => PdfOperatorType.doublequote;
        public decimal aw { get; set; }
        public decimal ac { get; set; }
        public byte[] text { get; set; }
        public doublequote_Op(decimal aw, decimal ac, byte[] text)
        {
            this.aw = aw;
            this.ac = ac;
            this.text = text;
        }
    }
}