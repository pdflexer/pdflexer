// Port of pdf.js JPEG decoder (jpg.js) - Licensed under Apache License 2.0
// Original: https://github.com/nickmasteryet/jpgjs
// See: ITU CCITT Recommendation T.81, JFIF specification, Adobe Technical Note #5116

namespace PdfLexer.Filters;

/// <summary>
/// Exception thrown when JPEG decoding fails.
/// </summary>
public class JpegException : Exception
{
    public JpegException(string message) : base(message) { }
}

/// <summary>
/// JPEG decoder that decompresses DCT-encoded image data.
/// Ported from pdf.js implementation.
/// </summary>
internal class JpegDecoder
{
    // DCT zigzag ordering table
    private static readonly byte[] DctZigZag = new byte[]
    {
        0,
        1, 8,
        16, 9, 2,
        3, 10, 17, 24,
        32, 25, 18, 11, 4,
        5, 12, 19, 26, 33, 40,
        48, 41, 34, 27, 20, 13, 6,
        7, 14, 21, 28, 35, 42, 49, 56,
        57, 50, 43, 36, 29, 22, 15,
        23, 30, 37, 44, 51, 58,
        59, 52, 45, 38, 31,
        39, 46, 53, 60,
        61, 54, 47,
        55, 62,
        63
    };

    // DCT constants (scaled for fixed-point arithmetic)
    private const int DctCos1 = 4017;    // cos(pi/16)
    private const int DctSin1 = 799;     // sin(pi/16)
    private const int DctCos3 = 3406;    // cos(3*pi/16)
    private const int DctSin3 = 2276;    // sin(3*pi/16)
    private const int DctCos6 = 1567;    // cos(6*pi/16)
    private const int DctSin6 = 3784;    // sin(6*pi/16)
    private const int DctSqrt2 = 5793;   // sqrt(2)
    private const int DctSqrt1d2 = 2896; // sqrt(2) / 2

    // JPEG markers
    private const int MarkerSOI = 0xFFD8;  // Start of Image
    private const int MarkerEOI = 0xFFD9;  // End of Image
    private const int MarkerSOF0 = 0xFFC0; // Baseline DCT
    private const int MarkerSOF1 = 0xFFC1; // Extended DCT
    private const int MarkerSOF2 = 0xFFC2; // Progressive DCT
    private const int MarkerDHT = 0xFFC4;  // Define Huffman Tables
    private const int MarkerDQT = 0xFFDB;  // Define Quantization Tables
    private const int MarkerDRI = 0xFFDD;  // Define Restart Interval
    private const int MarkerSOS = 0xFFDA;  // Start of Scan
    private const int MarkerDNL = 0xFFDC;  // Define Number of Lines
    private const int MarkerAPP0 = 0xFFE0; // JFIF
    private const int MarkerAPP14 = 0xFFEE; // Adobe

    private readonly int _colorTransform;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int NumComponents { get; private set; }

    private JfifInfo? _jfif;
    private AdobeInfo? _adobe;
    private List<ComponentInfo>? _components;

    public JpegDecoder(int colorTransform = -1)
    {
        _colorTransform = colorTransform;
    }

    #region Data Structures

    private class JfifInfo
    {
        public int VersionMajor;
        public int VersionMinor;
        public int DensityUnits;
        public int XDensity;
        public int YDensity;
    }

    private class AdobeInfo
    {
        public int Version;
        public int Flags0;
        public int Flags1;
        public int TransformCode;
    }

    private class FrameInfo
    {
        public bool Extended;
        public bool Progressive;
        public int Precision;
        public int ScanLines;
        public int SamplesPerLine;
        public List<FrameComponentInfo> Components = new();
        public Dictionary<int, int> ComponentIds = new();
        public int MaxH;
        public int MaxV;
        public int McusPerLine;
        public int McusPerColumn;
    }

    private class FrameComponentInfo
    {
        public int Index;
        public int H;
        public int V;
        public int QuantizationId;
        public ushort[]? QuantizationTable;
        public short[]? BlockData;
        public int BlocksPerLine;
        public int BlocksPerColumn;
        public object[]? HuffmanTableDC;
        public object[]? HuffmanTableAC;
        public int Pred;
    }

    private class ComponentInfo
    {
        public int Index;
        public short[] Output = Array.Empty<short>();
        public double ScaleX;
        public double ScaleY;
        public int BlocksPerLine;
        public int BlocksPerColumn;
    }

    #endregion

    #region Parsing

