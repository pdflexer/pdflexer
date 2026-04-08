using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.FunctionalTests;

public class HtmlRichTextParserTests
{
    private static readonly TextStyle DefaultStyle = new("FixtureSans", 400, 12);
    public static TheoryData<string> CanonicalHtmlSnippets => new()
    {
        """
        <p style="text-align:center; line-height:16pt; margin-bottom:8pt;"><span style="font-family:'FixtureSans'; font-size:12pt; font-weight:700;">Heading</span></p>
        """,
        """
        <ul style="list-style-type:disc; margin-bottom:6pt;"><li><p><span style="font-family:'FixtureSans'; font-size:12pt;">Item one wraps</span></p></li></ul>
        """,
        """
        <table style="border:1pt solid #505050; cell-border-color:#999999; cell-border-width:0.5pt; cellpadding:4pt;"><tr><td style="width:60pt;"><p><span style="font-family:'FixtureSans'; font-size:12pt;">A</span></p></td><td style="width:90pt;"><p><span style="font-family:'FixtureSerif'; font-size:13pt;">B</span></p></td></tr></table>
        """
    };

    [Fact]
    public void Parse_ParagraphListAndTable_BuildsExpectedModel()
    {
        var html = """
            <p style="text-align:center; line-height:16pt; margin-bottom:8pt;">
              <span style="font-family:'FixtureSans'; font-weight:700; font-size:13pt;">Heading</span>
            </p>
            <ul>
              <li><p style="line-height:15pt;"><span style="font-family:'FixtureSans'; font-weight:400; font-size:12pt;">Item one</span></p></li>
            </ul>
            <table style="border:1pt solid #505050; cell-border-color:#999999; cell-border-width:0.5pt; cellpadding:4pt;">
              <tr>
                <th><p style="line-height:16pt;"><span style="font-family:'FixtureSans'; font-weight:700; font-size:12pt;">Header</span></p></th>
                <td style="background-color:#f0f0f0;"><p style="line-height:16pt;"><span style="font-family:'FixtureSerif'; font-weight:400; font-size:13pt;">Body</span></p></td>
              </tr>
            </table>
            """;

        var parser = new HtmlRichTextParser();
        var blocks = parser.Parse(html, DefaultStyle);

        var paragraph = Assert.IsType<ParagraphBlock>(blocks[0]);
        Assert.Equal(TextHorizontalAlignment.Center, paragraph.Style?.TextAlign);
        Assert.Equal(16d, paragraph.Style?.LineHeight);
        Assert.Equal(8d, paragraph.Style?.MarginBlockEnd);

        var unordered = Assert.IsType<UnorderedListBlock>(blocks[1]);
        Assert.Single(unordered.Items);

        var table = Assert.IsType<TableBlock>(blocks[2]);
        Assert.Equal(1d, table.Style?.BorderWidth);
        Assert.Equal(0.5d, table.Style?.CellBorderWidth);
        Assert.Equal(4d, table.Style?.CellPadding);
        Assert.Equal(2, table.Columns.Count);
        Assert.Single(table.Sections);
        Assert.Equal(TableSectionKind.Body, table.Sections[0].Kind);
        Assert.Single(table.Rows);
        Assert.IsType<TableHeaderCellBlock>(table.Rows[0].Cells[0]);
        Assert.IsType<TableDataCellBlock>(table.Rows[0].Cells[1]);
    }

    [Fact]
    public void ParseAndWrite_TableWidthAndColGroup_RoundTripsExplicitGridMetadata()
    {
        var html = """
            <table style="width:75%; border:1pt solid #505050;">
              <colgroup>
                <col style="width:25%"/>
                <col style="width:75%"/>
              </colgroup>
              <thead>
                <tr>
                  <th><p><span style="font-family:'FixtureSans'; font-size:12pt;">H1</span></p></th>
                  <th><p><span style="font-family:'FixtureSans'; font-size:12pt;">H2</span></p></th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td><p><span style="font-family:'FixtureSans'; font-size:12pt;">A</span></p></td>
                  <td><p><span style="font-family:'FixtureSans'; font-size:12pt;">B</span></p></td>
                </tr>
              </tbody>
            </table>
            """;

        var parser = new HtmlRichTextParser();
        var blocks = parser.Parse(html, DefaultStyle);
        var table = Assert.IsType<TableBlock>(Assert.Single(blocks));

        Assert.IsType<TablePercentWidth>(table.Width);
        Assert.Equal(2, table.Columns.Count);
        Assert.IsType<ColumnPercentWidth>(table.Columns[0].Width);
        Assert.Equal(TableSectionKind.Header, table.Sections[0].Kind);
        Assert.Equal(TableSectionKind.Body, table.Sections[1].Kind);

        var normalized = parser.ToHtml(blocks);
        Assert.Contains("<colgroup>", normalized);
        Assert.Contains("width:75%", normalized);
        Assert.Contains("<thead>", normalized);
        Assert.Contains("<tbody>", normalized);
    }

    [Fact]
    public void ToHtml_RoundTripsToDeterministicCanonicalMarkup()
    {
        var html = """
            <p style="line-height:16pt">
              <span style="font-family:'FixtureSans'; font-size:12pt; font-weight:700; color:#aa2200;">Hello</span>
              <span style="font-family:'FixtureSans'; font-size:12pt; text-decoration:underline line-through;"> world</span>
            </p>
            """;

        var parser = new HtmlRichTextParser();
        var blocks = parser.Parse(html, DefaultStyle);
        var normalized = parser.ToHtml(blocks);

        Assert.StartsWith("<p style=\"text-align:left;line-height:16pt\">", normalized);
        Assert.Contains("font-family:'FixtureSans'", normalized);
        Assert.Contains("font-size:12pt", normalized);
        Assert.Contains("color:#aa2200", normalized);
        Assert.Contains("text-decoration:underline line-through", normalized);
    }

    [Fact]
    public void Parse_UnsupportedTag_Throws()
    {
        var parser = new HtmlRichTextParser();
        var ex = Assert.Throws<NotSupportedException>(() => parser.Parse("<section>Unsupported</section>", DefaultStyle));
        Assert.Contains("Unsupported block element 'section'", ex.Message);
    }

    [Fact]
    public void Parse_InlineLineHeight_IsRejectedAsLayoutPending()
    {
        var parser = new HtmlRichTextParser();
        var ex = Assert.Throws<NotSupportedException>(() => parser.Parse("<p><span style=\"line-height:18pt\">Unsupported</span></p>", DefaultStyle));
        Assert.Contains("not supported end-to-end", ex.Message);
    }

    [Fact]
    public void Parse_ParagraphTextIndent_IsRejectedAsLayoutPending()
    {
        var parser = new HtmlRichTextParser();
        var ex = Assert.Throws<NotSupportedException>(() => parser.Parse("<p style=\"text-indent:12pt\">Unsupported</p>", DefaultStyle));
        Assert.Contains("not supported end-to-end", ex.Message);
    }

    [Theory]
    [MemberData(nameof(CanonicalHtmlSnippets))]
    public void ToHtml_CanonicalSnippets_RoundTripDeterministically(string html)
    {
        var parser = new HtmlRichTextParser();
        var blocks = parser.Parse(html, DefaultStyle);
        var normalized = parser.ToHtml(blocks);
        var reparsed = parser.Parse(normalized, DefaultStyle);
        var renormalized = parser.ToHtml(reparsed);

        Assert.Equal(normalized, renormalized);
    }
}
