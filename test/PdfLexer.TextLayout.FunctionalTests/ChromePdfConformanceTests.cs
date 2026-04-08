using System.Text;
using pdflexer.TestCaseGen;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.TextLayout.FunctionalTests;

public partial class ChromePdfConformanceTests
{
    private const double InternalRenderScaleFactor = 1.0;
    private const double PositionTolerance = 6.0;
    private const double RelativeLineXTolerance = 1.5;
    private const double RelativeLineYTolerance = 2.25;
    private const double RelativeWordXTolerance = 4.0;
    private const double RelativeWordYTolerance = 1.5;
    private const double LineGroupingTolerance = 1.0;
    private const double SizeTolerance = 8.0;
    private const double ReviewMargin = 24.0;
    private const double ReviewGutter = 24.0;
    private const double ReviewTargetPreviewWidth = 540.0;
    private const double ReviewTargetPreviewHeight = 720.0;
    private const string SansFamily = "FixtureSans";
    private const string SerifFamily = "FixtureSerif";
    private const string MonoFamily = "FixtureMono";
    private const string CondensedFamily = "FixtureCondensed";
    private const string SemiCondensedFamily = "FixtureSemiCondensed";
    private static readonly string TestRoot = PathUtil.GetPathFromSegmentOfCurrent("test");
    private static readonly IReadOnlyList<ConformanceCase> AllConformanceCases = BuildConformanceCases();

    public static IEnumerable<object[]> ConformanceCases()
    {
        foreach (var testCase in AllConformanceCases)
        {
            yield return new object[] { testCase.Name };
        }
    }

