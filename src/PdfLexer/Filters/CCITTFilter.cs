using PdfLexer.Parsers;
using System.IO;

namespace PdfLexer.Filters;

public class CCITTFilter : IDecoder
{
    private readonly ParsingContext? _ctx;
    public static CCITTFilter Instance { get; } = new CCITTFilter(null);
    public CCITTFilter(ParsingContext? ctx)
    {
        _ctx = ctx;
    }

    public Stream Decode(Stream stream, PdfDictionary? filterParams) 
    {
        return new CCITTStream(
            filterParams?.Get<PdfNumber>("K"),
            filterParams?.Get<PdfBoolean>("EndOfLine"),
            filterParams?.Get<PdfBoolean>("EndOfBlock"),
            filterParams?.Get<PdfBoolean>("BlackIs1"),
            filterParams?.Get<PdfBoolean>("EncodedByteAlign"),
            filterParams?.Get<PdfNumber>("Columns"),
            filterParams?.Get<PdfNumber>("Rows"),
            new BufferedStream(stream),
            _ctx
            );
    }

}

// CCITT decode stream MODIFIED / PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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

internal class CCITTStream : DecodeStream
{

    private const int ccittEOL = -2;
    private const int ccittEOF = -1;
    private const int twoDimPass = 0;
    private const int twoDimHoriz = 1;
    private const int twoDimVert0 = 2;
    private const int twoDimVertR1 = 3;
    private const int twoDimVertL1 = 4;
    private const int twoDimVertR2 = 5;
    private const int twoDimVertL2 = 6;
    private const int twoDimVertR3 = 7;
    private const int twoDimVertL3 = 8;

