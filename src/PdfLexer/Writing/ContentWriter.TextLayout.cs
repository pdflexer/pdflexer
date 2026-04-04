using System.Numerics;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;

namespace PdfLexer.Writing;

public partial class ContentWriter<T> where T : struct, IFloatingPoint<T>
{
    public TextBoxLayoutResult MeasureTextBox(TextBoxLayoutRequest request)
    {
        var engine = new TextBoxLayoutEngine();
        return engine.Layout(request);
    }

    public TextBoxLayoutResult WriteTextBox(PdfRect<T> area, TextBoxLayoutRequest request, PdfTextLayoutFontLibrary fontLibrary)
    {
        ArgumentNullException.ThrowIfNull(fontLibrary);
        var engine = new TextBoxLayoutEngine();
        var layout = engine.Layout(request);
        return WriteTextBox(area, layout, fontLibrary);
    }

    public TextBoxLayoutResult WriteTextBox(PdfRect<T> area, TextBoxLayoutResult layout, PdfTextLayoutFontLibrary fontLibrary)
    {
        ArgumentNullException.ThrowIfNull(fontLibrary);
        ArgumentNullException.ThrowIfNull(layout);

        if (layout.Status == TextLayoutStatus.Error)
        {
            return layout;
        }

        var underlines = new List<(double X1, double Y, double X2, double Thickness)>();

        BeginText();
        foreach (var line in layout.Lines)
        {
            foreach (var run in line.Runs)
            {
                if (string.IsNullOrEmpty(run.Text))
                {
                    continue;
                }

                if (!fontLibrary.TryGetWritableFont(run.FaceId, out var writableFont) || writableFont == null)
                {
                    throw new PdfLexerException($"No writable font was found for face '{run.FaceId}'.");
                }

                Font(writableFont, FPC<T>.Util.FromDouble<T>(run.FontSize), false);
                CharSpacing(FPC<T>.Util.FromDouble<T>(run.CharacterSpacing));
                WordSpacing(FPC<T>.Util.FromDouble<T>(run.WordSpacing));

                var x = FPC<T>.Util.FromDouble<T>(FPC<T>.Util.ToDouble(area.LLx) + line.X + run.X);
                var y = FPC<T>.Util.FromDouble<T>(FPC<T>.Util.ToDouble(area.URy) - run.BaselineY);
                TextMove(x, y);
                Text(run.Text);

                if (run.Underline && run.MeasuredWidth > 0)
                {
                    var underlineY = FPC<T>.Util.ToDouble(area.URy) - run.BaselineY - (run.FontSize * 0.12d);
                    var startX = FPC<T>.Util.ToDouble(area.LLx) + line.X + run.X;
                    underlines.Add((startX, underlineY, startX + run.MeasuredWidth, Math.Max(0.5d, run.FontSize * 0.05d)));
                }
            }
        }

        EndText();

        foreach (var underline in underlines)
        {
            LineWidth(FPC<T>.Util.FromDouble<T>(underline.Thickness));
            MoveTo(FPC<T>.Util.FromDouble<T>(underline.X1), FPC<T>.Util.FromDouble<T>(underline.Y));
            LineTo(FPC<T>.Util.FromDouble<T>(underline.X2), FPC<T>.Util.FromDouble<T>(underline.Y));
            Stroke();
        }

        return layout;
    }
}
