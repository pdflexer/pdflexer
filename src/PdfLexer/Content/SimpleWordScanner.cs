using PdfLexer.DOM;
using System.Text;

namespace PdfLexer.Content;

public struct WordInfo
{
    public string Text;
    public PdfRect<double> BoundingBox;
}

/// <summary>
/// Returns words from PDF by scanning chars sequentially as they are written in the PDF
/// and splitting into words if spacing between chars is significant or a word delimiting
/// character is encountered.
/// </summary>
public ref struct SimpleWordScanner
{
    public static List<string> GetWords(ParsingContext ctx, PdfPage page, HashSet<char>? wordDelimiters = null)
    {
        var words = new List<string>();
        var reader = new SimpleWordScanner(ctx, page, wordDelimiters);
        while (reader.Advance())
        {
            words.Add(reader.CurrentWord);
        }
        return words;
    }
    public SimpleWordScanner(ParsingContext ctx, PdfPage page, HashSet<char>? wordDelimiters = null)
    {
        Page = page;
        Scanner = new TextScanner(ctx, page);
        CurrentWord = "";
        Position = default;
        sb = new StringBuilder();
        last = null;
        lastPt = null;
        first = null;
        firstPt = null;
        prevbb = null;
        prevPt = null;
        prev = default;
        pw = 0;
        customDelims = wordDelimiters;
    }


    public PdfPage Page { get; }
    public GfxState<double> GraphicsState { get => Scanner.GraphicsState; }
    public string CurrentWord { get; private set; }
    public GfxMatrix<double> Position { get; private set; }

    private TextScanner Scanner;
    private HashSet<char>? customDelims;
    private PdfRect<double>? last;
    private PdfPoint<double>? lastPt;
    private PdfRect<double>? first;
    private PdfPoint<double>? firstPt;
    private PdfRect<double>? prevbb;
    private PdfPoint<double>? prevPt;
    private double pw;
    private GfxMatrix<double> prev;
    private GfxMatrix<double> prevTTM;
    private GfxMatrix<double> lastTTM;
    private readonly StringBuilder sb;

    public PdfRect<double> GetWordBoundingBox()
    {
        return CurrentWord.Length == 1 ?
         new PdfRect<double>
         {
             LLx = (first?.LLx ?? 0),
             LLy = (first?.LLy ?? 0),
             URx = (first?.URx ?? 0),
             URy = (first?.URy ?? 0)
         } : new PdfRect<double>
         {
             LLx = (first?.LLx ?? 0),
             LLy = (first?.LLy ?? 0),
             URx = (last?.URx ?? 0),
             URy = (last?.URy ?? 0)
         };
    }

    public PdfRect<double> GetWordPositionBox()
    {
        return CurrentWord.Length == 1 ?
         new PdfRect<double>
         {
             LLx = (firstPt?.X ?? 0),
             LLy = (firstPt?.Y ?? 0),
             URx = (firstPt?.X ?? 0),
             URy = (firstPt?.Y ?? 0)
         } : new PdfRect<double>
         {
             LLx = (firstPt?.X ?? 0),
             LLy = (firstPt?.Y ?? 0),
             URx = (lastPt?.X ?? 0),
             URy = (lastPt?.Y ?? 0)
         };
    }

    public bool Advance()
    {
        last = null;
        lastPt = null;
        first = prevbb;
        firstPt = prevPt;
        bool returnWord = false;
        while (Scanner.Advance())
        {
            var vert = Scanner.GraphicsState.Font?.IsVertical ?? false;
            var current = Scanner.GraphicsState.Text.TextMatrix;
            
            if (first == null)
            {
                Position = Scanner.GraphicsState.Text.TextRenderingMatrix;
                first ??= Scanner.GetCurrentBoundingBox();
                firstPt ??= Scanner.GetCurrentTextPoint();
            }
            else
            {
                if (prev.A != current.A || prev.B != current.B
                || prev.C != current.C || prev.D != current.D)
                {
                    CurrentWord = sb.ToString();
                    sb.Clear();
                    last = prevbb;
                    lastPt = prevPt;
                    lastTTM = prevTTM;
                    returnWord = true;
                }
                else
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
                        lastPt = prevPt;
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
                        lastPt = prevPt;
                        lastTTM = prevTTM;
                        prevbb = null;
                        prevPt = null;
                        return true;
                    }
                    else
                    {
                        first = null;
                        firstPt = null;
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
            prevPt = Scanner.GetCurrentTextPoint();
            prevTTM = Scanner.GraphicsState.Text.TextMatrix;
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
            lastPt = prevPt;
            lastTTM = prevTTM;
            return true;
        }

        return false;
    }

    public WordInfo GetInfo()
    {
        var pos = GetWordBoundingBox();
        return new WordInfo
        {
            Text = CurrentWord,
            BoundingBox = pos,
        };
    }

    public GfxMatrix<double> GetCurrentWordMatrix() => lastTTM;
}
