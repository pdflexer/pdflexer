using System.Globalization;
using System.Net;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using PdfLexer.TextLayout;

namespace PdfLexer.TextLayout.FunctionalTests;

public sealed class HtmlRichTextParser
{
    private enum HtmlFixtureSupportLevel
    {
        Supported,
        LayoutPending,
        Unsupported
    }

    public IReadOnlyList<RichTextBlock> Parse(string html, TextStyle defaultTextStyle)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(defaultTextStyle);

        var parser = new HtmlParser();
        using var document = parser.ParseDocument(html);

        var blocks = new List<RichTextBlock>();
        var nodes = document.Body?.ChildNodes;
        if (nodes is null)
        {
            return blocks;
        }

        foreach (var child in nodes)
        {
            foreach (var block in ParseBlockNode(child, defaultTextStyle))
            {
                blocks.Add(block);
            }
        }

        return blocks;
    }

    public string ToHtml(IReadOnlyList<RichTextBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var sb = new StringBuilder();
        foreach (var block in blocks)
        {
            WriteBlock(sb, block);
        }

        return sb.ToString();
    }

    private IEnumerable<RichTextBlock> ParseBlockNode(INode node, TextStyle inheritedTextStyle)
    {
        if (node.NodeType == NodeType.Text)
        {
            if (string.IsNullOrWhiteSpace(node.TextContent))
            {
                yield break;
            }

            yield return new ParagraphBlock(new InlineNode[] { new TextRunNode(node.TextContent, inheritedTextStyle) });
            yield break;
        }

        if (node is not IElement element)
        {
            yield break;
        }

        var tag = element.LocalName.ToLowerInvariant();
        switch (tag)
        {
            case "p":
            case "div":
                {
                    var blockStyle = ResolveParagraphStyle(element);
                    var inlineStyle = ResolveInheritedInlineStyle(element, inheritedTextStyle);
                    yield return new ParagraphBlock(ParseInlineChildren(element, inlineStyle), blockStyle);
                    yield break;
                }
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                {
                    var blockStyle = ResolveParagraphStyle(element);
                    var inlineStyle = ResolveInheritedInlineStyle(element, inheritedTextStyle);
                    var level = int.Parse(tag.AsSpan(1), CultureInfo.InvariantCulture);
                    yield return new HeadingBlock(level, ParseInlineChildren(element, inlineStyle), blockStyle);
                    yield break;
                }
            case "ul":
                {
                    var styles = ParseStyleMap(element);
                    ValidateSupportedStyleProperties(styles, "ul");
                    var marginBlockEnd = TryGetPoint(styles, "margin-bottom") ?? 0d;
                    var markerStyle = TryGetListMarkerStyle(styles, ordered: false) ?? ListMarkerStyle.Disc;
                    yield return new UnorderedListBlock(ParseListItems(element, inheritedTextStyle), marginBlockEnd, MarkerStyle: markerStyle);
                    yield break;
                }
            case "ol":
                {
                    var styles = ParseStyleMap(element);
                    ValidateSupportedStyleProperties(styles, "ol");
                    var marginBlockEnd = TryGetPoint(styles, "margin-bottom") ?? 0d;
                    var start = 1;
                    if (element.HasAttribute("start") && !int.TryParse(element.GetAttribute("start"), NumberStyles.Integer, CultureInfo.InvariantCulture, out start))
                    {
                        throw new NotSupportedException($"Unsupported ordered-list start value '{element.GetAttribute("start")}'.");
                    }

                    var markerStyle = TryGetListMarkerStyle(styles, ordered: true);
                    if (markerStyle is { } orderedStyle && orderedStyle is not ListMarkerStyle.Decimal and not ListMarkerStyle.LowerAlpha and not ListMarkerStyle.UpperAlpha)
                    {
                        throw new NotSupportedException($"Unsupported ordered list marker style '{orderedStyle}'.");
                    }

                    yield return new OrderedListBlock(ParseListItems(element, inheritedTextStyle), start, marginBlockEnd, markerStyle ?? ListMarkerStyle.Decimal);
                    yield break;
                }
            case "table":
                yield return ParseTable(element, inheritedTextStyle);
                yield break;
            case "thead":
            case "tbody":
            case "tr":
            case "td":
            case "th":
            case "li":
                throw new NotSupportedException($"Element '{tag}' is only supported in its valid parent context.");
            default:
                throw new NotSupportedException($"Unsupported block element '{tag}'.");
        }
    }

    private IReadOnlyList<ListItemBlock> ParseListItems(IElement listElement, TextStyle inheritedTextStyle)
    {
        var items = new List<ListItemBlock>();
        foreach (var child in listElement.Children)
        {
            if (!child.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"Unsupported list child '{child.LocalName}'. Only 'li' is supported.");
            }

            var blocks = ParseListItemBlocks(child, inheritedTextStyle);
            items.Add(new ListItemBlock(blocks));
        }

        return items;
    }

    private IReadOnlyList<RichTextBlock> ParseListItemBlocks(IElement listItemElement, TextStyle inheritedTextStyle)
    {
        var blocks = new List<RichTextBlock>();
        var pendingInlineNodes = new List<INode>();

        void FlushPending()
        {
            if (pendingInlineNodes.Count == 0)
            {
                return;
            }

            var paragraph = ParseSyntheticParagraph(pendingInlineNodes, inheritedTextStyle);
            if (paragraph is not null)
            {
                blocks.Add(paragraph);
            }

            pendingInlineNodes.Clear();
        }

        foreach (var child in listItemElement.ChildNodes)
        {
            if (IsInlineNode(child))
            {
                pendingInlineNodes.Add(child);
                continue;
            }

            FlushPending();
            foreach (var block in ParseBlockNode(child, inheritedTextStyle))
            {
                blocks.Add(block);
            }
        }

        FlushPending();
        return blocks;
    }

    private ParagraphBlock? ParseSyntheticParagraph(IReadOnlyList<INode> nodes, TextStyle inheritedTextStyle)
    {
        var inlines = ParseInlineNodes(nodes, inheritedTextStyle);
        if (inlines.Count == 0)
        {
            return null;
        }

        return new ParagraphBlock(inlines);
    }

    private TableBlock ParseTable(IElement tableElement, TextStyle inheritedTextStyle)
    {
        var styles = ParseStyleMap(tableElement);
        ValidateSupportedStyleProperties(styles, "table");

        var tableStyle = new TableStyle(
            BackgroundColor: TryGetColor(styles, "background-color"),
            BorderColor: ResolveBorderColor(styles),
            BorderWidth: ResolveBorderWidth(styles),
            CellBorderColor: TryGetColor(styles, "cell-border-color"),
            CellBorderWidth: TryGetPoint(styles, "cell-border-width") ?? 0d,
            CellPadding: TryGetPoint(styles, "cellpadding") ?? 4d,
            MarginBlockEnd: TryGetPoint(styles, "margin-bottom") ?? 0d);

        var columns = ParseTableColumns(tableElement);
        var sections = new List<TableSectionBlock>();
        var bodyRows = new List<TableRowBlock>();
        foreach (var child in tableElement.Children)
        {
            var tag = child.LocalName.ToLowerInvariant();
            switch (tag)
            {
                case "thead":
                case "tbody":
                case "tfoot":
                    var sectionRows = new List<TableRowBlock>();
                    foreach (var sectionChild in child.Children)
                    {
                        if (!sectionChild.LocalName.Equals("tr", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new NotSupportedException($"Unsupported table section child '{sectionChild.LocalName}'.");
                        }

                        sectionRows.Add(ParseTableRow(sectionChild, inheritedTextStyle, headerSection: tag == "thead"));
                    }

                    sections.Add(new TableSectionBlock(ParseSectionKind(tag), sectionRows));
                    break;
                case "tr":
                    bodyRows.Add(ParseTableRow(child, inheritedTextStyle, headerSection: false));
                    break;
                case "colgroup":
                    break;
                default:
                    throw new NotSupportedException($"Unsupported table child '{tag}'.");
            }
        }

        if (bodyRows.Count > 0)
        {
            sections.Add(new TableSectionBlock(TableSectionKind.Body, bodyRows));
        }

        if (columns.Count == 0)
        {
            columns = NormalizeCellWidthHintsToColumns(tableElement);
        }

        return new TableBlock(
            columns,
            sections,
            tableStyle,
            new TableLayoutSpec(TryGetTableWidthSpec(tableElement, styles)));
    }

    private TableRowBlock ParseTableRow(IElement rowElement, TextStyle inheritedTextStyle, bool headerSection)
    {
        var cells = new List<TableCellBlock>();
        foreach (var child in rowElement.Children)
        {
            var tag = child.LocalName.ToLowerInvariant();
            if (tag is not ("td" or "th"))
            {
                throw new NotSupportedException($"Unsupported table row child '{tag}'.");
            }

            cells.Add(ParseTableCell(child, inheritedTextStyle, headerSection || tag == "th"));
        }

        return new TableRowBlock(cells);
    }

    private TableCellBlock ParseTableCell(IElement cellElement, TextStyle inheritedTextStyle, bool isHeader)
    {
        var styles = ParseStyleMap(cellElement);
        ValidateSupportedStyleProperties(styles, cellElement.LocalName.ToLowerInvariant());

        var cellStyle = new TableCellStyle(
            BackgroundColor: TryGetColor(styles, "background-color"),
            Padding: TryGetPoint(styles, "padding"),
            TextAlign: TryGetHorizontalAlignment(styles, "text-align"));

        var blocks = ParseCellBlocks(cellElement, ResolveInheritedInlineStyle(cellElement, inheritedTextStyle));
        var colspan = ParsePositiveIntAttribute(cellElement, "colspan", 1);
        var rowspan = ParsePositiveIntAttribute(cellElement, "rowspan", 1);
        return isHeader
            ? new TableHeaderCellBlock(blocks, colspan, rowspan, cellStyle)
            : new TableDataCellBlock(blocks, colspan, rowspan, cellStyle);
    }

    private static TableSectionKind ParseSectionKind(string tag)
        => tag switch
        {
            "thead" => TableSectionKind.Header,
            "tfoot" => TableSectionKind.Footer,
            _ => TableSectionKind.Body
        };

    private static List<TableColumnDefinition> ParseTableColumns(IElement tableElement)
    {
        var columns = new List<TableColumnDefinition>();
        foreach (var colgroup in tableElement.Children.Where(x => x.LocalName.Equals("colgroup", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var col in colgroup.Children)
            {
                if (!col.LocalName.Equals("col", StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException($"Unsupported colgroup child '{col.LocalName}'.");
                }

                var styles = ParseStyleMap(col);
                ValidateSupportedStyleProperties(styles, "col");
                columns.Add(new TableColumnDefinition(
                    TryGetColumnWidthSpec(col, styles) ?? new ColumnAutoWidth(),
                    Style: new TableColumnStyle(TryGetColor(styles, "background-color"))));
            }
        }

        return columns;
    }

    private static List<TableColumnDefinition> NormalizeCellWidthHintsToColumns(IElement tableElement)
    {
        var firstRow = tableElement.QuerySelector("tr");
        if (firstRow is null)
        {
            return new List<TableColumnDefinition>();
        }

        var columns = new List<TableColumnDefinition>();
        foreach (var cell in firstRow.Children)
        {
            if (cell.LocalName is not ("td" or "th"))
            {
                continue;
            }

            var styles = ParseStyleMap(cell);
            var spec = TryGetColumnWidthSpec(cell, styles);
            var span = ParsePositiveIntAttribute(cell, "colspan", 1);
            for (var i = 0; i < span; i++)
            {
                columns.Add(new TableColumnDefinition(spec ?? new ColumnAutoWidth()));
            }
        }

        return columns;
    }

    private static TableWidthSpec TryGetTableWidthSpec(IElement element, IReadOnlyDictionary<string, string> styles)
    {
        if (styles.TryGetValue("width", out var widthValue))
        {
            return ParseTableWidthSpec(widthValue, "width");
        }

        if (element.HasAttribute("width"))
        {
            var width = element.GetAttribute("width");
            return ParseTableWidthSpec(width!, "width");
        }

        return new TableAutoWidth();
    }

    private static ColumnWidthSpec? TryGetColumnWidthSpec(IElement element, IReadOnlyDictionary<string, string> styles)
    {
        if (styles.TryGetValue("width", out var widthValue))
        {
            return ParseColumnWidthSpec(widthValue, "width");
        }

        if (element.HasAttribute("width"))
        {
            var width = element.GetAttribute("width");
            return ParseColumnWidthSpec(width!, "width");
        }

        return null;
    }

    private IReadOnlyList<RichTextBlock> ParseCellBlocks(IElement container, TextStyle inheritedTextStyle)
    {
        var blocks = new List<RichTextBlock>();
        var pendingInlineNodes = new List<INode>();

        void FlushPending()
        {
            if (pendingInlineNodes.Count == 0)
            {
                return;
            }

            var paragraph = ParseSyntheticParagraph(pendingInlineNodes, inheritedTextStyle);
            if (paragraph is not null)
            {
                blocks.Add(paragraph);
            }

            pendingInlineNodes.Clear();
        }

        foreach (var child in container.ChildNodes)
        {
            if (IsInlineNode(child))
            {
                pendingInlineNodes.Add(child);
                continue;
            }

            FlushPending();
            foreach (var block in ParseBlockNode(child, inheritedTextStyle))
            {
                blocks.Add(block);
            }
        }

        FlushPending();
        return blocks;
    }

    private IReadOnlyList<InlineNode> ParseInlineChildren(IElement container, TextStyle inheritedTextStyle)
        => ParseInlineNodes(container.ChildNodes, inheritedTextStyle);

    private IReadOnlyList<InlineNode> ParseInlineNodes(IEnumerable<INode> nodes, TextStyle inheritedTextStyle)
    {
        var inlines = new List<InlineNode>();
        foreach (var node in nodes)
        {
            ParseInlineNode(node, inheritedTextStyle, inlines);
        }

        return inlines;
    }

    private void ParseInlineNode(INode node, TextStyle inheritedTextStyle, List<InlineNode> destination)
    {
        if (node.NodeType == NodeType.Text)
        {
            if (string.IsNullOrWhiteSpace(node.TextContent))
            {
                return;
            }

            if (!string.IsNullOrEmpty(node.TextContent))
            {
                destination.Add(new TextRunNode(node.TextContent, inheritedTextStyle));
            }

            return;
        }

        if (node is not IElement element)
        {
            return;
        }

        var tag = element.LocalName.ToLowerInvariant();
        if (tag == "br")
        {
            destination.Add(new LineBreakNode());
            return;
        }

        if (tag is "p" or "div" or "ul" or "ol" or "li" or "table" or "thead" or "tbody" or "tr" or "td" or "th" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
        {
            throw new NotSupportedException($"Block element '{tag}' is not supported inside inline content.");
        }

        var nextStyle = ResolveInheritedInlineStyle(element, inheritedTextStyle);
        foreach (var child in element.ChildNodes)
        {
            ParseInlineNode(child, nextStyle, destination);
        }
    }

    private TextStyle ResolveInheritedInlineStyle(IElement element, TextStyle inheritedStyle)
    {
        var styles = ParseStyleMap(element);
        ValidateSupportedStyleProperties(styles, element.LocalName.ToLowerInvariant());

        var style = inheritedStyle;
        style = tagApplyInlineDefaults(element.LocalName.ToLowerInvariant(), style);

        if (styles.TryGetValue("font-family", out var familyName))
        {
            style = style with { FamilyName = UnquoteCssValue(familyName) };
        }

        if (styles.TryGetValue("font-size", out var fontSize))
        {
            style = style with { FontSize = ParsePoint(fontSize, "font-size") };
        }

        if (styles.TryGetValue("font-weight", out var fontWeight))
        {
            style = style with { Weight = ParseFontWeight(fontWeight) };
        }

        if (styles.TryGetValue("font-style", out var fontStyle))
        {
            style = style with { Italic = ParseFontStyle(fontStyle) };
        }

        if (styles.TryGetValue("letter-spacing", out var letterSpacing))
        {
            style = style with { CharacterSpacing = ParsePoint(letterSpacing, "letter-spacing") };
        }

        if (styles.TryGetValue("word-spacing", out var wordSpacing))
        {
            style = style with { WordSpacing = ParsePoint(wordSpacing, "word-spacing") };
        }

        if (styles.TryGetValue("color", out var foregroundColor))
        {
            style = style with { ForegroundColor = ParseColor(foregroundColor, "color") };
        }

        if (styles.TryGetValue("background-color", out var backgroundColor))
        {
            style = style with { BackgroundColor = ParseColor(backgroundColor, "background-color") };
        }

        if (styles.TryGetValue("text-decoration", out var textDecoration))
        {
            var decorations = ParseTextDecoration(textDecoration);
            style = style with
            {
                Underline = decorations.Underline,
                StrikeThrough = decorations.StrikeThrough
            };
        }

        return style;
    }

    private static TextStyle tagApplyInlineDefaults(string tag, TextStyle style)
        => tag switch
        {
            "b" or "strong" => style with { Weight = Math.Max(style.Weight, 700) },
            "i" or "em" => style with { Italic = true },
            "u" => style with { Underline = true },
            "s" => style with { StrikeThrough = true },
            "span" => style,
            _ => style
        };

    private ParagraphStyle ResolveParagraphStyle(IElement element)
    {
        var styles = ParseStyleMap(element);
        ValidateSupportedStyleProperties(styles, element.LocalName.ToLowerInvariant());
        return new ParagraphStyle(
            TextAlign: TryGetHorizontalAlignment(styles, "text-align") ?? TextHorizontalAlignment.Left,
            LineHeight: TryGetPoint(styles, "line-height"),
            MarginBlockEnd: TryGetPoint(styles, "margin-bottom") ?? 0d,
            MarginBlockStart: TryGetPoint(styles, "margin-top") ?? 0d);
    }

    private static Dictionary<string, string> ParseStyleMap(IElement element)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var styleText = element.GetAttribute("style");
        if (string.IsNullOrWhiteSpace(styleText))
        {
            return map;
        }

        foreach (var declaration in styleText.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = declaration.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex == declaration.Length - 1)
            {
                throw new NotSupportedException($"Unsupported style declaration '{declaration}'.");
            }

            var name = declaration[..separatorIndex].Trim().ToLowerInvariant();
            var value = declaration[(separatorIndex + 1)..].Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            map[name] = value;
        }

        return map;
    }

    private static void ValidateSupportedStyleProperties(IReadOnlyDictionary<string, string> styles, string context)
    {
        foreach (var key in styles.Keys)
        {
            var support = GetSupportLevel(context, key);
            if (support == HtmlFixtureSupportLevel.Supported)
            {
                continue;
            }

            if (support == HtmlFixtureSupportLevel.LayoutPending)
            {
                throw new NotSupportedException($"Style property '{key}' on <{context}> is recognized by the model but not supported end-to-end in conformance fixtures.");
            }

            if (support == HtmlFixtureSupportLevel.Unsupported)
            {
                throw new NotSupportedException($"Unsupported style property '{key}' on <{context}>.");
            }
        }
    }

    private static HtmlFixtureSupportLevel GetSupportLevel(string context, string propertyName)
    {
        var contextName = context.ToLowerInvariant();
        var property = propertyName.ToLowerInvariant();

        if (contextName is "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6")
        {
            return property switch
            {
                "text-align" or "line-height" or "margin-bottom" or "margin-top" => HtmlFixtureSupportLevel.Supported,
                "text-indent" or "white-space" or "overflow-wrap" or "word-break" => HtmlFixtureSupportLevel.LayoutPending,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        if (contextName is "span" or "b" or "strong" or "i" or "em" or "u" or "s")
        {
            return property switch
            {
                "font-family" or "font-size" or "font-weight" or "font-style" or "color" or "background-color" or "text-decoration" or "letter-spacing" or "word-spacing" => HtmlFixtureSupportLevel.Supported,
                "line-height" or "text-align" or "margin-bottom" or "margin-top" or "padding" or "border" or "border-color" or "border-width" or "width" or "list-style-type" or "cellpadding" or "cell-border-color" or "cell-border-width" => HtmlFixtureSupportLevel.LayoutPending,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        if (contextName is "ul" or "ol")
        {
            return property switch
            {
                "margin-bottom" or "list-style-type" => HtmlFixtureSupportLevel.Supported,
                "padding-inline-start" or "list-style-position" => HtmlFixtureSupportLevel.LayoutPending,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        if (contextName == "table")
        {
            return property switch
            {
                "background-color" or "border" or "border-color" or "border-width" or "margin-bottom" or "cellpadding" or "cell-border-color" or "cell-border-width" or "width" or "table-layout" or "box-sizing" => HtmlFixtureSupportLevel.Supported,
                "vertical-align" => HtmlFixtureSupportLevel.LayoutPending,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        if (contextName is "td" or "th")
        {
            return property switch
            {
                "background-color" or "padding" or "text-align" or "width" or "box-sizing" => HtmlFixtureSupportLevel.Supported,
                "vertical-align" => HtmlFixtureSupportLevel.LayoutPending,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        if (contextName == "col")
        {
            return property switch
            {
                "width" or "background-color" or "box-sizing" => HtmlFixtureSupportLevel.Supported,
                _ => HtmlFixtureSupportLevel.Unsupported
            };
        }

        return HtmlFixtureSupportLevel.Unsupported;
    }

    private static bool IsInlineNode(INode node)
    {
        if (node.NodeType == NodeType.Text)
        {
            return true;
        }

        if (node is not IElement element)
        {
            return false;
        }

        return element.LocalName.ToLowerInvariant() is "span" or "b" or "strong" or "i" or "em" or "u" or "s" or "br";
    }

    private static int ParsePositiveIntAttribute(IElement element, string attributeName, int defaultValue)
    {
        if (!element.HasAttribute(attributeName))
        {
            return defaultValue;
        }

        var value = element.GetAttribute(attributeName);
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed < 1)
        {
            throw new NotSupportedException($"Unsupported {attributeName} value '{value}'.");
        }

        return parsed;
    }

    private static double ParsePoint(string value, string propertyName)
    {
        var normalized = value.Trim();
        if (normalized.EndsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^2].Trim();
        }

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            throw new NotSupportedException($"Unsupported point value '{value}' for '{propertyName}'.");
        }

        return result;
    }

    private static double ParsePercent(string value, string propertyName)
    {
        var normalized = value.Trim();
        if (normalized.EndsWith('%'))
        {
            normalized = normalized[..^1].Trim();
        }

        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            throw new NotSupportedException($"Unsupported percent value '{value}' for '{propertyName}'.");
        }

        return result;
    }

    private static TableWidthSpec ParseTableWidthSpec(string value, string propertyName)
        => value.Trim().EndsWith('%')
            ? new TablePercentWidth(ParsePercent(value, propertyName))
            : new TableFixedWidth(ParsePoint(value, propertyName));

    private static ColumnWidthSpec ParseColumnWidthSpec(string value, string propertyName)
        => value.Trim().EndsWith('%')
            ? new ColumnPercentWidth(ParsePercent(value, propertyName))
            : new ColumnFixedWidth(ParsePoint(value, propertyName));

    private static int ParseFontWeight(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "normal" => 400,
            "bold" => 700,
            _ when int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var weight) => weight,
            _ => throw new NotSupportedException($"Unsupported font-weight '{value}'.")
        };
    }

    private static bool ParseFontStyle(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "normal" => false,
            "italic" => true,
            _ => throw new NotSupportedException($"Unsupported font-style '{value}'.")
        };

    private static (bool Underline, bool StrikeThrough) ParseTextDecoration(string value)
    {
        var underline = false;
        var strikeThrough = false;
        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            switch (token.Trim().ToLowerInvariant())
            {
                case "none":
                    underline = false;
                    strikeThrough = false;
                    break;
                case "underline":
                    underline = true;
                    break;
                case "line-through":
                    strikeThrough = true;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported text-decoration token '{token}'.");
            }
        }

        return (underline, strikeThrough);
    }

    private static TextColor ParseColor(string value, string propertyName)
    {
        var normalized = value.Trim();
        if (normalized.StartsWith('#'))
        {
            if (normalized.Length != 7)
            {
                throw new NotSupportedException($"Unsupported color value '{value}' for '{propertyName}'.");
            }

            return new TextColor(
                byte.Parse(normalized.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                byte.Parse(normalized.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                byte.Parse(normalized.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        }

        if (normalized.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && normalized.EndsWith(')'))
        {
            var parts = normalized[4..^1].Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                throw new NotSupportedException($"Unsupported color value '{value}' for '{propertyName}'.");
            }

            return new TextColor(
                byte.Parse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture),
                byte.Parse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture),
                byte.Parse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture));
        }

        throw new NotSupportedException($"Unsupported color value '{value}' for '{propertyName}'.");
    }

    private static TextColor? TryGetColor(IReadOnlyDictionary<string, string> styles, string propertyName)
        => styles.TryGetValue(propertyName, out var value) ? ParseColor(value, propertyName) : null;

    private static double? TryGetPoint(IReadOnlyDictionary<string, string> styles, string propertyName)
        => styles.TryGetValue(propertyName, out var value) ? ParsePoint(value, propertyName) : null;

    private static TextHorizontalAlignment? TryGetHorizontalAlignment(IReadOnlyDictionary<string, string> styles, string propertyName)
    {
        if (!styles.TryGetValue(propertyName, out var value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "left" => TextHorizontalAlignment.Left,
            "center" => TextHorizontalAlignment.Center,
            "right" => TextHorizontalAlignment.Right,
            _ => throw new NotSupportedException($"Unsupported text-align '{value}'.")
        };
    }

    private static double ResolveBorderWidth(IReadOnlyDictionary<string, string> styles)
    {
        if (styles.TryGetValue("border-width", out var borderWidth))
        {
            return ParsePoint(borderWidth, "border-width");
        }

        if (styles.TryGetValue("border", out var border))
        {
            var tokens = border.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length < 2 || !tokens.Any(x => x.Equals("solid", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException($"Unsupported border value '{border}'.");
            }

            return ParsePoint(tokens[0], "border");
        }

        return 0d;
    }

    private static TextColor? ResolveBorderColor(IReadOnlyDictionary<string, string> styles)
    {
        if (styles.TryGetValue("border-color", out var borderColor))
        {
            return ParseColor(borderColor, "border-color");
        }

        if (styles.TryGetValue("border", out var border))
        {
            var tokens = border.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length < 3)
            {
                return null;
            }

            return ParseColor(tokens[^1], "border");
        }

        return null;
    }

    private static ListMarkerStyle? TryGetListMarkerStyle(IReadOnlyDictionary<string, string> styles, bool ordered)
    {
        if (!styles.TryGetValue("list-style-type", out var value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "disc" => ListMarkerStyle.Disc,
            "circle" => ListMarkerStyle.Circle,
            "square" => ListMarkerStyle.Square,
            "decimal" => ListMarkerStyle.Decimal,
            "lower-alpha" => ListMarkerStyle.LowerAlpha,
            "upper-alpha" => ListMarkerStyle.UpperAlpha,
            _ => throw new NotSupportedException($"Unsupported list-style-type '{value}' for {(ordered ? "ordered" : "unordered")} list.")
        };
    }

    private static string UnquoteCssValue(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && ((trimmed[0] == '\'' && trimmed[^1] == '\'') || (trimmed[0] == '"' && trimmed[^1] == '"')))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    private void WriteBlock(StringBuilder sb, RichTextBlock block)
    {
        switch (block)
        {
            case ParagraphBlock paragraph:
                WriteFlowBlock(sb, "p", paragraph.Inlines, paragraph.Style);
                break;
            case HeadingBlock heading:
                WriteFlowBlock(sb, $"h{Math.Clamp(heading.Level, 1, 6)}", heading.Inlines, heading.Style);
                break;
            case UnorderedListBlock unordered:
                WriteListBlock(sb, "ul", unordered.Items, unordered.MarginBlockEnd, unordered.MarkerStyle, null);
                break;
            case OrderedListBlock ordered:
                WriteListBlock(sb, "ol", ordered.Items, ordered.MarginBlockEnd, ordered.MarkerStyle, ordered.StartIndex);
                break;
            case TableBlock table:
                WriteTable(sb, table);
                break;
            default:
                throw new NotSupportedException($"Unsupported block type '{block.GetType().Name}' for HTML normalization.");
        }
    }

    private void WriteFlowBlock(StringBuilder sb, string tagName, IReadOnlyList<InlineNode> inlines, ParagraphStyle? style)
    {
        sb.Append('<').Append(tagName);
        AppendStyleAttribute(sb, BuildParagraphStyle(style));
        sb.Append('>');
        foreach (var inline in inlines)
        {
            WriteInline(sb, inline);
        }
        sb.Append("</").Append(tagName).Append('>');
    }

    private void WriteListBlock(StringBuilder sb, string tagName, IReadOnlyList<ListItemBlock> items, double marginBlockEnd, ListMarkerStyle? markerStyle, int? startIndex)
    {
        sb.Append('<').Append(tagName);
        if (startIndex.HasValue && startIndex.Value != 1)
        {
            sb.Append(" start=\"").Append(startIndex.Value.ToString(CultureInfo.InvariantCulture)).Append('"');
        }

        var style = new List<string>();
        if (marginBlockEnd > 0)
        {
            style.Add($"margin-bottom:{FormatPoint(marginBlockEnd)}");
        }

        if (markerStyle.HasValue)
        {
            style.Add($"list-style-type:{ToCssListMarkerStyle(markerStyle.Value)}");
        }

        AppendStyleAttribute(sb, style);
        sb.Append('>');
        foreach (var item in items)
        {
            sb.Append("<li>");
            foreach (var block in item.Blocks)
            {
                WriteBlock(sb, block);
            }
            sb.Append("</li>");
        }
        sb.Append("</").Append(tagName).Append('>');
    }

    private void WriteTable(StringBuilder sb, TableBlock table)
    {
        sb.Append("<table");
        AppendStyleAttribute(sb, BuildTableStyle(table));
        sb.Append('>');

        if (table.Columns.Count > 0)
        {
            sb.Append("<colgroup>");
            foreach (var column in table.Columns)
            {
                sb.Append("<col");
                AppendStyleAttribute(sb, BuildTableColumnStyle(column));
                sb.Append("/>");
            }
            sb.Append("</colgroup>");
        }

        foreach (var section in table.Sections)
        {
            var sectionTag = section.Kind switch
            {
                TableSectionKind.Header => "thead",
                TableSectionKind.Footer => "tfoot",
                _ => "tbody"
            };
            sb.Append('<').Append(sectionTag).Append('>');
            foreach (var row in section.Rows)
            {
                sb.Append("<tr>");
                foreach (var cell in row.Cells)
                {
                    WriteTableCell(sb, cell);
                }
                sb.Append("</tr>");
            }
            sb.Append("</").Append(sectionTag).Append('>');
        }
        sb.Append("</table>");
    }

    private void WriteTableCell(StringBuilder sb, TableCellBlock cell)
    {
        var tagName = cell is TableHeaderCellBlock ? "th" : "td";
        sb.Append('<').Append(tagName);
        if (cell.ColSpan > 1)
        {
            sb.Append(" colspan=\"").Append(cell.ColSpan.ToString(CultureInfo.InvariantCulture)).Append('"');
        }
        if (cell.RowSpan > 1)
        {
            sb.Append(" rowspan=\"").Append(cell.RowSpan.ToString(CultureInfo.InvariantCulture)).Append('"');
        }
        AppendStyleAttribute(sb, BuildTableCellStyle(cell.Style));
        sb.Append('>');
        foreach (var block in cell.Blocks)
        {
            WriteBlock(sb, block);
        }
        sb.Append("</").Append(tagName).Append('>');
    }

    private void WriteInline(StringBuilder sb, InlineNode inline)
    {
        switch (inline)
        {
            case LineBreakNode:
                sb.Append("<br/>");
                break;
            case TextRunNode run:
                sb.Append("<span");
                AppendStyleAttribute(sb, BuildInlineStyle(run.Style));
                sb.Append('>');
                sb.Append(WebUtility.HtmlEncode(run.Text));
                sb.Append("</span>");
                break;
            default:
                throw new NotSupportedException($"Unsupported inline type '{inline.GetType().Name}' for HTML normalization.");
        }
    }

    private static IReadOnlyList<string> BuildParagraphStyle(ParagraphStyle? style)
    {
        if (style is null)
        {
            return Array.Empty<string>();
        }

        var result = new List<string>
        {
            $"text-align:{ToCssAlign(style.TextAlign)}"
        };
        if (style.LineHeight.HasValue)
        {
            result.Add($"line-height:{FormatPoint(style.LineHeight.Value)}");
        }
        if (style.MarginBlockStart > 0)
        {
            result.Add($"margin-top:{FormatPoint(style.MarginBlockStart)}");
        }
        if (style.MarginBlockEnd > 0)
        {
            result.Add($"margin-bottom:{FormatPoint(style.MarginBlockEnd)}");
        }
        return result;
    }

    private static IReadOnlyList<string> BuildInlineStyle(TextStyle style)
    {
        var result = new List<string>
        {
            $"font-family:'{EscapeCss(style.FamilyName)}'",
            $"font-size:{FormatPoint(style.FontSize)}",
            $"font-weight:{style.Weight.ToString(CultureInfo.InvariantCulture)}",
            $"font-style:{(style.Italic ? "italic" : "normal")}"
        };
        if (style.CharacterSpacing != 0)
        {
            result.Add($"letter-spacing:{FormatPoint(style.CharacterSpacing)}");
        }
        if (style.WordSpacing != 0)
        {
            result.Add($"word-spacing:{FormatPoint(style.WordSpacing)}");
        }
        if (style.ForegroundColor is { } foregroundColor)
        {
            result.Add($"color:{ToCssColor(foregroundColor)}");
        }
        if (style.BackgroundColor is { } backgroundColor)
        {
            result.Add($"background-color:{ToCssColor(backgroundColor)}");
        }
        if (style.Underline || style.StrikeThrough)
        {
            result.Add($"text-decoration:{BuildTextDecoration(style)}");
        }
        return result;
    }

    private static IReadOnlyList<string> BuildTableStyle(TableBlock table)
    {
        var style = table.Style;
        var result = new List<string> { "box-sizing:border-box" };
        AppendWidthDeclaration(result, table.Width);
        if (style is null)
        {
            return result;
        }

        if (style.BackgroundColor is { } backgroundColor)
        {
            result.Add($"background-color:{ToCssColor(backgroundColor)}");
        }
        if (style.BorderWidth > 0)
        {
            result.Add($"border:{FormatPoint(style.BorderWidth)} solid {ToCssColor(style.BorderColor)}");
        }
        if (style.CellBorderColor is { } cellBorderColor)
        {
            result.Add($"cell-border-color:{ToCssColor(cellBorderColor)}");
        }
        if (style.CellBorderWidth > 0)
        {
            result.Add($"cell-border-width:{FormatPoint(style.CellBorderWidth)}");
        }
        if (style.CellPadding > 0)
        {
            result.Add($"cellpadding:{FormatPoint(style.CellPadding)}");
        }
        if (style.MarginBlockEnd > 0)
        {
            result.Add($"margin-bottom:{FormatPoint(style.MarginBlockEnd)}");
        }
        return result;
    }

    private static IReadOnlyList<string> BuildTableColumnStyle(TableColumnDefinition column)
    {
        var result = new List<string> { "box-sizing:border-box" };
        if (column.Style?.BackgroundColor is { } backgroundColor)
        {
            result.Add($"background-color:{ToCssColor(backgroundColor)}");
        }

        switch (column.Width)
        {
            case ColumnFixedWidth fixedWidth:
                result.Add($"width:{FormatPoint(fixedWidth.Points)}");
                break;
            case ColumnPercentWidth percentWidth:
                result.Add($"width:{FormatPercent(percentWidth.Percent)}");
                break;
        }

        return result;
    }

    private static IReadOnlyList<string> BuildTableCellStyle(TableCellStyle? style)
    {
        var result = new List<string> { "box-sizing:border-box" };
        if (style?.BackgroundColor is { } backgroundColor)
        {
            result.Add($"background-color:{ToCssColor(backgroundColor)}");
        }
        if (style?.Padding is { } padding)
        {
            result.Add($"padding:{FormatPoint(padding)}");
        }
        if (style?.TextAlign is { } textAlign)
        {
            result.Add($"text-align:{ToCssAlign(textAlign)}");
        }
        if (style?.VerticalAlign is { } verticalAlign)
        {
            result.Add($"vertical-align:{verticalAlign.ToString().ToLowerInvariant()}");
        }
        return result;
    }

    private static void AppendWidthDeclaration(List<string> declarations, TableWidthSpec width)
    {
        switch (width)
        {
            case TableFixedWidth fixedWidth:
                declarations.Add($"width:{FormatPoint(fixedWidth.Points)}");
                break;
            case TablePercentWidth percentWidth:
                declarations.Add($"width:{FormatPercent(percentWidth.Percent)}");
                break;
            case TableAutoWidth:
                declarations.Add("width:100%");
                break;
        }
    }

    private static void AppendStyleAttribute(StringBuilder sb, IReadOnlyList<string> declarations)
    {
        if (declarations.Count == 0)
        {
            return;
        }

        sb.Append(" style=\"");
        for (var i = 0; i < declarations.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(';');
            }
            sb.Append(declarations[i]);
        }
        sb.Append('"');
    }

    private static string BuildTextDecoration(TextStyle style)
    {
        if (style.Underline && style.StrikeThrough)
        {
            return "underline line-through";
        }

        if (style.Underline)
        {
            return "underline";
        }

        if (style.StrikeThrough)
        {
            return "line-through";
        }

        return "none";
    }

    private static string ToCssAlign(TextHorizontalAlignment alignment)
        => alignment switch
        {
            TextHorizontalAlignment.Left => "left",
            TextHorizontalAlignment.Center => "center",
            TextHorizontalAlignment.Right => "right",
            _ => "left"
        };

    private static string ToCssListMarkerStyle(ListMarkerStyle style)
        => style switch
        {
            ListMarkerStyle.Disc => "disc",
            ListMarkerStyle.Circle => "circle",
            ListMarkerStyle.Square => "square",
            ListMarkerStyle.Decimal => "decimal",
            ListMarkerStyle.LowerAlpha => "lower-alpha",
            ListMarkerStyle.UpperAlpha => "upper-alpha",
            _ => "disc"
        };

    private static string ToCssColor(TextColor? color)
        => color is null ? "transparent" : $"#{color.Value.R:x2}{color.Value.G:x2}{color.Value.B:x2}";

    private static string EscapeCss(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);

    private static string FormatPoint(double value)
        => $"{value.ToString("0.###", CultureInfo.InvariantCulture)}pt";

    private static string FormatPercent(double value)
        => $"{value.ToString("0.###", CultureInfo.InvariantCulture)}%";
}