    private static IReadOnlyList<ConformanceCase> BuildConformanceCases()
    {
        var cases = new List<ConformanceCase>
        {
            ConformanceCase.FromSegments(
                "ExplicitLines",
                new[]
                {
                    new TextSegment("Hello  world\nNext line", new TextSegmentStyle(SansFamily, 400, 14, Underline: true, LineSpacing: 18))
                }),

            ConformanceCase.FromSegments(
                "WrappedTextNarrow",
                new[]
                {
                    new TextSegment("This is a wrapped sample that should span multiple lines in both engines.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
                },
                boxWidth: 150,
                boxHeight: 90),

            ConformanceCase.FromSegments(
                "WrappedTextCenterAligned",
                new[]
                {
                    new TextSegment("Centered wrapped text should preserve word ordering and similar line breaks.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
                },
                horizontalAlignment: TextHorizontalAlignment.Center,
                boxWidth: 150,
                boxHeight: 90),

            ConformanceCase.FromSegments(
                "WrappedTextRightAligned",
                new[]
                {
                    new TextSegment("Right aligned wrapped text exercises horizontal offset calculations.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
                },
                horizontalAlignment: TextHorizontalAlignment.Right,
                boxWidth: 150,
                boxHeight: 90),

            ConformanceCase.FromSegments(
                "WordSpacing",
                new[]
                {
                    new TextSegment("Word spacing should remain comparable between browser and pdf output.", new TextSegmentStyle(SansFamily, 400, 12, WordSpacing: 4, LineSpacing: 15))
                },
                boxWidth: 165,
                boxHeight: 90),

            ConformanceCase.FromSegments(
                "CharacterSpacing",
                new[]
                {
                    new TextSegment("Character spacing affects measured width and wrapping decisions.", new TextSegmentStyle(SansFamily, 400, 12, CharacterSpacing: 0.75, LineSpacing: 15))
                },
                boxWidth: 175,
                boxHeight: 90),

            ConformanceCase.FromSegments(
                "TallLineSpacing",
                new[]
                {
                    new TextSegment("Larger line spacing should keep baselines and word grouping comparable across lines.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 22))
                },
                boxWidth: 160,
                boxHeight: 110),

            ConformanceCase.FromSegments(
                "TrailingWhitespaceAndBlankLine",
                new[]
                {
                    new TextSegment("Alpha   \n\nBeta", new TextSegmentStyle(SansFamily, 400, 13, LineSpacing: 18))
                },
                boxWidth: 180,
                boxHeight: 110),

            ConformanceCase.FromSegments(
                "PaddedBorderedTextBox",
                new[]
                {
                    new TextSegment("Padded text should shift inward from the border box.", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 15))
                },
                boxWidth: 180,
                boxHeight: 90,
                boxStyle: new TextBoxStyle(
                    BackgroundColor: new TextColor(240, 248, 255),
                    BorderColor: new TextColor(30, 80, 140),
                    BorderWidth: 2,
                    BorderRadius: 8,
                    Padding: 10)),

            ConformanceCase.FromSegments(
                "MixedSegmentsMultipleSizesAndFamilies",
                new[]
                {
                    new TextSegment("Lead sentence in sans. ", new TextSegmentStyle(SansFamily, 400, 12, LineSpacing: 16)),
                    new TextSegment("A larger bold interruption ", new TextSegmentStyle(SansFamily, 700, 15, LineSpacing: 18)),
                    new TextSegment("followed by a serif clause and a mono tail.", new TextSegmentStyle(SerifFamily, 400, 13, LineSpacing: 17)),
                    new TextSegment(" code-span-like tail", new TextSegmentStyle(MonoFamily, 400, 11, LineSpacing: 15, CharacterSpacing: 0.3))
                },
                boxWidth: 185,
                boxHeight: 95)
        };

        var richStyle = new TextStyle(SansFamily, 400, 12);
        var sansBoldStyle = new TextStyle(SansFamily, 700, 12);
        var sansItalicStyle = new TextStyle(SansFamily, 400, 12, Italic: true);
        var sansBoldItalicStyle = new TextStyle(SansFamily, 700, 12, Italic: true);
        var serifStyle = new TextStyle(SerifFamily, 400, 13);
        var serifBoldStyle = new TextStyle(SerifFamily, 700, 13);
        var monoStyle = new TextStyle(MonoFamily, 400, 11);
        var condensedStyle = new TextStyle(CondensedFamily, 400, 12);
        var condensedBoldStyle = new TextStyle(CondensedFamily, 700, 12);
        var condensedItalicStyle = new TextStyle(CondensedFamily, 400, 12, Italic: true);
        var semiCondensedStyle = new TextStyle(SemiCondensedFamily, 400, 12);
        cases.Add(
            ConformanceCase.FromBlocks(
                "RichTextColorAndBackground",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Color ", new TextStyle(SansFamily, 400, 12, ForegroundColor: new TextColor(200, 20, 20))),
                            new TextRunNode("background", new TextStyle(SansFamily, 400, 12, BackgroundColor: new TextColor(250, 240, 120))),
                            new TextRunNode(" sample", new TextStyle(SansFamily, 400, 12, ForegroundColor: new TextColor(20, 80, 180)))
                        },
                        new ParagraphStyle(LineHeight: 16))
                },
                boxWidth: 180,
                boxHeight: 70));

        cases.Add(
            ConformanceCase.FromHtml(
                "RichParagraphSpacingAndAlignment",
                $$"""
                <p style="text-align:center; line-height:18pt; margin-bottom:10pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:13pt;">Centered heading line</span>
                </p>
                <p style="line-height:16pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Second paragraph should sit lower because margin block end is explicit.</span>
                </p>
                """,
                richStyle,
                boxWidth: 180,
                boxHeight: 90));

        cases.Add(
            ConformanceCase.FromHtml(
                "HeadingAndParagraph",
                $$"""
                <h2 style="line-height:20pt; margin-bottom:8pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:16pt;">Section Heading</span>
                </h2>
                <p style="line-height:16pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">The body paragraph beneath the heading should wrap consistently.</span>
                </p>
                """,
                richStyle,
                boxWidth: 170,
                boxHeight: 100));

        cases.Add(
            ConformanceCase.FromHtml(
                "HtmlInlineFontVariants",
                $$"""
                <p style="line-height:18pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Sans regular. </span>
                  <span style="font-family:'{{SerifFamily}}'; font-weight:700; font-size:13pt;">Serif bold. </span>
                  <span style="font-family:'{{CondensedFamily}}'; font-weight:400; font-size:12pt; font-style:italic;">Condensed italic. </span>
                  <span style="font-family:'{{MonoFamily}}'; font-weight:400; font-size:11pt;">Mono tail.</span>
                </p>
                """,
                richStyle,
                boxWidth: 188,
                boxHeight: 88));

        cases.Add(
            ConformanceCase.FromHtml(
                "HtmlInlineColorAndDecorationStyles",
                $$"""
                <p style="line-height:17pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; color:#b41424;">Color sample</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> with </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; background-color:#f7e39a;">background highlight</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> plus </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; text-decoration:underline;">underline</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> and </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; text-decoration:line-through;">strike</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> styling.</span>
                </p>
                """,
                richStyle,
                boxWidth: 188,
                boxHeight: 88));

        cases.Add(
            ConformanceCase.FromHtml(
                "HtmlInlineCharacterAndWordSpacing",
                $$"""
                <p style="line-height:17pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; letter-spacing:0.8pt;">Tracking sample</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> beside </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; word-spacing:3.5pt;">wider word spacing across this phrase</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> for comparison.</span>
                </p>
                """,
                richStyle,
                boxWidth: 192,
                boxHeight: 88));

        cases.Add(
            ConformanceCase.FromHtml(
                "HtmlParagraphMarginsAndRightAlignment",
                $$"""
                <p style="line-height:16pt; margin-top:6pt; margin-bottom:10pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">First paragraph uses both top and bottom margins to exercise stacked block spacing.</span>
                </p>
                <p style="text-align:right; line-height:16pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">The second paragraph is right aligned and should begin lower because of the prior margins.</span>
                </p>
                """,
                richStyle,
                boxWidth: 184,
                boxHeight: 112));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticParagraphStackSameStyleNoMargins",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First paragraph uses the baseline rich text style and wraps twice for a clean paragraph stacking comparison.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Second paragraph uses the exact same style and line height so any vertical drift is easier to attribute to block stacking rather than font changes.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Third paragraph repeats the same conditions again and should sit directly after the previous paragraph with no extra space.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 165));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticParagraphStackSameStyleWithMargins",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Paragraph one uses the same text style but adds an explicit paragraph end margin.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Paragraph two also uses the same style and a different explicit margin so the test isolates block spacing math.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 6)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Paragraph three closes the stack without extra margin.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 150));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticParagraphStackMixedSizesSameFamily",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Large opener paragraph in the same sans family.", new TextStyle(SansFamily, 400, 18))
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Middle paragraph drops to the normal size while keeping the same family and alignment.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Small closing paragraph checks whether size changes alone cause the accumulated drift.", new TextStyle(SansFamily, 400, 11))
                        },
                        new ParagraphStyle(LineHeight: 14, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 145,
                metricPreference: TextFontMetricSource.HorizontalHeader));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticParagraphAlignmentMatrixSameSize",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Centered paragraph with the default style and no family change.", richStyle)
                        },
                        new ParagraphStyle(TextAlign: TextHorizontalAlignment.Center, LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Left aligned paragraph uses the same style to isolate alignment from vertical spacing.", richStyle)
                        },
                        new ParagraphStyle(TextAlign: TextHorizontalAlignment.Left, LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Right aligned paragraph closes the sequence under the same metrics.", richStyle)
                        },
                        new ParagraphStyle(TextAlign: TextHorizontalAlignment.Right, LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 125));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticParagraphLineHeightMatrix",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph uses a tight line height and wraps over multiple lines for comparison.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 14, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph uses a medium line height under the same font size and family.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 18, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph uses a tall line height so the suite can isolate leading differences from paragraph margins.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 190));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticHeadingThenSameFamilyParagraph",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        2,
                        new InlineNode[]
                        {
                            new TextRunNode("Diagnostic Heading", new TextStyle(SansFamily, 700, 18))
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph keeps the same family and uses extraction safe vocabulary so the case isolates heading to paragraph stacking.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 185,
                boxHeight: 115));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticHeadingThenLargerSerifParagraph",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        2,
                        new InlineNode[]
                        {
                            new TextRunNode("Heading Above Serif Body", new TextStyle(SansFamily, 700, 18))
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A larger serif paragraph follows the heading and uses plain wording to isolate family and size transitions across block boundaries.", serifStyle with { FontSize = 14 })
                        },
                        new ParagraphStyle(LineHeight: 19, MarginBlockEnd: 0))
                },
                boxWidth: 190,
                boxHeight: 135,
                metricPreference: TextFontMetricSource.Windows));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticInlineStylesWithinParagraphStack",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("The first paragraph includes ", richStyle),
                            new TextRunNode("highlighted words", richStyle with { BackgroundColor = new TextColor(248, 240, 150) }),
                            new TextRunNode(" and ", richStyle),
                            new TextRunNode("underlined words", richStyle with { Underline = true }),
                            new TextRunNode(" while keeping otherwise simple content.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("The second paragraph switches runs between sans and serif styles without using lists or punctuation that tends to split extraction.", richStyle),
                            new TextRunNode(" Serif run sample.", serifStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 190,
                boxHeight: 145));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticBoxStyledMultiParagraphRichText",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        3,
                        new InlineNode[]
                        {
                            new TextRunNode("Box Styled Diagnostic", new TextStyle(SansFamily, 700, 16))
                        },
                        new ParagraphStyle(LineHeight: 20, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph sits inside a padded and bordered content box so the case can isolate container insets from paragraph stacking.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A second paragraph keeps the same family and line height to make any drift inside the box easier to interpret.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 190,
                boxHeight: 150,
                boxStyle: new TextBoxStyle(
                    BackgroundColor: new TextColor(252, 252, 250),
                    BorderColor: new TextColor(190, 190, 190),
                    BorderWidth: 1,
                    Padding: 10)));

        cases.Add(
            ConformanceCase.FromBlocks(
                "DiagnosticMixedFamiliesAcrossParagraphBoundary",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A sans paragraph establishes the first block and wraps enough to create more than one line for comparison.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A serif paragraph follows with the same nominal size and safe wording so the case isolates the family transition only.", new TextStyle(SerifFamily, 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A mono paragraph closes the sequence and checks whether the next family change creates the same drift.", new TextStyle(MonoFamily, 400, 12))
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 0))
                },
                boxWidth: 190,
                boxHeight: 155));

        cases.Add(
            ConformanceCase.FromBlocks(
                "UnorderedList",
                new RichTextBlock[]
                {
                    new UnorderedListBlock(
                        new[]
                        {
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("First bullet item wraps over more than one line.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("Second bullet item", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            })
                        },
                        MarginBlockEnd: 0),
                },
                boxWidth: 175,
                boxHeight: 95));

        cases.Add(
            ConformanceCase.FromBlocks(
                "OrderedList",
                new RichTextBlock[]
                {
                    new OrderedListBlock(
                        new[]
                        {
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("Ordered item one", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("Ordered item two wraps wider and checks marker alignment.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            })
                        },
                        StartIndex: 3),
                },
                boxWidth: 175,
                boxHeight: 100));

        cases.Add(
            ConformanceCase.FromBlocks(
                "LongFormMultiParagraphMixedFamilies",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        2,
                        new InlineNode[]
                        {
                            new TextRunNode("Long Form Comparison Fixture", new TextStyle(SansFamily, 700, 17))
                        },
                        new ParagraphStyle(LineHeight: 21, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This paragraph uses the default sans family and is intentionally long enough to wrap across several lines so the comparison exercises paragraph shaping, line height, and content box alignment over a larger surface area. ", richStyle),
                            new TextRunNode("A highlighted phrase", new TextStyle(SansFamily, 400, 12, BackgroundColor: new TextColor(255, 245, 180))),
                            new TextRunNode(" appears in the middle to make sure inline backgrounds remain stable inside longer prose.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 9)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("The second paragraph switches to a serif family at a slightly larger size to pressure font resolution and line-breaking behavior. It should still maintain comparable word geometry after extraction from both PDFs.", serifStyle)
                        },
                        new ParagraphStyle(LineHeight: 18, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Finally, a compact mono styled concluding paragraph checks that narrower glyph metrics and subtle character spacing keep wrapping decisions comparable in a dense block of text.", monoStyle with { CharacterSpacing = 0.25 })
                        },
                        new ParagraphStyle(LineHeight: 15))
                },
                boxWidth: 190,
                boxHeight: 165,
                metricPreference: TextFontMetricSource.Windows,
                boxStyle: new TextBoxStyle(
                    BackgroundColor: new TextColor(252, 252, 250),
                    BorderColor: new TextColor(190, 190, 190),
                    BorderWidth: 1,
                    Padding: 10)));

        cases.Add(
            ConformanceCase.FromBlocks(
                "LargeDocumentWithListsAndParagraphs",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        3,
                        new InlineNode[]
                        {
                            new TextRunNode("Release Notes", new TextStyle(SansFamily, 700, 16))
                        },
                        new ParagraphStyle(LineHeight: 20, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This introductory paragraph explains the context for the list that follows. It is intentionally verbose so the fixture exercises more wrapping, more vertical flow, and more opportunities for word extraction to drift if either side is unstable.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new OrderedListBlock(
                        new[]
                        {
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("The first item contains enough content to wrap and confirm that numbered markers stay aligned with the first content line.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 4))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("The second item mixes ", richStyle),
                                        new TextRunNode("serif emphasis", serifStyle),
                                        new TextRunNode(" and ", richStyle),
                                        new TextRunNode("mono snippets", monoStyle),
                                        new TextRunNode(" within the same list item.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 4))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("The third item is shorter.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            })
                        },
                        StartIndex: 7,
                        MarginBlockEnd: 8),
                    new UnorderedListBlock(
                        new[]
                        {
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("Bullet follow-up one with a little more explanatory text.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("Bullet follow-up two closes the fixture.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            })
                        })
                },
                boxWidth: 190,
                boxHeight: 190));

        cases.Add(
            ConformanceCase.FromBlocks(
                "MultiParagraphDifferentSizesAndAlignments",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Large centered opener with explicit spacing", new TextStyle(SansFamily, 700, 18))
                        },
                        new ParagraphStyle(TextAlign: TextHorizontalAlignment.Center, LineHeight: 22, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("A smaller left-aligned paragraph follows, using serif metrics to ensure a different shaping profile and longer wrapped body text for comparison.", serifStyle)
                        },
                        new ParagraphStyle(LineHeight: 17, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Right aligned closer.", new TextStyle(MonoFamily, 400, 11))
                        },
                        new ParagraphStyle(TextAlign: TextHorizontalAlignment.Right, LineHeight: 14))
                },
                boxWidth: 180,
                boxHeight: 130,
                metricPreference: TextFontMetricSource.Windows));

        cases.Add(
            ConformanceCase.FromBlocks(
                "FontFaceMatrixAcrossParagraphs",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        3,
                        new InlineNode[]
                        {
                            new TextRunNode("Font Face Matrix", new TextStyle(SansFamily, 700, 16))
                        },
                        new ParagraphStyle(LineHeight: 20, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Regular sans text starts the comparison. ", richStyle),
                            new TextRunNode("Bold follows in the same family. ", sansBoldStyle),
                            new TextRunNode("Italic continues the line. ", sansItalicStyle),
                            new TextRunNode("Bold italic closes the paragraph.", sansBoldItalicStyle)
                        },
                        new ParagraphStyle(LineHeight: 17, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Serif regular text sits in a second paragraph to check a genuinely different family. ", serifStyle),
                            new TextRunNode("Bold serif follows immediately to pressure family plus weight resolution.", serifBoldStyle)
                        },
                        new ParagraphStyle(LineHeight: 18))
                },
                boxWidth: 195,
                boxHeight: 130,
                metricPreference: TextFontMetricSource.Windows));

        cases.Add(
            ConformanceCase.FromBlocks(
                "CondensedFamiliesAndSpacing",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Condensed regular text should wrap later because glyphs are narrower. ", condensedStyle with { CharacterSpacing = 0.2 }),
                            new TextRunNode("Bold condensed text shifts emphasis without changing family. ", condensedBoldStyle),
                            new TextRunNode("Italic condensed text checks the separate face.", condensedItalicStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Semi condensed text gives the suite one more family profile without exploding the asset count.", semiCondensedStyle)
                        },
                        new ParagraphStyle(LineHeight: 16))
                },
                boxWidth: 190,
                boxHeight: 120));

        cases.Add(
            ConformanceCase.FromBlocks(
                "RichTextDeepArticleFixture",
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        2,
                        new InlineNode[]
                        {
                            new TextRunNode("Conformance Stress Article", new TextStyle(SansFamily, 700, 18))
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("This first paragraph is intentionally long and dense. It uses the default sans family, several wraps, and a highlighted inline fragment so the comparison observes text flow, run backgrounds, and content inset over a broader area than the earlier small fixtures. ", richStyle),
                            new TextRunNode("Highlighted inline fragment", richStyle with { BackgroundColor = new TextColor(248, 240, 150) }),
                            new TextRunNode(" stays embedded in the same flow to verify that browser extraction still agrees with the PdfLexer output when many words surround it.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 9)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("The second paragraph moves to serif text at a larger size. It should maintain stable word order and comparable line breaks while materially changing glyph metrics and the width consumed by each sentence.", serifStyle with { FontSize = 14 })
                        },
                        new ParagraphStyle(LineHeight: 19, MarginBlockEnd: 9)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("The third paragraph mixes ", richStyle),
                            new TextRunNode("condensed", condensedStyle),
                            new TextRunNode(", ", richStyle),
                            new TextRunNode("mono", monoStyle),
                            new TextRunNode(", and ", richStyle),
                            new TextRunNode("italic", sansItalicStyle),
                            new TextRunNode(" spans so the fixture exercises multiple family switches inside one wrapped block.", richStyle)
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 8)),
                    new OrderedListBlock(
                        new[]
                        {
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("The first ordered item keeps a full sentence so marker alignment and wrapped continuation lines are both tested.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 4)),
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("A second paragraph inside the same item confirms multi paragraph list items.", condensedStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 15))
                            }),
                            new ListItemBlock(new RichTextBlock[]
                            {
                                new ParagraphBlock(
                                    new InlineNode[]
                                    {
                                        new TextRunNode("The next item includes ", richStyle),
                                        new TextRunNode("bold serif", serifBoldStyle),
                                        new TextRunNode(" and ", richStyle),
                                        new TextRunNode("mono code like text", monoStyle with { CharacterSpacing = 0.25 }),
                                        new TextRunNode(" to widen the extraction surface.", richStyle)
                                    },
                                    new ParagraphStyle(LineHeight: 16))
                            })
                        },
                        StartIndex: 11)
                },
                boxWidth: 200,
                boxHeight: 220,
                boxStyle: new TextBoxStyle(
                    BackgroundColor: new TextColor(251, 250, 246),
                    BorderColor: new TextColor(180, 180, 180),
                    BorderWidth: 1,
                    BorderRadius: 6,
                    Padding: 10)));

        cases.Add(
            ConformanceCase.FromBlocks(
                "ItalicParagraphWhenItalicFacePresent",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("An italic face fixture now runs unconditionally because the matching italic TTF is present in the test directory.", sansItalicStyle),
                            new TextRunNode(" It is followed by a bold italic clause to exercise the separate face as well.", sansBoldItalicStyle)
                        },
                        new ParagraphStyle(LineHeight: 16))
                },
                boxWidth: 190,
                boxHeight: 72));

        cases.Add(
            ConformanceCase.FromHtml(
                "WrappedInlineBackgroundAndUnderline",
                $$"""
                <p style="line-height:16pt;">
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">A wrapped paragraph begins with neutral text, then </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; background-color:#fceeA4;">a highlighted phrase that should span more than one line in both engines</span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">, and finally </span>
                  <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; text-decoration:underline;">an underlined closing segment that also wraps cleanly</span>
                </p>
                """,
                richStyle,
                boxWidth: 175,
                boxHeight: 105));

        cases.Add(
            ConformanceCase.FromHtml(
                "UnorderedListWrappedItems",
                $$"""
                <ul>
                  <li>
                    <p style="line-height:16pt;">
                      <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">First unordered item wraps to a second line and verifies the bullet gap against the content column.</span>
                    </p>
                  </li>
                  <li>
                    <p style="line-height:16pt;">
                      <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Second unordered item includes </span>
                      <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; background-color:#f4e8aa;">background emphasis</span>
                      <span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;"> inside the wrapped content.</span>
                    </p>
                  </li>
                </ul>
                """,
                richStyle,
                boxWidth: 182,
                boxHeight: 118,
                boxStyle: new TextBoxStyle(
                    BorderColor: new TextColor(170, 170, 170),
                    BorderWidth: 0.75)));

        cases.Add(
            ConformanceCase.FromHtml(
                "ListMarkerStyleVariants",
                $$"""
                <ul style="list-style-type:disc; margin-bottom:8pt;">
                  <li><p style="line-height:15pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Disc bullet sample wraps into the next line for spacing review.</span></p></li>
                </ul>
                <ol style="list-style-type:upper-alpha;" start="3">
                  <li><p style="line-height:15pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Ordered alpha marker continues from a custom start index.</span></p></li>
                </ol>
                """,
                richStyle,
                boxWidth: 188,
                boxHeight: 126));

        cases.Add(
            ConformanceCase.FromBlocks(
                "ParagraphSpacingWithStyledRuns",
                new RichTextBlock[]
                {
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("First paragraph carries a wrapped highlighted run that should remain aligned across line breaks within the same block. ", richStyle),
                            new TextRunNode("Highlighted wrapped content remains continuous here.", richStyle with { BackgroundColor = new TextColor(250, 240, 170) })
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode("Second paragraph switches to underline styling and should sit below the first paragraph only by the configured margin and line height, not by any hidden browser defaults. ", richStyle with { Underline = true }),
                            new TextRunNode("A serif closing clause checks mixed family flow.", serifStyle)
                        },
                        new ParagraphStyle(LineHeight: 17))
                },
                boxWidth: 186,
                boxHeight: 132));

        cases.Add(
            ConformanceCase.FromHtml(
                "BasicTableConformance",
                $$"""
                <table style="background-color:#f5f5f5; border:1pt solid #505050; cell-border-color:#a0a0a0; cell-border-width:0.75pt; cellpadding:6pt; margin-bottom:8pt;">
                  <tr>
                    <th style="width:106pt;"><p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:12pt;">Header A</span></p></th>
                    <th style="width:54pt;"><p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:12pt;">Header B</span></p></th>
                  </tr>
                  <tr>
                    <td style="width:106pt; background-color:#faf5be;"><p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Cell one wraps into multiple words for comparison.</span></p></td>
                    <td style="width:54pt;"><p style="line-height:17pt;"><span style="font-family:'{{SerifFamily}}'; font-weight:400; font-size:13pt;">Cell two</span></p></td>
                  </tr>
                </table>
                """,
                richStyle,
                boxWidth: 190,
                boxHeight: 120));

        cases.Add(
            ConformanceCase.FromHtml(
                "TableSpanAndNestedContentConformance",
                $$"""
                <table style="border:1pt solid #505050; cell-border-color:#787878; cell-border-width:0.5pt; cellpadding:4pt; margin-bottom:8pt;">
                  <tr>
                    <th colspan="2" style="width:170pt;"><p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:12pt;">Span Header</span></p></th>
                  </tr>
                  <tr>
                    <td rowspan="2" style="width:108pt; background-color:#f3f7ff; padding:5pt;">
                      <p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Tall spanning cell with wrapped content.</span></p>
                      <ul>
                        <li><p style="line-height:15pt;"><span style="font-family:'{{CondensedFamily}}'; font-weight:400; font-size:12pt;">Nested bullet one</span></p></li>
                        <li><p style="line-height:15pt;"><span style="font-family:'{{CondensedFamily}}'; font-weight:400; font-size:12pt;">Nested bullet two</span></p></li>
                      </ul>
                    </td>
                    <td style="width:62pt; text-align:center;"><p style="line-height:17pt;"><span style="font-family:'{{SerifFamily}}'; font-weight:400; font-size:13pt;">Top right</span></p></td>
                  </tr>
                  <tr>
                    <td style="width:62pt; background-color:#faf4e4;"><p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt; text-decoration:underline;">Bottom right with underline</span></p></td>
                  </tr>
                </table>
                """,
                richStyle,
                boxWidth: 195,
                boxHeight: 165));

        cases.Add(
            ConformanceCase.FromHtml(
                "TableCellWidthAndAlignmentStyles",
                $$"""
                <table style="border:1pt solid #666666; cell-border-color:#999999; cell-border-width:0.75pt; cellpadding:5pt;">
                  <tr>
                    <td style="width:58pt; text-align:right; background-color:#f6efe0;">
                      <p style="line-height:16pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Narrow right cell wraps earlier.</span></p>
                    </td>
                    <td style="width:96pt; text-align:center; background-color:#eef4ff;">
                      <p style="line-height:16pt;"><span style="font-family:'{{SerifFamily}}'; font-weight:400; font-size:13pt;">Wider centered cell.</span></p>
                    </td>
                  </tr>
                </table>
                """,
                richStyle,
                boxWidth: 195,
                boxHeight: 108));

        cases.Add(
            ConformanceCase.FromHtml(
                "TableChromeAndSectionStyles",
                $$"""
                <table style="background-color:#f7f7f2; border-width:1pt; border-color:#4e4e4e; cell-border-color:#9c9c9c; cell-border-width:0.5pt; cellpadding:7pt; margin-bottom:8pt;">
                  <thead>
                    <tr>
                      <th style="width:58pt; background-color:#e9edf4;"><p style="line-height:15pt;"><span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:12pt;">Label</span></p></th>
                      <th style="width:84pt; background-color:#e9edf4;"><p style="line-height:15pt;"><span style="font-family:'{{SansFamily}}'; font-weight:700; font-size:12pt;">Value</span></p></th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td style="width:58pt;"><p style="line-height:15pt;"><span style="font-family:'{{SansFamily}}'; font-weight:400; font-size:12pt;">Alpha</span></p></td>
                      <td style="width:84pt;"><p style="line-height:15pt;"><span style="font-family:'{{MonoFamily}}'; font-weight:400; font-size:11pt;">A-1024</span></p></td>
                    </tr>
                  </tbody>
                </table>
                """,
                richStyle,
                boxWidth: 184,
                boxHeight: 110));

        cases.Add(
            ConformanceCase.FromBlocks(
                "WeightedRowContainer",
                new RichTextBlock[]
                {
                    new RowBlock(
                        new[]
                        {
                            new LayoutChild(
                                new RichTextBlock[]
                                {
                                    new ParagraphBlock(
                                        new InlineNode[]
                                        {
                                            new TextRunNode("Primary column text should take roughly twice the width of the adjacent columns and wrap later.", richStyle)
                                        },
                                        new ParagraphStyle(LineHeight: 16))
                                },
                                Weight: 2,
                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(248, 248, 252), BorderColor: new TextColor(180, 180, 210), BorderWidth: 1, Padding: 6)),
                            new LayoutChild(
                                new RichTextBlock[]
                                {
                                    new ParagraphBlock(
                                        new InlineNode[]
                                        {
                                            new TextRunNode("Side one wraps sooner.", condensedStyle)
                                        },
                                        new ParagraphStyle(LineHeight: 15))
                                },
                                Weight: 1,
                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(252, 248, 245), BorderColor: new TextColor(210, 190, 180), BorderWidth: 1, Padding: 6)),
                            new LayoutChild(
                                new RichTextBlock[]
                                {
                                    new ParagraphBlock(
                                        new InlineNode[]
                                        {
                                            new TextRunNode("Side two also uses a narrower box.", monoStyle)
                                        },
                                        new ParagraphStyle(LineHeight: 14))
                                },
                                Weight: 1,
                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(246, 251, 246), BorderColor: new TextColor(180, 210, 180), BorderWidth: 1, Padding: 6))
                        },
                        Height: 92,
                        Style: new LayoutContainerStyle(
                            BackgroundColor: new TextColor(252, 252, 250),
                            BorderColor: new TextColor(170, 170, 170),
                            BorderWidth: 1,
                            Padding: 8,
                            Gap: 8))
                },
                boxWidth: 190,
                boxHeight: 110));

        cases.Add(
            ConformanceCase.FromBlocks(
                "NestedColumnWithWeightedRow",
                new RichTextBlock[]
                {
                    new ColumnBlock(
                        new[]
                        {
                            new LayoutChild(
                                new RichTextBlock[]
                                {
                                    new RowBlock(
                                        new[]
                                        {
                                            new LayoutChild(
                                                new RichTextBlock[]
                                                {
                                                    new HeadingBlock(
                                                        3,
                                                        new InlineNode[]
                                                        {
                                                            new TextRunNode("Lead", new TextStyle(SansFamily, 700, 16))
                                                        },
                                                        new ParagraphStyle(LineHeight: 20, MarginBlockEnd: 4)),
                                                    new ParagraphBlock(
                                                        new InlineNode[]
                                                        {
                                                            new TextRunNode("The lead column carries more content and should occupy half of the row width.", richStyle)
                                                        },
                                                        new ParagraphStyle(LineHeight: 16))
                                                },
                                                Weight: 2,
                                                VerticalAlignment: TextVerticalAlignment.Center,
                                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(248, 250, 255), BorderColor: new TextColor(170, 185, 220), BorderWidth: 1, Padding: 6)),
                                            new LayoutChild(
                                                new RichTextBlock[]
                                                {
                                                    new ParagraphBlock(
                                                        new InlineNode[]
                                                        {
                                                            new TextRunNode("Upper side note.", serifStyle)
                                                        },
                                                        new ParagraphStyle(LineHeight: 17))
                                                },
                                                Weight: 1,
                                                VerticalAlignment: TextVerticalAlignment.Bottom,
                                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(255, 249, 244), BorderColor: new TextColor(215, 190, 170), BorderWidth: 1, Padding: 6))
                                        },
                                        Height: 86,
                                        Style: new LayoutContainerStyle(
                                            BackgroundColor: new TextColor(252, 252, 252),
                                            BorderColor: new TextColor(175, 175, 175),
                                            BorderWidth: 1,
                                            Padding: 8,
                                            Gap: 8,
                                            MarginBlockEnd: 10))
                                }),
                            new LayoutChild(
                                new RichTextBlock[]
                                {
                                    new ParagraphBlock(
                                        new InlineNode[]
                                        {
                                            new TextRunNode("A full-width paragraph follows below the weighted row. Its position should track the measured height of the row above.", richStyle)
                                        },
                                        new ParagraphStyle(LineHeight: 16))
                                },
                                BoxStyle: new TextBoxStyle(BackgroundColor: new TextColor(250, 250, 246), BorderColor: new TextColor(185, 185, 170), BorderWidth: 1, Padding: 8))
                        },
                        Style: new LayoutContainerStyle(
                            BackgroundColor: new TextColor(254, 254, 252),
                            BorderColor: new TextColor(160, 160, 160),
                            BorderWidth: 1,
                            Padding: 8,
                            Gap: 10))
                },
                boxWidth: 195,
                boxHeight: 180));

        return cases;
    }

    [ChromiumTheory]
    [MemberData(nameof(ConformanceCases))]
    public async Task ChromePdf_And_PdfLexerPdf_HaveComparableWordGeometry(string caseName)
    {
        var testCase = AllConformanceCases.Single(x => x.Name == caseName);
        var effectiveTestCase = ResolveEffectiveHeights(testCase);
        var renderTestCase = ScaleForRender(effectiveTestCase, InternalRenderScaleFactor);
        var browserFixture = CreateBrowserFixture(renderTestCase);
        var chromePdf = browserFixture switch
        {
            HtmlTextBoxFixture simple => await BrowserFixtureRenderer.RenderPdfAsync(simple),
            RichHtmlTextBoxFixture rich => await BrowserFixtureRenderer.RenderPdfAsync(rich),
            NormalizedRichHtmlTextBoxFixture normalizedRich => await BrowserFixtureRenderer.RenderPdfAsync(normalizedRich),
            _ => throw new NotSupportedException($"Unsupported fixture type '{browserFixture.GetType().Name}'.")
        };
        var pdfLexerPdf = RenderWithPdfLexer(renderTestCase);

        var chromeText = NormalizeExtractedText(ExtractText(chromePdf), InternalRenderScaleFactor);
        var libraryText = NormalizeExtractedText(ExtractText(pdfLexerPdf), InternalRenderScaleFactor);
        var chromeWords = chromeText.Words;
        var libraryWords = libraryText.Words;

        try
        {
            AssertWordGeometryComparable(chromeText, libraryText);
        }
        catch
        {
            WriteReviewArtifacts(effectiveTestCase, browserFixture, chromePdf, pdfLexerPdf, chromeWords, libraryWords, writeSummary: true);
            throw;
        }

        WriteReviewArtifacts(effectiveTestCase, browserFixture, chromePdf, pdfLexerPdf, chromeWords, libraryWords, writeSummary: false);
        WriteCurrentRegressionSnapshot(effectiveTestCase, pdfLexerPdf);
    }

    private static object CreateBrowserFixture(ConformanceCase testCase)
        => testCase.Segments is not null
            ? new HtmlTextBoxFixture(
                testCase.PageWidth,
                testCase.PageHeight,
                testCase.BoxLeft,
                testCase.BoxTop,
                testCase.BoxWidth,
                testCase.BoxHeight,
                testCase.Fonts,
                testCase.Segments,
                testCase.HorizontalAlignment,
                testCase.BoxStyle)
            : testCase.NormalizedHtml is not null
            ? new NormalizedRichHtmlTextBoxFixture(
                testCase.PageWidth,
                testCase.PageHeight,
                testCase.BoxLeft,
                testCase.BoxTop,
                testCase.BoxWidth,
                testCase.BoxHeight,
                testCase.Fonts,
                testCase.NormalizedHtml,
                testCase.ListIndent,
                testCase.ListMarkerGap,
                testCase.BoxStyle)
            : new RichHtmlTextBoxFixture(
                testCase.PageWidth,
                testCase.PageHeight,
                testCase.BoxLeft,
                testCase.BoxTop,
                testCase.BoxWidth,
                testCase.BoxHeight,
                testCase.Fonts,
                testCase.Blocks!,
                testCase.ListIndent,
                testCase.ListMarkerGap,
                testCase.BoxStyle);

    private static ConformanceCase ScaleForRender(ConformanceCase testCase, double scale)
        => testCase with
        {
            PageWidth = testCase.PageWidth * scale,
            PageHeight = testCase.PageHeight * scale,
            BoxLeft = testCase.BoxLeft * scale,
            BoxTop = testCase.BoxTop * scale,
            BoxWidth = testCase.BoxWidth * scale,
            BoxHeight = testCase.BoxHeight * scale,
            ListIndent = testCase.ListIndent * scale,
            ListMarkerGap = testCase.ListMarkerGap * scale,
            BoxStyle = ScaleBoxStyle(testCase.BoxStyle, scale),
            Segments = testCase.Segments?.Select(x => ScaleSegment(x, scale)).ToArray(),
            Blocks = testCase.Blocks?.Select(x => ScaleBlock(x, scale)).ToArray()
        };

    private static ConformanceCase ResolveEffectiveHeights(ConformanceCase testCase)
    {
        var measurementLibrary = CreateFontLibrary(testCase.Fonts);
        var requiredBoxHeight = MeasureRequiredBoxHeight(testCase, measurementLibrary);
        var bottomMargin = Math.Max(20d, testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight);
        var resolvedBoxHeight = Math.Max(testCase.BoxHeight, requiredBoxHeight);
        var resolvedPageHeight = Math.Max(testCase.PageHeight, testCase.BoxTop + resolvedBoxHeight + bottomMargin);
        if (resolvedBoxHeight <= testCase.BoxHeight + 0.0001d && resolvedPageHeight <= testCase.PageHeight + 0.0001d)
        {
            return testCase;
        }

        return testCase with
        {
            BoxHeight = resolvedBoxHeight,
            PageHeight = resolvedPageHeight
        };
    }

    private static double MeasureRequiredBoxHeight(ConformanceCase testCase, PdfTextLayoutFontLibrary library)
    {
        const double measurementHeight = 1_000_000d;
        TextBoxLayoutResult layout;
        if (testCase.Segments is not null)
        {
            var request = new TextBoxLayoutRequest(
                testCase.BoxWidth,
                measurementHeight,
                library.CreateLayoutLibrary(),
                testCase.Segments)
            {
                HorizontalAlignment = testCase.HorizontalAlignment,
                VerticalAlignment = TextVerticalAlignment.Top,
                OverflowMode = TextOverflowMode.Visible,
                BoxStyle = testCase.BoxStyle,
                MetricPreference = testCase.MetricPreference
            };
            layout = new TextBoxLayoutEngine().Layout(request);
        }
        else
        {
            var request = new RichTextBoxLayoutRequest(
                testCase.BoxWidth,
                measurementHeight,
                library.CreateLayoutLibrary(),
                testCase.Blocks!)
            {
                VerticalAlignment = TextVerticalAlignment.Top,
                OverflowMode = TextOverflowMode.Visible,
                ListIndent = testCase.ListIndent,
                ListMarkerGap = testCase.ListMarkerGap,
                BoxStyle = testCase.BoxStyle,
                MetricPreference = testCase.MetricPreference
            };
            layout = new RichTextBoxLayoutEngine().Layout(request);
        }

        return Math.Ceiling(Math.Max(testCase.BoxHeight, layout.MeasuredHeight + 4d));
    }

    private static void WriteReviewArtifacts(
        ConformanceCase testCase,
        object fixture,
        byte[] chromePdf,
        byte[] pdfLexerPdf,
        IReadOnlyList<ExtractedWord> chromeWords,
        IReadOnlyList<ExtractedWord> libraryWords,
        bool writeSummary)
    {
        using var cDoc = PdfDocument.Open(chromePdf);
        using var pDoc = PdfDocument.Open(pdfLexerPdf);
        using var outDoc = PdfDocument.Create();

        var chromePage = cDoc.Pages[0];
        var pdfLexerPage = pDoc.Pages[0];
        var chromePageWidth = (double)(chromePage.MediaBox.URx - chromePage.MediaBox.LLx);
        var chromePageHeight = (double)(chromePage.MediaBox.URy - chromePage.MediaBox.LLy);
        var pdfLexerPageWidth = (double)(pdfLexerPage.MediaBox.URx - pdfLexerPage.MediaBox.LLx);
        var pdfLexerPageHeight = (double)(pdfLexerPage.MediaBox.URy - pdfLexerPage.MediaBox.LLy);
        var previewScale = Math.Max(
            1.0,
            Math.Min(
                ReviewTargetPreviewWidth / Math.Max(chromePageWidth, pdfLexerPageWidth),
                ReviewTargetPreviewHeight / Math.Max(chromePageHeight, pdfLexerPageHeight)));
        var chromePreviewWidth = chromePageWidth * previewScale;
        var chromePreviewHeight = chromePageHeight * previewScale;
        var pdfLexerPreviewWidth = pdfLexerPageWidth * previewScale;
        var pdfLexerPreviewHeight = pdfLexerPageHeight * previewScale;
        var reviewPageWidth = (ReviewMargin * 2) + chromePreviewWidth + ReviewGutter + pdfLexerPreviewWidth;
        var reviewPageHeight = (ReviewMargin * 2) + Math.Max(chromePreviewHeight, pdfLexerPreviewHeight);

        var pg = outDoc.AddPage(reviewPageWidth, reviewPageHeight);
        using (var writer = pg.GetWriter())
        {
            var cf = PdfLexer.DOM.XObjForm.FromPage(cDoc.Pages[0]);
            var pf = PdfLexer.DOM.XObjForm.FromPage(pDoc.Pages[0]);

            writer.Save()
                .Translate(ReviewMargin, ReviewMargin)
                .Scale(previewScale, previewScale)
                .Form(cf)
                .Restore();

            writer.Save()
                .Translate(ReviewMargin + chromePreviewWidth + ReviewGutter, ReviewMargin)
                .Scale(previewScale, previewScale)
                .Form(pf)
                .Restore();

            DrawCornerOverlays(
                writer,
                chromeWords,
                ReviewMargin,
                ReviewMargin,
                previewScale,
                210,
                60,
                60);
            DrawAreaOverlay(
                writer,
                testCase.BoxLeft,
                testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight,
                testCase.BoxWidth,
                testCase.BoxHeight,
                ReviewMargin,
                ReviewMargin,
                previewScale,
                40,
                160,
                70);
            DrawCornerOverlays(
                writer,
                libraryWords,
                ReviewMargin + chromePreviewWidth + ReviewGutter,
                ReviewMargin,
                previewScale,
                40,
                90,
                210);
            DrawAreaOverlay(
                writer,
                testCase.BoxLeft,
                testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight,
                testCase.BoxWidth,
                testCase.BoxHeight,
                ReviewMargin + chromePreviewWidth + ReviewGutter,
                ReviewMargin,
                previewScale,
                40,
                160,
                70);
        }

        var resultsDir = Path.Combine(PathUtil.GetPathFromSegmentOfCurrent("test"), "results", "chrome-conformance");
        Directory.CreateDirectory(resultsDir);
        File.WriteAllBytes(Path.Combine(resultsDir, $"{testCase.Name}.pdf"), outDoc.Save());
        File.WriteAllText(Path.Combine(resultsDir, $"{testCase.Name}.html"), GetHtml(fixture));
        if (testCase.HtmlSource is not null)
        {
            File.WriteAllText(Path.Combine(resultsDir, $"{testCase.Name}.source.html"), testCase.HtmlSource);
        }
        if (testCase.NormalizedHtml is not null)
        {
            File.WriteAllText(Path.Combine(resultsDir, $"{testCase.Name}.normalized.html"), testCase.NormalizedHtml);
        }
        var summaryPath = Path.Combine(resultsDir, $"{testCase.Name}.summary.md");
        var failureSummaryPath = Path.Combine(resultsDir, $"{testCase.Name}.failure.summary.md");
        var summaryContent = BuildWordGeometrySummary(testCase, chromeWords, libraryWords);
        File.WriteAllText(summaryPath, summaryContent);
        if (writeSummary)
        {
            File.WriteAllText(failureSummaryPath, summaryContent);
        }
        else if (File.Exists(failureSummaryPath))
        {
            File.Delete(failureSummaryPath);
        }
    }

    private static void DrawCornerOverlays(
        ContentWriter<double> writer,
        IReadOnlyList<ExtractedWord> words,
        double translateX,
        double translateY,
        double previewScale,
        int r,
        int g,
        int b)
    {
        if (words.Count == 0)
        {
            return;
        }

        writer.Save()
            .Translate(translateX, translateY)
            .Scale(previewScale, previewScale)
            .SetStrokingRGB(r, g, b)
            .LineWidth(Math.Max(0.2d, 0.5d / previewScale));

        foreach (var word in words)
        {
            var llx = word.BoundingBox.LLx;
            var lly = word.BoundingBox.LLy;
            var urx = word.BoundingBox.URx;
            var ury = word.BoundingBox.URy;
            var width = Math.Max(0d, urx - llx);
            var height = Math.Max(0d, ury - lly);
            var cornerLength = Math.Max(1.5d, Math.Min(4d, Math.Min(width, height) * 0.22d));

            DrawCorner(writer, llx, lly, cornerLength, 1d, 0d, 0d, 1d);
            DrawCorner(writer, llx, ury, cornerLength, 1d, 0d, 0d, -1d);
            DrawCorner(writer, urx, lly, cornerLength, -1d, 0d, 0d, 1d);
            DrawCorner(writer, urx, ury, cornerLength, -1d, 0d, 0d, -1d);
        }

        writer.Restore();
    }

    private static void DrawCorner(
        ContentWriter<double> writer,
        double x,
        double y,
        double length,
        double dx1,
        double dy1,
        double dx2,
        double dy2)
    {
        writer.MoveTo(x, y)
            .LineTo(x + (length * dx1), y + (length * dy1))
            .Stroke();
        writer.MoveTo(x, y)
            .LineTo(x + (length * dx2), y + (length * dy2))
            .Stroke();
    }

    private static void DrawAreaOverlay(
        ContentWriter<double> writer,
        double x,
        double y,
        double width,
        double height,
        double translateX,
        double translateY,
        double previewScale,
        int r,
        int g,
        int b)
    {
        writer.Save()
            .Translate(translateX, translateY)
            .Scale(previewScale, previewScale)
            .SetStrokingRGB(r, g, b)
            .LineWidth(Math.Max(0.25d, 0.8d / previewScale));

        var cornerLength = Math.Max(3d, Math.Min(8d, Math.Min(width, height) * 0.08d));
        DrawCorner(writer, x, y, cornerLength, 1d, 0d, 0d, 1d);
        DrawCorner(writer, x, y + height, cornerLength, 1d, 0d, 0d, -1d);
        DrawCorner(writer, x + width, y, cornerLength, -1d, 0d, 0d, 1d);
        DrawCorner(writer, x + width, y + height, cornerLength, -1d, 0d, 0d, -1d);

        writer.Restore();
    }

    private static string GetHtml(object fixture)
        => fixture switch
        {
            HtmlTextBoxFixture simple => BrowserFixtureRenderer.GetHtml(simple),
            RichHtmlTextBoxFixture rich => BrowserFixtureRenderer.GetHtml(rich),
            NormalizedRichHtmlTextBoxFixture normalizedRich => BrowserFixtureRenderer.GetHtml(normalizedRich),
            _ => throw new NotSupportedException($"Unsupported fixture type '{fixture.GetType().Name}'.")
        };

    private static string BuildWordGeometrySummary(
        ConformanceCase testCase,
        IReadOnlyList<ExtractedWord> chromeWords,
        IReadOnlyList<ExtractedWord> libraryWords)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Chrome Conformance Summary: {testCase.Name}");
        sb.AppendLine();
        sb.AppendLine("## Fixture");
        sb.AppendLine();
        sb.AppendLine($"- Page: {testCase.PageWidth:0.###} x {testCase.PageHeight:0.###} pt");
        sb.AppendLine($"- Box: left={testCase.BoxLeft:0.###} top={testCase.BoxTop:0.###} width={testCase.BoxWidth:0.###} height={testCase.BoxHeight:0.###} pt");
        sb.AppendLine($"- Horizontal alignment: {testCase.HorizontalAlignment}");
        if (testCase.HtmlSource is not null)
        {
            sb.AppendLine("- Fixture authoring: HTML -> model -> HTML");
            sb.AppendLine($"- Source HTML length: {testCase.HtmlSource.Length}");
            sb.AppendLine($"- Normalized HTML length: {testCase.NormalizedHtml?.Length ?? 0}");
        }
        if (testCase.Segments is not null)
        {
            sb.AppendLine($"- Segment count: {testCase.Segments.Count}");
            for (var i = 0; i < testCase.Segments.Count; i++)
            {
                var segment = testCase.Segments[i];
                sb.AppendLine($"- Segment {i}: text={FormatValue(segment.Text)} fontSize={segment.Style.FontSize:0.###} lineSpacing={(segment.Style.LineSpacing ?? segment.Style.FontSize):0.###} charSpacing={segment.Style.CharacterSpacing:0.###} wordSpacing={segment.Style.WordSpacing:0.###} underline={segment.Style.Underline}");
            }
        }
        else
        {
            sb.AppendLine($"- Block count: {testCase.Blocks!.Count}");
            for (var i = 0; i < testCase.Blocks.Count; i++)
            {
                sb.AppendLine($"- Block {i}: {DescribeBlock(testCase.Blocks[i])}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Aggregate");
        sb.AppendLine();
        sb.AppendLine($"- Chrome word count: {chromeWords.Count}");
        sb.AppendLine($"- PdfLexer word count: {libraryWords.Count}");
        sb.AppendLine($"- Word count match: {chromeWords.Count == libraryWords.Count}");

        var chromeAnchor = chromeWords.Count > 0 ? chromeWords[0] : null;
        var pdfAnchor = libraryWords.Count > 0 ? libraryWords[0] : null;
        if (chromeAnchor is not null && pdfAnchor is not null)
        {
            sb.AppendLine($"- Initial first-word offset x: {pdfAnchor.BoundingBox.LLx - chromeAnchor.BoundingBox.LLx:0.###} pt");
            sb.AppendLine($"- Initial first-word offset y: {pdfAnchor.BoundingBox.LLy - chromeAnchor.BoundingBox.LLy:0.###} pt");
            sb.AppendLine($"- Initial first-word width delta: {pdfAnchor.BoundingBox.Width() - chromeAnchor.BoundingBox.Width():0.###} pt");
            sb.AppendLine($"- Initial first-word height delta: {pdfAnchor.BoundingBox.Height() - chromeAnchor.BoundingBox.Height():0.###} pt");
        }

        if (chromeWords.Count > 0 && libraryWords.Count > 0)
        {
            var chromeLayout = BuildWordLineLayout(new ExtractedText(chromeWords, Array.Empty<IReadOnlyList<ExtractedWord>>()));
            var libraryLayout = BuildWordLineLayout(new ExtractedText(libraryWords, Array.Empty<IReadOnlyList<ExtractedWord>>()));
            var pairCount = Math.Min(chromeWords.Count, libraryWords.Count);
            var maxDx = 0d;
            var maxDy = 0d;
            var maxDw = 0d;
            var maxDh = 0d;
            var maxRelativeLineDx = 0d;
            var maxRelativeLineDy = 0d;
            var maxRelativeWordDx = 0d;
            var maxRelativeWordDy = 0d;
            var sumDx = 0d;
            var sumDy = 0d;
            var sumDw = 0d;
            var sumDh = 0d;
            var textMismatches = 0;

            for (var i = 0; i < pairCount; i++)
            {
                var dx = Math.Abs(chromeWords[i].BoundingBox.LLx - libraryWords[i].BoundingBox.LLx);
                var dy = Math.Abs(chromeWords[i].BoundingBox.LLy - libraryWords[i].BoundingBox.LLy);
                var dw = Math.Abs(chromeWords[i].BoundingBox.Width() - libraryWords[i].BoundingBox.Width());
                var dh = Math.Abs(chromeWords[i].BoundingBox.Height() - libraryWords[i].BoundingBox.Height());
                maxDx = Math.Max(maxDx, dx);
                maxDy = Math.Max(maxDy, dy);
                maxDw = Math.Max(maxDw, dw);
                maxDh = Math.Max(maxDh, dh);
                sumDx += dx;
                sumDy += dy;
                sumDw += dw;
                sumDh += dh;
                if (!string.Equals(chromeWords[i].Text, libraryWords[i].Text, StringComparison.Ordinal))
                {
                    textMismatches++;
                }

                if (i < chromeLayout.Words.Count && i < libraryLayout.Words.Count
                    && chromeLayout.Words[i].LineIndex == libraryLayout.Words[i].LineIndex)
                {
                    var chromeWord = chromeLayout.Words[i];
                    var libraryWord = libraryLayout.Words[i];
                    if (chromeWord.IsLineStart && libraryWord.IsLineStart && chromeWord.LineIndex > 0)
                    {
                        var chromePrev = chromeLayout.Lines[chromeWord.LineIndex - 1].Anchor;
                        var chromeCurrent = chromeLayout.Lines[chromeWord.LineIndex].Anchor;
                        var libraryPrev = libraryLayout.Lines[libraryWord.LineIndex - 1].Anchor;
                        var libraryCurrent = libraryLayout.Lines[libraryWord.LineIndex].Anchor;
                        maxRelativeLineDx = Math.Max(maxRelativeLineDx, Math.Abs((libraryCurrent.BoundingBox.LLx - libraryPrev.BoundingBox.LLx) - (chromeCurrent.BoundingBox.LLx - chromePrev.BoundingBox.LLx)));
                        maxRelativeLineDy = Math.Max(maxRelativeLineDy, Math.Abs((libraryCurrent.BoundingBox.LLy - libraryPrev.BoundingBox.LLy) - (chromeCurrent.BoundingBox.LLy - chromePrev.BoundingBox.LLy)));
                    }
                    else if (!chromeWord.IsLineStart && !libraryWord.IsLineStart)
                    {
                        var chromeAnchorLine = chromeLayout.Lines[chromeWord.LineIndex].Anchor;
                        var libraryAnchorLine = libraryLayout.Lines[libraryWord.LineIndex].Anchor;
                        maxRelativeWordDx = Math.Max(maxRelativeWordDx, Math.Abs((libraryWords[i].BoundingBox.LLx - libraryAnchorLine.BoundingBox.LLx) - (chromeWords[i].BoundingBox.LLx - chromeAnchorLine.BoundingBox.LLx)));
                        maxRelativeWordDy = Math.Max(maxRelativeWordDy, Math.Abs((libraryWords[i].BoundingBox.LLy - libraryAnchorLine.BoundingBox.LLy) - (chromeWords[i].BoundingBox.LLy - chromeAnchorLine.BoundingBox.LLy)));
                    }
                }
            }

            sb.AppendLine($"- Compared word pairs: {pairCount}");
            sb.AppendLine($"- Chrome line count: {chromeLayout.Lines.Count}");
            sb.AppendLine($"- PdfLexer line count: {libraryLayout.Lines.Count}");
            sb.AppendLine($"- Text mismatches by index: {textMismatches}");
            sb.AppendLine($"- Max |delta x|: {maxDx:0.###} pt");
            sb.AppendLine($"- Max |delta y|: {maxDy:0.###} pt");
            sb.AppendLine($"- Max |delta width|: {maxDw:0.###} pt");
            sb.AppendLine($"- Max |delta height|: {maxDh:0.###} pt");
            sb.AppendLine($"- Max |relative line delta x|: {maxRelativeLineDx:0.###} pt");
            sb.AppendLine($"- Max |relative line delta y|: {maxRelativeLineDy:0.###} pt");
            sb.AppendLine($"- Max |relative word delta x|: {maxRelativeWordDx:0.###} pt");
            sb.AppendLine($"- Max |relative word delta y|: {maxRelativeWordDy:0.###} pt");
            sb.AppendLine($"- Mean |delta x|: {sumDx / pairCount:0.###} pt");
            sb.AppendLine($"- Mean |delta y|: {sumDy / pairCount:0.###} pt");
            sb.AppendLine($"- Mean |delta width|: {sumDw / pairCount:0.###} pt");
            sb.AppendLine($"- Mean |delta height|: {sumDh / pairCount:0.###} pt");
        }

        var analysisSummary = BuildPdfLexerAnalysisSummary(testCase);
        if (!string.IsNullOrWhiteSpace(analysisSummary))
        {
            sb.AppendLine();
            sb.AppendLine("## PdfLexer Analysis");
            sb.AppendLine();
            sb.Append(analysisSummary);
        }

        sb.AppendLine();
        sb.AppendLine("## Word Pairs");
        sb.AppendLine();
        sb.AppendLine("Interpretation:");
        sb.AppendLine("- Table positions are normalized to the first extracted word in each document.");
        sb.AppendLine("- Global `dx`/`dy` remain normalized to the first extracted word in each document.");
        sb.AppendLine("- Pass/fail uses a hybrid rule: absolute anchor on the first line, relative line-to-line deltas for later line starts, and relative in-line offsets for later words.");
        sb.AppendLine("- `dx = (pdflexer.llx - pdflexer.first.llx) - (chrome.llx - chrome.first.llx)`");
        sb.AppendLine("- `dy = (pdflexer.lly - pdflexer.first.lly) - (chrome.lly - chrome.first.lly)`");
        sb.AppendLine("- `dw = pdflexer.width - chrome.width`");
        sb.AppendLine("- `dh = pdflexer.height - chrome.height`");
        sb.AppendLine();
        sb.AppendLine("| i | chrome text | pdflexer text | text match | chrome norm llx | chrome norm lly | chrome w | chrome h | pdf norm llx | pdf norm lly | pdf w | pdf h | dx | dy | dw | dh |");
        sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");

        var rows = Math.Max(chromeWords.Count, libraryWords.Count);
        for (var i = 0; i < rows; i++)
        {
            var chrome = i < chromeWords.Count ? chromeWords[i] : null;
            var pdf = i < libraryWords.Count ? libraryWords[i] : null;

            var chromeText = chrome is null ? "(missing)" : FormatValue(chrome.Text);
            var pdfText = pdf is null ? "(missing)" : FormatValue(pdf.Text);
            var textMatch = chrome is not null && pdf is not null && string.Equals(chrome.Text, pdf.Text, StringComparison.Ordinal);

            var chromeLlx = chrome is null || chromeAnchor is null ? "" : (chrome.BoundingBox.LLx - chromeAnchor.BoundingBox.LLx).ToString("0.###");
            var chromeLly = chrome is null || chromeAnchor is null ? "" : (chrome.BoundingBox.LLy - chromeAnchor.BoundingBox.LLy).ToString("0.###");
            var chromeW = chrome is null ? "" : chrome.BoundingBox.Width().ToString("0.###");
            var chromeH = chrome is null ? "" : chrome.BoundingBox.Height().ToString("0.###");
            var pdfLlx = pdf is null || pdfAnchor is null ? "" : (pdf.BoundingBox.LLx - pdfAnchor.BoundingBox.LLx).ToString("0.###");
            var pdfLly = pdf is null || pdfAnchor is null ? "" : (pdf.BoundingBox.LLy - pdfAnchor.BoundingBox.LLy).ToString("0.###");
            var pdfW = pdf is null ? "" : pdf.BoundingBox.Width().ToString("0.###");
            var pdfH = pdf is null ? "" : pdf.BoundingBox.Height().ToString("0.###");

            var dx = chrome is not null && pdf is not null && chromeAnchor is not null && pdfAnchor is not null
                ? ((pdf.BoundingBox.LLx - pdfAnchor.BoundingBox.LLx) - (chrome.BoundingBox.LLx - chromeAnchor.BoundingBox.LLx)).ToString("0.###")
                : "";
            var dy = chrome is not null && pdf is not null && chromeAnchor is not null && pdfAnchor is not null
                ? ((pdf.BoundingBox.LLy - pdfAnchor.BoundingBox.LLy) - (chrome.BoundingBox.LLy - chromeAnchor.BoundingBox.LLy)).ToString("0.###")
                : "";
            var dw = chrome is not null && pdf is not null ? (pdf.BoundingBox.Width() - chrome.BoundingBox.Width()).ToString("0.###") : "";
            var dh = chrome is not null && pdf is not null ? (pdf.BoundingBox.Height() - chrome.BoundingBox.Height()).ToString("0.###") : "";

            sb.AppendLine($"| {i} | {chromeText} | {pdfText} | {textMatch} | {chromeLlx} | {chromeLly} | {chromeW} | {chromeH} | {pdfLlx} | {pdfLly} | {pdfW} | {pdfH} | {dx} | {dy} | {dw} | {dh} |");
        }

        return sb.ToString();
    }

    private static string BuildPdfLexerAnalysisSummary(ConformanceCase testCase)
    {
        var context = new TextLayoutAnalysisContext().EnableLineDiagnostics();
        TextLayoutPlan plan;
        var library = CreateFontLibrary(testCase.Fonts);

        if (testCase.Segments is not null)
        {
            var request = new TextBoxLayoutRequest(
                testCase.BoxWidth,
                testCase.BoxHeight,
                library.CreateLayoutLibrary(),
                testCase.Segments)
            {
                HorizontalAlignment = testCase.HorizontalAlignment,
                OverflowMode = TextOverflowMode.Clip,
                BoxStyle = testCase.BoxStyle,
                MetricPreference = testCase.MetricPreference
            };

            plan = new TextBoxLayoutEngine().Analyze(request, context);
        }
        else
        {
            var request = new RichTextBoxLayoutRequest(
                testCase.BoxWidth,
                testCase.BoxHeight,
                library.CreateLayoutLibrary(),
                testCase.Blocks!)
            {
                OverflowMode = TextOverflowMode.Clip,
                ListIndent = testCase.ListIndent,
                ListMarkerGap = testCase.ListMarkerGap,
                BoxStyle = testCase.BoxStyle,
                MetricPreference = testCase.MetricPreference
            };

            plan = new RichTextBoxLayoutEngine().Analyze(request, context);
        }

        var sb = new StringBuilder();
        foreach (var listNode in EnumerateNodes(plan.Root).Where(static node => node.ListDiagnostics is not null))
        {
            var metrics = listNode.ListDiagnostics!;
            sb.AppendLine($"- List `{listNode.Source.Path}`: contentStart={metrics.ContentStart:0.###} markerColumn={metrics.MarkerColumnWidth:0.###} markerTextArea={metrics.MarkerTextAreaWidth:0.###} gap={metrics.MarkerGap:0.###} markerVisualWidth={metrics.MarkerVisualWidth:0.###} markerFontSize={metrics.MarkerFontSize:0.###}");
        }

        foreach (var lineNode in EnumerateNodes(plan.Root).Where(static node => node.LineDiagnostics is not null).Take(8))
        {
            var diagnostics = lineNode.LineDiagnostics!;
            var requestedLineHeight = diagnostics.RequestedLineHeight?.ToString("0.###") ?? "n/a";
            sb.AppendLine($"- Line `{lineNode.Source.Path}`: height={diagnostics.ResolvedLineHeight:0.###} baselineOffset={diagnostics.BaselineOffset:0.###} ascent={diagnostics.NaturalAscent:0.###} descent={diagnostics.NaturalDescent:0.###} requestedLineHeight={requestedLineHeight} expanded={diagnostics.ExpandedBeyondRequestedLineHeight}");
        }

        return sb.ToString();
    }

    private static IEnumerable<TextLayoutPlanNode> EnumerateNodes(TextLayoutPlanNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in EnumerateNodes(child))
            {
                yield return descendant;
            }
        }
    }

    private static string DescribeBlock(RichTextBlock block)
        => block switch
        {
            ParagraphBlock paragraph => $"Paragraph align={(paragraph.Style ?? new ParagraphStyle()).TextAlign} marginEnd={(paragraph.Style ?? new ParagraphStyle()).MarginBlockEnd:0.###} text={FormatValue(string.Concat(paragraph.Inlines.OfType<TextRunNode>().Select(x => x.Text)))}",
            HeadingBlock heading => $"Heading level={heading.Level} marginEnd={(heading.Style ?? new ParagraphStyle()).MarginBlockEnd:0.###} text={FormatValue(string.Concat(heading.Inlines.OfType<TextRunNode>().Select(x => x.Text)))}",
            UnorderedListBlock unordered => $"UnorderedList items={unordered.Items.Count} marginEnd={unordered.MarginBlockEnd:0.###}",
            OrderedListBlock ordered => $"OrderedList start={ordered.StartIndex} items={ordered.Items.Count} marginEnd={ordered.MarginBlockEnd:0.###}",
            TableBlock table => $"Table rows={table.Rows.Count} borderWidth={(table.Style ?? new TableStyle()).BorderWidth:0.###} marginEnd={(table.Style ?? new TableStyle()).MarginBlockEnd:0.###}",
            RowBlock row => $"Row children={row.Children.Count} height={(row.Height.HasValue ? row.Height.Value.ToString("0.###") : "auto")} gap={(row.Style ?? new LayoutContainerStyle()).Gap:0.###} marginEnd={(row.Style ?? new LayoutContainerStyle()).MarginBlockEnd:0.###}",
            ColumnBlock column => $"Column children={column.Children.Count} height={(column.Height.HasValue ? column.Height.Value.ToString("0.###") : "auto")} gap={(column.Style ?? new LayoutContainerStyle()).Gap:0.###} marginEnd={(column.Style ?? new LayoutContainerStyle()).MarginBlockEnd:0.###}",
            _ => block.GetType().Name
        };

    private static string FormatValue(string value)
        => $"`{value.Replace("`", "\\`").Replace("\r", "\\r").Replace("\n", "\\n")}`";

    private static byte[] RenderWithPdfLexer(ConformanceCase testCase)
    {
        var library = CreateFontLibrary(testCase.Fonts);

        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        page.MediaBox.LLx = 0;
        page.MediaBox.LLy = 0;
        page.MediaBox.URx = (decimal)testCase.PageWidth;
        page.MediaBox.URy = (decimal)testCase.PageHeight;

        var boxBottom = testCase.PageHeight - testCase.BoxTop - testCase.BoxHeight;
        using (var writer = page.GetWriter())
        {
            if (testCase.Segments is not null)
            {
                var request = new TextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Segments)
                {
                    HorizontalAlignment = testCase.HorizontalAlignment,
                    OverflowMode = TextOverflowMode.Clip,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                    request,
                    library);
            }
            else
            {
                var request = new RichTextBoxLayoutRequest(
                    testCase.BoxWidth,
                    testCase.BoxHeight,
                    library.CreateLayoutLibrary(),
                    testCase.Blocks!)
                {
                    OverflowMode = TextOverflowMode.Clip,
                    ListIndent = testCase.ListIndent,
                    ListMarkerGap = testCase.ListMarkerGap,
                    BoxStyle = testCase.BoxStyle,
                    MetricPreference = testCase.MetricPreference
                };

                writer.WriteTextBox(
                    new PdfRect<double>(testCase.BoxLeft, boxBottom, testCase.BoxLeft + testCase.BoxWidth, boxBottom + testCase.BoxHeight),
                    request,
                    library);
            }
        }

        return doc.Save();
    }

    private static ExtractedText ExtractText(byte[] pdf)
    {
        using var doc = PdfDocument.Open(pdf);
        var page = doc.Pages[0];
        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions
        {
            Order = TextOrder.Reading,
            Mode = StructuredTextMode.Deduplicated
        });

        var words = structured.Words
            .Select(x => new ExtractedWord(x.Text, x.BoundingBox))
            .ToList();
        var lines = structured.Lines
            .Select(line => line.Words.Select(word => new ExtractedWord(word.Text, word.BoundingBox)).ToList() as IReadOnlyList<ExtractedWord>)
            .ToList();
        return new ExtractedText(words, lines);
    }

    private static IReadOnlyList<ExtractedWord> ExtractWords(byte[] pdf)
        => ExtractText(pdf).Words;

    private static ExtractedText NormalizeExtractedText(ExtractedText text, double scale)
    {
        if (Math.Abs(scale - 1d) < 0.0001d)
        {
            return text;
        }

        static ExtractedWord ScaleWord(ExtractedWord x, double scale)
            => new(
                x.Text,
                new PdfRect<double>(
                    x.BoundingBox.LLx / scale,
                    x.BoundingBox.LLy / scale,
                    x.BoundingBox.URx / scale,
                    x.BoundingBox.URy / scale));

        return new ExtractedText(
            text.Words.Select(x => ScaleWord(x, scale)).ToList(),
            text.Lines.Select(line => line.Select(x => ScaleWord(x, scale)).ToList() as IReadOnlyList<ExtractedWord>).ToList());
    }

    private static void AssertWordGeometryComparable(ExtractedText expected, ExtractedText actual)
    {
        Assert.Equal(expected.Words.Count, actual.Words.Count);
        var expectedLayout = BuildWordLineLayout(expected);
        var actualLayout = BuildWordLineLayout(actual);
        Assert.Equal(expectedLayout.Lines.Count, actualLayout.Lines.Count);

        for (var i = 0; i < expected.Words.Count; i++)
        {
            Assert.Equal(expected.Words[i].Text, actual.Words[i].Text);
            var expectedWord = expectedLayout.Words[i];
            var actualWord = actualLayout.Words[i];
            Assert.Equal(expectedWord.LineIndex, actualWord.LineIndex);

            if (expectedWord.IsLineStart)
            {
                Assert.True(actualWord.IsLineStart, $"Word {i} ('{expected.Words[i].Text}') should begin a new line in both outputs.");

                if (expectedWord.LineIndex == 0)
                {
                    Assert.InRange(Math.Abs(expected.Words[i].BoundingBox.LLx - actual.Words[i].BoundingBox.LLx), 0, PositionTolerance);
                    Assert.InRange(Math.Abs(expected.Words[i].BoundingBox.LLy - actual.Words[i].BoundingBox.LLy), 0, PositionTolerance);
                }
                else
                {
                    var expectedPreviousLine = expectedLayout.Lines[expectedWord.LineIndex - 1];
                    var actualPreviousLine = actualLayout.Lines[actualWord.LineIndex - 1];
                    var expectedCurrentLine = expectedLayout.Lines[expectedWord.LineIndex];
                    var actualCurrentLine = actualLayout.Lines[actualWord.LineIndex];

                    var expectedDeltaX = expectedCurrentLine.Anchor.BoundingBox.LLx - expectedPreviousLine.Anchor.BoundingBox.LLx;
                    var actualDeltaX = actualCurrentLine.Anchor.BoundingBox.LLx - actualPreviousLine.Anchor.BoundingBox.LLx;
                    var expectedDeltaY = expectedCurrentLine.Anchor.BoundingBox.LLy - expectedPreviousLine.Anchor.BoundingBox.LLy;
                    var actualDeltaY = actualCurrentLine.Anchor.BoundingBox.LLy - actualPreviousLine.Anchor.BoundingBox.LLy;

                    Assert.InRange(Math.Abs(expectedDeltaX - actualDeltaX), 0, RelativeLineXTolerance);
                    Assert.InRange(Math.Abs(expectedDeltaY - actualDeltaY), 0, RelativeLineYTolerance);
                }
            }
            else
            {
                Assert.False(actualWord.IsLineStart, $"Word {i} ('{expected.Words[i].Text}') should remain on the same line in both outputs.");

                var expectedLine = expectedLayout.Lines[expectedWord.LineIndex];
                var actualLine = actualLayout.Lines[actualWord.LineIndex];
                var expectedRelativeX = expected.Words[i].BoundingBox.LLx - expectedLine.Anchor.BoundingBox.LLx;
                var actualRelativeX = actual.Words[i].BoundingBox.LLx - actualLine.Anchor.BoundingBox.LLx;
                var expectedRelativeY = expected.Words[i].BoundingBox.LLy - expectedLine.Anchor.BoundingBox.LLy;
                var actualRelativeY = actual.Words[i].BoundingBox.LLy - actualLine.Anchor.BoundingBox.LLy;

                Assert.InRange(Math.Abs(expectedRelativeX - actualRelativeX), 0, RelativeWordXTolerance);
                Assert.InRange(Math.Abs(expectedRelativeY - actualRelativeY), 0, RelativeWordYTolerance);
            }

            Assert.InRange(Math.Abs(expected.Words[i].BoundingBox.Width() - actual.Words[i].BoundingBox.Width()), 0, SizeTolerance);
            Assert.InRange(Math.Abs(expected.Words[i].BoundingBox.Height() - actual.Words[i].BoundingBox.Height()), 0, SizeTolerance);
        }
    }

    private static ExtractedWordLineLayout BuildWordLineLayout(ExtractedText text)
    {
        if (TryBuildWordLineLayoutFromExtractedLines(text, out var structuredLayout))
        {
            return structuredLayout;
        }

        var words = text.Words;
        var lines = new List<ExtractedLineInfo>();
        var mappedWords = new List<ExtractedWordLineInfo>(words.Count);
        if (words.Count == 0)
        {
            return new ExtractedWordLineLayout(lines, mappedWords);
        }

        for (var i = 0; i < words.Count; i++)
        {
            var word = words[i];
            var isLineStart = i == 0 || Math.Abs(word.BoundingBox.LLy - words[i - 1].BoundingBox.LLy) > LineGroupingTolerance;
            if (isLineStart)
            {
                lines.Add(new ExtractedLineInfo(lines.Count, word, i));
            }

            mappedWords.Add(new ExtractedWordLineInfo(i, lines.Count - 1, isLineStart));
        }

        return new ExtractedWordLineLayout(lines, mappedWords);
    }

    private static bool TryBuildWordLineLayoutFromExtractedLines(ExtractedText text, out ExtractedWordLineLayout layout)
    {
        var lines = new List<ExtractedLineInfo>();
        var mappedWords = new List<ExtractedWordLineInfo>(text.Words.Count);
        var flatIndex = 0;

        for (var lineIndex = 0; lineIndex < text.Lines.Count; lineIndex++)
        {
            var line = text.Lines[lineIndex];
            if (line.Count == 0)
            {
                continue;
            }

            lines.Add(new ExtractedLineInfo(lines.Count, line[0], flatIndex));
            for (var wordIndex = 0; wordIndex < line.Count; wordIndex++)
            {
                if (flatIndex >= text.Words.Count)
                {
                    layout = default!;
                    return false;
                }

                var flatWord = text.Words[flatIndex];
                var lineWord = line[wordIndex];
                if (!string.Equals(flatWord.Text, lineWord.Text, StringComparison.Ordinal))
                {
                    layout = default!;
                    return false;
                }

                mappedWords.Add(new ExtractedWordLineInfo(flatIndex, lines.Count - 1, wordIndex == 0));
                flatIndex++;
            }
        }

        if (flatIndex != text.Words.Count)
        {
            layout = default!;
            return false;
        }

        layout = new ExtractedWordLineLayout(lines, mappedWords);
        return true;
    }

    private static TextSegment ScaleSegment(TextSegment segment, double scale)
        => new(segment.Text, ScaleSegmentStyle(segment.Style, scale));

    private static TextSegmentStyle ScaleSegmentStyle(TextSegmentStyle style, double scale)
        => new(
            style.FamilyName,
            style.Weight,
            style.FontSize * scale,
            style.Underline,
            style.CharacterSpacing * scale,
            style.WordSpacing * scale,
            style.LineSpacing.HasValue ? style.LineSpacing.Value * scale : null,
            style.LineBoxSizing,
            style.Italic,
            style.ForegroundColor,
            style.BackgroundColor,
            style.StrikeThrough);

    private static TextStyle ScaleTextStyle(TextStyle style, double scale)
        => style with
        {
            FontSize = style.FontSize * scale,
            CharacterSpacing = style.CharacterSpacing * scale,
            WordSpacing = style.WordSpacing * scale
        };

    private static ParagraphStyle ScaleParagraphStyle(ParagraphStyle style, double scale)
        => style with
        {
            LineHeight = style.LineHeight.HasValue ? style.LineHeight.Value * scale : null,
            MarginBlockEnd = style.MarginBlockEnd * scale
        };

    private static TextBoxStyle ScaleBoxStyle(TextBoxStyle style, double scale)
        => style with
        {
            BorderWidth = style.BorderWidth * scale,
            BorderRadius = style.BorderRadius * scale,
            Padding = style.Padding * scale
        };

    private static RichTextBlock ScaleBlock(RichTextBlock block, double scale)
        => block switch
        {
            ParagraphBlock paragraph => paragraph with
            {
                Inlines = paragraph.Inlines.Select(x => ScaleInline(x, scale)).ToArray(),
                Style = paragraph.Style is null ? null : ScaleParagraphStyle(paragraph.Style, scale)
            },
            HeadingBlock heading => heading with
            {
                Inlines = heading.Inlines.Select(x => ScaleInline(x, scale)).ToArray(),
                Style = heading.Style is null ? null : ScaleParagraphStyle(heading.Style, scale)
            },
            UnorderedListBlock unordered => unordered with
            {
                Items = unordered.Items.Select(x => ScaleListItem(x, scale)).ToArray(),
                MarginBlockEnd = unordered.MarginBlockEnd * scale
            },
            OrderedListBlock ordered => ordered with
            {
                Items = ordered.Items.Select(x => ScaleListItem(x, scale)).ToArray(),
                MarginBlockEnd = ordered.MarginBlockEnd * scale
            },
            TableBlock table => table with
            {
                Columns = table.Columns.Select(x => ScaleTableColumn(x, scale)).ToArray(),
                Sections = table.Sections.Select(x => new TableSectionBlock(x.Kind, x.Rows.Select(r => ScaleTableRow(r, scale)).ToArray())).ToArray(),
                Style = table.Style is null ? null : ScaleTableStyle(table.Style, scale),
                Layout = table.Layout is null ? null : ScaleTableLayout(table.Layout, scale)
            },
            RowBlock row => row with
            {
                Children = row.Children.Select(x => ScaleLayoutChild(x, scale)).ToArray(),
                Height = row.Height.HasValue ? row.Height.Value * scale : null,
                Style = row.Style is null ? null : ScaleLayoutContainerStyle(row.Style, scale)
            },
            ColumnBlock column => column with
            {
                Children = column.Children.Select(x => ScaleLayoutChild(x, scale)).ToArray(),
                Height = column.Height.HasValue ? column.Height.Value * scale : null,
                Style = column.Style is null ? null : ScaleLayoutContainerStyle(column.Style, scale)
            },
            _ => throw new NotSupportedException($"Unsupported block type '{block.GetType().Name}'.")
        };

    private static ListItemBlock ScaleListItem(ListItemBlock item, double scale)
        => item with
        {
            Blocks = item.Blocks.Select(x => ScaleBlock(x, scale)).ToArray()
        };

    private static TableRowBlock ScaleTableRow(TableRowBlock row, double scale)
        => row with
        {
            Cells = row.Cells.Select(x => ScaleTableCell(x, scale)).ToArray(),
            Style = row.Style is null ? null : ScaleTableRowStyle(row.Style, scale)
        };

    private static TableCellBlock ScaleTableCell(TableCellBlock cell, double scale)
    {
        var scaledBlocks = cell.Blocks.Select(x => ScaleBlock(x, scale)).ToArray();
        var scaledStyle = cell.Style is null ? null : ScaleTableCellStyle(cell.Style, scale);
        return cell switch
        {
            TableHeaderCellBlock => new TableHeaderCellBlock(scaledBlocks, cell.ColSpan, cell.RowSpan, scaledStyle),
            TableDataCellBlock => new TableDataCellBlock(scaledBlocks, cell.ColSpan, cell.RowSpan, scaledStyle),
            _ => throw new NotSupportedException($"Unsupported table cell type '{cell.GetType().Name}'.")
        };
    }

    private static TableColumnDefinition ScaleTableColumn(TableColumnDefinition column, double scale)
        => column with
        {
            Width = ScaleColumnWidthSpec(column.Width, scale)
        };

    private static LayoutChild ScaleLayoutChild(LayoutChild child, double scale)
        => child with
        {
            Blocks = child.Blocks.Select(x => ScaleBlock(x, scale)).ToArray(),
            FixedSize = child.FixedSize.HasValue ? child.FixedSize.Value * scale : null,
            BoxStyle = child.BoxStyle is null ? null : ScaleBoxStyle(child.BoxStyle, scale)
        };

    private static TableStyle ScaleTableStyle(TableStyle style, double scale)
        => style with
        {
            BorderWidth = style.BorderWidth * scale,
            CellBorderWidth = style.CellBorderWidth * scale,
            CellPadding = style.CellPadding * scale,
            MarginBlockEnd = style.MarginBlockEnd * scale
        };

    private static TableCellStyle ScaleTableCellStyle(TableCellStyle style, double scale)
        => style with
        {
            Padding = style.Padding.HasValue ? style.Padding.Value * scale : null
        };

    private static TableRowStyle ScaleTableRowStyle(TableRowStyle style, double scale)
        => style with
        {
            MinHeight = style.MinHeight.HasValue ? style.MinHeight.Value * scale : null
        };

    private static TableLayoutSpec ScaleTableLayout(TableLayoutSpec layout, double scale)
        => layout with
        {
            Width = ScaleTableWidthSpec(layout.ResolvedWidth, scale)
        };

    private static TableWidthSpec ScaleTableWidthSpec(TableWidthSpec width, double scale)
        => width switch
        {
            TableFixedWidth fixedWidth => new TableFixedWidth(fixedWidth.Points * scale),
            TablePercentWidth percentWidth => percentWidth,
            TableAutoWidth autoWidth => autoWidth,
            _ => width
        };

    private static ColumnWidthSpec ScaleColumnWidthSpec(ColumnWidthSpec width, double scale)
        => width switch
        {
            ColumnFixedWidth fixedWidth => new ColumnFixedWidth(fixedWidth.Points * scale),
            ColumnPercentWidth percentWidth => percentWidth,
            ColumnMinMaxWidth minMaxWidth => new ColumnMinMaxWidth(
                minMaxWidth.MinPoints.HasValue ? minMaxWidth.MinPoints.Value * scale : null,
                minMaxWidth.MaxPoints.HasValue ? minMaxWidth.MaxPoints.Value * scale : null),
            ColumnFlexWidth flexWidth => flexWidth,
            ColumnAutoWidth autoWidth => autoWidth,
            _ => width
        };

    private static LayoutContainerStyle ScaleLayoutContainerStyle(LayoutContainerStyle style, double scale)
        => style with
        {
            BorderWidth = style.BorderWidth * scale,
            Padding = style.Padding * scale,
            Gap = style.Gap * scale,
            MarginBlockEnd = style.MarginBlockEnd * scale
        };

    private static InlineNode ScaleInline(InlineNode inline, double scale)
        => inline switch
        {
            TextRunNode run => run with { Style = ScaleTextStyle(run.Style, scale) },
            LineBreakNode lineBreak => lineBreak,
            _ => throw new NotSupportedException($"Unsupported inline type '{inline.GetType().Name}'.")
        };

    public sealed record ConformanceCase(
        string Name,
        IReadOnlyList<TextSegment>? Segments,
        IReadOnlyList<RichTextBlock>? Blocks,
        string? HtmlSource = null,
        string? NormalizedHtml = null,
        TextHorizontalAlignment HorizontalAlignment = TextHorizontalAlignment.Left,
        double PageWidth = 240,
        double PageHeight = 160,
        double BoxLeft = 20,
        double BoxTop = 20,
        double BoxWidth = 180,
        double BoxHeight = 100,
        double? ListIndent = null,
        double? ListMarkerGap = null,
        TextFontMetricSource MetricPreference = TextFontMetricSource.Windows,
        TextBoxStyle BoxStyle = null!,
        IReadOnlyList<FixtureFontAsset> Fonts = null!)
    {
        public static ConformanceCase FromSegments(
            string name,
            IReadOnlyList<TextSegment> segments,
            TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left,
            double pageWidth = 240,
            double pageHeight = 160,
            double boxLeft = 20,
            double boxTop = 20,
            double boxWidth = 180,
            double boxHeight = 100,
            TextFontMetricSource metricPreference = TextFontMetricSource.None,
            TextBoxStyle? boxStyle = null,
            IReadOnlyList<FixtureFontAsset>? fonts = null)
        {
            var resolvedPageHeight = Math.Max(pageHeight, boxTop + boxHeight + 20d);
            return new(name, segments, null, null, null, horizontalAlignment, pageWidth, resolvedPageHeight, boxLeft, boxTop, boxWidth, boxHeight, null, null, metricPreference, boxStyle ?? new TextBoxStyle(), fonts ?? CreateFontAssets());
        }

        public static ConformanceCase FromBlocks(
            string name,
            IReadOnlyList<RichTextBlock> blocks,
            double pageWidth = 240,
            double pageHeight = 160,
            double boxLeft = 20,
            double boxTop = 20,
            double boxWidth = 180,
            double boxHeight = 100,
            double? listIndent = null,
            double? listMarkerGap = null,
            TextFontMetricSource metricPreference = TextFontMetricSource.None,
            TextBoxStyle? boxStyle = null,
            IReadOnlyList<FixtureFontAsset>? fonts = null)
        {
            var resolvedPageHeight = Math.Max(pageHeight, boxTop + boxHeight + 20d);
            return new(name, null, blocks, null, null, TextHorizontalAlignment.Left, pageWidth, resolvedPageHeight, boxLeft, boxTop, boxWidth, boxHeight, listIndent, listMarkerGap, metricPreference, boxStyle ?? new TextBoxStyle(), fonts ?? CreateFontAssets());
        }

        public static ConformanceCase FromHtml(
            string name,
            string html,
            TextStyle defaultTextStyle,
            double pageWidth = 240,
            double pageHeight = 160,
            double boxLeft = 20,
            double boxTop = 20,
            double boxWidth = 180,
            double boxHeight = 100,
            double? listIndent = null,
            double? listMarkerGap = null,
            TextFontMetricSource metricPreference = TextFontMetricSource.None,
            TextBoxStyle? boxStyle = null,
            IReadOnlyList<FixtureFontAsset>? fonts = null)
        {
            var parser = new HtmlRichTextParser();
            var parsedBlocks = parser.Parse(html, defaultTextStyle);
            var normalizedHtml = parser.ToHtml(parsedBlocks);
            var resolvedPageHeight = Math.Max(pageHeight, boxTop + boxHeight + 20d);
            return new(name, null, parsedBlocks, html, normalizedHtml, TextHorizontalAlignment.Left, pageWidth, resolvedPageHeight, boxLeft, boxTop, boxWidth, boxHeight, listIndent, listMarkerGap, metricPreference, boxStyle ?? new TextBoxStyle(), fonts ?? CreateFontAssets());
        }
    }

    private sealed record ExtractedWord(string Text, PdfRect<double> BoundingBox);
    private sealed record ExtractedText(
        IReadOnlyList<ExtractedWord> Words,
        IReadOnlyList<IReadOnlyList<ExtractedWord>> Lines);
    private sealed record ExtractedLineInfo(int Index, ExtractedWord Anchor, int StartWordIndex);
    private sealed record ExtractedWordLineInfo(int WordIndex, int LineIndex, bool IsLineStart);
    private sealed record ExtractedWordLineLayout(
        IReadOnlyList<ExtractedLineInfo> Lines,
        IReadOnlyList<ExtractedWordLineInfo> Words);

    private static IReadOnlyList<FixtureFontAsset> CreateFontAssets()
        => new List<FixtureFontAsset>
        {
            new(SansFamily, 400, File.ReadAllBytes(RequireFont("Roboto-Regular.ttf"))),
            new(SansFamily, 700, File.ReadAllBytes(RequireFont("Roboto-Bold.ttf"))),
            new(SansFamily, 400, File.ReadAllBytes(RequireFont("Roboto-Italic.ttf")), Italic: true),
            new(SansFamily, 700, File.ReadAllBytes(RequireFont("Roboto-BoldItalic.ttf")), Italic: true),
            new(SerifFamily, 400, File.ReadAllBytes(RequireFont("NotoSerif-Regular.ttf"))),
            new(SerifFamily, 700, File.ReadAllBytes(RequireFont("NotoSerif-Bold.ttf"))),
            new(MonoFamily, 400, File.ReadAllBytes(RequireFont("RobotoMono-Regular.ttf"))),
            new(CondensedFamily, 400, File.ReadAllBytes(RequireFont("Roboto_Condensed-Regular.ttf"))),
            new(CondensedFamily, 700, File.ReadAllBytes(RequireFont("Roboto_Condensed-Bold.ttf"))),
            new(CondensedFamily, 400, File.ReadAllBytes(RequireFont("Roboto_Condensed-Italic.ttf")), Italic: true),
            new(SemiCondensedFamily, 400, File.ReadAllBytes(RequireFont("Roboto_SemiCondensed-Regular.ttf")))
        };

    private static PdfTextLayoutFontLibrary CreateFontLibrary(IReadOnlyList<FixtureFontAsset> fonts)
        => new(fonts.Select((font, index) =>
        {
            var embeddable = TrueTypeFont.CreateEmbeddableFont(font.FontData);
            if (embeddable is not TrueTypeEmbeddedFont trueType)
            {
                throw new InvalidOperationException($"Expected a TrueTypeEmbeddedFont for fixture family '{font.FamilyName}'.");
            }

            var metrics = trueType.GetLayoutMetrics();
            var sources = trueType.GetMetricSources();
            return new PdfTextLayoutFontFace(
                new TextFontFace(
                    $"fixture-{index}",
                    font.FamilyName,
                    font.Weight,
                    font.FontData,
                    Italic: font.Italic,
                    UnitsPerEm: metrics.UnitsPerEm,
                    Ascent: metrics.Ascent,
                    Descent: metrics.Descent,
                    LineGap: metrics.LineGap,
                    MetricSource: ToTextMetricSource(metrics.Source),
                    TypographicAscent: sources.Typographic.Ascent,
                    TypographicDescent: sources.Typographic.Descent,
                    TypographicLineGap: sources.Typographic.LineGap,
                    HorizontalHeaderAscent: sources.HorizontalHeader.Ascent,
                    HorizontalHeaderDescent: sources.HorizontalHeader.Descent,
                    HorizontalHeaderLineGap: sources.HorizontalHeader.LineGap,
                    WindowsAscent: sources.Windows.Ascent,
                    WindowsDescent: sources.Windows.Descent,
                    WindowsLineGap: sources.Windows.LineGap),
                trueType.GetDefaultEncodedFont());
        }));

    private static TextFontMetricSource ToTextMetricSource(FontLayoutMetricSource source)
        => source switch
        {
            FontLayoutMetricSource.Typographic => TextFontMetricSource.Typographic,
            FontLayoutMetricSource.HorizontalHeader => TextFontMetricSource.HorizontalHeader,
            FontLayoutMetricSource.Windows => TextFontMetricSource.Windows,
            _ => TextFontMetricSource.None
        };

    private static string RequireFont(string fileName)
    {
        var path = Path.Combine(TestRoot, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Expected conformance test font '{fileName}' was not found in '{TestRoot}'.", path);
        }

        return path;
    }
}
