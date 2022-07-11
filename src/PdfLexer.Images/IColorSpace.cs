using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLexer.Images;

public interface IColorSpace
{
    /// <summary>
    /// Number of color components per pixel.
    /// </summary>
    int Components { get; }
    /// <summary>
    /// Returns 16 bit RGB values from color components (as ushorts) at the specified offset.
    /// </summary>
    /// <param name="components"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset);
    /// <summary>
    /// Returns 8 bit RGB values from color components (as bytes) at the specified offset.
    /// </summary>
    /// <param name="components"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset);
}


public class DeviceRGB : IColorSpace
{
    public static DeviceRGB Instance = new DeviceRGB();
    public int Components => 3;

    public (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset)
    {
        return (components[offset], components[offset + 1], components[offset + 2]);

    }
    public (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset)
    {
        return ((byte)(components[offset] >> 8), (byte)(components[offset + 1] >> 8), (byte)(components[offset + 2] >> 8));
    }
}

// https://github.com/mozilla/pdf.js/blob/master/src/core/colorspace.js
// TODO license
public class DeviceCMYK : IColorSpace
{
    public static DeviceCMYK Instance = new DeviceCMYK();
    public int Components => 4;

    private (double r, double g, double b) GetDoubles(ushort[] components, int offset)
    {
        ushort c = components[offset];
        ushort m = components[offset + 1];
        ushort y = components[offset + 2];
        ushort k = components[offset + 3];

        var r = 65535 +
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
      65535 +
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

        var b = 65535 +
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

        return (r, g, b);
    }

    public (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset)
    {
        var (dr, dg, db) = GetDoubles(components, offset);
        return ((ushort)dr, (ushort)dg, (ushort)db);

    }
    public (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset)
    {
        var (dr, dg, db) = GetDoubles(components, offset);
        return ((byte)((ushort)dr >> 8), (byte)((ushort)dg >> 8), (byte)((ushort)db >> 8));
    }
}

public class DeviceGray : IColorSpace
{
    public static DeviceGray Instance = new DeviceGray();
    public int Components => 1;

    public (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset)
    {
        var c = components[offset];
        return (c, c, c);

    }
    public (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset)
    {
        var c = (byte)(components[offset] >> 8);
        return (c, c, c);
    }
}

public class Indexed : IColorSpace
{
    private IColorSpace _baseCs;
    private byte[] _lookup;
    private ushort[] buffer = new ushort[4];
    public int Components => 1;

    public Indexed(IColorSpace baseCs, byte[] lookup)
    {
        _baseCs = baseCs;
        _lookup = lookup;
    }

    private void FillBuffer(ushort[] components, int offset)
    {
        var c = _baseCs.Components;
        var luv = (byte)(components[offset] >> 8);
        for (var i = 0; i < c; i++)
        {
            buffer[i] = (ushort)(_lookup[luv + i] << 8);
        }
    }

    public (ushort r, ushort g, ushort b) GetRGB16(ushort[] components, int offset)
    {
        FillBuffer(components, offset);
        return _baseCs.GetRGB16(buffer, 0);

    }
    public (byte r, byte g, byte b) GetRGB8(ushort[] components, int offset)
    {
        FillBuffer(components, offset);
        return _baseCs.GetRGB8(buffer, 0);
    }

    public static Indexed FromArray(ParsingContext ctx, PdfArray arr)
    {
        if (arr.Count < 4) { throw new PdfLexerException($"Indexed colorspace had less than 4 entries."); }
        var baseCs = ImageSharpExts.GetColorspace(ctx, arr[1]);
        var hival = arr[2].GetValue<PdfNumber>();
        var data = arr[3].Resolve();
        byte[] lookup = null;
        if (data.Type == PdfObjectType.StringObj)
        {
            var str = (PdfString)data;
            lookup = str.GetRawBytes();
        }
        else if (data.Type == PdfObjectType.StreamObj)
        {
            lookup = ((PdfStream)data).Contents.GetDecodedData(ctx);
        }
        else
        {
            throw new ApplicationException("Index colorspace had unknown lookup table: " + data.GetPdfObjType());
        }

        return new Indexed(baseCs, lookup);
    }
}