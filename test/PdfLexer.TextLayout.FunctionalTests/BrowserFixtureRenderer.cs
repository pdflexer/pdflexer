using System.Text;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Xunit.Sdk;
using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.FunctionalTests;

internal static class BrowserFixtureRenderer
{
    private static readonly SemaphoreSlim BrowserLock = new(1, 1);
    private const double PlainVerticalAlignmentCompensationPt = 0;
    private const double RichVerticalAlignmentCompensationPt = 0;
    private static string? _executablePath = OperatingSystem.IsLinux() ? "/usr/bin/chromium-browser" : null;
    private static IBrowser? _browser;

    public static async Task<byte[]> RenderPdfAsync(HtmlTextBoxFixture fixture)
    {
        var browser = await EnsureBrowserAsync();
        var html = GetHtml(fixture);
        return await RenderPdfAsync(fixture.PageWidth, fixture.PageHeight, html, browser);
    }

    public static async Task<byte[]> RenderPdfAsync(RichHtmlTextBoxFixture fixture)
    {
        var browser = await EnsureBrowserAsync();
        var html = GetHtml(fixture);
        return await RenderPdfAsync(fixture.PageWidth, fixture.PageHeight, html, browser);
    }

    public static async Task<byte[]> RenderPdfAsync(NormalizedRichHtmlTextBoxFixture fixture)
    {
        var browser = await EnsureBrowserAsync();
        var html = GetHtml(fixture);
        return await RenderPdfAsync(fixture.PageWidth, fixture.PageHeight, html, browser);
    }

    private static async Task<byte[]> RenderPdfAsync(double pageWidth, double pageHeight, string html, IBrowser browser)
    {
        try
        {
            await using var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = (int)Math.Ceiling(pageWidth),
                Height = (int)Math.Ceiling(pageHeight)
            });
            await page.SetContentAsync(html, new NavigationOptions { WaitUntil = [WaitUntilNavigation.Load] });

