﻿using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;

/// <summary>
/// Auto-generated, do not modify.
/// </summary>

namespace PdfLexer.Operators
{
    public enum PdfOperatorType
    {
        Unknown,
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
        I = 73,
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
        rl = 27762,
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
            [100] =  d_Op.Parse, 
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
            [73] =  I_Op.Parse, 
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
            [27762] =  rl_Op.Parse, 
            [115] =  s_Op.Parse, 
            [83] =  S_Op.Parse, 
            [17235] =  PdfOperator.ParseSC, 
            [25459] =  sc_Op.Parse, 
            [5129043] =  SCN_Op.Parse, 
            [7234419] =  scn_Op.Parse, 
            [26739] =  sh_Op.Parse, 
            [10836] =  T_Star_Op.Parse, 
            [25428] =  Tc_Op.Parse, 
            [25684] =  Td_Op.Parse, 
            [17492] =  TD_Op.Parse, 
            [26196] =  Tf_Op.Parse, 
            [27220] =  Tj_Op.Parse, 
            [19028] =  TJ_Op.Parse, 
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
            [39] =  singlequote_Op.Parse, 
            [34] =  doublequote_Op.Parse, 
        };
    }

    public partial class b_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.b;
        public static b_Op Value = new b_Op();
        public b_Op()
        {

        }
        public static b_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class B_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.B;
        public static B_Op Value = new B_Op();
        public B_Op()
        {

        }
        public static B_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class b_Star_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.b_Star;
        public static b_Star_Op Value = new b_Star_Op();
        public b_Star_Op()
        {

        }
        public static b_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class B_Star_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.B_Star;
        public static B_Star_Op Value = new B_Star_Op();
        public B_Star_Op()
        {

        }
        public static B_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class BDC_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.BDC;
        public PdfName tag { get; set; }
        public PdfObject props { get; set; }
        public BDC_Op(PdfName tag, PdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }
    public partial class BI_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.BI;
        public static BI_Op Value = new BI_Op();
        public BI_Op()
        {

        }
        public static BI_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class BMC_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.BMC;
        public PdfName tag { get; set; }
        public BMC_Op(PdfName tag)
        {
            this.tag = tag;
        }
        public static BMC_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new BMC_Op(a0);
        }
    }
    public partial class BT_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.BT;
        public static BT_Op Value = new BT_Op();
        public BT_Op()
        {

        }
        public static BT_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class BX_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.BX;
        public static BX_Op Value = new BX_Op();
        public BX_Op()
        {

        }
        public static BX_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class c_Op : IPdfOperation
    {
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
        public static c_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new c_Op(a0, a1, a2, a3, a4, a5);
        }
    }
    public partial class cm_Op : IPdfOperation
    {
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
        public static cm_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new cm_Op(a0, a1, a2, a3, a4, a5);
        }
    }
    public partial class CS_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.CS;
        public PdfName name { get; set; }
        public CS_Op(PdfName name)
        {
            this.name = name;
        }
        public static CS_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new CS_Op(a0);
        }
    }
    public partial class cs_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.cs;
        public PdfName name { get; set; }
        public cs_Op(PdfName name)
        {
            this.name = name;
        }
        public static cs_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new cs_Op(a0);
        }
    }
    public partial class d_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.d;
        public static d_Op Value = new d_Op();
        public d_Op()
        {

        }
        public static d_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class d0_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.d0;
        public static d0_Op Value = new d0_Op();
        public d0_Op()
        {

        }
        public static d0_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class d1_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.d1;
        public static d1_Op Value = new d1_Op();
        public d1_Op()
        {

        }
        public static d1_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Do_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Do;
        public static Do_Op Value = new Do_Op();
        public Do_Op()
        {

        }
        public static Do_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class DP_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.DP;
        public PdfName tag { get; set; }
        public PdfObject props { get; set; }
        public DP_Op(PdfName tag, PdfObject props)
        {
            this.tag = tag;
            this.props = props;
        }
    }
    public partial class EI_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.EI;
        public static EI_Op Value = new EI_Op();
        public EI_Op()
        {

        }
        public static EI_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class EMC_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.EMC;
        public static EMC_Op Value = new EMC_Op();
        public EMC_Op()
        {

        }
        public static EMC_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class ET_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.ET;
        public static ET_Op Value = new ET_Op();
        public ET_Op()
        {

        }
        public static ET_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class EX_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.EX;
        public static EX_Op Value = new EX_Op();
        public EX_Op()
        {

        }
        public static EX_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class f_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.f;
        public static f_Op Value = new f_Op();
        public f_Op()
        {

        }
        public static f_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class F_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.F;
        public static F_Op Value = new F_Op();
        public F_Op()
        {

        }
        public static F_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class f_Star_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.f_Star;
        public static f_Star_Op Value = new f_Star_Op();
        public f_Star_Op()
        {

        }
        public static f_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class G_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.G;
        public static G_Op Value = new G_Op();
        public G_Op()
        {

        }
        public static G_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class g_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.g;
        public static g_Op Value = new g_Op();
        public g_Op()
        {

        }
        public static g_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class gs_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.gs;
        public static gs_Op Value = new gs_Op();
        public gs_Op()
        {

        }
        public static gs_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class h_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.h;
        public static h_Op Value = new h_Op();
        public h_Op()
        {

        }
        public static h_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class I_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.I;
        public static I_Op Value = new I_Op();
        public I_Op()
        {

        }
        public static I_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class ID_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.ID;
        public static ID_Op Value = new ID_Op();
        public ID_Op()
        {

        }
        public static ID_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class j_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.j;
        public static j_Op Value = new j_Op();
        public j_Op()
        {

        }
        public static j_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class J_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.J;
        public static J_Op Value = new J_Op();
        public J_Op()
        {

        }
        public static J_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class K_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.K;
        public static K_Op Value = new K_Op();
        public K_Op()
        {

        }
        public static K_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class k_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.k;
        public static k_Op Value = new k_Op();
        public k_Op()
        {

        }
        public static k_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class l_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.l;
        public decimal x { get; set; }
        public decimal y { get; set; }
        public l_Op(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }
        public static l_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new l_Op(a0, a1);
        }
    }
    public partial class m_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.m;
        public decimal x { get; set; }
        public decimal y { get; set; }
        public m_Op(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }
        public static m_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new m_Op(a0, a1);
        }
    }
    public partial class M_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.M;
        public static M_Op Value = new M_Op();
        public M_Op()
        {

        }
        public static M_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class MP_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.MP;
        public PdfName tag { get; set; }
        public MP_Op(PdfName tag)
        {
            this.tag = tag;
        }
        public static MP_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfName(ctx, data, operands[0]);
    
            return new MP_Op(a0);
        }
    }
    public partial class n_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.n;
        public static n_Op Value = new n_Op();
        public n_Op()
        {

        }
        public static n_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class q_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.q;
        public static q_Op Value = new q_Op();
        public q_Op()
        {

        }
        public static q_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Q_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Q;
        public static Q_Op Value = new Q_Op();
        public Q_Op()
        {

        }
        public static Q_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class re_Op : IPdfOperation
    {
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
        public static re_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new re_Op(a0, a1, a2, a3);
        }
    }
    public partial class RG_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.RG;
        public static RG_Op Value = new RG_Op();
        public RG_Op()
        {

        }
        public static RG_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class rg_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.rg;
        public static rg_Op Value = new rg_Op();
        public rg_Op()
        {

        }
        public static rg_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class rl_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.rl;
        public static rl_Op Value = new rl_Op();
        public rl_Op()
        {

        }
        public static rl_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class s_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.s;
        public static s_Op Value = new s_Op();
        public s_Op()
        {

        }
        public static s_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class S_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.S;
        public static S_Op Value = new S_Op();
        public S_Op()
        {

        }
        public static S_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class SC_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.SC;
        public List<decimal> colorInfo { get; set; }
        public SC_Op(List<decimal> colorInfo)
        {
            this.colorInfo = colorInfo;
        }
    }
    public partial class sc_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.sc;
        public static sc_Op Value = new sc_Op();
        public sc_Op()
        {

        }
        public static sc_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class SCN_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.SCN;
        public static SCN_Op Value = new SCN_Op();
        public SCN_Op()
        {

        }
        public static SCN_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class scn_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.scn;
        public static scn_Op Value = new scn_Op();
        public scn_Op()
        {

        }
        public static scn_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class sh_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.sh;
        public static sh_Op Value = new sh_Op();
        public sh_Op()
        {

        }
        public static sh_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class T_Star_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.T_Star;
        public static T_Star_Op Value = new T_Star_Op();
        public T_Star_Op()
        {

        }
        public static T_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Tc_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tc;
        public static Tc_Op Value = new Tc_Op();
        public Tc_Op()
        {

        }
        public static Tc_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Td_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Td;
        public decimal tx { get; set; }
        public decimal ty { get; set; }
        public Td_Op(decimal tx, decimal ty)
        {
            this.tx = tx;
            this.ty = ty;
        }
        public static Td_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new Td_Op(a0, a1);
        }
    }
    public partial class TD_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.TD;
        public decimal tx { get; set; }
        public decimal ty { get; set; }
        public TD_Op(decimal tx, decimal ty)
        {
            this.tx = tx;
            this.ty = ty;
        }
        public static TD_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
    
            return new TD_Op(a0, a1);
        }
    }
    public partial class Tf_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tf;
        public static Tf_Op Value = new Tf_Op();
        public Tf_Op()
        {

        }
        public static Tf_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Tj_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tj;
        public PdfString text { get; set; }
        public Tj_Op(PdfString text)
        {
            this.text = text;
        }
        public static Tj_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfString(ctx, data, operands[0]);
    
            return new Tj_Op(a0);
        }
    }
    public partial class TJ_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.TJ;
        public static TJ_Op Value = new TJ_Op();
        public TJ_Op()
        {

        }
        public static TJ_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class TL_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.TL;
        public static TL_Op Value = new TL_Op();
        public TL_Op()
        {

        }
        public static TL_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Tm_Op : IPdfOperation
    {
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
        public static Tm_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
            var a4 = PdfOperator.ParseDecimal(ctx, data, operands[4]);
            var a5 = PdfOperator.ParseDecimal(ctx, data, operands[5]);
    
            return new Tm_Op(a0, a1, a2, a3, a4, a5);
        }
    }
    public partial class Tr_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tr;
        public static Tr_Op Value = new Tr_Op();
        public Tr_Op()
        {

        }
        public static Tr_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Ts_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Ts;
        public static Ts_Op Value = new Ts_Op();
        public Ts_Op()
        {

        }
        public static Ts_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Tw_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tw;
        public static Tw_Op Value = new Tw_Op();
        public Tw_Op()
        {

        }
        public static Tw_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class Tz_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.Tz;
        public static Tz_Op Value = new Tz_Op();
        public Tz_Op()
        {

        }
        public static Tz_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class v_Op : IPdfOperation
    {
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
        public static v_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new v_Op(a0, a1, a2, a3);
        }
    }
    public partial class w_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.w;
        public static w_Op Value = new w_Op();
        public w_Op()
        {

        }
        public static w_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class W_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.W;
        public static W_Op Value = new W_Op();
        public W_Op()
        {

        }
        public static W_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class W_Star_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.W_Star;
        public static W_Star_Op Value = new W_Star_Op();
        public W_Star_Op()
        {

        }
        public static W_Star_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
    public partial class y_Op : IPdfOperation
    {
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
        public static y_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParseDecimal(ctx, data, operands[0]);
            var a1 = PdfOperator.ParseDecimal(ctx, data, operands[1]);
            var a2 = PdfOperator.ParseDecimal(ctx, data, operands[2]);
            var a3 = PdfOperator.ParseDecimal(ctx, data, operands[3]);
    
            return new y_Op(a0, a1, a2, a3);
        }
    }
    public partial class singlequote_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.singlequote;
        public PdfString text { get; set; }
        public singlequote_Op(PdfString text)
        {
            this.text = text;
        }
        public static singlequote_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
        {
            var a0 = PdfOperator.ParsePdfString(ctx, data, operands[0]);
    
            return new singlequote_Op(a0);
        }
    }
    public partial class doublequote_Op : IPdfOperation
    {
        public PdfOperatorType Type => PdfOperatorType.doublequote;
        public static doublequote_Op Value = new doublequote_Op();
        public doublequote_Op()
        {

        }
        public static doublequote_Op Parse(ParsingContext ctx, ReadOnlySpan<byte> data, List<OperandInfo> operands) 
            => Value;
    }
}