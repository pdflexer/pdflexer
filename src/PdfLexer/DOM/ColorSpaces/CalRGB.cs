using System.Numerics;

namespace PdfLexer.DOM.ColorSpaces;

// CalRGB conversion ported from PDF.JS (https://github.com/mozilla/pdf.js/blob/master/src/core/colorspace.js)
// PDF.JS is licensed as follows:
/* Copyright 2012 Mozilla Foundation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
internal class CalRGB : IColorSpace
{
    private static Matrix4x4 BRADFORD_SCALE_MATRIX = new (
        0.8951f, 0.2664f, -0.1614f, 0,
        -0.7502f, 1.7135f, 0.0367f, 0,
        0.0389f, -0.0685f, 1.0296f, 0,
        0, 0, 0, 1
    );

    private static Matrix4x4 BRADFORD_SCALE_INVERSE_MATRIX = new (
        0.9869929f, -0.1470543f, 0.1599627f, 0,
        0.4323053f, 0.5183603f, 0.0492912f, 0,
        -0.0085287f, 0.0400428f, 0.9684867f, 0,
        0, 0, 0, 1
    );

    private static Matrix4x4 SRGB_D65_XYZ_TO_RGB_MATRIX = new (
        3.2404542f, -1.5371385f, -0.4985314f, 0,
        -0.9692660f, 1.8760108f, 0.0415560f, 0,
        0.0556434f, -0.2040259f, 1.0572252f, 0,
        0, 0, 0, 1
    );

    private static Vector3 FLAT_WHITEPOINT_MATRIX = new (1f, 1f, 1f);

    private static Vector3 D65 = new (0.95047f, 1f, 1.08883f);

    private static readonly double DECODE_L_CONSTANT = Math.Pow(((8 + 16) / 116.0), 3) / 8.0;

    public int Components => 3;

    public PdfName Name => PdfName.CalGray;

    public CalRGB(Vector3 wp, Vector3 bp, Vector3 gamma, Matrix4x4 matrix)
    {
        WP = wp;
        BP = bp;
        G = gamma;
        XF = matrix;
    }

    private Vector3 WP;
    private Vector3 BP;
    private Vector3 G;
    private Matrix4x4 XF;

    private static Vector3 MProd(Matrix4x4 m, Vector3 v)
    {
        return new Vector3(
            m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z,
            m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z,
            m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z
            );
    }

    private (double, double, double) Get8BitVal(double A, double B, double C)
    {
        // A <---> AGR in the spec
        // B <---> BGG in the spec
        // C <---> CGB in the spec
        var ABC = new Vector3(
            A == 1 ? 1 : (float)Math.Pow(A, G.X),
            B == 1 ? 1 : (float)Math.Pow(B, G.Y),
            C == 1 ? 1 : (float)Math.Pow(C, G.Z)
            );

        // Computes intermediate variables L, M, N as per spec.
        // To decode X, Y, Z values map L, M, N directly to them.
        var XYZ = Vector3.Transform(ABC, XF);

        // The following calculations are based on this document:
        // http://www.adobe.com/content/dam/Adobe/en/devnet/photoshop/sdk/
        // AdobeBPC.pdf.

        var XYZ_Flat = normalizeWhitePointToFlat(XYZ);
        var XYZ_Black = compensateBlackPoint(XYZ_Flat);
        var XYZ_D65 = normalizeWhitePointToD65(XYZ_Black);

        var SRGB = MProd(SRGB_D65_XYZ_TO_RGB_MATRIX, XYZ_D65);

        // Convert the values to rgb range [0, 255].
        return (sRGBTransferFunction(SRGB.X), sRGBTransferFunction(SRGB.Y), sRGBTransferFunction(SRGB.Z));
    }

    private Vector3 normalizeWhitePointToFlat(Vector3 xyz)
    {
        if (WP.X == 1 && WP.Y == 1)
        {
            return xyz;
        }
        var LMS = MProd(BRADFORD_SCALE_MATRIX, xyz);

        var LMS_Flat = Vector3.Divide(LMS, WP);

        return MProd(BRADFORD_SCALE_INVERSE_MATRIX, LMS_Flat);
    }

    private double decodeL(double L)
    {
        if (L < 0)
        {
            return -decodeL(-L);
        }
        if (L > 8.0)
        {
            return Math.Pow(((L + 16) / 116), 3);
        }
        return L * DECODE_L_CONSTANT;
    }

    private Vector3 compensateBlackPoint(Vector3 xyz)
    {
        // In case the blackPoint is already the default blackPoint then there is
        // no need to do compensation.
        if (
          BP.X == 0 &&
          BP.Y == 0 &&
          BP.Z == 0
        )
        {
            return xyz;
        }

        // For the blackPoint calculation details, please see
        // http://www.adobe.com/content/dam/Adobe/en/devnet/photoshop/sdk/
        // AdobeBPC.pdf.
        // The destination blackPoint is the default blackPoint [0, 0, 0].
        var zeroDecodeL = decodeL(0);

        var X_DST = zeroDecodeL;
        var X_SRC = decodeL(BP.X);

        var Y_DST = zeroDecodeL;
        var Y_SRC = decodeL(BP.Y);

        var Z_DST = zeroDecodeL;
        var Z_SRC = decodeL(BP.Z);

        var X_Scale = (1 - X_DST) / (1 - X_SRC);
        var X_Offset = 1 - X_Scale;

        var Y_Scale = (1 - Y_DST) / (1 - Y_SRC);
        var Y_Offset = 1 - Y_Scale;

        var Z_Scale = (1 - Z_DST) / (1 - Z_SRC);
        var Z_Offset = 1 - Z_Scale;


        return new Vector3(
            (float)(xyz.X * X_Scale + X_Offset),
            (float)(xyz.Y * Y_Scale + Y_Offset),
            (float)(xyz.Z * Z_Scale + Z_Offset)
            );

    }

    private Vector3 normalizeWhitePointToD65(Vector3 xyz)
    {
        var LMS = MProd(BRADFORD_SCALE_MATRIX, xyz);

        var LMS_65 = Vector3.Divide(Vector3.Multiply(LMS, D65), FLAT_WHITEPOINT_MATRIX);
        // bp
        return MProd(BRADFORD_SCALE_INVERSE_MATRIX, LMS_65);
    }

    // revisit this for 16 bit
    private double sRGBTransferFunction(double color)
    {
        // See http://en.wikipedia.org/wiki/SRGB.
        if (color <= 0.0031308)
        {
            return adjustToRange(0, 1, 12.92 * color);
        }
        // Optimization:
        // If color is close enough to 1, skip calling the following transform
        // since calling Math.pow is expensive. If color is larger than
        // the threshold, the final result is larger than 254.5 since
        // ((1 + 0.055) * 0.99554525 ** (1 / 2.4) - 0.055) * 255 ===
        // 254.50000003134699
        if (color >= 0.99554525)
        {
            return 1;
        }
        return adjustToRange(0, 1, (1 + 0.055) * Math.Pow(color, (1 / 2.4)) - 0.055);
    }

    private double adjustToRange(double min, double max, double value)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        for (var p = 0; p < row.Length / 3; p++)
        {
            var os = p * 3;
            var oos = p * 4;
            var (r, g, b) = Get8BitVal(row[os] / 65535.0, row[os + 1] / 65535.0, row[os + 2] / 65535.0);
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
            var (r, g, b) = Get8BitVal(row[os] / 255.0, row[os + 1] / 255.0, row[os + 2] / 255.0);
            output[oos] = (byte)(r * 255.0);
            output[oos + 1] = (byte)(g * 255.0);
            output[oos + 2] = (byte)(b * 255.0);
            output[oos + 3] = 255;
        }
    }

    public static CalRGB FromObject(PdfDictionary dict)
    {
        var wp = dict.Get<PdfArray>("WhitePoint");
        if (wp == null)
        {
            throw new PdfLexerException("No whitepoint entry for CalRGB colorspace");
        }
        var wps = wp.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
        if (wps.Count != 3)
        {
            throw new PdfLexerException($"Invalid WhitePoint entry for CalRGB:" + wp.ToString());
        }
        var (xw, yw, zw) = (wps[0], wps[1], wps[2]);
        if (xw < 0 || zw < 0 || yw != 1)
        {
            throw new PdfLexerException($"Invalid WhitePoint entry for CalRGB:" + wp.ToString());
        }

        var (xb, yb, zb) = (0f, 0f, 0f);
        var bp = dict.Get<PdfArray>("BlackPoint");
        if (bp != null)
        {
            var bps = bp.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
            if (wps.Count == 3)
            {
                (xb, yb, zb) = (bps[0], bps[1], bps[2]);
            }
            if (xb < 0 || yb < 0 || zb < 0)
            {
                (xb, yb, zb) = (0f, 0f, 0f);
            }
        }

        if (xb < 0 || yb < 0 || zb < 0)
        {
            throw new PdfLexerException($"Invalid BlackPoint entry for CalRGB: {xb} {yb} {yb}");
        }

        var (gr, gg, gb) = (1f, 1f, 1f);
        var ga = dict.Get<PdfArray>("Gamma");
        if (ga != null)
        {
            var gav = ga.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
            if (gav.Count == 3)
            {
                (gr, gg, gb) = (gav[0], gav[1], gav[2]);
            }
            if (gr < 0 || gg < 0 || gb < 0)
            {
                (gr, gg, gb) = (1f, 1f, 1f);
            }
        }

        var mxa = 1f;
        var mya = 0f;
        var mza = 0f;
        var mxb = 0f;
        var myb = 1f;
        var mzb = 0f;
        var mxc = 0f;
        var myc = 0f;
        var mzc = 1f;
        var m = dict.Get<PdfArray>("Matrix");
        if (m != null)
        {
            var mv = m.Select(x => x.GetValueOrNull<PdfNumber>()).Where(x => x != null).Select(x => (float)x!).ToList();
            if (mv.Count == 9)
            {
                mxa = mv[0];
                mya = mv[1];
                mza = mv[2];
                mxb = mv[3];
                myb = mv[4];
                mzb = mv[5];
                mxc = mv[6];
                myc = mv[7];
                mzc = mv[8];
            }
        }

        return new CalRGB(
            new Vector3(xw, yw, zw),
            new Vector3(xb, yb, zb),
            new Vector3(gr, gg, gb),
            new Matrix4x4(
                mxa, mya, mza, 0f,
                mxb, myb, mzb, 0f,
                mxc, myc, mzc, 0f,
                0f, 0f, 0f, 1f
                )
            );
    }
}
