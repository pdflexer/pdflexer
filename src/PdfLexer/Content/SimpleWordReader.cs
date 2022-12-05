using PdfLexer.DOM;
using PdfLexer.Parsers;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content;

public struct WordInfo
{
    public string word;
    public float llx;
    public float lly;
    public float urx;
    public float ury;
}

/// <summary>
/// Returns words from PDF by scanning chars sequentially as they are written in the PDF
/// and splitting into words if spacing between chars is significant or a word delimiting
/// character is encountered.
/// </summary>
public ref struct SimpleWordReader
{
    public SimpleWordReader(ParsingContext ctx, PdfPage page, HashSet<char>? wordDelimiters = null)
    {
        Page = page;
        Scanner = new TextScanner(ctx, page);
        CurrentWord = "";
        Position = default;
        sb = new StringBuilder();
        last = null;
        first = null;
        prevbb = null;
        prev = default;
        pw = 0;
        customDelims = wordDelimiters;
    }

    private TextScanner Scanner;
    private HashSet<char>? customDelims;
    public PdfPage Page { get; }


    public string CurrentWord { get; private set; }
    public Matrix4x4 Position { get; private set; }
    private (float llx, float lly, float urx, float ury)? last;
    private (float llx, float lly, float urx, float ury)? first;
    private (float llx, float lly, float urx, float ury)? prevbb;
    private float pw;
    private Matrix4x4 prev;
    private readonly StringBuilder sb;

    public (float llx, float lly, float urx, float ury) GetWordBoundingBox()
    {
        return (first?.llx ?? 0, first?.lly ?? 0, last?.urx ?? 0, last?.ury ?? 0);
    }

    public bool Advance()
    {
        last = null;
        first = prevbb;
        bool returnWord = false;
        while (Scanner.Advance())
        {
            var vert = Scanner.GraphicsState.Font?.IsVertical ?? false;
            var current = Scanner.TextState.TextMatrix;
            if (first == null)
            {
                Position = current;
                first ??= Scanner.GetCurrentBoundingBox();
            }
            else
            {
                if (prev.M11 != current.M11 || prev.M12 != current.M12
                || prev.M21 != current.M21 || prev.M22 != current.M22)
                {
                    CurrentWord = sb.ToString();
                    sb.Clear();
                    last = prevbb;
                    returnWord = true;
                } else
                {
                    Matrix4x4.Invert(prev, out var iv);
                    var change = current * iv;
                    var dw = vert ? change.M32 : change.M31;
                    var dh = vert ? change.M31 : change.M32;
                    if (Math.Abs(pw - dw) > 0.25 * pw || Math.Abs(dh) > pw * 0.25)
                    {
                        CurrentWord = sb.ToString();
                        sb.Clear();
                        last = prevbb;
                        returnWord = true;
                    }
                }
            }

            if (Scanner.Glyph == null)
            {
                break;
            }

            if (Scanner.Glyph.MultiChar != null)
            {
                sb.Append(Scanner.Glyph.MultiChar);
            }
            else
            {
                var c = Scanner.Glyph.Char;
                if ((customDelims != null && customDelims.Contains(c))
                    || (customDelims == null && (c == ' ' || c == ',' || c == '\t' || c == ':' || c == ';')))
                {
                    if (sb.Length > 0)
                    {
                        CurrentWord = sb.ToString();
                        sb.Clear();
                        last = prevbb;
                        prevbb = null;
                        return true;
                    }
                    else
                    {
                        first = null;
                        continue;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            pw = vert ? Scanner.Glyph.w1 : Scanner.Glyph.w0;
            pw = pw * Scanner.GraphicsState.FontSize + Scanner.GraphicsState.CharSpacing;
            prev = current;
            prevbb = Scanner.GetCurrentBoundingBox();
            if (returnWord)
            {
                if (CurrentWord.Length > 0)
                {
                    return true;
                }
            }
        }

        if (sb.Length > 0)
        {
            CurrentWord = sb.ToString();
            sb.Clear();
            last = prevbb;
            return true;
        }

        return false;
    }

    public WordInfo GetInfo()
    {
        var pos = GetWordBoundingBox();
        return new WordInfo
        {
            word = CurrentWord,
            llx = pos.llx,
            lly = pos.lly,
            urx = pos.urx,
            ury = pos.ury
        };
    }
}