    public void Parse(byte[] data, int? dnlScanLines = null)
    {
        int offset = 0;
        JfifInfo? jfif = null;
        AdobeInfo? adobe = null;
        FrameInfo? frame = null;
        int resetInterval = 0;
        int numSOSMarkers = 0;
        var quantizationTables = new ushort[16][];
        var huffmanTablesAC = new object[16][];
        var huffmanTablesDC = new object[16][];

        int fileMarker = ReadUint16(data, offset);
        offset += 2;

        if (fileMarker != MarkerSOI)
        {
            throw new JpegException("SOI not found");
        }

        fileMarker = ReadUint16(data, offset);
        offset += 2;

        while (fileMarker != MarkerEOI)
        {
            switch (fileMarker)
            {
                case >= MarkerAPP0 and <= 0xFFEF: // APP0-APP15
                case 0xFFFE: // COM (Comment)
                    {
                        var (appData, newOffset) = ReadDataBlock(data, offset);
                        offset = newOffset;

                        if (fileMarker == MarkerAPP0)
                        {
                            // Check for 'JFIF\0'
                            if (appData.Length >= 5 &&
                                appData[0] == 0x4A && appData[1] == 0x46 &&
                                appData[2] == 0x49 && appData[3] == 0x46 &&
                                appData[4] == 0)
                            {
                                jfif = new JfifInfo
                                {
                                    VersionMajor = appData[5],
                                    VersionMinor = appData[6],
                                    DensityUnits = appData[7],
                                    XDensity = (appData[8] << 8) | appData[9],
                                    YDensity = (appData[10] << 8) | appData[11]
                                };
                            }
                        }

                        if (fileMarker == MarkerAPP14)
                        {
                            // Check for 'Adobe'
                            if (appData.Length >= 12 &&
                                appData[0] == 0x41 && appData[1] == 0x64 &&
                                appData[2] == 0x6F && appData[3] == 0x62 &&
                                appData[4] == 0x65)
                            {
                                adobe = new AdobeInfo
                                {
                                    Version = (appData[5] << 8) | appData[6],
                                    Flags0 = (appData[7] << 8) | appData[8],
                                    Flags1 = (appData[9] << 8) | appData[10],
                                    TransformCode = appData[11]
                                };
                            }
                        }
                    }
                    break;

                case MarkerDQT: // Define Quantization Tables
                    {
                        int length = ReadUint16(data, offset);
                        offset += 2;
                        int endOffset = offset + length - 2;

                        while (offset < endOffset)
                        {
                            int tableSpec = data[offset++];
                            var tableData = new ushort[64];

                            if ((tableSpec >> 4) == 0)
                            {
                                // 8-bit values
                                for (int j = 0; j < 64; j++)
                                {
                                    int z = DctZigZag[j];
                                    tableData[z] = data[offset++];
                                }
                            }
                            else if ((tableSpec >> 4) == 1)
                            {
                                // 16-bit values
                                for (int j = 0; j < 64; j++)
                                {
                                    int z = DctZigZag[j];
                                    tableData[z] = (ushort)ReadUint16(data, offset);
                                    offset += 2;
                                }
                            }
                            else
                            {
                                throw new JpegException("DQT - invalid table spec");
                            }

                            quantizationTables[tableSpec & 15] = tableData;
                        }
                    }
                    break;

                case MarkerSOF0: // Baseline DCT
                case MarkerSOF1: // Extended DCT
                case MarkerSOF2: // Progressive DCT
                    {
                        if (frame != null)
                        {
                            throw new JpegException("Only single frame JPEGs supported");
                        }

                        offset += 2; // Skip marker length

                        frame = new FrameInfo
                        {
                            Extended = fileMarker == MarkerSOF1,
                            Progressive = fileMarker == MarkerSOF2,
                            Precision = data[offset++]
                        };

                        int sofScanLines = ReadUint16(data, offset);
                        offset += 2;
                        frame.ScanLines = dnlScanLines ?? sofScanLines;
                        frame.SamplesPerLine = ReadUint16(data, offset);
                        offset += 2;

                        int componentsCount = data[offset++];
                        int maxH = 0, maxV = 0;

                        for (int i = 0; i < componentsCount; i++)
                        {
                            int componentId = data[offset];
                            int h = data[offset + 1] >> 4;
                            int v = data[offset + 1] & 15;

                            if (maxH < h) maxH = h;
                            if (maxV < v) maxV = v;

                            int qId = data[offset + 2];
                            frame.Components.Add(new FrameComponentInfo
                            {
                                H = h,
                                V = v,
                                QuantizationId = qId
                            });
                            frame.ComponentIds[componentId] = frame.Components.Count - 1;
                            offset += 3;
                        }

                        frame.MaxH = maxH;
                        frame.MaxV = maxV;
                        PrepareComponents(frame);
                    }
                    break;

                case MarkerDHT: // Define Huffman Tables
                    {
                        int length = ReadUint16(data, offset);
                        offset += 2;

                        for (int i = 2; i < length;)
                        {
                            int tableSpec = data[offset++];
                            var codeLengths = new byte[16];
                            int codeLengthSum = 0;

                            for (int j = 0; j < 16; j++, offset++)
                            {
                                codeLengthSum += codeLengths[j] = data[offset];
                            }

                            var huffmanValues = new byte[codeLengthSum];
                            for (int j = 0; j < codeLengthSum; j++, offset++)
                            {
                                huffmanValues[j] = data[offset];
                            }

                            i += 17 + codeLengthSum;

                            var table = BuildHuffmanTable(codeLengths, huffmanValues);
                            if ((tableSpec >> 4) == 0)
                            {
                                huffmanTablesDC[tableSpec & 15] = table;
                            }
                            else
                            {
                                huffmanTablesAC[tableSpec & 15] = table;
                            }
                        }
                    }
                    break;

                case MarkerDRI: // Define Restart Interval
                    offset += 2; // Skip marker length
                    resetInterval = ReadUint16(data, offset);
                    offset += 2;
                    break;

                case MarkerSOS: // Start of Scan
                    {
                        bool parseDNLMarker = ++numSOSMarkers == 1 && dnlScanLines == null;
                        offset += 2; // Skip marker length

                        int selectorsCount = data[offset++];
                        var components = new List<FrameComponentInfo>();

                        for (int i = 0; i < selectorsCount; i++)
                        {
                            int index = data[offset++];
                            int componentIndex = frame!.ComponentIds[index];
                            var component = frame.Components[componentIndex];
                            component.Index = index;

                            int tableSpec = data[offset++];
                            component.HuffmanTableDC = huffmanTablesDC[tableSpec >> 4];
                            component.HuffmanTableAC = huffmanTablesAC[tableSpec & 15];
                            components.Add(component);
                        }

                        int spectralStart = data[offset++];
                        int spectralEnd = data[offset++];
                        int successiveApproximation = data[offset++];

                        try
                        {
                            int processed = DecodeScan(
                                data, offset, frame!,
                                components,
                                resetInterval,
                                spectralStart, spectralEnd,
                                successiveApproximation >> 4,
                                successiveApproximation & 15,
                                parseDNLMarker);
                            offset += processed;
                        }
                        catch (DnlMarkerException ex)
                        {
                            // Re-parse with correct scan lines
                            Parse(data, ex.ScanLines);
                            return;
                        }
                        catch (EoiMarkerException)
                        {
                            // Ignore rest of image data
                            goto exitMarkerLoop;
                        }
                    }
                    break;

                case MarkerDNL:
                    offset += 4;
                    break;

                case 0xFFFF: // Fill bytes
                    if (data[offset] != 0xFF)
                    {
                        offset--;
                    }
                    break;

                default:
                    {
                        var nextMarker = FindNextFileMarker(data, offset - 2, offset - 3);
                        if (nextMarker != null && nextMarker.Invalid != null)
                        {
                            offset = nextMarker.Offset;
                            break;
                        }
                        if (nextMarker == null || offset >= data.Length - 1)
                        {
                            goto exitMarkerLoop;
                        }
                        throw new JpegException($"Unknown marker: {fileMarker:X4}");
                    }
            }

            // Check bounds before reading next marker
            if (offset >= data.Length - 1)
            {
                goto exitMarkerLoop;
            }
            fileMarker = ReadUint16(data, offset);
            offset += 2;
        }

    exitMarkerLoop:
        if (frame == null)
        {
            throw new JpegException("No frame data found");
        }

        Width = frame.SamplesPerLine;
        Height = frame.ScanLines;
        _jfif = jfif;
        _adobe = adobe;
        _components = new List<ComponentInfo>();

        foreach (var component in frame.Components)
        {
            var quantizationTable = quantizationTables[component.QuantizationId];
            if (quantizationTable != null)
            {
                component.QuantizationTable = quantizationTable;
            }

            var output = BuildComponentData(frame, component);
            _components.Add(new ComponentInfo
            {
                Index = component.Index,
                Output = output,
                ScaleX = (double)component.H / frame.MaxH,
                ScaleY = (double)component.V / frame.MaxV,
                BlocksPerLine = component.BlocksPerLine,
                BlocksPerColumn = component.BlocksPerColumn
            });
        }

        NumComponents = _components.Count;
    }

