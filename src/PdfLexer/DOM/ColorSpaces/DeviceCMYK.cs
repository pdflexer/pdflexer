
namespace PdfLexer.DOM.ColorSpaces;

internal class DeviceCMYK : IColorSpace
{
    public static readonly DeviceCMYK Instance = new ();
    public int Components => 4;
    public PdfName Name => PdfName.DeviceCMYK;

    public bool IsPredefined => true;

    // CMYK conversion ported from PDF.JS (https://github.com/mozilla/pdf.js/blob/master/src/core/colorspace.js)
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
    private (double r, double g, double b) GetRgbDoubles(double c, double m, double y, double k)
    {
        var r = 255 +
      c *
        (-4.387332384609988 * c +
          54.48615194189176 * m +
          18.82290502165302 * y +
          212.25662451639585 * k +
          -285.2331026137004) +
      m *
        (1.7149763477362134 * m -
          5.6096736904047315 * y +
          -17.873870861415444 * k -
          5.497006427196366) +
      y *
        (-2.5217340131683033 * y - 21.248923337353073 * k + 17.5119270841813) +
      k * (-21.86122147463605 * k - 189.48180835922747);

        var g =
      255 +
      c *
        (8.841041422036149 * c +
          60.118027045597366 * m +
          6.871425592049007 * y +
          31.159100130055922 * k +
          -79.2970844816548) +
      m *
        (-15.310361306967817 * m +
          17.575251261109482 * y +
          131.35250912493976 * k -
          190.9453302588951) +
      y * (4.444339102852739 * y + 9.8632861493405 * k - 24.86741582555878) +
      k * (-20.737325471181034 * k - 187.80453709719578);

        var b = 255 +
      c *
        (0.8842522430003296 * c +
          8.078677503112928 * m +
          30.89978309703729 * y -
          0.23883238689178934 * k +
          -14.183576799673286) +
      m *
        (10.49593273432072 * m +
          63.02378494754052 * y +
          50.606957656360734 * k -
          112.23884253719248) +
      y *
        (0.03296041114873217 * y +
          115.60384449646641 * k +
          -193.58209356861505) +
      k * (-22.33816807309886 * k - 180.12613974708367);
        r = Math.Max(0, Math.Min(r, 255));
        g = Math.Max(0, Math.Min(g, 255));
        b = Math.Max(0, Math.Min(b, 255));

        return (r, g, b);
    }

    public void CopyRowToRBGA8Span(ReadOnlySpan<byte> row, Span<byte> output)
    {
        for (var p = 0; p < row.Length / 4; p++)
        {
            var os = p * 4;
            var (r, g, b) = GetRgbDoubles(row[os] / 255.0, row[os + 1] / 255.0, row[os + 2] / 255.0, row[os + 3] / 255.0);
            output[os] = (byte)r;
            output[os + 1] = (byte)g;
            output[os + 2] = (byte)b;
            output[os + 3] = 255;
        }
    }

    public void CopyRowToRBGA16Span(ReadOnlySpan<ushort> row, Span<ushort> output)
    {
        for (var p = 0; p < row.Length / 4; p++)
        {
            var os = p * 4;
            var (r, g, b) = GetRgbDoubles(row[os] / 65535.0, row[os + 1] / 65535.0, row[os + 2] / 65535.0, row[os + 3] / 65535.0);

            output[os] = (ushort)(r * 257);
            output[os + 1] = (ushort)(g * 257);
            output[os + 2] = (ushort)(b * 257);
            output[os + 3] = ushort.MaxValue;
        }
    }
}