            return await page.PdfDataAsync(new PdfOptions
            {
                Width = pageWidth * 96.0 / 72.0,
                Height = pageHeight * 96.0 / 72.0,
                PrintBackground = true,
                PreferCSSPageSize = true,
                WaitForFonts = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Right = "0",
                    Bottom = "0",
                    Left = "0"
                }
            });
        }
        catch (ProcessException ex)
        {
            throw SkipException.ForSkip($"Chromium could not be launched for functional conformance tests: {ex.ToString()}");
        }
    }

    private static async Task<IBrowser> EnsureBrowserAsync()
    {
        if (_browser is { IsClosed: false })
        {
            return _browser;
        }

        await BrowserLock.WaitAsync();
        try
        {
            if (_browser is { IsClosed: false })
            {
                return _browser;
            }

            if (string.IsNullOrEmpty(_executablePath))
            {
                var fetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = Path.Combine(Path.GetTempPath(), "pdflexer-puppeteer")
                });

                var installed = await fetcher.DownloadAsync();
                _executablePath = installed.GetExecutablePath();
            }

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = _executablePath,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            _browser = await Puppeteer.LaunchAsync(launchOptions);
            return _browser;
        }
        catch (ProcessException ex)
        {
            throw SkipException.ForSkip($"Chromium could not be launched for functional conformance tests: {ex.ToString()}");
        }
        finally
        {
            BrowserLock.Release();
        }
    }

    public static string GetHtml(HtmlTextBoxFixture fixture)
    {
        var baseStyle = fixture.Segments.FirstOrDefault()?.Style ?? new PdfLexer.TextLayout.TextSegmentStyle(fixture.Fonts.First().FamilyName, fixture.Fonts.First().Weight, 12);
        var boxStyle = BuildBoxStyle(fixture.BoxStyle);
        var contentInset = GetContentInset(fixture.BoxStyle);
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<style>");
        sb.AppendLine($"@page {{ size: {fixture.PageWidth}pt {fixture.PageHeight}pt; margin: 0; }}");
        AppendFontFaces(sb, fixture.Fonts);
        sb.AppendLine("html, body { margin: 0; padding: 0; }");
        sb.AppendLine($".box {{ position: absolute; left: {fixture.BoxLeft}pt; top: {fixture.BoxTop}pt; width: {fixture.BoxWidth}pt; height: {fixture.BoxHeight}pt; margin: 0; text-align: {ToCssAlign(fixture.HorizontalAlignment)}; font-family: '{EscapeCss(baseStyle.FamilyName)}'; font-weight: {baseStyle.Weight}; font-size: {baseStyle.FontSize}pt; line-height: {(baseStyle.LineSpacing ?? baseStyle.FontSize)}pt; font-variant-ligatures: none; font-feature-settings: 'liga' 0, 'clig' 0; }}");
        sb.AppendLine($".box-chrome {{ position: absolute; inset: 0; margin: 0; box-sizing: border-box; {boxStyle} }}");
        sb.AppendLine($".box-inner {{ position: absolute; left: {contentInset:0.###}pt; right: {contentInset:0.###}pt; top: {contentInset:0.###}pt; bottom: {contentInset:0.###}pt; margin: 0; padding: 0; }}");
        sb.AppendLine($".segment-flow {{ position: absolute; left: 0; top: {PlainVerticalAlignmentCompensationPt:0.###}pt; width: 100%; margin: 0; padding: 0; white-space: pre-wrap; }}");
        sb.AppendLine(".segment { margin: 0; padding: 0; }");
        sb.AppendLine("</style></head><body>");
        sb.Append("<div class=\"box\"><div class=\"box-chrome\"></div><div class=\"box-inner\"><div class=\"segment-flow\">");
        foreach (var segment in fixture.Segments)
        {
            sb.Append("<span class=\"segment\" style=\"");
            sb.Append($"font-family:'{EscapeCss(segment.Style.FamilyName)}'; font-weight:{segment.Style.Weight}; font-size:{segment.Style.FontSize}pt; ");
            sb.Append($"letter-spacing:{segment.Style.CharacterSpacing}pt; word-spacing:{segment.Style.WordSpacing}pt; ");
            sb.Append($"line-height:{(segment.Style.LineSpacing ?? segment.Style.FontSize)}pt; ");
            sb.Append($"font-style:{(segment.Style.Italic ? "italic" : "normal")};");
            if (segment.Style.ForegroundColor is { } fg)
            {
                sb.Append($"color:{ToCssColor(fg)};");
            }

            if (segment.Style.BackgroundColor is { } bg)
            {
                sb.Append($"background-color:{ToCssColor(bg)};");
            }
            sb.Append($"text-decoration:{(segment.Style.Underline ? "underline" : "none")};");
            sb.Append("\">");
            sb.Append(EscapeHtml(segment.Text));
            sb.Append("</span>");
        }

        sb.Append("</div></div></div></body></html>");
        return sb.ToString();
    }

    public static string GetHtml(RichHtmlTextBoxFixture fixture)
    {
        var boxStyle = BuildBoxStyle(fixture.BoxStyle);
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<style>");
        sb.AppendLine($"@page {{ size: {fixture.PageWidth}pt {fixture.PageHeight}pt; margin: 0; }}");
        AppendFontFaces(sb, fixture.Fonts);
        sb.AppendLine("html, body { margin: 0; padding: 0; }");
        var contentInset = GetContentInset(fixture.BoxStyle);
        sb.AppendLine($".box {{ position: absolute; left: {fixture.BoxLeft}pt; top: {fixture.BoxTop}pt; width: {fixture.BoxWidth}pt; height: {fixture.BoxHeight}pt; margin: 0; font-variant-ligatures: none; font-feature-settings: 'liga' 0, 'clig' 0; }}");
        sb.AppendLine($".box-chrome {{ position: absolute; inset: 0; margin: 0; box-sizing: border-box; {boxStyle} }}");
        sb.AppendLine($".box-inner {{ position: absolute; left: {contentInset:0.###}pt; right: {contentInset:0.###}pt; top: {contentInset:0.###}pt; bottom: {contentInset:0.###}pt; margin: 0; padding: {RichVerticalAlignmentCompensationPt:0.###}pt 0 0 0; box-sizing: border-box; }}");
        sb.AppendLine(".paragraph, .heading, .list-item-content { margin: 0; padding: 0; }");
        sb.AppendLine(".list { margin: 0; padding: 0; }");
        sb.AppendLine(".list-item { display: flex; align-items: flex-start; margin: 0; padding: 0; }");
        sb.AppendLine(".list-marker { text-align: right; box-sizing: border-box; white-space: pre; }");
        sb.AppendLine(".list-item-content { flex: 1 1 auto; min-width: 0; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class=\"box\"><div class=\"box-chrome\"></div><div class=\"box-inner\">");
        foreach (var block in fixture.Blocks)
        {
            AppendBlockHtml(sb, block, fixture);
        }

        sb.AppendLine("</div></div></body></html>");
        return sb.ToString();
    }

    public static string GetHtml(NormalizedRichHtmlTextBoxFixture fixture)
    {
        var parser = new HtmlRichTextParser();
        var defaultStyle = new TextStyle(fixture.Fonts.First().FamilyName, fixture.Fonts.First().Weight, 12);
        var blocks = parser.Parse(fixture.NormalizedHtml, defaultStyle);
        return GetHtml(new RichHtmlTextBoxFixture(
            fixture.PageWidth,
            fixture.PageHeight,
            fixture.BoxLeft,
            fixture.BoxTop,
            fixture.BoxWidth,
            fixture.BoxHeight,
            fixture.Fonts,
            blocks,
            fixture.ListIndent,
            fixture.ListMarkerGap,
            fixture.BoxStyle));
    }

    private static void AppendBlockHtml(StringBuilder sb, PdfLexer.TextLayout.RichTextBlock block, RichHtmlTextBoxFixture fixture)
    {
        switch (block)
        {
            case PdfLexer.TextLayout.ParagraphBlock paragraph:
                AppendFlowBlockHtml(sb, "p", "paragraph", paragraph.Inlines, paragraph.Style ?? new PdfLexer.TextLayout.ParagraphStyle());
                break;
            case PdfLexer.TextLayout.HeadingBlock heading:
                var headingTag = $"h{Math.Clamp(heading.Level, 1, 6)}";
                AppendFlowBlockHtml(sb, headingTag, "heading", heading.Inlines, heading.Style ?? new PdfLexer.TextLayout.ParagraphStyle());
                break;
            case PdfLexer.TextLayout.UnorderedListBlock unordered:
                AppendListHtml(sb, unordered.Items, unordered.MarginBlockEnd, fixture, index => unordered.Marker, ResolveUnorderedListMetrics(fixture, unordered));
                break;
            case PdfLexer.TextLayout.OrderedListBlock ordered:
                AppendListHtml(sb, ordered.Items, ordered.MarginBlockEnd, fixture, index => OrderedListMarkerFormatter.Format(ordered.MarkerStyle, ordered.StartIndex + index), ResolveOrderedListMetrics(fixture, ordered));
                break;
            case PdfLexer.TextLayout.RowBlock row:
                AppendContainerHtml(sb, "row-container", true, row.Children, row.Height, row.Style ?? new PdfLexer.TextLayout.LayoutContainerStyle(), fixture);
                break;
            case PdfLexer.TextLayout.ColumnBlock column:
                AppendContainerHtml(sb, "column-container", false, column.Children, column.Height, column.Style ?? new PdfLexer.TextLayout.LayoutContainerStyle(), fixture);
                break;
            case PdfLexer.TextLayout.TableBlock table:
                AppendTableHtml(sb, table, fixture);
                break;
            default:
                throw new NotSupportedException($"Unsupported block type '{block.GetType().Name}'.");
        }
    }

    private static void AppendContainerHtml(
        StringBuilder sb,
        string cssClass,
        bool horizontal,
        IReadOnlyList<PdfLexer.TextLayout.LayoutChild> children,
        double? fixedHeight,
        PdfLexer.TextLayout.LayoutContainerStyle style,
        RichHtmlTextBoxFixture fixture)
    {
        sb.Append($"<div class=\"{cssClass}\" style=\"");
        sb.Append("display:flex;");
        sb.Append($"flex-direction:{(horizontal ? "row" : "column")};");
        sb.Append($"gap:{Math.Max(0d, style.Gap):0.###}pt;");
        sb.Append("box-sizing:border-box;");
        sb.Append(BuildBoxStyle(style.ToTextBoxStyle()));
        sb.Append($"margin:0 0 {style.MarginBlockEnd:0.###}pt 0;");
        if (fixedHeight.HasValue)
        {
            sb.Append($"height:{fixedHeight.Value:0.###}pt;");
            sb.Append("overflow:hidden;");
        }
        else if (horizontal)
        {
            sb.Append("align-items:stretch;");
        }

        sb.Append("\">");
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            sb.Append("<div class=\"container-child\" style=\"");
            sb.Append("display:flex;flex-direction:column;box-sizing:border-box;min-width:0;");
            sb.Append($"justify-content:{ToCssVerticalAlign(child.VerticalAlignment)};");
            if (fixedHeight.HasValue)
            {
                sb.Append("overflow:hidden;");
            }
            if (child.BoxStyle is { } childBoxStyle)
            {
                sb.Append(BuildBoxStyle(childBoxStyle));
            }

            if (horizontal)
            {
                if (child.FixedSize.HasValue)
                {
                    sb.Append($"flex:0 0 {child.FixedSize.Value:0.###}pt;");
                    sb.Append($"width:{child.FixedSize.Value:0.###}pt;");
                }
                else
                {
                    sb.Append($"flex:{Math.Max(0d, child.Weight):0.###} 1 0;");
                }
            }
            else
            {
                if (child.FixedSize.HasValue)
                {
                    sb.Append($"flex:0 0 {child.FixedSize.Value:0.###}pt;");
                    sb.Append($"height:{child.FixedSize.Value:0.###}pt;");
                }
                else if (fixedHeight.HasValue)
                {
                    sb.Append($"flex:{Math.Max(0d, child.Weight):0.###} 1 0;");
                }
                else
                {
                    sb.Append("flex:0 0 auto;");
                }
                sb.Append("width:100%;");
            }

            sb.Append("\">");
            foreach (var childBlock in child.Blocks)
            {
                AppendBlockHtml(sb, childBlock, fixture);
            }

            sb.Append("</div>");
        }

        sb.AppendLine("</div>");
    }

    private static void AppendFlowBlockHtml(
        StringBuilder sb,
        string tagName,
        string cssClass,
        IReadOnlyList<PdfLexer.TextLayout.InlineNode> inlines,
        PdfLexer.TextLayout.ParagraphStyle style)
    {
        var effectiveLineHeight = style.LineHeight ?? GetFallbackLineHeight(inlines);
        // Set font-size on the block element to match the content font-size.
        // Without this, the block element inherits the browser default (~12pt), creating
        // a CSS strut whose leading distributes differently from the larger inline spans.
        // When strut and span have different font-sizes but the same line-height, CSS
        // computes the line box as the union of both inline boxes (max above-baseline from
        // each + max below-baseline from each), which can exceed the specified line-height.
        // For example, 18pt content with line-height:22pt inside a 12pt-strut parent yields
        // a ~24pt line box, not 22pt. Matching font-sizes eliminates this strut expansion.
        var effectiveFontSize = GetFallbackFontSize(inlines);
        sb.Append($"<{tagName} class=\"{cssClass}\" style=\"");
        sb.Append($"font-size:{effectiveFontSize:0.###}pt;");
        if (style.TextAlign != PdfLexer.TextLayout.TextHorizontalAlignment.Left)
        {
            sb.Append($"text-align:{ToCssAlign(style.TextAlign)};");
        }
        sb.Append($"line-height:{effectiveLineHeight:0.###}pt;");
        sb.Append($"margin:0 0 {style.MarginBlockEnd:0.###}pt 0;");
        sb.Append("\">");
        AppendInlineHtml(sb, inlines, style.LineHeight);
        sb.AppendLine($"</{tagName}>");
    }

    private static void AppendListHtml(
        StringBuilder sb,
        IReadOnlyList<PdfLexer.TextLayout.ListItemBlock> items,
        double marginBlockEnd,
        RichHtmlTextBoxFixture fixture,
        Func<int, string> markerFactory,
        ResolvedListMetrics metrics)
    {
        sb.Append($"<div class=\"list\" style=\"margin:0 0 {marginBlockEnd:0.###}pt 0;\">");
        for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            var item = items[itemIndex];
            var markerStyle = FindMarkerStyle(item);
            sb.Append("<div class=\"list-item\">");
            sb.Append($"<div class=\"list-marker\" style=\"box-sizing:border-box;flex:0 0 {metrics.MarkerColumnWidth:0.###}pt;width:{metrics.MarkerColumnWidth:0.###}pt;padding-right:{metrics.MarkerGap:0.###}pt;");
            if (markerStyle is not null)
            {
                sb.Append(BuildMarkerStyle(markerStyle.Value.Style, markerStyle.Value.ParagraphStyle));
            }
            sb.Append("\">");
            sb.Append(EscapeHtml(markerFactory(itemIndex) + " "));
            sb.Append("</div>");
            sb.Append("<div class=\"list-item-content\">");
            var blocks = item.Blocks;
            for (var i = 0; i < blocks.Count; i++)
            {
                AppendBlockHtml(sb, blocks[i], fixture);
            }

            sb.Append("</div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div>");
    }

    private static void AppendTableHtml(
        StringBuilder sb,
        PdfLexer.TextLayout.TableBlock table,
        RichHtmlTextBoxFixture fixture)
    {
        var style = table.Style ?? new PdfLexer.TextLayout.TableStyle();
        sb.Append("<table class=\"rich-table\" style=\"");
        switch (table.Width)
        {
            case PdfLexer.TextLayout.TableFixedWidth fixedWidth:
                sb.Append($"width:{fixedWidth.Points:0.###}pt;");
                break;
            case PdfLexer.TextLayout.TablePercentWidth percentWidth:
                sb.Append($"width:{percentWidth.Percent:0.###}%;");
                break;
            default:
                sb.Append("width:100%;");
                break;
        }
        sb.Append("border-collapse:collapse;");
        sb.Append($"margin:0 0 {style.MarginBlockEnd:0.###}pt 0;");
        sb.Append($"border-style:solid;border-width:{Math.Max(0d, style.BorderWidth):0.###}pt;");
        sb.Append($"border-color:{ToCssColor(style.BorderColor)};");
        sb.Append($"background-color:{ToCssColor(style.BackgroundColor)};");
        sb.Append("\">");

        if (table.Columns.Count > 0)
        {
            sb.Append("<colgroup>");
            foreach (var column in table.Columns)
            {
                sb.Append("<col style=\"box-sizing:border-box;");
                switch (column.Width)
                {
                    case PdfLexer.TextLayout.ColumnFixedWidth fixedWidth:
                        sb.Append($"width:{fixedWidth.Points:0.###}pt;");
                        break;
                    case PdfLexer.TextLayout.ColumnPercentWidth percentWidth:
                        sb.Append($"width:{percentWidth.Percent:0.###}%;");
                        break;
                }

                sb.Append("\"/>");
            }
            sb.Append("</colgroup>");
        }

        foreach (var section in table.Sections)
        {
            var sectionTag = section.Kind switch
            {
                PdfLexer.TextLayout.TableSectionKind.Header => "thead",
                PdfLexer.TextLayout.TableSectionKind.Footer => "tfoot",
                _ => "tbody"
            };
            sb.Append('<').Append(sectionTag).Append('>');
            foreach (var row in section.Rows)
            {
                sb.Append("<tr>");
                foreach (var cell in row.Cells)
                {
                    AppendTableCellHtml(sb, cell, style, fixture);
                }

                sb.Append("</tr>");
            }
            sb.Append("</").Append(sectionTag).Append('>');
        }

        sb.AppendLine("</table>");
    }

    private static void AppendTableCellHtml(
        StringBuilder sb,
        PdfLexer.TextLayout.TableCellBlock cell,
        PdfLexer.TextLayout.TableStyle tableStyle,
        RichHtmlTextBoxFixture fixture)
    {
        var tagName = cell is PdfLexer.TextLayout.TableHeaderCellBlock ? "th" : "td";
        var cellStyle = cell.Style ?? new PdfLexer.TextLayout.TableCellStyle();
        var padding = Math.Max(0d, cellStyle.ResolvePadding(tableStyle));
        var textAlign = cellStyle.TextAlign ?? PdfLexer.TextLayout.TextHorizontalAlignment.Left;

        sb.Append('<');
        sb.Append(tagName);
        if (cell.ColSpan > 1)
        {
            sb.Append($" colspan=\"{cell.ColSpan}\"");
        }

        if (cell.RowSpan > 1)
        {
            sb.Append($" rowspan=\"{cell.RowSpan}\"");
        }

        sb.Append(" style=\"");
        sb.Append("vertical-align:top;box-sizing:border-box;");
        sb.Append($"padding:{padding:0.###}pt;");
        sb.Append($"text-align:{ToCssAlign(textAlign)};");
        sb.Append($"border-style:solid;border-width:{Math.Max(0d, tableStyle.CellBorderWidth):0.###}pt;");
        sb.Append($"border-color:{ToCssColor(tableStyle.CellBorderColor)};");
        sb.Append($"background-color:{ToCssColor(cellStyle.BackgroundColor)};");

        sb.Append("\">");
        foreach (var block in cell.Blocks)
        {
            AppendBlockHtml(sb, block, fixture);
        }

        sb.Append("</");
        sb.Append(tagName);
        sb.Append('>');
    }

    private static void AppendInlineHtml(StringBuilder sb, IReadOnlyList<PdfLexer.TextLayout.InlineNode> inlines, double? paragraphLineHeight)
    {
        foreach (var inline in inlines)
        {
            switch (inline)
            {
                case PdfLexer.TextLayout.TextRunNode run:
                    sb.Append("<span style=\"");
                    sb.Append(BuildInlineStyle(run.Style, paragraphLineHeight));
                    sb.Append("\">");
                    sb.Append(EscapeHtml(run.Text));
                    sb.Append("</span>");
                    break;
                case PdfLexer.TextLayout.LineBreakNode:
                    sb.Append("<br />");
                    break;
                default:
                    throw new NotSupportedException($"Unsupported inline type '{inline.GetType().Name}'.");
            }
        }
    }

    private static string BuildInlineStyle(PdfLexer.TextLayout.TextStyle style, double? paragraphLineHeight)
    {
        var sb = new StringBuilder();
        sb.Append($"font-family:'{EscapeCss(style.FamilyName)}';");
        sb.Append($"font-weight:{style.Weight};");
        sb.Append($"font-size:{style.FontSize:0.###}pt;");
        sb.Append($"font-style:{(style.Italic ? "italic" : "normal")};");
        sb.Append($"line-height:{(paragraphLineHeight ?? style.FontSize):0.###}pt;");
        sb.Append($"letter-spacing:{style.CharacterSpacing:0.###}pt;");
        sb.Append($"word-spacing:{style.WordSpacing:0.###}pt;");
        sb.Append("font-variant-ligatures:none;");
        sb.Append("font-feature-settings:'liga' 0, 'clig' 0;");
        sb.Append($"text-decoration:{(style.Underline ? "underline" : "none")};");
        if (style.ForegroundColor is { } fg)
        {
            sb.Append($"color:{ToCssColor(fg)};");
        }

        if (style.BackgroundColor is { } bg)
        {
            sb.Append($"background-color:{ToCssColor(bg)};");
        }

        return sb.ToString();
    }

    private static string BuildMarkerStyle(PdfLexer.TextLayout.TextStyle style, PdfLexer.TextLayout.ParagraphStyle paragraphStyle)
    {
        var sb = new StringBuilder();
        sb.Append($"font-family:'{EscapeCss(style.FamilyName)}';");
        sb.Append($"font-weight:{style.Weight};");
        sb.Append($"font-size:{style.FontSize:0.###}pt;");
        sb.Append($"font-style:{(style.Italic ? "italic" : "normal")};");
        sb.Append("font-variant-ligatures:none;");
        sb.Append("font-feature-settings:'liga' 0, 'clig' 0;");
        sb.Append($"line-height:{(paragraphStyle.LineHeight ?? style.FontSize):0.###}pt;");
        if (style.ForegroundColor is { } fg)
        {
            sb.Append($"color:{ToCssColor(fg)};");
        }

        return sb.ToString();
    }

    private static string BuildBoxStyle(PdfLexer.TextLayout.TextBoxStyle style)
    {
        var sb = new StringBuilder();
        sb.Append($"padding:{Math.Max(0d, style.Padding):0.###}pt;");
        sb.Append($"border-style:solid;border-width:{Math.Max(0d, style.BorderWidth):0.###}pt;");
        sb.Append($"border-radius:{Math.Max(0d, style.BorderRadius):0.###}pt;");
        sb.Append($"border-color:{ToCssColor(style.BorderColor)};");
        sb.Append($"background-color:{ToCssColor(style.BackgroundColor)};");
        return sb.ToString();
    }

    private static double GetContentInset(PdfLexer.TextLayout.TextBoxStyle style)
        => Math.Max(0d, style.BorderWidth) + Math.Max(0d, style.Padding);

    private static void AppendFontFaces(StringBuilder sb, IReadOnlyList<FixtureFontAsset> fonts)
    {
        foreach (var font in fonts)
        {
            var fontBase64 = Convert.ToBase64String(font.FontData);
            sb.AppendLine("@font-face {");
            sb.AppendLine($"  font-family: '{EscapeCss(font.FamilyName)}';");
            sb.AppendLine($"  font-weight: {font.Weight};");
            sb.AppendLine($"  font-style: {(font.Italic ? "italic" : "normal")};");
            sb.AppendLine($"  src: url(data:font/ttf;base64,{fontBase64}) format('truetype');");
            sb.AppendLine("}");
        }
    }

    private static string ToCssColor(PdfLexer.TextLayout.TextColor color)
        => $"rgb({color.R},{color.G},{color.B})";

    private static string ToCssColor(PdfLexer.TextLayout.TextColor? color)
        => color is { } c ? ToCssColor(c) : "transparent";

    private static string EscapeCss(string value)
        => value.Replace("\\", "\\\\").Replace("'", "\\'");

    private static double GetFallbackLineHeight(IReadOnlyList<PdfLexer.TextLayout.InlineNode> inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is PdfLexer.TextLayout.TextRunNode run)
            {
                return run.Style.FontSize;
            }
        }

        return 12d;
    }

    private static double GetFallbackFontSize(IReadOnlyList<PdfLexer.TextLayout.InlineNode> inlines)
    {
        foreach (var inline in inlines)
        {
            if (inline is PdfLexer.TextLayout.TextRunNode run)
            {
                return run.Style.FontSize;
            }
        }

        return 12d;
    }

    private static (PdfLexer.TextLayout.TextStyle Style, PdfLexer.TextLayout.ParagraphStyle ParagraphStyle)? FindMarkerStyle(PdfLexer.TextLayout.ListItemBlock item)
    {
        foreach (var block in item.Blocks)
        {
            if (block is PdfLexer.TextLayout.ParagraphBlock paragraph)
            {
                var style = paragraph.Inlines.OfType<PdfLexer.TextLayout.TextRunNode>().Select(x => x.Style).FirstOrDefault();
                if (style is not null)
                {
                    return (style, paragraph.Style ?? new PdfLexer.TextLayout.ParagraphStyle());
                }
            }

            if (block is PdfLexer.TextLayout.HeadingBlock heading)
            {
                var style = heading.Inlines.OfType<PdfLexer.TextLayout.TextRunNode>().Select(x => x.Style).FirstOrDefault();
                if (style is not null)
                {
                    return (style, heading.Style ?? new PdfLexer.TextLayout.ParagraphStyle());
                }
            }
        }

        return null;
    }

    private static ResolvedListMetrics ResolveUnorderedListMetrics(RichHtmlTextBoxFixture fixture, PdfLexer.TextLayout.UnorderedListBlock list)
    {
        var firstItem = list.Items.FirstOrDefault();
        var markerStyle = firstItem is not null ? FindMarkerStyle(firstItem) : null;
        var fontSize = markerStyle?.Style.FontSize ?? 12d;
        var markerWidth = list.MarkerStyle == PdfLexer.TextLayout.ListMarkerStyle.Disc && string.Equals(list.Marker, "\u2022", StringComparison.Ordinal)
            ? Math.Max(1d, Math.Round(fontSize * 0.42d, 3))
            : EstimateMarkerTextWidth(list.Marker, fontSize);
        return ListLayoutMetricsResolver.Build(fixture.ListIndent, fixture.ListMarkerGap, fontSize, markerWidth);
    }

    private static ResolvedListMetrics ResolveOrderedListMetrics(RichHtmlTextBoxFixture fixture, PdfLexer.TextLayout.OrderedListBlock list)
    {
        var firstItem = list.Items.FirstOrDefault();
        var markerStyle = firstItem is not null ? FindMarkerStyle(firstItem) : null;
        var fontSize = markerStyle?.Style.FontSize ?? 12d;
        var largestIndex = Math.Max(list.StartIndex, list.StartIndex + Math.Max(0, list.Items.Count - 1));
        var markerWidth = EstimateMarkerTextWidth(OrderedListMarkerFormatter.Format(list.MarkerStyle, largestIndex, includeTrailingSpace: true), fontSize);
        return ListLayoutMetricsResolver.Build(fixture.ListIndent, fixture.ListMarkerGap, fontSize, markerWidth);
    }

    private static double EstimateMarkerTextWidth(string markerText, double fontSize)
    {
        var nonWhitespace = markerText.Count(c => !char.IsWhiteSpace(c));
        var whitespace = markerText.Length - nonWhitespace;
        return Math.Max(1d, Math.Round((nonWhitespace * fontSize * 0.52d) + (whitespace * fontSize * 0.28d), 3));
    }

    private static string ToCssAlign(PdfLexer.TextLayout.TextHorizontalAlignment alignment) => alignment switch
    {
        PdfLexer.TextLayout.TextHorizontalAlignment.Center => "center",
        PdfLexer.TextLayout.TextHorizontalAlignment.Right => "right",
        _ => "left"
    };

    private static string ToCssVerticalAlign(PdfLexer.TextLayout.TextVerticalAlignment alignment) => alignment switch
    {
        PdfLexer.TextLayout.TextVerticalAlignment.Center => "center",
        PdfLexer.TextLayout.TextVerticalAlignment.Bottom => "flex-end",
        _ => "flex-start"
    };

    private static string EscapeHtml(string text)
        => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}