    #endregion

    #region Data Reading Utilities

    private static int ReadUint16(byte[] data, int offset)
    {
        return (data[offset] << 8) | data[offset + 1];
    }

    private static (byte[] AppData, int NewOffset) ReadDataBlock(byte[] data, int offset)
    {
        int length = ReadUint16(data, offset);
        offset += 2;
        int endOffset = offset + length - 2;

        var marker = FindNextFileMarker(data, endOffset, offset);
        if (marker?.Invalid != null)
        {
            endOffset = marker.Offset;
        }

        var appData = new byte[endOffset - offset];
        Array.Copy(data, offset, appData, 0, appData.Length);
        return (appData, offset + appData.Length);
    }

    private class MarkerInfo
    {
        public string? Invalid;
        public int Marker;
        public int Offset;
    }

    private static MarkerInfo? FindNextFileMarker(byte[] data, int currentPos, int startPos)
    {
        int maxPos = data.Length - 1;
        int newPos = startPos < currentPos ? startPos : currentPos;

        if (currentPos >= maxPos)
        {
            return null;
        }

        int currentMarker = ReadUint16(data, currentPos);
        if (currentMarker >= 0xFFC0 && currentMarker <= 0xFFFE)
        {
            return new MarkerInfo
            {
                Invalid = null,
                Marker = currentMarker,
                Offset = currentPos
            };
        }

        int newMarker = ReadUint16(data, newPos);
        while (!(newMarker >= 0xFFC0 && newMarker <= 0xFFFE))
        {
            if (++newPos >= maxPos)
            {
                return null;
            }
            newMarker = ReadUint16(data, newPos);
        }

        return new MarkerInfo
        {
            Invalid = currentMarker.ToString("X4"),
            Marker = newMarker,
            Offset = newPos
        };
    }

    #endregion

    #region Huffman Tables

