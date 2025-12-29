namespace PdfLexer.Filters;

/// <summary>
/// DCTDecode filter for JPEG-compressed image data.
/// Decodes data that has been encoded using JPEG baseline format (Discrete Cosine Transform).
/// Per PDF spec section 7.4.8.
/// </summary>
public class DCTFilter : IDecoder
{
    /// <summary>
    /// Singleton instance of the DCT filter.
    /// </summary>
    public static DCTFilter Instance { get; } = new DCTFilter();

    private DCTFilter() { }

    /// <summary>
    /// Decodes JPEG-compressed data to raw uncompressed bytes.
    /// </summary>
    /// <param name="stream">The input stream containing JPEG data.</param>
    /// <param name="filterParams">Optional filter parameters (ColorTransform).</param>
    /// <returns>A stream containing the decompressed image data.</returns>
    public Stream Decode(Stream stream, PdfDictionary? filterParams)
    {
        // Read all data from input stream
        byte[] data;
        if (stream is MemoryStream ms && ms.TryGetBuffer(out var buffer))
        {
            data = buffer.ToArray();
        }
        else
        {
            using var tempMs = new MemoryStream();
            stream.CopyTo(tempMs);
            data = tempMs.ToArray();
        }

        // Get ColorTransform parameter if present
        // Per PDF spec Table 13:
        // - 0 = No transformation
        // - 1 = RGB/CMYK to YCbCr/YCbCrK transformation
        // - Default: 1 for 3-component images, 0 otherwise
        // Note: Adobe APP14 marker takes precedence if present
        int colorTransform = -1; // -1 means use default/auto-detect
        if (filterParams != null)
        {
            var ct = filterParams.GetOptionalValue<PdfNumber>(new PdfName("ColorTransform"));
            if (ct != null)
            {
                colorTransform = (int)ct;
            }
        }

        // Create decoder and parse JPEG
        var decoder = new JpegDecoder(colorTransform);
        decoder.Parse(data);

        // Get decompressed data at original dimensions
        var output = decoder.GetData(decoder.Width, decoder.Height, isSourcePDF: true);

        return new MemoryStream(output);
    }
}
