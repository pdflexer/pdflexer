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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Tests
{
    public class Util
    {
        public static decimal GetDocumentHashCode(string path)
        {
            return GetDocumentHashCode(File.ReadAllBytes(path));
        }
        public static decimal GetDocumentHashCode(byte[] data)
        {
            decimal hc = 0;
            using var doc = UglyToad.PdfPig.PdfDocument.Open(data);
            foreach (var page in doc.GetPages())
            {
                hc = unchecked(hc + GetPageHashCode(page));
            }
            return hc;
        }

        public static int CountResources(byte[] pdfDoc)
        {
            var found = new HashSet<ulong>();
            using var doc = PdfDocument.Open(pdfDoc);
            foreach (var page in doc.Pages)
            {
                if (page.NativeObject.TryGetValue<PdfDictionary>(PdfName.Resources, out var res))
                {
                    foreach (var kvp in res.Where(x=>x.Key == PdfName.Font || x.Key == PdfName.XObject))
                    {
                        var inner = kvp.Value.GetValue<PdfDictionary>();
                        foreach (var kvpRes in inner)
                        {
                            var ir = kvpRes.Value as PdfIndirectRef;
                            found.Add(ir.Reference.GetId());
                        }
                    }
                }
            }
            return found.Count;
        }

            public static decimal GetPageHashCode(UglyToad.PdfPig.Content.Page page)
        {
            decimal hc = 0;
            foreach (var letter in page.Letters)
            {
                hc = unchecked(hc + (decimal)letter.Location.X);
                hc = unchecked(hc + (decimal)letter.Location.Y);
                var (r, b, g) = letter.Color.ToRGBValues();
                hc = unchecked(hc + r);
                hc = unchecked(hc + b);
                hc = unchecked(hc + g);
                hc = unchecked(hc + (decimal)letter.Width);
                hc = unchecked(hc + (decimal)letter.PointSize);
                hc = unchecked(hc + (decimal)letter.Font.Name.GetHashCode());
            }
            foreach (var img in page.GetImages())
            {
                hc = unchecked(hc + (decimal)img.Bounds.BottomLeft.X);
                hc = unchecked(hc + (decimal)img.Bounds.BottomLeft.Y);
                hc = unchecked(hc + (decimal)img.Bounds.TopRight.X);
                hc = unchecked(hc + (decimal)img.Bounds.TopRight.Y);
            }

            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.BottomLeft.X);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.BottomLeft.Y);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.TopRight.X);
            hc = unchecked(hc + (decimal)(int)page.MediaBox.Bounds.TopRight.Y);
            // pdf pig has weird handling for media box, will convert to int if on page but not if inherited
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.BottomLeft.X);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.BottomLeft.Y);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.TopRight.X);
            // hc = unchecked(hc + (decimal)page.MediaBox.Bounds.TopRight.Y);
            return hc;
        }
    }
}