internal sealed record HtmlTextBoxFixture(
    double PageWidth,
    double PageHeight,
    double BoxLeft,
    double BoxTop,
    double BoxWidth,
    double BoxHeight,
    IReadOnlyList<FixtureFontAsset> Fonts,
    IReadOnlyList<PdfLexer.TextLayout.TextSegment> Segments,
    PdfLexer.TextLayout.TextHorizontalAlignment HorizontalAlignment,
    PdfLexer.TextLayout.TextBoxStyle BoxStyle);

internal sealed record RichHtmlTextBoxFixture(
    double PageWidth,
    double PageHeight,
    double BoxLeft,
    double BoxTop,
    double BoxWidth,
    double BoxHeight,
    IReadOnlyList<FixtureFontAsset> Fonts,
    IReadOnlyList<PdfLexer.TextLayout.RichTextBlock> Blocks,
    double? ListIndent = null,
    double? ListMarkerGap = null,
    PdfLexer.TextLayout.TextBoxStyle BoxStyle = default!);

internal sealed record NormalizedRichHtmlTextBoxFixture(
    double PageWidth,
    double PageHeight,
    double BoxLeft,
    double BoxTop,
    double BoxWidth,
    double BoxHeight,
    IReadOnlyList<FixtureFontAsset> Fonts,
    string NormalizedHtml,
    double? ListIndent = null,
    double? ListMarkerGap = null,
    PdfLexer.TextLayout.TextBoxStyle BoxStyle = default!);

public sealed record FixtureFontAsset(
    string FamilyName,
    int Weight,
    byte[] FontData,
    bool Italic = false);
