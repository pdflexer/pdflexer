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
        prevbb = null;
        prevPt = null;
        returnedBounds = null;
        returnedPositionBounds = null;
        currentBounds = null;
        currentPositionBounds = null;
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
    private PdfRect<double>? prevbb;
    private PdfPoint<double>? prevPt;
    private PdfRect<double>? returnedBounds;
    private PdfRect<double>? returnedPositionBounds;
    private PdfRect<double>? currentBounds;
    private PdfRect<double>? currentPositionBounds;
    private double pw;
    private GfxMatrix<double> prev;
    private GfxMatrix<double> prevTTM;
    private GfxMatrix<double> lastTTM;
    private readonly StringBuilder sb;

    public PdfRect<double> GetWordBoundingBox()
    {
        return returnedBounds ?? new PdfRect<double> { LLx = 0, LLy = 0, URx = 0, URy = 0 };
    }

    public PdfRect<double> GetWordPositionBox()
    {
        return returnedPositionBounds ?? new PdfRect<double> { LLx = 0, LLy = 0, URx = 0, URy = 0 };
    }

    public bool Advance()
    {
        returnedBounds = null;
        returnedPositionBounds = null;
        bool returnWord = false;
        while (Scanner.Advance())
        {
            var vert = Scanner.GraphicsState.Font?.IsVertical ?? false;
            var current = Scanner.GraphicsState.Text.TextMatrix;
            
            if (currentBounds == null)
            {
                Position = Scanner.GraphicsState.Text.TextRenderingMatrix;
            }
            else
            {
                if (prev.A != current.A || prev.B != current.B
                || prev.C != current.C || prev.D != current.D)
                {
                    FinalizeCurrentWord();
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
                        FinalizeCurrentWord();
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
                        FinalizeCurrentWord();
                        prevbb = null;
                        prevPt = null;
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            AppendBounds(Scanner.GetCurrentBoundingBox(), Scanner.GetCurrentTextPoint());
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
            FinalizeCurrentWord();
            return true;
        }

        return false;
    }

    private void FinalizeCurrentWord()
    {
        CurrentWord = sb.ToString();
        sb.Clear();
        returnedBounds = currentBounds;
        returnedPositionBounds = currentPositionBounds;
        currentBounds = null;
        currentPositionBounds = null;
        lastTTM = prevTTM;
    }

    private void AppendBounds(PdfRect<double> box, PdfPoint<double> point)
    {
        currentBounds = currentBounds == null
            ? box
            : new PdfRect<double>
            {
                LLx = Math.Min(currentBounds.LLx, box.LLx),
                LLy = Math.Min(currentBounds.LLy, box.LLy),
                URx = Math.Max(currentBounds.URx, box.URx),
                URy = Math.Max(currentBounds.URy, box.URy)
            };

        var pointRect = new PdfRect<double>
        {
            LLx = point.X,
            LLy = point.Y,
            URx = point.X,
            URy = point.Y
        };

        currentPositionBounds = currentPositionBounds == null
            ? pointRect
            : new PdfRect<double>
            {
                LLx = Math.Min(currentPositionBounds.LLx, pointRect.LLx),
                LLy = Math.Min(currentPositionBounds.LLy, pointRect.LLy),
                URx = Math.Max(currentPositionBounds.URx, pointRect.URx),
                URy = Math.Max(currentPositionBounds.URy, pointRect.URy)
            };
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
