using PdfLexer.CMaps;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.Fonts
{
    internal class CMapReader
    {
        internal enum ToUnicodeState
        {
            None,
            ReadChars,
            ReadRange,
            ReadSpace
        }

        private static byte[] spaceStart = Encoding.ASCII.GetBytes("begincodespacerange");
        private static byte[] spaceEnd = Encoding.ASCII.GetBytes("endcodespacerange");
        private static byte[] charStart = Encoding.ASCII.GetBytes("beginbfchar");
        private static byte[] charEnd = Encoding.ASCII.GetBytes("endbfchar");
        private static byte[] rangeStart = Encoding.ASCII.GetBytes("beginbfrange");
        private static byte[] rangeEnd = Encoding.ASCII.GetBytes("endbfrange");
        private static UnicodeEncoding ucBO = new UnicodeEncoding(true, true, false);
        private static UnicodeEncoding uc = new UnicodeEncoding(true, false, false);
        public static (List<CRange> Ranges, List<Glyph> Glyphs) GetGlyphsFromToUnicode(ParsingContext ctx, ReadOnlySpan<byte> data)
        {
            var ranges = new List<CRange>();
            var glyphs = new List<Glyph>();
            
            ToUnicodeState state = ToUnicodeState.None;
            Span<byte> buffer = stackalloc byte[256];
            var chars = new char[1];
            int bufferUsed = 0;
            int pos = 0;
            var scanner = new Scanner(ctx, data);

            PdfTokenType type = PdfTokenType.Unknown;
            
            while ((type = scanner.Peek()) != PdfTokenType.EOS)
            {
                if (type == PdfTokenType.Unknown)
                {
                    var token = scanner.GetCurrentData();
                    if (token.SequenceEqual(charStart))
                    {
                        state = ToUnicodeState.ReadChars;
                    }
                    else if (token.SequenceEqual(charEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    else if (token.SequenceEqual(rangeStart))
                    {
                        state = ToUnicodeState.ReadRange;
                    }
                    else if (token.SequenceEqual(rangeEnd))
                    {
                        state = ToUnicodeState.None;
                    } else if (token.SequenceEqual(spaceStart))
                    {
                        state = ToUnicodeState.ReadSpace;
                    }
                    else if (token.SequenceEqual(spaceEnd))
                    {
                        state = ToUnicodeState.None;
                    }
                    scanner.SkipCurrent();
                    continue;
                }

                if (state == ToUnicodeState.ReadChars)
                {
                    var g = new Glyph();

                    // c1
                    var token = scanner.GetCurrentData();
                    g.CodePoint = ReadCodePoint(ctx, token, buffer);
                    g.Bytes = (token.Length - 2) / 2;
                    scanner.SkipCurrent();

                    // vals ->
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    switch (type)
                    {
                        case PdfTokenType.StringStart:
                            AddStringVal(ctx, g, token, buffer);
                            break;
                        case PdfTokenType.NameObj:
                            // todo lookup fallback for postscript names (not technically allowed in tounicode)
                            break;
                    }

                    glyphs.Add(g);
                } else if (state == ToUnicodeState.ReadRange)
                {
                    // c1
                    var token = scanner.GetCurrentData();
                    var bytes = (token.Length - 2) / 2; 
                    var cp1 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();
                    // c2
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var cp2 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();


                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var ct = (int)(cp2 - cp1);
                    switch (type)
                    {
                        case PdfTokenType.StringStart:
                            bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);

                            for (var c = 0; c <= ct; c++)
                            {
                                var g = new Glyph();
                                g.CodePoint = cp1 + (uint)c;
                                g.Bytes = bytes;
                                AddStringVal(g, buffer.Slice(0, bufferUsed));
                                glyphs.Add(g);
                                buffer[bufferUsed - 1] = (byte)(buffer[bufferUsed - 1] + 1);
                            }
                            break;
                        case PdfTokenType.ArrayStart:
                            for (var c = 0; c <= ct; c++)
                            {
                                scanner.SkipCurrent();
                                type = scanner.Peek();
                                token = scanner.GetCurrentData();
                                bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
                                var g = new Glyph();
                                g.Bytes = bytes;
                                g.CodePoint = cp1 + (uint)c;
                                AddStringVal(g, buffer.Slice(0, bufferUsed));
                                glyphs.Add(g);
                            }
                            scanner.SkipCurrent(); // ]
                            break;
                    }
                    // cpstart cpend string
                    // foreach start to end, string last byte +1

                    // cpstart cpend [string...]
                }
                else if (state == ToUnicodeState.ReadSpace)
                {
                    // c1
                    var token = scanner.GetCurrentData();
                    var bytes = (token.Length - 2) / 2;
                    var cp1 = ReadCodePoint(ctx, token, buffer);
                    scanner.SkipCurrent();
                    // c2
                    type = scanner.Peek();
                    token = scanner.GetCurrentData();
                    var cp2 = ReadCodePoint(ctx, token, buffer);
                    ranges.Add(new CRange
                    {
                        Start = cp1,
                        End = cp2,
                        Bytes = bytes
                    });
                }
                scanner.SkipCurrent();
            }

            return (ranges, glyphs);
        }

        private static uint ReadCodePoint(ParsingContext ctx, ReadOnlySpan<byte> token, Span<byte> buffer)
        {
            int bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);

            uint cp = buffer[0];
            for (var i = 1; i < bufferUsed; i++)
            {
                cp = cp << 8 | buffer[i];
            }
            return cp;
        }

        private static void AddStringVal(ParsingContext ctx, Glyph g, ReadOnlySpan<byte> token, Span<byte> buffer)
        {
            int bufferUsed = ctx.StringParser.ConvertBytes(token, buffer);
            string value = null;
            if (bufferUsed > 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                value = ucBO.GetString(buffer.Slice(0, bufferUsed));
            }
            else
            {
                value = uc.GetString(buffer.Slice(0, bufferUsed));
            }

            if (value.Length == 1)
            {
                g.Char = value[0];
            }
            else
            {
                g.MultiChar = value;
            }
        }
        private static void AddStringVal(Glyph g, Span<byte> buffer)
        {
            string value = null;
            if (buffer.Length > 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                value = ucBO.GetString(buffer);
            }
            else
            {
                value = uc.GetString(buffer);
            }

            if (value.Length == 1)
            {
                g.Char = value[0];
            }
            else
            {
                g.MultiChar = value;
            }
        }
    }
}