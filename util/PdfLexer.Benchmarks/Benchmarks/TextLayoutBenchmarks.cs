using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;
using pdflexer.TestCaseGen;
using PdfLexer;
using PdfLexer.Content;
using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.Benchmarks.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class TextLayoutBenchmarks : IDisposable
{
    private readonly TextBoxLayoutEngine _flatEngine = new();
    private readonly RichTextBoxLayoutEngine _richEngine = new();

    private TextBoxLayoutRequest _flatRequest = null!;
    private RichTextBoxLayoutRequest _richRequest = null!;
    private PdfTextLayoutFontLibrary _pdfFontLibrary = null!;
    private TextLayoutAnalysisContext _context;

    [GlobalSetup]
    public void Setup()
    {
        CMaps.AddKnownPdfCMaps();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var fixture = TextLayoutBenchmarkFixture.Create();
        _flatRequest = fixture.FlatRequest;
        _richRequest = fixture.RichRequest;
        _pdfFontLibrary = fixture.PdfFontLibrary;
        _context = new TextLayoutAnalysisContext();
        FlatLayout_LongParagraph();
        RichAnalyzeFit_NestedContent();
        RichWrite_PrecomputedPlan();
    }

    [Benchmark(Baseline = true)]
    public int FlatLayout_LongParagraph()
    {
        _context.ClearMeasurements();
        return _flatEngine.Layout(_flatRequest, _context).LineCount;
    }
        

    [Benchmark]
    public int RichAnalyzeFit_NestedContent()
    {
        _context.ClearMeasurements();
        return _richEngine.AnalyzeFit(_richRequest, _context).FittedSelection.Flatten().Layout.LineCount;
    }

    [Benchmark]
    public long RichWrite_PrecomputedPlan()
    {
        _context.ClearMeasurements();
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = page.GetWriter();

        var plan = _richEngine.Analyze(_richRequest, _context);
        var result = writer.WriteTextBox(new PdfRect<double>(24, 24, 572, 760), plan, _pdfFontLibrary);

        using var stream = new MemoryStream();
        doc.SaveTo(stream);
        return stream.Length + result.LineCount;
    }

    public void Dispose() => _context.Dispose();

    private static class TextLayoutBenchmarkFixture
    {
        private static readonly string RobotoPath = Path.GetFullPath(Path.Combine(PathUtil.GetPathFromSegmentOfCurrent("pdflexer"), "test", "Roboto-Regular.ttf"));
        private static readonly Lazy<byte[]> RobotoBytes = new(() => File.ReadAllBytes(RobotoPath));

        public static FixtureData Create()
        {
            
            var textFace = CreateFace("roboto-regular", "Roboto", 400);
            var italicFace = CreateFace("roboto-italic", "Roboto", 400, italic: true);

            var library = new TextFontLibrary(new[] { textFace, italicFace });
            var pdfLibrary = new PdfTextLayoutFontLibrary(new[]
            {
                new PdfTextLayoutFontFace(textFace, TrueTypeFont.CreateWritableFont(RobotoBytes.Value)),
                new PdfTextLayoutFontFace(italicFace, TrueTypeFont.CreateWritableFont(RobotoBytes.Value))
            });

            return new FixtureData(
                CreateFlatRequest(library),
                CreateRichRequest(library),
                pdfLibrary);
        }

        private static TextBoxLayoutRequest CreateFlatRequest(TextFontLibrary library)
            => new(
                220,
                600,
                library,
                CreateFlatSegments())
            {
                OverflowMode = TextOverflowMode.Clip
            };

        private static IReadOnlyList<TextSegment> CreateFlatSegments()
        {
            var segments = new List<TextSegment>();
            var style = new TextSegmentStyle(
                "Roboto",
                400,
                12,
                CharacterSpacing: 0.2,
                WordSpacing: 0.4);
            var emphasis = new TextSegmentStyle(
                "Roboto",
                400,
                12,
                Italic: true,
                Underline: true,
                CharacterSpacing: 0.2,
                WordSpacing: 0.4);

            for (var i = 0; i < 18; i++)
            {
                segments.Add(new TextSegment(
                    $"Paragraph {i} explores TextLayout throughput with repeated shaping, wrapping, tabs\tand explicit line breaks.\n",
                    style,
                    0,
                    $"Paragraph {i} explores TextLayout throughput with repeated shaping, wrapping, tabs\tand explicit line breaks.\n".Length,
                    $"Segments[{i}]",
                    $"flat-{i}"));
                segments.Add(new TextSegment(
                    "Instrumentation should make cache hits, line wrapping, and glyph positioning workloads obvious under BenchmarkDotNet. ",
                    emphasis,
                    0,
                    "Instrumentation should make cache hits, line wrapping, and glyph positioning workloads obvious under BenchmarkDotNet. ".Length,
                    $"Segments[{i}]-emphasis",
                    $"flat-emphasis-{i}"));
            }

            return segments;
        }

        private static RichTextBoxLayoutRequest CreateRichRequest(TextFontLibrary library)
            => new(
                320,
                420,
                library,
                new RichTextBlock[]
                {
                    new HeadingBlock(
                        2,
                        new InlineNode[]
                        {
                            new TextRunNode("Performance Review", new TextStyle("Roboto", 400, 18, Underline: true))
                        },
                        new ParagraphStyle(LineHeight: 22, MarginBlockEnd: 10)),
                    new ParagraphBlock(
                        new InlineNode[]
                        {
                            new TextRunNode(
                                "This benchmark mixes nested containers, lists, and tables so the rich measurement path cannot collapse into a trivial paragraph-only case. ",
                                new TextStyle("Roboto", 400, 12)),
                            new TextRunNode(
                                "The goal is to exercise repeated subtree measurement and write-path allocation pressure in a single representative request.",
                                new TextStyle("Roboto", 400, 12, Italic: true, BackgroundColor: new TextColor(245, 245, 180)))
                        },
                        new ParagraphStyle(LineHeight: 16, MarginBlockEnd: 10)),
                    new ColumnBlock(
                        new LayoutChild[]
                        {
                            new(
                                new RichTextBlock[]
                                {
                                    new UnorderedListBlock(new[]
                                    {
                                        new ListItemBlock(new RichTextBlock[]
                                        {
                                            new ParagraphBlock(new InlineNode[]
                                            {
                                                new TextRunNode("Profile candidate rich blocks with shared analysis context.", new TextStyle("Roboto", 400, 12))
                                            }, new ParagraphStyle(LineHeight: 15))
                                        }),
                                        new ListItemBlock(new RichTextBlock[]
                                        {
                                            new ParagraphBlock(new InlineNode[]
                                            {
                                                new TextRunNode("Reduce token and glyph allocation churn in the flat paragraph pipeline.", new TextStyle("Roboto", 400, 12))
                                            }, new ParagraphStyle(LineHeight: 15))
                                        }),
                                        new ListItemBlock(new RichTextBlock[]
                                        {
                                            new ParagraphBlock(new InlineNode[]
                                            {
                                                new TextRunNode("Verify the write path preserves HarfBuzz-based advances.", new TextStyle("Roboto", 400, 12))
                                            }, new ParagraphStyle(LineHeight: 15))
                                        })
                                    })
                                },
                                Weight: 1),
                            new(
                                new RichTextBlock[]
                                {
                                    new TableBlock(new[]
                                    {
                                        new TableRowBlock(new[]
                                        {
                                            new TableHeaderCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Area", new TextStyle("Roboto", 400, 11, Underline: true))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            }),
                                            new TableHeaderCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Focus", new TextStyle("Roboto", 400, 11, Underline: true))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            })
                                        }),
                                        new TableRowBlock(new[]
                                        {
                                            new TableDataCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Rich fit", new TextStyle("Roboto", 400, 11))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            }),
                                            new TableDataCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Repeated subtree measurement and fragmentation", new TextStyle("Roboto", 400, 11))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            })
                                        }),
                                        new TableRowBlock(new[]
                                        {
                                            new TableDataCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Writer", new TextStyle("Roboto", 400, 11))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            }),
                                            new TableDataCellBlock(new RichTextBlock[]
                                            {
                                                new ParagraphBlock(new InlineNode[]
                                                {
                                                    new TextRunNode("Per-run glyph translation and spacing adjustments", new TextStyle("Roboto", 400, 11))
                                                }, new ParagraphStyle(LineHeight: 14))
                                            })
                                        })
                                    },
                                    new TableStyle(CellPadding: 4, CellBorderWidth: 1, CellBorderColor: new TextColor(80, 80, 80)))
                                },
                                Weight: 1)
                        },
                        null,
                        new LayoutContainerStyle(Gap: 10))
                })
            {
                OverflowMode = TextOverflowMode.Fragment,
                PreserveTrailingWhitespaceInWidth = true
            };

        private static TextFontFace CreateFace(string faceId, string familyName, int weight, bool italic = false)
            => new(faceId, familyName, weight, RobotoBytes.Value, Italic: italic);
    }

    private sealed record FixtureData(
        TextBoxLayoutRequest FlatRequest,
        RichTextBoxLayoutRequest RichRequest,
        PdfTextLayoutFontLibrary PdfFontLibrary);
}