    private static object[] BuildHuffmanTable(byte[] codeLengths, byte[] values)
    {
        int k = 0;
        int length = 16;

        while (length > 0 && codeLengths[length - 1] == 0)
        {
            length--;
        }

        var code = new List<(object[] Children, int Index)>
        {
            (new object[2], 0)
        };

        var p = code[0];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < codeLengths[i]; j++)
            {
                p = code[^1];
                code.RemoveAt(code.Count - 1);
                p.Children[p.Index] = (int)values[k];

                while (p.Index > 0)
                {
                    p = code[^1];
                    code.RemoveAt(code.Count - 1);
                }

                p.Index++;
                code.Add(p);

                while (code.Count <= i)
                {
                    var q = (Children: new object[2], Index: 0);
                    p.Children[p.Index] = q.Children;
                    p = q;
                    code.Add(q);
                }
                k++;
            }

            if (i + 1 < length)
            {
                var q = (Children: new object[2], Index: 0);
                p.Children[p.Index] = q.Children;
                code.Add(q);
                p = q;  // Update p to point to new node
            }
        }

        return code[0].Children;
    }

    #endregion

    #region Component Preparation

    private static void PrepareComponents(FrameInfo frame)
    {
        int mcusPerLine = (int)Math.Ceiling((double)frame.SamplesPerLine / 8 / frame.MaxH);
        int mcusPerColumn = (int)Math.Ceiling((double)frame.ScanLines / 8 / frame.MaxV);

        foreach (var component in frame.Components)
        {
            int blocksPerLine = (int)Math.Ceiling(
                (double)(int)Math.Ceiling((double)frame.SamplesPerLine / 8) * component.H / frame.MaxH);
            int blocksPerColumn = (int)Math.Ceiling(
                (double)(int)Math.Ceiling((double)frame.ScanLines / 8) * component.V / frame.MaxV);
            int blocksPerLineForMcu = mcusPerLine * component.H;
            int blocksPerColumnForMcu = mcusPerColumn * component.V;

            int blocksBufferSize = 64 * blocksPerColumnForMcu * (blocksPerLineForMcu + 1);
            component.BlockData = new short[blocksBufferSize];
            component.BlocksPerLine = blocksPerLine;
            component.BlocksPerColumn = blocksPerColumn;
        }

        frame.McusPerLine = mcusPerLine;
        frame.McusPerColumn = mcusPerColumn;
    }

    private static int GetBlockBufferOffset(FrameComponentInfo component, int row, int col)
    {
        return 64 * ((component.BlocksPerLine + 1) * row + col);
    }

    #endregion

    #region Scan Decoding

    private class DnlMarkerException : Exception
    {
        public int ScanLines { get; }
        public DnlMarkerException(int scanLines) : base("DNL marker found")
        {
            ScanLines = scanLines;
        }
    }

    private class EoiMarkerException : Exception
    {
        public EoiMarkerException() : base("EOI marker found") { }
    }

    private int DecodeScan(
        byte[] data,
        int offset,
        FrameInfo frame,
        List<FrameComponentInfo> components,
        int resetInterval,
        int spectralStart,
        int spectralEnd,
        int successivePrev,
        int successive,
        bool parseDNLMarker = false)
    {
        int mcusPerLine = frame.McusPerLine;
        bool progressive = frame.Progressive;
        int startOffset = offset;

        int bitsData = 0;
        int bitsCount = 0;
        int eobrun = 0;
        int blockRow = 0;

        int successiveACState = 0;
        int successiveACNextValue = 0;

        int ReadBit()
        {
            if (bitsCount > 0)
            {
                bitsCount--;
                return (bitsData >> bitsCount) & 1;
            }
            bitsData = data[offset++];
            if (bitsData == 0xFF)
            {
                int nextByte = data[offset++];
                if (nextByte != 0)
                {
                    if (nextByte == 0xDC && parseDNLMarker)
                    {
                        offset += 2; // Skip marker length
                        int scanLines = ReadUint16(data, offset);
                        offset += 2;
                        if (scanLines > 0 && scanLines != frame.ScanLines)
                        {
                            throw new DnlMarkerException(scanLines);
                        }
                    }
                    else if (nextByte == 0xD9)
                    {
                        if (parseDNLMarker)
                        {
                            int maybeScanLines = blockRow * (frame.Precision == 8 ? 8 : 0);
                            if (maybeScanLines > 0 &&
                                Math.Round((double)frame.ScanLines / maybeScanLines) >= 5)
                            {
                                throw new DnlMarkerException(maybeScanLines);
                            }
                        }
                        throw new EoiMarkerException();
                    }
                    throw new JpegException($"Unexpected marker {((bitsData << 8) | nextByte):X4}");
                }
            }
            bitsCount = 7;
            return bitsData >>> 7;
        }

        int DecodeHuffman(object[] tree)
        {
            object node = tree;
            int depth = 0;
            while (true)
            {
                int bit = ReadBit();
                var arr = node as object[];
                if (arr == null)
                {
                    throw new JpegException($"Invalid huffman sequence: expected array node at depth {depth}, got {node?.GetType().Name ?? "null"} at offset {offset}");
                }
                if (bit < 0 || bit > 1)
                {
                    throw new JpegException($"Invalid huffman sequence: invalid bit {bit} at depth {depth}, offset {offset}");
                }
                node = arr[bit];
                depth++;
                
                if (node is int value)
                {
                    return value;
                }
                if (node == null)
                {
                    throw new JpegException($"Invalid huffman sequence: null node at depth {depth}, offset {offset}, bitsData={bitsData:X2}, bitsCount={bitsCount}");
                }
                if (node is not object[])
                {
                    throw new JpegException($"Invalid huffman sequence: unexpected node type {node.GetType().Name} (value={node}) at depth {depth}, offset {offset}");
                }
            }
        }

        int Receive(int length)
        {
            int n = 0;
            while (length > 0)
            {
                n = (n << 1) | ReadBit();
                length--;
            }
            return n;
        }

        int ReceiveAndExtend(int length)
        {
            if (length == 1)
            {
                return ReadBit() == 1 ? 1 : -1;
            }
            int n = Receive(length);
            if (n >= 1 << (length - 1))
            {
                return n;
            }
            return n + (-1 << length) + 1;
        }

        void DecodeBaseline(FrameComponentInfo component, int blockOffset)
        {
            int t = DecodeHuffman(component.HuffmanTableDC!);
            int diff = t == 0 ? 0 : ReceiveAndExtend(t);
            component.BlockData![blockOffset] = (short)(component.Pred += diff);

            int k = 1;
            while (k < 64)
            {
                int rs = DecodeHuffman(component.HuffmanTableAC!);
                int s = rs & 15;
                int r = rs >> 4;
                if (s == 0)
                {
                    if (r < 15) break;
                    k += 16;
                    continue;
                }
                k += r;
                if (k >= 64)
                {
                    // Malformed data - k exceeded block size
                    break;
                }
                int z = DctZigZag[k];
                component.BlockData[blockOffset + z] = (short)ReceiveAndExtend(s);
                k++;
            }
        }

        void DecodeDCFirst(FrameComponentInfo component, int blockOffset)
        {
            int t = DecodeHuffman(component.HuffmanTableDC!);
            int diff = t == 0 ? 0 : ReceiveAndExtend(t) << successive;
            component.BlockData![blockOffset] = (short)(component.Pred += diff);
        }

        void DecodeDCSuccessive(FrameComponentInfo component, int blockOffset)
        {
            component.BlockData![blockOffset] |= (short)(ReadBit() << successive);
        }

        void DecodeACFirst(FrameComponentInfo component, int blockOffset)
        {
            if (eobrun > 0)
            {
                eobrun--;
                return;
            }
            int k = spectralStart;
            int e = spectralEnd;
            while (k <= e)
            {
                int rs = DecodeHuffman(component.HuffmanTableAC!);
                int s = rs & 15;
                int r = rs >> 4;
                if (s == 0)
                {
                    if (r < 15)
                    {
                        eobrun = Receive(r) + (1 << r) - 1;
                        break;
                    }
                    k += 16;
                    continue;
                }
                k += r;
                if (k >= 64)
                {
                    break;
                }
                int z = DctZigZag[k];
                component.BlockData![blockOffset + z] = (short)(ReceiveAndExtend(s) * (1 << successive));
                k++;
            }
        }

        void DecodeACSuccessive(FrameComponentInfo component, int blockOffset)
        {
            int k = spectralStart;
            int e = spectralEnd;
            int r = 0;

            while (k <= e)
            {
                if (k >= 64)
                {
                    break;
                }
                int offsetZ = blockOffset + DctZigZag[k];
                int sign = component.BlockData![offsetZ] < 0 ? -1 : 1;

                switch (successiveACState)
                {
                    case 0: // initial state
                        {
                            int rs = DecodeHuffman(component.HuffmanTableAC!);
                            int s = rs & 15;
                            r = rs >> 4;
                            if (s == 0)
                            {
                                if (r < 15)
                                {
                                    eobrun = Receive(r) + (1 << r);
                                    successiveACState = 4;
                                }
                                else
                                {
                                    r = 16;
                                    successiveACState = 1;
                                }
                            }
                            else
                            {
                                if (s != 1)
                                {
                                    throw new JpegException("Invalid ACn encoding");
                                }
                                successiveACNextValue = ReceiveAndExtend(s);
                                successiveACState = r != 0 ? 2 : 3;
                            }
                        }
                        continue;
                    case 1:
                    case 2:
                        if (component.BlockData[offsetZ] != 0)
                        {
                            component.BlockData[offsetZ] += (short)(sign * (ReadBit() << successive));
                        }
                        else
                        {
                            r--;
                            if (r == 0)
                            {
                                successiveACState = successiveACState == 2 ? 3 : 0;
                            }
                        }
                        break;
                    case 3:
                        if (component.BlockData[offsetZ] != 0)
                        {
                            component.BlockData[offsetZ] += (short)(sign * (ReadBit() << successive));
                        }
                        else
                        {
                            component.BlockData[offsetZ] = (short)(successiveACNextValue << successive);
                            successiveACState = 0;
                        }
                        break;
                    case 4:
                        if (component.BlockData[offsetZ] != 0)
                        {
                            component.BlockData[offsetZ] += (short)(sign * (ReadBit() << successive));
                        }
                        break;
                }
                k++;
            }

            if (successiveACState == 4)
            {
                eobrun--;
                if (eobrun == 0)
                {
                    successiveACState = 0;
                }
            }
        }

        void DecodeMcu(FrameComponentInfo component, Action<FrameComponentInfo, int> decode, int mcu, int row, int col)
        {
            int mcuRow = mcu / mcusPerLine;
            int mcuCol = mcu % mcusPerLine;
            blockRow = mcuRow * component.V + row;
            int blockCol = mcuCol * component.H + col;
            int blockOffset = GetBlockBufferOffset(component, blockRow, blockCol);
            decode(component, blockOffset);
        }

        void DecodeBlock(FrameComponentInfo component, Action<FrameComponentInfo, int> decode, int mcu)
        {
            blockRow = mcu / component.BlocksPerLine;
            int blockCol = mcu % component.BlocksPerLine;
            int blockOffset = GetBlockBufferOffset(component, blockRow, blockCol);
            decode(component, blockOffset);
        }

        int componentsLength = components.Count;
        Action<FrameComponentInfo, int> decodeFn;

        if (progressive)
        {
            if (spectralStart == 0)
            {
                decodeFn = successivePrev == 0 ? DecodeDCFirst : DecodeDCSuccessive;
            }
            else
            {
                decodeFn = successivePrev == 0 ? DecodeACFirst : DecodeACSuccessive;
            }
        }
        else
        {
            decodeFn = DecodeBaseline;
        }

        int mcu = 0;
        int mcuExpected = componentsLength == 1
            ? components[0].BlocksPerLine * components[0].BlocksPerColumn
            : mcusPerLine * frame.McusPerColumn;

        while (mcu <= mcuExpected)
        {
            int mcuToRead = resetInterval != 0
                ? Math.Min(mcuExpected - mcu, resetInterval)
                : mcuExpected;

            if (mcuToRead > 0)
            {
                foreach (var component in components)
                {
                    component.Pred = 0;
                }
                eobrun = 0;

                if (componentsLength == 1)
                {
                    var component = components[0];
                    for (int n = 0; n < mcuToRead; n++)
                    {
                        DecodeBlock(component, decodeFn, mcu);
                        mcu++;
                    }
                }
                else
                {
                    for (int n = 0; n < mcuToRead; n++)
                    {
                        foreach (var component in components)
                        {
                            int h = component.H;
                            int v = component.V;
                            for (int j = 0; j < v; j++)
                            {
                                for (int k = 0; k < h; k++)
                                {
                                    DecodeMcu(component, decodeFn, mcu, j, k);
                                }
                            }
                        }
                        mcu++;
                    }
                }
            }

            bitsCount = 0;
            var fileMarker = FindNextFileMarker(data, offset, offset);
            if (fileMarker == null)
            {
                break;
            }
            if (fileMarker.Invalid != null)
            {
                offset = fileMarker.Offset;
            }
            if (fileMarker.Marker >= 0xFFD0 && fileMarker.Marker <= 0xFFD7)
            {
                offset += 2;
            }
            else
            {
                break;
            }
        }

        return offset - startOffset;
    }

    #endregion

    #region IDCT and Quantization

    private static void QuantizeAndInverse(FrameComponentInfo component, int blockBufferOffset, short[] p)
    {
        var qt = component.QuantizationTable;
        var blockData = component.BlockData!;

        if (qt == null)
        {
            throw new JpegException("Missing required Quantization Table");
        }

        int v0, v1, v2, v3, v4, v5, v6, v7;
        int p0, p1, p2, p3, p4, p5, p6, p7;
        int t;

        // Inverse DCT on rows
        for (int row = 0; row < 64; row += 8)
        {
            p0 = blockData[blockBufferOffset + row];
            p1 = blockData[blockBufferOffset + row + 1];
            p2 = blockData[blockBufferOffset + row + 2];
            p3 = blockData[blockBufferOffset + row + 3];
            p4 = blockData[blockBufferOffset + row + 4];
            p5 = blockData[blockBufferOffset + row + 5];
            p6 = blockData[blockBufferOffset + row + 6];
            p7 = blockData[blockBufferOffset + row + 7];

            // Dequant p0
            p0 *= qt[row];

            // Check for all-zero AC coefficients
            if ((p1 | p2 | p3 | p4 | p5 | p6 | p7) == 0)
            {
                t = (DctSqrt2 * p0 + 512) >> 10;
                p[row] = (short)t;
                p[row + 1] = (short)t;
                p[row + 2] = (short)t;
                p[row + 3] = (short)t;
                p[row + 4] = (short)t;
                p[row + 5] = (short)t;
                p[row + 6] = (short)t;
                p[row + 7] = (short)t;
                continue;
            }

            // Dequant p1...p7
            p1 *= qt[row + 1];
            p2 *= qt[row + 2];
            p3 *= qt[row + 3];
            p4 *= qt[row + 4];
            p5 *= qt[row + 5];
            p6 *= qt[row + 6];
            p7 *= qt[row + 7];

            // Stage 4
            v0 = (DctSqrt2 * p0 + 128) >> 8;
            v1 = (DctSqrt2 * p4 + 128) >> 8;
            v2 = p2;
            v3 = p6;
            v4 = (DctSqrt1d2 * (p1 - p7) + 128) >> 8;
            v7 = (DctSqrt1d2 * (p1 + p7) + 128) >> 8;
            v5 = p3 << 4;
            v6 = p5 << 4;

            // Stage 3
            v0 = (v0 + v1 + 1) >> 1;
            v1 = v0 - v1;
            t = (v2 * DctSin6 + v3 * DctCos6 + 128) >> 8;
            v2 = (v2 * DctCos6 - v3 * DctSin6 + 128) >> 8;
            v3 = t;
            v4 = (v4 + v6 + 1) >> 1;
            v6 = v4 - v6;
            v7 = (v7 + v5 + 1) >> 1;
            v5 = v7 - v5;

            // Stage 2
            v0 = (v0 + v3 + 1) >> 1;
            v3 = v0 - v3;
            v1 = (v1 + v2 + 1) >> 1;
            v2 = v1 - v2;
            t = (v4 * DctSin3 + v7 * DctCos3 + 2048) >> 12;
            v4 = (v4 * DctCos3 - v7 * DctSin3 + 2048) >> 12;
            v7 = t;
            t = (v5 * DctSin1 + v6 * DctCos1 + 2048) >> 12;
            v5 = (v5 * DctCos1 - v6 * DctSin1 + 2048) >> 12;
            v6 = t;

            // Stage 1
            p[row] = (short)(v0 + v7);
            p[row + 7] = (short)(v0 - v7);
            p[row + 1] = (short)(v1 + v6);
            p[row + 6] = (short)(v1 - v6);
            p[row + 2] = (short)(v2 + v5);
            p[row + 5] = (short)(v2 - v5);
            p[row + 3] = (short)(v3 + v4);
            p[row + 4] = (short)(v3 - v4);
        }

        // Inverse DCT on columns
        for (int col = 0; col < 8; col++)
        {
            p0 = p[col];
            p1 = p[col + 8];
            p2 = p[col + 16];
            p3 = p[col + 24];
            p4 = p[col + 32];
            p5 = p[col + 40];
            p6 = p[col + 48];
            p7 = p[col + 56];

            // Check for all-zero AC coefficients
            if ((p1 | p2 | p3 | p4 | p5 | p6 | p7) == 0)
            {
                t = (DctSqrt2 * p0 + 8192) >> 14;
                // Convert to 8-bit
                if (t < -2040) t = 0;
                else if (t >= 2024) t = 255;
                else t = (t + 2056) >> 4;

                blockData[blockBufferOffset + col] = (short)t;
                blockData[blockBufferOffset + col + 8] = (short)t;
                blockData[blockBufferOffset + col + 16] = (short)t;
                blockData[blockBufferOffset + col + 24] = (short)t;
                blockData[blockBufferOffset + col + 32] = (short)t;
                blockData[blockBufferOffset + col + 40] = (short)t;
                blockData[blockBufferOffset + col + 48] = (short)t;
                blockData[blockBufferOffset + col + 56] = (short)t;
                continue;
            }

            // Stage 4
            v0 = (DctSqrt2 * p0 + 2048) >> 12;
            v1 = (DctSqrt2 * p4 + 2048) >> 12;
            v2 = p2;
            v3 = p6;
            v4 = (DctSqrt1d2 * (p1 - p7) + 2048) >> 12;
            v7 = (DctSqrt1d2 * (p1 + p7) + 2048) >> 12;
            v5 = p3;
            v6 = p5;

            // Stage 3 (shift by 128.5 << 5)
            v0 = ((v0 + v1 + 1) >> 1) + 4112;
            v1 = v0 - v1;
            t = (v2 * DctSin6 + v3 * DctCos6 + 2048) >> 12;
            v2 = (v2 * DctCos6 - v3 * DctSin6 + 2048) >> 12;
            v3 = t;
            v4 = (v4 + v6 + 1) >> 1;
            v6 = v4 - v6;
            v7 = (v7 + v5 + 1) >> 1;
            v5 = v7 - v5;

            // Stage 2
            v0 = (v0 + v3 + 1) >> 1;
            v3 = v0 - v3;
            v1 = (v1 + v2 + 1) >> 1;
            v2 = v1 - v2;
            t = (v4 * DctSin3 + v7 * DctCos3 + 2048) >> 12;
            v4 = (v4 * DctCos3 - v7 * DctSin3 + 2048) >> 12;
            v7 = t;
            t = (v5 * DctSin1 + v6 * DctCos1 + 2048) >> 12;
            v5 = (v5 * DctCos1 - v6 * DctSin1 + 2048) >> 12;
            v6 = t;

            // Stage 1
            p0 = v0 + v7;
            p7 = v0 - v7;
            p1 = v1 + v6;
            p6 = v1 - v6;
            p2 = v2 + v5;
            p5 = v2 - v5;
            p3 = v3 + v4;
            p4 = v3 - v4;

            // Convert to 8-bit integers
            blockData[blockBufferOffset + col] = (short)Clamp(p0);
            blockData[blockBufferOffset + col + 8] = (short)Clamp(p1);
            blockData[blockBufferOffset + col + 16] = (short)Clamp(p2);
            blockData[blockBufferOffset + col + 24] = (short)Clamp(p3);
            blockData[blockBufferOffset + col + 32] = (short)Clamp(p4);
            blockData[blockBufferOffset + col + 40] = (short)Clamp(p5);
            blockData[blockBufferOffset + col + 48] = (short)Clamp(p6);
            blockData[blockBufferOffset + col + 56] = (short)Clamp(p7);
        }

        static int Clamp(int value)
        {
            if (value < 16) return 0;
            if (value >= 4080) return 255;
            return value >> 4;
        }
    }

    private static short[] BuildComponentData(FrameInfo frame, FrameComponentInfo component)
    {
        int blocksPerLine = component.BlocksPerLine;
        int blocksPerColumn = component.BlocksPerColumn;
        var computationBuffer = new short[64];

        for (int blockRow = 0; blockRow < blocksPerColumn; blockRow++)
        {
            for (int blockCol = 0; blockCol < blocksPerLine; blockCol++)
            {
                int offset = GetBlockBufferOffset(component, blockRow, blockCol);
                QuantizeAndInverse(component, offset, computationBuffer);
            }
        }

        return component.BlockData!;
    }

    #endregion

    #region Output Data

    public byte[] GetData(int width, int height, bool isSourcePDF = true)
    {
        if (NumComponents > 4)
        {
            throw new JpegException("Unsupported color mode");
        }

        var data = GetLinearizedBlockData(width, height, isSourcePDF);

        if (NumComponents == 3 && IsColorConversionNeeded)
        {
            return ConvertYccToRgb(data);
        }
        else if (NumComponents == 4)
        {
            if (IsColorConversionNeeded)
            {
                return ConvertYcckToCmyk(data);
            }
        }

        return data;
    }

    private byte[] GetLinearizedBlockData(int width, int height, bool isSourcePDF)
    {
        double scaleX = (double)Width / width;
        double scaleY = (double)Height / height;

        int numComponents = _components!.Count;
        int dataLength = width * height * numComponents;
        var data = new byte[dataLength];
        var xScaleBlockOffset = new uint[width];
        const uint mask3LSB = 0xFFFFFFF8;
        double lastComponentScaleX = 0;

        for (int i = 0; i < numComponents; i++)
        {
            var component = _components[i];
            double componentScaleX = component.ScaleX * scaleX;
            double componentScaleY = component.ScaleY * scaleY;
            int offset = i;
            var output = component.Output;
            int blocksPerScanline = (component.BlocksPerLine + 1) << 3;

            if (componentScaleX != lastComponentScaleX)
            {
                for (int x = 0; x < width; x++)
                {
                    int j = (int)(x * componentScaleX);
                    xScaleBlockOffset[x] = (uint)(((j & mask3LSB) << 3) | (j & 7));
                }
                lastComponentScaleX = componentScaleX;
            }

            for (int y = 0; y < height; y++)
            {
                int j = (int)(y * componentScaleY);
                int index = (int)((blocksPerScanline * (j & mask3LSB)) | ((j & 7) << 3));

                for (int x = 0; x < width; x++)
                {
                    data[offset] = (byte)Math.Clamp((int)output[index + (int)xScaleBlockOffset[x]], 0, 255);
                    offset += numComponents;
                }
            }
        }

        return data;
    }

    private bool IsColorConversionNeeded
    {
        get
        {
            if (_adobe != null)
            {
                return _adobe.TransformCode != 0;
            }

            if (NumComponents == 3)
            {
                if (_colorTransform == 0)
                {
                    return false;
                }
                // Check for RGB component indices
                if (_components != null &&
                    _components[0].Index == 0x52 && // 'R'
                    _components[1].Index == 0x47 && // 'G'
                    _components[2].Index == 0x42)   // 'B'
                {
                    return false;
                }
                return true;
            }

            if (_colorTransform == 1)
            {
                return true;
            }

            return false;
        }
    }

    private static byte[] ConvertYccToRgb(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 3)
        {
            int y = data[i];
            int cb = data[i + 1];
            int cr = data[i + 2];

            data[i] = (byte)Math.Clamp((int)(y - 179.456 + 1.402 * cr), 0, 255);
            data[i + 1] = (byte)Math.Clamp((int)(y + 135.459 - 0.344 * cb - 0.714 * cr), 0, 255);
            data[i + 2] = (byte)Math.Clamp((int)(y - 226.816 + 1.772 * cb), 0, 255);
        }
        return data;
    }

    private static byte[] ConvertYcckToCmyk(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            int y = data[i];
            int cb = data[i + 1];
            int cr = data[i + 2];
            // K in data[i + 3] is unchanged

            data[i] = (byte)Math.Clamp((int)(434.456 - y - 1.402 * cr), 0, 255);
            data[i + 1] = (byte)Math.Clamp((int)(119.541 - y + 0.344 * cb + 0.714 * cr), 0, 255);
            data[i + 2] = (byte)Math.Clamp((int)(481.816 - y - 1.772 * cb), 0, 255);
        }
        return data;
    }

    #endregion
}
