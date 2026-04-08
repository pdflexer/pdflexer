using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.Tests;

public class StyleResolverTests
{
    [Fact]
    public void Resolve_ParagraphAndInlineStyles_NormalizeIntoComputedStyles()
    {
        var paragraph = StyleResolver.Resolve(new ParagraphStyle(
            TextAlign: TextHorizontalAlignment.Right,
            LineHeight: 18,
            MarginBlockEnd: 6,
            MarginBlockStart: 4,
            TextIndent: 12,
            WhiteSpace: TextWhiteSpaceMode.PreWrap,
            OverflowWrap: TextOverflowWrapMode.Anywhere,
            WordBreak: TextWordBreakMode.BreakAll));
        var inline = StyleResolver.Resolve(new TextStyle(
            "Roboto",
            700,
            13,
            Italic: true,
            Underline: true,
            CharacterSpacing: 0.5,
            WordSpacing: 1.25,
            ForegroundColor: new TextColor(10, 20, 30),
            BackgroundColor: new TextColor(240, 240, 200),
            StrikeThrough: true));

        Assert.Equal(TextHorizontalAlignment.Right, paragraph.TextAlign);
        Assert.Equal(18d, paragraph.LineHeight);
        Assert.Equal(TextLineBoxSizing.AtLeastLineHeight, paragraph.LineBoxSizing);
        Assert.Equal(6d, paragraph.MarginBlockEnd);
        Assert.Equal(4d, paragraph.MarginBlockStart);
        Assert.Equal(12d, paragraph.TextIndent);
        Assert.Equal(TextWhiteSpaceMode.PreWrap, paragraph.WhiteSpace);
        Assert.Equal(TextOverflowWrapMode.Anywhere, paragraph.OverflowWrap);
        Assert.Equal(TextWordBreakMode.BreakAll, paragraph.WordBreak);

        Assert.Equal("Roboto", inline.FamilyName);
        Assert.Equal(700, inline.Weight);
        Assert.Equal(13d, inline.FontSize);
        Assert.True(inline.Italic);
        Assert.True(inline.Underline);
        Assert.True(inline.StrikeThrough);
        Assert.Equal(0.5d, inline.CharacterSpacing);
        Assert.Equal(1.25d, inline.WordSpacing);
        Assert.Equal(new TextColor(10, 20, 30), inline.ForegroundColor);
        Assert.Equal(new TextColor(240, 240, 200), inline.BackgroundColor);
    }

    [Fact]
    public void Resolve_BoxContainerAndTableStyles_ExpandUniformEdges()
    {
        var box = StyleResolver.Resolve(new TextBoxStyle(
            BackgroundColor: new TextColor(250, 250, 250),
            BorderColor: new TextColor(20, 30, 40),
            BorderWidth: 1.5,
            BorderRadius: 3,
            Padding: 4));
        var container = StyleResolver.Resolve(new LayoutContainerStyle(
            BackgroundColor: new TextColor(245, 245, 245),
            BorderColor: new TextColor(60, 70, 80),
            BorderWidth: 2,
            Padding: 5,
            Gap: 9,
            MarginBlockEnd: 7));
        var table = StyleResolver.Resolve(new TableStyle(
            BackgroundColor: new TextColor(240, 240, 240),
            BorderColor: new TextColor(90, 100, 110),
            BorderWidth: 1,
            CellBorderColor: new TextColor(120, 130, 140),
            CellBorderWidth: 0.75,
            CellPadding: 6,
            MarginBlockEnd: 8));
        var cell = StyleResolver.Resolve(new TableCellStyle(
            BackgroundColor: new TextColor(255, 250, 220),
            Padding: 3,
            TextAlign: TextHorizontalAlignment.Center), table);

        Assert.Equal(11d, box.Edges.HorizontalInset);
        Assert.Equal(11d, box.Edges.VerticalInset);
        Assert.Equal(3d, box.BorderRadius);
        Assert.True(box.Border.HasVisibleStroke);

        Assert.Equal(9d, container.Gap);
        Assert.Equal(7d, container.MarginBlockEnd);
        Assert.Equal(14d, container.Box.Edges.HorizontalInset);
        Assert.Equal(14d, container.Box.Edges.VerticalInset);

        Assert.Equal(8d, table.MarginBlockEnd);
        Assert.Equal(12d, table.CellPadding.Horizontal);
        Assert.Equal(12d, table.CellPadding.Vertical);
        Assert.True(table.Border.HasVisibleStroke);
        Assert.True(table.CellBorder.HasVisibleStroke);

        Assert.Equal(new TextColor(255, 250, 220), cell.BackgroundColor);
        Assert.Equal(6d, cell.Padding.Horizontal);
        Assert.Equal(6d, cell.Padding.Vertical);
        Assert.Equal(TextHorizontalAlignment.Center, cell.TextAlign);
    }

    [Fact]
    public void Resolve_ListStyles_PreserveMarkerSemantics()
    {
        var unordered = StyleResolver.Resolve(new UnorderedListBlock(
            new[] { new ListItemBlock(Array.Empty<RichTextBlock>()) },
            MarginBlockEnd: 5,
            Marker: "\u2022",
            MarkerStyle: ListMarkerStyle.Disc));
        var ordered = StyleResolver.Resolve(new OrderedListBlock(
            new[] { new ListItemBlock(Array.Empty<RichTextBlock>()) },
            StartIndex: 3,
            MarginBlockEnd: 4));

        Assert.Equal("\u2022", unordered.MarkerText);
        Assert.Equal(ListMarkerStyle.Disc, unordered.MarkerStyle);
        Assert.True(unordered.UseVectorMarker);
        Assert.Equal(5d, unordered.MarginBlockEnd);

        Assert.Equal(3, ordered.StartIndex);
        Assert.Equal(4d, ordered.MarginBlockEnd);
    }
}