    private static readonly int[][] twoDimTable = new int[][] {
new int[] { -1, -1 },  new int[] { -1, -1 },                   // 000000x
new int[] { 7, twoDimVertL3},                    // 0000010
new int[] { 7, twoDimVertR3},                    // 0000011
new int[] { 6, twoDimVertL2},  new int[] { 6, twoDimVertL2 }, // 000010x
new int[] { 6, twoDimVertR2},  new int[] { 6, twoDimVertR2 }, // 000011x
new int[] { 4, twoDimPass },  new int[] { 4, twoDimPass },     // 0001xxx
new int[] { 4, twoDimPass },  new int[] { 4, twoDimPass },
new int[] { 4, twoDimPass },  new int[] { 4, twoDimPass },
new int[] { 4, twoDimPass },  new int[] { 4, twoDimPass },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },   // 001xxxx
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz},  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimHoriz },  new int[] { 3, twoDimHoriz },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 }, // 010xxxx
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertL1},  new int[] { 3, twoDimVertL1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 }, // 011xxxx
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 3, twoDimVertR1},  new int[] { 3, twoDimVertR1 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },   // 1xxxxxx
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 },
new int[] { 1, twoDimVert0 },  new int[] { 1, twoDimVert0 }
};
    private static readonly int[][] whiteTable1 = new int[][] {
new int[] { -1, -1 },                               // 00000
new int[] { 12, ccittEOL},                         // 00001
new int[] { -1, -1}, new int[] {-1, -1},                     // 0001x
new int[] { -1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, // 001xx
new int[] { -1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, // 010xx
new int[] { -1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, // 011xx
new int[] { 11, 1792}, new int[] {11, 1792},                 // 1000x
new int[] { 12, 1984},                             // 10010
new int[] { 12, 2048},                             // 10011
new int[] { 12, 2112},                             // 10100
new int[] { 12, 2176},                             // 10101
new int[] { 12, 2240},                             // 10110
new int[] { 12, 2304},                             // 10111
new int[] { 11, 1856}, new int[] {11, 1856},                 // 1100x
new int[] { 11, 1920}, new int[] {11, 1920},                 // 1101x
new int[] { 12, 2368},                             // 11100
new int[] { 12, 2432},                             // 11101
new int[] { 12, 2496},                             // 11110
new int[] { 12, 2560}                              // 11111
};
    private static readonly int[][] whiteTable2 = new int[][] {
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},     // 0000000xx
new int[] {8, 29}, new int[] {8, 29},                           // 00000010x
new int[] {8, 30}, new int[] {8, 30},                           // 00000011x
new int[] {8, 45}, new int[] {8, 45},                           // 00000100x
new int[] {8, 46}, new int[] {8, 46},                           // 00000101x
new int[] {7, 22}, new int[] {7, 22}, new int[] {7, 22}, new int[] {7, 22},         // 0000011xx
new int[] {7, 23}, new int[] {7, 23}, new int[] {7, 23}, new int[] {7, 23},         // 0000100xx
new int[] {8, 47}, new int[] {8, 47},                           // 00001010x
new int[] {8, 48}, new int[] {8, 48},                           // 00001011x
new int[] {6, 13}, new int[] {6, 13}, new int[] {6, 13}, new int[] {6, 13},         // 000011xxx
new int[] {6, 13}, new int[] {6, 13}, new int[] {6, 13}, new int[] {6, 13},
new int[] {7, 20}, new int[] {7, 20}, new int[] {7, 20}, new int[] {7, 20},         // 0001000xx
new int[] {8, 33}, new int[] {8, 33},                           // 00010010x
new int[] {8, 34}, new int[] {8, 34},                           // 00010011x
new int[] {8, 35}, new int[] {8, 35},                           // 00010100x
new int[] {8, 36}, new int[] {8, 36},                           // 00010101x
new int[] {8, 37}, new int[] {8, 37},                           // 00010110x
new int[] {8, 38}, new int[] {8, 38},                           // 00010111x
new int[] {7, 19}, new int[] {7, 19}, new int[] {7, 19}, new int[] {7, 19},         // 0001100xx
new int[] {8, 31}, new int[] {8, 31},                           // 00011010x
new int[] {8, 32}, new int[] {8, 32},                           // 00011011x
new int[] {6, 1}, new int[] {6, 1}, new int[] {6, 1}, new int[] {6, 1},             // 000111xxx
new int[] {6, 1}, new int[] {6, 1}, new int[] {6, 1}, new int[] {6, 1},
new int[] {6, 12}, new int[] {6, 12}, new int[] {6, 12}, new int[] {6, 12},         // 001000xxx
new int[] {6, 12}, new int[] {6, 12}, new int[] {6, 12}, new int[] {6, 12},
new int[] {8, 53}, new int[] {8, 53},                           // 00100100x
new int[] {8, 54}, new int[] {8, 54},                           // 00100101x
new int[] {7, 26}, new int[] {7, 26}, new int[] {7, 26}, new int[] {7, 26},         // 0010011xx
new int[] {8, 39}, new int[] {8, 39},                           // 00101000x
new int[] {8, 40}, new int[] {8, 40},                           // 00101001x
new int[] {8, 41}, new int[] {8, 41},                           // 00101010x
new int[] {8, 42}, new int[] {8, 42},                           // 00101011x
new int[] {8, 43}, new int[] {8, 43},                           // 00101100x
new int[] {8, 44}, new int[] {8, 44},                           // 00101101x
new int[] {7, 21}, new int[] {7, 21}, new int[] {7, 21}, new int[] {7, 21},         // 0010111xx
new int[] {7, 28}, new int[] {7, 28}, new int[] {7, 28}, new int[] {7, 28},         // 0011000xx
new int[] {8, 61}, new int[] {8, 61},                           // 00110010x
new int[] {8, 62}, new int[] {8, 62},                           // 00110011x
new int[] {8, 63}, new int[] {8, 63},                           // 00110100x
new int[] {8, 0}, new int[] {8, 0},                             // 00110101x
new int[] {8, 320}, new int[] {8, 320},                         // 00110110x
new int[] {8, 384}, new int[] {8, 384},                         // 00110111x
new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10},         // 00111xxxx
new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10},
new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10},
new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10}, new int[] {5, 10},
new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11},         // 01000xxxx
new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11},
new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11},
new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11}, new int[] {5, 11},
new int[] {7, 27}, new int[] {7, 27}, new int[] {7, 27}, new int[] {7, 27},         // 0100100xx
new int[] {8, 59}, new int[] {8, 59},                           // 01001010x
new int[] {8, 60}, new int[] {8, 60},                           // 01001011x
new int[] {9, 1472},                                  // 010011000
new int[] {9, 1536},                                  // 010011001
new int[] {9, 1600},                                  // 010011010
new int[] {9, 1728},                                  // 010011011
new int[] {7, 18}, new int[] {7, 18}, new int[] {7, 18}, new int[] {7, 18},         // 0100111xx
new int[] {7, 24}, new int[] {7, 24}, new int[] {7, 24}, new int[] {7, 24},         // 0101000xx
new int[] {8, 49}, new int[] {8, 49},                           // 01010010x
new int[] {8, 50}, new int[] {8, 50},                           // 01010011x
new int[] {8, 51}, new int[] {8, 51},                           // 01010100x
new int[] {8, 52}, new int[] {8, 52},                           // 01010101x
new int[] {7, 25}, new int[] {7, 25}, new int[] {7, 25}, new int[] {7, 25},         // 0101011xx
new int[] {8, 55}, new int[] {8, 55},                           // 01011000x
new int[] {8, 56}, new int[] {8, 56},                           // 01011001x
new int[] {8, 57}, new int[] {8, 57},                           // 01011010x
new int[] {8, 58}, new int[] {8, 58},                           // 01011011x
new int[] {6, 192}, new int[] {6, 192}, new int[] {6, 192}, new int[] {6, 192},     // 010111xxx
new int[] {6, 192}, new int[] {6, 192}, new int[] {6, 192}, new int[] {6, 192},
new int[] {6, 1664}, new int[] {6, 1664}, new int[] {6, 1664}, new int[] {6, 1664}, // 011000xxx
new int[] {6, 1664}, new int[] {6, 1664}, new int[] {6, 1664}, new int[] {6, 1664},
new int[] {8, 448}, new int[] {8, 448},                         // 01100100x
new int[] {8, 512}, new int[] {8, 512},                         // 01100101x
new int[] {9, 704},                                   // 011001100
new int[] {9, 768},                                   // 011001101
new int[] {8, 640}, new int[] {8, 640},                         // 01100111x
new int[] {8, 576}, new int[] {8, 576},                         // 01101000x
new int[] {9, 832},                                   // 011010010
new int[] {9, 896},                                   // 011010011
new int[] {9, 960},                                   // 011010100
new int[] {9, 1024},                                  // 011010101
new int[] {9, 1088},                                  // 011010110
new int[] {9, 1152},                                  // 011010111
new int[] {9, 1216},                                  // 011011000
new int[] {9, 1280},                                  // 011011001
new int[] {9, 1344},                                  // 011011010
new int[] {9, 1408},                                  // 011011011
new int[] {7, 256}, new int[] {7, 256}, new int[] {7, 256}, new int[] {7, 256},     // 0110111xx
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},             // 0111xxxxx
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2}, new int[] {4, 2},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},             // 1000xxxxx
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3}, new int[] {4, 3},
new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128},     // 10010xxxx
new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128},
new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128},
new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128}, new int[] {5, 128},
new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8},             // 10011xxxx
new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8},
new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8},
new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8}, new int[] {5, 8},
new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9},             // 10100xxxx
new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9},
new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9},
new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9}, new int[] {5, 9},
new int[] {6, 16}, new int[] {6, 16}, new int[] {6, 16}, new int[] {6, 16},         // 101010xxx
new int[] {6, 16}, new int[] {6, 16}, new int[] {6, 16}, new int[] {6, 16},
new int[] {6, 17}, new int[] {6, 17}, new int[] {6, 17}, new int[] {6, 17},         // 101011xxx
new int[] {6, 17}, new int[] {6, 17}, new int[] {6, 17}, new int[] {6, 17},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},             // 1011xxxxx
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4}, new int[] {4, 4},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},             // 1100xxxxx
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},
new int[] {6, 14}, new int[] {6, 14}, new int[] {6, 14}, new int[] {6, 14},         // 110100xxx
new int[] {6, 14}, new int[] {6, 14}, new int[] {6, 14}, new int[] {6, 14},
new int[] {6, 15}, new int[] {6, 15}, new int[] {6, 15}, new int[] {6, 15},         // 110101xxx
new int[] {6, 15}, new int[] {6, 15}, new int[] {6, 15}, new int[] {6, 15},
new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64},         // 11011xxxx
new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64},
new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64},
new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64}, new int[] {5, 64},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},             // 1110xxxxx
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},             // 1111xxxxx
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7},
new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}, new int[] {4, 7}
    };

    private static readonly int[][] blackTable1 = new int[][] {
new int[] {-1, -1}, new int[] {-1, -1},                             // 000000000000x
new int[] {12, ccittEOL}, new int[] {12, ccittEOL},                 // 000000000001x
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000001xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000010xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000011xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000100xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000101xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000110xx
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1},         // 00000000111xx
new int[] {11, 1792}, new int[] {11, 1792}, new int[] {11, 1792}, new int[] {11, 1792}, // 00000001000xx
new int[] {12, 1984}, new int[] {12, 1984},                         // 000000010010x
new int[] {12, 2048}, new int[] {12, 2048},                         // 000000010011x
new int[] {12, 2112}, new int[] {12, 2112},                         // 000000010100x
new int[] {12, 2176}, new int[] {12, 2176},                         // 000000010101x
new int[] {12, 2240}, new int[] {12, 2240},                         // 000000010110x
new int[] {12, 2304}, new int[] {12, 2304},                         // 000000010111x
new int[] {11, 1856}, new int[] {11, 1856}, new int[] {11, 1856}, new int[] {11, 1856}, // 00000001100xx
new int[] {11, 1920}, new int[] {11, 1920}, new int[] {11, 1920}, new int[] {11, 1920}, // 00000001101xx
new int[] {12, 2368}, new int[] {12, 2368},                         // 000000011100x
new int[] {12, 2432}, new int[] {12, 2432},                         // 000000011101x
new int[] {12, 2496}, new int[] {12, 2496},                         // 000000011110x
new int[] {12, 2560}, new int[] {12, 2560},                         // 000000011111x
new int[] {10, 18}, new int[] {10, 18}, new int[] {10, 18}, new int[] {10, 18},         // 0000001000xxx
new int[] {10, 18}, new int[] {10, 18}, new int[] {10, 18}, new int[] {10, 18},
new int[] {12, 52}, new int[] {12, 52},                             // 000000100100x
new int[] {13, 640},                                      // 0000001001010
new int[] {13, 704},                                      // 0000001001011
new int[] {13, 768},                                      // 0000001001100
new int[] {13, 832},                                      // 0000001001101
new int[] {12, 55}, new int[] {12, 55},                             // 000000100111x
new int[] {12, 56}, new int[] {12, 56},                             // 000000101000x
new int[] {13, 1280},                                     // 0000001010010
new int[] {13, 1344},                                     // 0000001010011
new int[] {13, 1408},                                     // 0000001010100
new int[] {13, 1472},                                     // 0000001010101
new int[] {12, 59}, new int[] {12, 59},                             // 000000101011x
new int[] {12, 60}, new int[] {12, 60},                             // 000000101100x
new int[] {13, 1536},                                     // 0000001011010
new int[] {13, 1600},                                     // 0000001011011
new int[] {11, 24}, new int[] {11, 24}, new int[] {11, 24}, new int[] {11, 24},         // 00000010111xx
new int[] {11, 25}, new int[] {11, 25}, new int[] {11, 25}, new int[] {11, 25},         // 00000011000xx
new int[] {13, 1664},                                     // 0000001100100
new int[] {13, 1728},                                     // 0000001100101
new int[] {12, 320}, new int[] {12, 320},                           // 000000110011x
new int[] {12, 384}, new int[] {12, 384},                           // 000000110100x
new int[] {12, 448}, new int[] {12, 448},                           // 000000110101x
new int[] {13, 512},                                      // 0000001101100
new int[] {13, 576},                                      // 0000001101101
new int[] {12, 53}, new int[] {12, 53},                             // 000000110111x
new int[] {12, 54}, new int[] {12, 54},                             // 000000111000x
new int[] {13, 896},                                      // 0000001110010
new int[] {13, 960},                                      // 0000001110011
new int[] {13, 1024},                                     // 0000001110100
new int[] {13, 1088},                                     // 0000001110101
new int[] {13, 1152},                                     // 0000001110110
new int[] {13, 1216},                                     // 0000001110111
new int[] {10, 64}, new int[] {10, 64}, new int[] {10, 64}, new int[] {10, 64},         // 0000001111xxx
new int[] {10, 64}, new int[] {10, 64}, new int[] {10, 64}, new int[] {10, 64}
};
    private static readonly int[][] blackTable2 = new int[][] {
new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13},     // 00000100xxxx
new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13},
new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13},
new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13}, new int[] {8, 13},
new int[] {11, 23}, new int[] {11, 23},                     // 00000101000x
new int[] {12, 50},                               // 000001010010
new int[] {12, 51},                               // 000001010011
new int[] {12, 44},                               // 000001010100
new int[] {12, 45},                               // 000001010101
new int[] {12, 46},                               // 000001010110
new int[] {12, 47},                               // 000001010111
new int[] {12, 57},                               // 000001011000
new int[] {12, 58},                               // 000001011001
new int[] {12, 61},                               // 000001011010
new int[] {12, 256},                              // 000001011011
new int[] {10, 16}, new int[] {10, 16}, new int[] {10, 16}, new int[] {10, 16}, // 0000010111xx
new int[] {10, 17}, new int[] {10, 17}, new int[] {10, 17}, new int[] {10, 17}, // 0000011000xx
new int[] {12, 48},                               // 000001100100
new int[] {12, 49},                               // 000001100101
new int[] {12, 62},                               // 000001100110
new int[] {12, 63},                               // 000001100111
new int[] {12, 30},                               // 000001101000
new int[] {12, 31},                               // 000001101001
new int[] {12, 32},                               // 000001101010
new int[] {12, 33},                               // 000001101011
new int[] {12, 40},                               // 000001101100
new int[] {12, 41},                               // 000001101101
new int[] {11, 22}, new int[] {11, 22},                     // 00000110111x
new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14},     // 00000111xxxx
new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14},
new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14},
new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14}, new int[] {8, 14},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},     // 0000100xxxxx
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10}, new int[] {7, 10},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},     // 0000101xxxxx
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11}, new int[] {7, 11},
new int[] {9, 15}, new int[] {9, 15}, new int[] {9, 15}, new int[] {9, 15},     // 000011000xxx
new int[] {9, 15}, new int[] {9, 15}, new int[] {9, 15}, new int[] {9, 15},
new int[] {12, 128},                              // 000011001000
new int[] {12, 192},                              // 000011001001
new int[] {12, 26},                               // 000011001010
new int[] {12, 27},                               // 000011001011
new int[] {12, 28},                               // 000011001100
new int[] {12, 29},                               // 000011001101
new int[] {11, 19}, new int[] {11, 19},                     // 00001100111x
new int[] {11, 20}, new int[] {11, 20},                     // 00001101000x
new int[] {12, 34},                               // 000011010010
new int[] {12, 35},                               // 000011010011
new int[] {12, 36},                               // 000011010100
new int[] {12, 37},                               // 000011010101
new int[] {12, 38},                               // 000011010110
new int[] {12, 39},                               // 000011010111
new int[] {11, 21}, new int[] {11, 21},                     // 00001101100x
new int[] {12, 42},                               // 000011011010
new int[] {12, 43},                               // 000011011011
new int[] {10, 0}, new int[] {10, 0}, new int[] {10, 0}, new int[] {10, 0},     // 0000110111xx
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},     // 0000111xxxxx
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12},
new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}, new int[] {7, 12}
};
    private static readonly int[][] blackTable3 = new int[][] {
new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, new int[] {-1, -1}, // 0000xx
new int[] {6, 9},                                 // 000100
new int[] {6, 8},                                 // 000101
new int[] {5, 7}, new int[] {5, 7},                         // 00011x
new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6}, new int[] {4, 6},         // 0010xx
new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5}, new int[] {4, 5},         // 0011xx
new int[] {3, 1}, new int[] {3, 1}, new int[] {3, 1}, new int[] {3, 1},         // 010xxx
new int[] {3, 1}, new int[] {3, 1}, new int[] {3, 1}, new int[] {3, 1},
new int[] {3, 4}, new int[] {3, 4}, new int[] {3, 4}, new int[] {3, 4},         // 011xxx
new int[] {3, 4}, new int[] {3, 4}, new int[] {3, 4}, new int[] {3, 4},
new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3},         // 10xxxx
new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3},
new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3},
new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3}, new int[] {2, 3},
new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2},         // 11xxxx
new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2},
new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2},
new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2}, new int[] {2, 2}
};
    private readonly ParsingContext? ctx;
    private readonly int encoding;
    private bool eoline;
    private bool byteAlign;
    private int columns;
    private int rows;
    private bool black;
    private int inputBits;
    private int outputBits;
    private int inputBuf;
    private bool eoblock;
    private bool err;
    private int codingPos;
    private readonly int[] codingLine;
    private bool eof;
    private bool rowsDone;
    private int[] refLine;
    private bool nextLine2D;
    private int row;

    public CCITTStream(int? k, bool? endofLine, bool? endOfBlock, bool? blackIs1, bool? encodedByteAlign, int? columns,
        int? rows, Stream inner, ParsingContext? ctx) : base(inner)
    {
        this.ctx = ctx;
        encoding = k ?? 0;
        eoline = endofLine ?? false;
        byteAlign = encodedByteAlign ?? false;
        this.columns = columns ?? 1728;
        this.rows = rows ?? 0;
        this.eoblock = endOfBlock ?? true;
        black = blackIs1 ?? false;
        codingLine = new int[this.columns + 1];
        refLine = new int[this.columns + 2];
        this.codingLine[0] = this.columns;
        this.codingPos = 0;
        this.row = 0;
        this.nextLine2D = this.encoding < 0;
        this.inputBits = 0;
        this.inputBuf = 0;
        this.rowsDone = false;

        int code1;
        while ((code1 = this._lookBits(12)) == 0)
        {
            this._eatBits(1);
        }
        if (code1 == 1)
        {
            this._eatBits(12);
        }
        if (this.encoding > 0)
        {
            this.nextLine2D = (this._lookBits(1) == 0);
            this._eatBits(1);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var c = readNextChar();
            if (c == -1)
            {
                return i;
            }
            buffer[offset + i] = (byte)c;
        }
        return count;
    }

    int readNextChar()
    {
        if (this.eof)
        {
            return -1;
        }
        var refLine = this.refLine;
        var codingLine = this.codingLine;
        var columns = this.columns;

        int refPos, blackPixels, bits, i;

        if (this.outputBits == 0)
        {
            if (this.rowsDone)
            {
                this.eof = true;
            }
            if (this.eof)
            {
                return -1;
            }
            this.err = false;

            int code1, code2, code3;
            if (this.nextLine2D)
            {
                for (i = 0; codingLine[i] < columns; ++i)
                {
                    refLine[i] = codingLine[i];
                }
                refLine[i++] = columns;
                refLine[i] = columns;
                codingLine[0] = 0;
                this.codingPos = 0;
                refPos = 0;
                blackPixels = 0;

                while (codingLine[this.codingPos] < columns)
                {
                    code1 = this._getTwoDimCode();
                    switch (code1)
                    {
                        case twoDimPass:
                            this._addPixels(refLine[refPos + 1], blackPixels);
                            if (refLine[refPos + 1] < columns)
                            {
                                refPos += 2;
                            }
                            break;
                        case twoDimHoriz:
                            code1 = code2 = 0;
                            if (blackPixels != 0)
                            {
                                do
                                {
                                    code1 += code3 = this._getBlackCode();
                                } while (code3 >= 64);
                                do
                                {
                                    code2 += code3 = this._getWhiteCode();
                                } while (code3 >= 64);
                            }
                            else
                            {
                                do
                                {
                                    code1 += code3 = this._getWhiteCode();
                                } while (code3 >= 64);
                                do
                                {
                                    code2 += code3 = this._getBlackCode();
                                } while (code3 >= 64);
                            }
                            this._addPixels(codingLine[this.codingPos] + code1, blackPixels);
                            if (codingLine[this.codingPos] < columns)
                            {
                                this._addPixels(
                                  codingLine[this.codingPos] + code2,
                                  blackPixels ^ 1
                                );
                            }
                            while (
                              refLine[refPos] <= codingLine[this.codingPos] &&
                              refLine[refPos] < columns
                            )
                            {
                                refPos += 2;
                            }
                            break;
                        case twoDimVertR3:
                            this._addPixels(refLine[refPos] + 3, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                ++refPos;
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVertR2:
                            this._addPixels(refLine[refPos] + 2, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                ++refPos;
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVertR1:
                            this._addPixels(refLine[refPos] + 1, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                ++refPos;
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVert0:
                            this._addPixels(refLine[refPos], blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                ++refPos;
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVertL3:
                            this._addPixelsNeg(refLine[refPos] - 3, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                if (refPos > 0)
                                {
                                    --refPos;
                                }
                                else
                                {
                                    ++refPos;
                                }
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVertL2:
                            this._addPixelsNeg(refLine[refPos] - 2, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                if (refPos > 0)
                                {
                                    --refPos;
                                }
                                else
                                {
                                    ++refPos;
                                }
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case twoDimVertL1:
                            this._addPixelsNeg(refLine[refPos] - 1, blackPixels);
                            blackPixels ^= 1;
                            if (codingLine[this.codingPos] < columns)
                            {
                                if (refPos > 0)
                                {
                                    --refPos;
                                }
                                else
                                {
                                    ++refPos;
                                }
                                while (
                                  refLine[refPos] <= codingLine[this.codingPos] &&
                                  refLine[refPos] < columns
                                )
                                {
                                    refPos += 2;
                                }
                            }
                            break;
                        case ccittEOF:
                            this._addPixels(columns, 0);
                            this.eof = true;
                            break;
                        default:
                            ctx?.Error("bad 2d code in ccitt");
                            this._addPixels(columns, 0);
                            this.err = true;
                            break;
                    }
                }
            }
            else
            {
                codingLine[0] = 0;
                this.codingPos = 0;
                blackPixels = 0;
                while (codingLine[this.codingPos] < columns)
                {
                    code1 = 0;
                    if (blackPixels != 0)
                    {
                        do
                        {
                            code1 += code3 = this._getBlackCode();
                        } while (code3 >= 64);
                    }
                    else
                    {
                        do
                        {
                            code1 += code3 = this._getWhiteCode();
                        } while (code3 >= 64);
                    }
                    this._addPixels(codingLine[this.codingPos] + code1, blackPixels);
                    blackPixels ^= 1;
                }
            }

            var gotEOL = false;

            if (this.byteAlign)
            {
                this.inputBits &= ~7;
            }

            if (!this.eoblock && this.row == this.rows - 1)
            {
                this.rowsDone = true;
            }
            else
            {
                code1 = this._lookBits(12);
                if (this.eoline)
                {
                    while (code1 != ccittEOF && code1 != 1)
                    {
                        this._eatBits(1);
                        code1 = this._lookBits(12);
                    }
                }
                else
                {
                    while (code1 == 0)
                    {
                        this._eatBits(1);
                        code1 = this._lookBits(12);
                    }
                }
                if (code1 == 1)
                {
                    this._eatBits(12);
                    gotEOL = true;
                }
                else if (code1 == ccittEOF)
                {
                    this.eof = true;
                }
            }

            if (!this.eof && this.encoding > 0 && !this.rowsDone)
            {
                this.nextLine2D = this._lookBits(1) == 0;
                this._eatBits(1);
            }

            if (this.eoblock && gotEOL && this.byteAlign)
            {
                code1 = this._lookBits(12);
                if (code1 == 1)
                {
                    this._eatBits(12);
                    if (this.encoding > 0)
                    {
                        this._lookBits(1);
                        this._eatBits(1);
                    }
                    if (this.encoding >= 0)
                    {
                        for (i = 0; i < 4; ++i)
                        {
                            code1 = this._lookBits(12);
                            if (code1 != 1)
                            {
                                ctx?.Error("bad rtc code in ccitt: " + code1);
                            }
                            this._eatBits(12);
                            if (this.encoding > 0)
                            {
                                this._lookBits(1);
                                this._eatBits(1);
                            }
                        }
                    }
                    this.eof = true;
                }
            }
            else if (this.err && this.eoline)
            {
                while (true)
                {
                    code1 = this._lookBits(13);
                    if (code1 == ccittEOF)
                    {
                        this.eof = true;
                        return -1;
                    }
                    if (code1 >> 1 == 1)
                    {
                        break;
                    }
                    this._eatBits(1);
                }
                this._eatBits(12);
                if (this.encoding > 0)
                {
                    this._eatBits(1);
                    this.nextLine2D = (code1 & 1) == 0;
                }
            }

            if (codingLine[0] > 0)
            {
                this.outputBits = codingLine[(this.codingPos = 0)];
            }
            else
            {
                this.outputBits = codingLine[(this.codingPos = 1)];
            }
            this.row++;
        }

        int c;
        if (this.outputBits >= 8)
        {
            c = (this.codingPos & 1) != 0 ? 0 : 0xff;
            this.outputBits -= 8;
            if (this.outputBits == 0 && codingLine[this.codingPos] < columns)
            {
                this.codingPos++;
                this.outputBits =
                  codingLine[this.codingPos] - codingLine[this.codingPos - 1];
            }
        }
        else
        {
            bits = 8;
            c = 0;
            do
            {
                // TODO going to have out of range errors most likely
                // if (typeof this.outputBits !== "number") {
                //     throw new FormatError(
                //       'Invalid /CCITTFaxDecode data, "outputBits" must be a number.'
                //     );
                // }

                if (this.outputBits > bits)
                {
                    c <<= bits;
                    if ((this.codingPos & 1) == 0)
                    {
                        c |= 0xff >> (8 - bits);
                    }
                    this.outputBits -= bits;
                    bits = 0;
                }
                else
                {
                    c <<= this.outputBits;
                    if ((this.codingPos & 1) == 0)
                    {
                        c |= 0xff >> (8 - this.outputBits);
                    }
                    bits -= this.outputBits;
                    this.outputBits = 0;
                    if (codingLine[this.codingPos] < columns)
                    {
                        this.codingPos++;
                        this.outputBits =
                          codingLine[this.codingPos] - codingLine[this.codingPos - 1];
                    }
                    else if (bits > 0)
                    {
                        c <<= bits;
                        bits = 0;
                    }
                }
            } while (bits > 0);
        }
        if (this.black)
        {
            c ^= 0xff;
        }
        return c;
    }

    /**
     * @private
     */
    void _addPixels(int a1, int blackPixels)
    {
        var codingLine = this.codingLine;
        var codingPos = this.codingPos;

        if (a1 > codingLine[codingPos])
        {
            if (a1 > this.columns)
            {
                ctx?.Error("row is wrong length in ccitt");
                this.err = true;
                a1 = this.columns;
            }
            if (((codingPos & 1) ^ blackPixels) != 0)
            {
                ++codingPos;
            }

            codingLine[codingPos] = a1;
        }
        this.codingPos = codingPos;
    }

    /**
     * @private
     */
    void _addPixelsNeg(int a1, int blackPixels)
    {
        int[] codingLine = this.codingLine;
        int codingPos = this.codingPos;

        if (a1 > codingLine[codingPos])
        {
            if (a1 > this.columns)
            {
                ctx?.Error("row is wrong length in ccitt");
                this.err = true;
                a1 = this.columns;
            }
            if (((codingPos & 1) ^ blackPixels) != 0)
            {
                ++codingPos;
            }

            codingLine[codingPos] = a1;
        }
        else if (a1 < codingLine[codingPos])
        {
            if (a1 < 0)
            {
                ctx?.Error("invalid code in ccitt a1: " + a1);
                this.err = true;
                a1 = 0;
            }
            while (codingPos > 0 && a1 < codingLine[codingPos - 1])
            {
                --codingPos;
            }
            codingLine[codingPos] = a1;
        }

        this.codingPos = codingPos;
    }

    /**
    * This function returns the code found from the table.
    * The start and end parameters set the boundaries for searching the table.
    * The limit parameter is optional. Function returns an array with three
    * values. The first array element indicates whether a valid code is being
    * returned. The second array element is the actual code. The third array
    * element indicates whether EOF was reached.
    * @private
    */
    private (bool, int, bool) _findTableCode(int start, int end, int[][] table, int limitValue = 0)
    {
        for (var i = start; i <= end; ++i)
        {
            var code = this._lookBits(i);
            if (code == ccittEOF)
            {
                return (true, 1, false);
            }
            if (i < end)
            {
                code <<= end - i;
            }
            if (limitValue == 0 || code >= limitValue)
            {
                var p = table[code - limitValue];
                if (p[0] == i)
                {
                    this._eatBits(i);
                    return (true, p[1], true);
                }
            }
        }
        return (false, 0, false);
    }

    /**
     * @private
     */
    private int _getTwoDimCode()
    {
        var code = 0;
        int[] p;
        if (this.eoblock)
        {
            code = this._lookBits(7);
            if (twoDimTable.Length > code)
            {
                p = twoDimTable[code];
                if (p[0] > 0)
                {
                    this._eatBits(p[0]);
                    return p[1];
                }
            }
        }
        else
        {
            var result = this._findTableCode(1, 7, twoDimTable);
            if (result.Item1 && result.Item3)
            {
                return result.Item2;
            }
        }
        ctx?.Error("Bad two dim code in ccitt");
        return ccittEOF;
    }


    /**
* @private
*/
    private int _getWhiteCode()
    {
        var code = 0;
        int[] p;
        if (this.eoblock)
        {
            code = this._lookBits(12);
            if (code == ccittEOF)
            {
                return 1;
            }

            if (code >> 5 == 0)
            {
                p = whiteTable1[code];
            }
            else
            {
                p = whiteTable2[code >> 3];
            }

            if (p[0] > 0)
            {
                this._eatBits(p[0]);
                return p[1];
            }
        }
        else
        {
            var result = this._findTableCode(1, 9, whiteTable2);
            if (result.Item1)
            {
                return result.Item2;
            }

            result = this._findTableCode(11, 12, whiteTable1);
            if (result.Item1)
            {
                return result.Item2;
            }
        }
        ctx?.Error("bad white code in ccitt");
        this._eatBits(1);
        return 1;
    }

    /**
* @private
*/
    private int _getBlackCode()
    {
        int code;
        int[] p;
        if (this.eoblock)
        {
            code = this._lookBits(13);
            if (code == ccittEOF)
            {
                return 1;
            }
            if (code >> 7 == 0)
            {
                p = blackTable1[code];
            }
            else if (code >> 9 == 0 && code >> 7 != 0)
            {
                p = blackTable2[(code >> 1) - 64];
            }
            else
            {
                p = blackTable3[code >> 7];
            }

            if (p[0] > 0)
            {
                this._eatBits(p[0]);
                return p[1];
            }
        }
        else
        {
            var result = this._findTableCode(2, 6, blackTable3);
            if (result.Item1)
            {
                return result.Item2;
            }

            result = this._findTableCode(7, 12, blackTable2, 64);
            if (result.Item1)
            {
                return result.Item2;
            }

            result = this._findTableCode(10, 13, blackTable1);
            if (result.Item1)
            {
                return result.Item2;
            }
        }

        ctx?.Error("bad black code in ccitt");
        this._eatBits(1);
        return 1;
    }

    /**
* @private
*/
    private int _lookBits(int n)
    {
        int c;
        while (this.inputBits < n)
        {
            if ((c = this.inner.ReadByte()) == -1)
            {
                if (this.inputBits == 0)
                {
                    return ccittEOF;
                }
                return (this.inputBuf << (n - this.inputBits)) & (0xffff >> (16 - n));
            }
            this.inputBuf = (this.inputBuf << 8) | c;
            this.inputBits += 8;
        }
        return (this.inputBuf >> (this.inputBits - n)) & (0xffff >> (16 - n));
    }

    /**
     * @private
     */
    void _eatBits(int n)
    {
        if ((this.inputBits -= n) < 0)
        {
            this.inputBits = 0;
        }
    }
}
