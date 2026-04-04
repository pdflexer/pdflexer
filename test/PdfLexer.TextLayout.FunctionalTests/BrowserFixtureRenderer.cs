using System.Text;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Xunit.Sdk;

namespace PdfLexer.TextLayout.FunctionalTests;

internal static class BrowserFixtureRenderer
{
    private static readonly SemaphoreSlim BrowserLock = new(1, 1);
    private static string? _executablePath;

    public static async Task<byte[]> RenderPdfAsync(HtmlTextBoxFixture fixture)
    {
        var executablePath = await EnsureBrowserAsync();
        var html = BuildHtml(fixture);

        var launchOptions = new LaunchOptions
        {
            Headless = true,
            ExecutablePath = executablePath,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        };

        try
        {
            await using var browser = await Puppeteer.LaunchAsync(launchOptions);
            await using var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = (int)Math.Ceiling(fixture.PageWidth),
                Height = (int)Math.Ceiling(fixture.PageHeight)
            });
            await page.SetContentAsync(html);
            await page.WaitForNetworkIdleAsync();

            return await page.PdfDataAsync(new PdfOptions
            {
                Width = $"{fixture.PageWidth}pt",
                Height = $"{fixture.PageHeight}pt",
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
            throw SkipException.ForSkip($"Chromium could not be launched for functional conformance tests: {ex.Message}");
        }
    }

    private static async Task<string> EnsureBrowserAsync()
    {
        if (!string.IsNullOrEmpty(_executablePath))
        {
            return _executablePath;
        }

        await BrowserLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_executablePath))
            {
                return _executablePath;
            }

            var fetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = Path.Combine(Path.GetTempPath(), "pdflexer-puppeteer")
            });

            var installed = await fetcher.DownloadAsync();
            _executablePath = installed.GetExecutablePath();
            return _executablePath;
        }
        finally
        {
            BrowserLock.Release();
        }
    }

    private static string BuildHtml(HtmlTextBoxFixture fixture)
    {
        var fontBase64 = Convert.ToBase64String(fixture.FontData);
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html><head><meta charset=\"utf-8\" />");
        sb.AppendLine("<style>");
        sb.AppendLine($"@page {{ size: {fixture.PageWidth}pt {fixture.PageHeight}pt; margin: 0; }}");
        sb.AppendLine("@font-face {");
        sb.AppendLine("  font-family: 'FixtureFont';");
        sb.AppendLine("  font-weight: 400;");
        sb.AppendLine($"  src: url(data:font/ttf;base64,{fontBase64}) format('truetype');");
        sb.AppendLine("}");
        sb.AppendLine("html, body { margin: 0; padding: 0; }");
        sb.AppendLine($".box {{ position: absolute; left: {fixture.BoxLeft}pt; top: {fixture.BoxTop}pt; width: {fixture.BoxWidth}pt; height: {fixture.BoxHeight}pt; white-space: pre-wrap; box-sizing: border-box; text-align: {ToCssAlign(fixture.HorizontalAlignment)}; }}");
        sb.AppendLine(".segment { display: inline; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class=\"box\">");
        foreach (var segment in fixture.Segments)
        {
            sb.Append("<span class=\"segment\" style=\"");
            sb.Append($"font-family:'FixtureFont'; font-weight:{segment.Style.Weight}; font-size:{segment.Style.FontSize}pt; ");
            sb.Append($"letter-spacing:{segment.Style.CharacterSpacing}pt; word-spacing:{segment.Style.WordSpacing}pt; ");
            sb.Append($"line-height:{(segment.Style.LineSpacing ?? segment.Style.FontSize)}pt; ");
            sb.Append($"text-decoration:{(segment.Style.Underline ? "underline" : "none")};");
            sb.Append("\">");
            sb.Append(EscapeHtml(segment.Text));
            sb.AppendLine("</span>");
        }

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    private static string ToCssAlign(PdfLexer.TextLayout.TextHorizontalAlignment alignment) => alignment switch
    {
        PdfLexer.TextLayout.TextHorizontalAlignment.Center => "center",
        PdfLexer.TextLayout.TextHorizontalAlignment.Right => "right",
        _ => "left"
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
    byte[] FontData,
    IReadOnlyList<PdfLexer.TextLayout.TextSegment> Segments,
    PdfLexer.TextLayout.TextHorizontalAlignment HorizontalAlignment);
