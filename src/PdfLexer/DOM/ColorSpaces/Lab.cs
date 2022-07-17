using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PdfLexer.DOM.ColorSpaces
{
    internal class Lab : IColorSpace
    {
        public int Components => 3;

        public PdfName Name => PdfName.Lab;

        double amin; double amax; double bmin; double bmax;
        Vector3 WP;
        Vector3 BP;
        public Lab(Vector3 wp, Vector3 bp, double amin, double amax, double bmin, double bmax)
        {
            WP = wp;
            BP = bp;
            this.amin = amin;
            this.amax = amax;
            this.bmin = bmin;
            this.bmax = bmax;
        }

        private (double r, double b, double g) Convert(double Ls, double As, double bs, double? maxValue)
        {
            if (maxValue != null)
            {
                Ls = decode(Ls, maxValue.Value, 0, 100);
                As = decode(As, maxValue.Value, amin, amax);
                bs = decode(bs, maxValue.Value, bmin, bmax);
            }

            if (As > amax)
            {
                As = amax;
            }
            else if (As < amin)
            {
                As = amin;
            }
            if (bs > bmax)
            {
                bs = bmax;
            }
            else if (bs < bmin)
            {
                bs = bmin;
            }

            // Computes intermediate variables X,Y,Z as per spec
            var M = (Ls + 16) / 116;
            var L = M + As / 500;
            var N = M - bs / 200;

            var X = WP.X * fn_g(L);
            var Y = WP.Y * fn_g(M);
            var Z = WP.Z * fn_g(N);

            double r, g, b;
            // Using different conversions for D50 and D65 white points,
            // per http://www.color.org/srgb.pdf
            if (WP.Z < 1)
            {
                // Assuming D50 (X=0.9642, Y=1.00, Z=0.8249)
                r = X * 3.1339 + Y * -1.617 + Z * -0.4906;
                g = X * -0.9785 + Y * 1.916 + Z * 0.0333;
                b = X * 0.072 + Y * -0.229 + Z * 1.4057;
            }
            else
            {
                // Assuming D65 (X=0.9505, Y=1.00, Z=1.0888)
                r = X * 3.2406 + Y * -1.5372 + Z * -0.4986;
                g = X * -0.9689 + Y * 1.8758 + Z * 0.0415;
                b = X * 0.0557 + Y * -0.204 + Z * 1.057;
            }

            return (Math.Sqrt(r), Math.Sqrt(g), Math.Sqrt(b));
        }

        private double fn_g(double x)
        {
            double result;
            if (x >= 6 / 29.0)
            {
                result = Math.Pow(x, 3);
            }
            else
            {
                result = (108 / 841.0) * (x - 4 / 29.0);
            }
            return result;
        }

        private double decode(double value, double h1, double l2, double h2)
        {
            return l2 + (value * (h2 - l2)) / h1;
        }

        public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
        {
            for (var p = 0; p < row.Length / 3; p++)
            {
                var os = p * 3;
                var oos = p * 4;
                var (r, g, b) = Convert(row[os], row[os + 1], row[os + 2], 65535);
                output[oos] = (ushort)(r * 65535.0);
                output[oos + 1] = (ushort)(g * 65535.0);
                output[oos + 2] = (ushort)(b * 65535.0);
                output[oos + 3] = ushort.MaxValue;
            }
        }

        public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
        {
            for (var p = 0; p < row.Length / 3; p++)
            {
                var os = p * 3;
                var oos = p * 4;
                var (r, g, b) = Convert(row[os], row[os + 1], row[os + 2], 255);
                output[oos] = (byte)(r * 255.0);
                output[oos + 1] = (byte)(g * 255.0);
                output[oos + 2] = (byte)(b * 255.0);
                output[oos + 3] = 255;
            }
        }

        public static Lab FromObject(PdfDictionary dict)
        {
            var wp = dict.Get<PdfArray>("/WhitePoint");
            if (wp == null)
            {
                throw new PdfLexerException("No whitepoint entry for Lab colorspace");
            }
            var wps = wp.Select(x => x.GetValue<PdfNumber>(false)).Where(x => x != null).Select(x => (float)x).ToList();
            if (wps.Count != 3)
            {
                throw new PdfLexerException($"Invalid WhitePoint entry for Lab:" + wp.ToString());
            }
            var (xw, yw, zw) = (wps[0], wps[1], wps[2]);
            if (xw < 0 || zw < 0 || yw != 1)
            {
                throw new PdfLexerException($"Invalid WhitePoint entry for Lab:" + wp.ToString());
            }

            var (xb, yb, zb) = (0f, 0f, 0f);
            var bp = dict.Get<PdfArray>("/BlackPoint");
            if (bp != null)
            {
                var bps = bp.Select(x => x.GetValue<PdfNumber>(false)).Where(x => x != null).Select(x => (float)x).ToList();
                if (wps.Count == 3)
                {
                    (xb, yb, zb) = (bps[0], bps[1], bps[2]);
                }
                if (xb < 0 || yb < 0 || zb < 0)
                {
                    (xb, yb, zb) = (0f, 0f, 0f);
                }
            }

            double amin = -100, amax = 100, bmin = -100, bmax = 100;
            var range = dict.Get<PdfArray>("/Range");
            if (range != null)
            {
                var rng = range.Select(x => x.GetValue<PdfNumber>(false)).Where(x => x != null).Select(x => (float)x).ToList();
                if (rng.Count == 4 && rng[0] < rng[1] && rng[2] < rng[3])
                {
                    amin = rng[0];
                    amax = rng[1];
                    bmin = rng[2];
                    bmax = rng[3];
                }
            }

            return new Lab(
                new Vector3(xw, yw, zw),
                new Vector3(xb, yb, zb),
                amin, amax, bmin, bmax
                );
        }
    }
}
