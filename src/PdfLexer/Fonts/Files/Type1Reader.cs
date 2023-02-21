using PdfLexer.Lexing;
using System.Buffers;
using System.Buffers.Text;

namespace PdfLexer.Fonts.Files;

// Type1Reader PORTED FROM PDF.JS, PDF.JS is licensed as follows:
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

/// <summary>
/// 
/// 
/// </summary>
internal ref struct Type1Reader
{
    private const int EEXEC_ENCRYPT_KEY = 55665;
    private const int CHAR_STRS_ENCRYPT_KEY = 4330;
    private const int PFB_HEADER_SIZE = 6;

    
    private readonly ParsingContext Ctx;
    private ReadOnlySpan<byte> Data;
    private byte[]? rented;
    public Type1Reader(ParsingContext ctx, ReadOnlySpan<byte> data)
    {
        // we only care about encoding so don't need to worry about encryption etc
        Ctx = ctx;
        Data = data;
        var pfbHeaderPresent = data[0] == 0x80 && data[1] == 0x01;
        if (pfbHeaderPresent)
        {
            Data = data.Slice(PFB_HEADER_SIZE);
        }
        rented = null;
    }

    private static byte[] def = new byte[] { (byte)'d', (byte)'e', (byte)'f' };
    private static byte[] dup = new byte[] { (byte)'d', (byte)'u', (byte)'p' };
    public bool TryGetEncoding([NotNullWhen(true)]out string?[]? names)
    {
        var scanner = new Scanner(Ctx, Data);
        PdfTokenType type;
        while ((type = scanner.Peek()) != PdfTokenType.NameObj || scanner.GetCurrentObject().GetAs<PdfName>().Value != "Encoding")
        {
            scanner.SkipCurrent();
        }
        var nxt = scanner.Peek();
        if (nxt == PdfTokenType.NameObj)
        {
            var result = Encodings.GetEncoding(scanner.GetCurrentObject().GetAs<PdfName>());
            if (result == null)
            {
                names = null;
                return false;
            }
            names = result;
            return true;
        }
        var encoding = new string?[256];
        var current = scanner.GetCurrentData();

        while (!current.SequenceEqual(def) && !current.SequenceEqual(dup))
        {
            scanner.SkipCurrent();
            current = scanner.GetCurrentData();
        }

        if (current.SequenceEqual(def))
        {
            names = null;
            return false;
        }

        scanner.SkipCurrent();

        int num = -1;
        while (true)
        {
            nxt = scanner.Peek();
            if (nxt == PdfTokenType.NameObj)
            {
                if (num != -1 && num < 256)
                {
                    encoding[num] = scanner.GetCurrentObject()?.GetAs<PdfName>()?.Value;
                }
            } else if (nxt == PdfTokenType.NumericObj)
            {
                num = scanner.GetCurrentObject().GetAs<PdfNumber>();
            } else if (nxt == PdfTokenType.EOS || scanner.GetCurrentData().SequenceEqual(def))
            {
                break;
            } else
            {
                scanner.SkipCurrent();
            }
        }
        names = encoding;
        return true;
    }

    private void Read(ReadOnlySpan<byte> data)
    {
        var isBinary = !(
        (isHexDigit(data[0]) || CommonUtil.IsWhiteSpace(data[0])) &&
            isHexDigit(data[1]) &&
            isHexDigit(data[2]) &&
            isHexDigit(data[3]) &&
            isHexDigit(data[4]) &&
            isHexDigit(data[5]) &&
            isHexDigit(data[6]) &&
            isHexDigit(data[7])
        );
        Data = isBinary ? decrypt(data, EEXEC_ENCRYPT_KEY, 4) : decryptASCII(data, EEXEC_ENCRYPT_KEY, 4);
    }


    public static bool IsType1File(ReadOnlySpan<byte> data)
    {
        // All Type1 font programs must begin with the comment '%!' (0x25 + 0x21).
        if (data[0] == 0x25 && data[1] == 0x21)
        {
            return true;
        }
        // ... obviously some fonts violate that part of the specification,
        // please refer to the comment in |Type1Font| below (pfb file header).
        if (data[0] == 0x80 && data[1] == 0x01)
        {
            return true;
        }
        return false;
    }


    private static bool isHexDigit(byte code)
    {
        return (
          (code >= 48 && code <= 57) || // '0'-'9'
          (code >= 65 && code <= 70) || // 'A'-'F'
          (code >= 97 && code <= 102) // 'a'-'f'
        );
    }

    private static bool isSpecial(byte c)
    {
        return (
          c == /* '/' = */ 0x2f ||
          c == /* '[' = */ 0x5b ||
          c == /* ']' = */ 0x5d ||
          c == /* '{' = */ 0x7b ||
          c == /* '}' = */ 0x7d ||
          c == /* '(' = */ 0x28 ||
          c == /* ')' = */ 0x29
        );
    }

    private ReadOnlySpan<byte> decrypt(ReadOnlySpan<byte> data, int? key, int discardNumber)
    {
        if (discardNumber >= data.Length)
        {
            return new ReadOnlySpan<byte>();
        }

        int c1 = 52845,
            c2 = 22719;

        int r = key ?? 0, i = 0, j = 0;
        for (i = 0; i < discardNumber; i++)
        {
            r = ((data[i] + r) * c1 + c2) & ((1 << 16) - 1);
        }
        var count = data.Length - discardNumber;
        rented = ArrayPool<byte>.Shared.Rent(count);
        Span<byte> decrypted = rented;
        decrypted = decrypted.Slice(0, count);
        for (i = discardNumber, j = 0; j < count; i++, j++)
        {
            var value = data[i];
            decrypted[j] = (byte)(value ^ (r >> 8));
            r = ((value + r) * c1 + c2) & ((1 << 16) - 1);
        }
        return decrypted;
    }

    private ReadOnlySpan<byte> decryptASCII(ReadOnlySpan<byte> data, int? key, int discardNumber)
    {
        Span<byte> hexBuffer = stackalloc byte[2];
        int c1 = 52845,
            c2 = 22719;
        int r = key ?? 0;
        var count = data.Length;
        var maybeLength = (uint)(count >> 1);

        rented = ArrayPool<byte>.Shared.Rent(count);
        Span<byte> decrypted = rented;
        decrypted = decrypted.Slice(0, count);

        int i = 0, j = 0;
        for (i = 0, j = 0; i < count; i++)
        {
            var digit1 = data[i];
            if (!isHexDigit(digit1))
            {
                continue;
            }
            i++;
            byte digit2;
            while (i < count && !isHexDigit((digit2 = data[i])))
            {
                i++;
            }
            if (i < count)
            {
                if (!Utf8Parser.TryParse(hexBuffer, out byte value, out int consumed, 'x'))
                {
                    Ctx.Error(CommonUtil.DisplayDataError(hexBuffer, i, "Bad hex string in embedded typ1 font."));
                }

                decrypted[j++] = (byte)(value ^ (r >> 8));
                r = ((value + r) * c1 + c2) & ((1 << 16) - 1);
            }
        }
        return decrypted.Slice(discardNumber, j);
    }
}
