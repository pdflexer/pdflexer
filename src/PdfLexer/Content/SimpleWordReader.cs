using PdfLexer.DOM;
using System.Numerics;
using System.Text;

namespace PdfLexer.Content;

public struct WordInfo
{
    public string word;
    public double llx;
    public double lly;
    public double urx;
    public double ury;
}

/// <summary>
/// Returns words from PDF by scanning chars sequentially as they are written in the PDF
/// and splitting into words if spacing between chars is significant or a word delimiting
/// character is encountered.
/// </summary>
public ref struct SimpleWordReader
{
    public static List<string> GetWords(ParsingContext ctx, PdfPage page, HashSet<char>? wordDelimiters = null)
    {
        var words = new List<string>();
        var reader = new SimpleWordReader(ctx, page, wordDelimiters);
        while (reader.Advance())
        {
            words.Add(reader.CurrentWord);
        }
        return words;
    }
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
    public GfxMatrix<double> Position { get; private set; }
    private PdfRect<double>? last;
    private PdfRect<double>? first;
    private PdfRect<double>? prevbb;
    private double pw;
    private GfxMatrix<double> prev;
    private readonly StringBuilder sb;

    public (double llx, double lly, double urx, double ury) GetWordBoundingBox()
    {
        return (first?.LLx ?? 0, first?.LLy ?? 0, last?.URx ?? 0, last?.URy ?? 0);
    }

    public bool Advance()
    {
        last = null;
        first = prevbb;
        bool returnWord = false;
        while (Scanner.Advance())
        {
            var vert = Scanner.GraphicsState.Font?.IsVertical ?? false;
            var current = Scanner.GraphicsState.Text.TextMatrix;
            if (first == null)
            {
                Position = current;
                first ??= Scanner.GetCurrentBoundingBox();
            }
            else
            {
                if (prev.A != current.A || prev.B != current.B
                || prev.C != current.C || prev.D != current.D)
                {
                    CurrentWord = sb.ToString();
                    sb.Clear();
                    last = prevbb;
                    returnWord = true;
                } else
                {
                    prev.Invert(out var iv);
                    var change = current * iv;
                    var dw = vert ? change.F : change.E;
                    var dh = vert ? change.E : change.F;
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
