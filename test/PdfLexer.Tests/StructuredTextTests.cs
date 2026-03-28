using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class StructuredTextTests
{
    public StructuredTextTests()
    {
        CMaps.AddKnownPdfCMaps();
    }

    [Fact]
    public void StructuredText_GroupsLinesAndParagraphs()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("First paragraph line one")
                .TextMove(72, 686)
                .Text("First paragraph line two")
                .TextMove(72, 650)
                .Text("Second paragraph");
        }

        var structured = page.GetStructuredText(doc.Context);

        Assert.Equal(3, structured.Lines.Count);
        Assert.Equal(2, structured.Paragraphs.Count);
        Assert.Equal("First paragraph line one", structured.Lines[0].Text);
        Assert.Equal("First paragraph line two", structured.Lines[1].Text);
        Assert.Equal("Second paragraph", structured.Paragraphs[1].Text);
    }

    [Fact]
    public void StructuredText_PreservesCompactSourceReferences()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha Beta");
        }

        var structured = page.GetStructuredText(doc.Context);
        var character = structured.Characters.First();
        var word = structured.Words.First();
        var line = structured.Lines.First();
        var paragraph = structured.Paragraphs.First();

        Assert.True(character.SourceReference.OperatorLength > 0);
        Assert.Single(word.SourceReferences);
        Assert.Single(line.SourceReferences);
        Assert.Single(paragraph.SourceReferences);
        Assert.Contains(character.SourceReference, word.SourceReferences);
        foreach (var sourceRef in word.SourceReferences)
        {
            Assert.Contains(sourceRef, line.SourceReferences);
            Assert.Contains(sourceRef, paragraph.SourceReferences);
        }
    }

    [Fact]
    public void StructuredText_KeepsSequenceOrderingSeparateFromCompactSourceReferences()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha");
        }

        var structured = page.GetStructuredText(doc.Context);
        var characterRefs = structured.Characters.Select(x => x.SourceReference).Distinct().ToList();
        var characterOrder = structured.Characters.Select(x => x.Position).ToList();

        Assert.Single(characterRefs);
        Assert.Equal(5, characterOrder.Count);
        Assert.Equal("Alpha", structured.Words.Single().Text);
        Assert.Single(structured.Words.Single().SourceReferences);
    }

    [Fact]
    public void StructuredText_CanReturnContentAndReadingOrderText()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 650)
                .Text("Lower line first")
                .TextMove(72, 700)
                .Text("Upper line second");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions { Order = TextOrder.Reading });

        Assert.StartsWith("Lower line first", structured.GetText(TextOrder.Content));
        Assert.StartsWith("Upper line second", structured.GetText(TextOrder.Reading));
        Assert.Equal("Upper line second", structured.Lines[0].Text);
    }

    [Fact]
    public void StructuredText_AllowsCustomGrouperToOverrideLayout()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha Beta");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions
        {
            Grouper = new OneWordPerLineGrouper()
        });

        Assert.Equal(2, structured.Lines.Count);
        Assert.Equal("Alpha", structured.Lines[0].Text);
        Assert.Equal("Beta", structured.Lines[1].Text);
        Assert.Equal("Alpha" + Environment.NewLine + Environment.NewLine + "Beta", structured.GetText());
    }

    [Fact]
    public void StructuredText_ContentOrderGrouperKeepsReadingCollectionsInContentOrder()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 650)
                .Text("Lower line first")
                .TextMove(72, 700)
                .Text("Upper line second");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions
        {
            Order = TextOrder.Reading,
            Grouper = new ContentOrderStructuredTextGrouper()
        });

        Assert.StartsWith("Lower line first", structured.GetText(TextOrder.Reading));
        Assert.Equal("Lower line first", structured.GetLines(TextOrder.Reading, StructuredTextMode.Raw)[0].Text);
    }

    [Fact]
    public void StructuredText_DocstrumLikeGrouperReadsColumnsBeforeMovingRight()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Left one")
                .TextMove(300, 700)
                .Text("Right one")
                .TextMove(72, 686)
                .Text("Left two")
                .TextMove(300, 686)
                .Text("Right two");
        }

        var heuristic = page.GetStructuredText(doc.Context, new StructuredTextOptions { Order = TextOrder.Reading });
        var docstrumLike = page.GetStructuredText(doc.Context, new StructuredTextOptions
        {
            Order = TextOrder.Reading,
            Grouper = new DocstrumLikeStructuredTextGrouper()
        });

        Assert.Equal(
            new[] { "Left one", "Right one", "Left two", "Right two" },
            heuristic.GetLines(TextOrder.Reading, StructuredTextMode.Raw).Select(x => x.Text).ToArray());
        Assert.Equal(
            new[] { "Left one", "Left two", "Right one", "Right two" },
            docstrumLike.GetLines(TextOrder.Reading, StructuredTextMode.Raw).Select(x => x.Text).ToArray());
    }

    [Fact]
    public void StructuredText_RebuildsReadingParagraphsFromReadingOrderedLines()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 640)
                .Text("Bottom paragraph")
                .TextMove(72, 700)
                .Text("Top paragraph line one")
                .TextMove(72, 686)
                .Text("Top paragraph line two");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions { Order = TextOrder.Reading });
        var readingParagraphs = structured.GetParagraphs(TextOrder.Reading, StructuredTextMode.Raw);

        Assert.Equal(2, readingParagraphs.Count);
        Assert.Equal("Top paragraph line one" + Environment.NewLine + "Top paragraph line two", readingParagraphs[0].Text);
        Assert.Equal("Bottom paragraph", readingParagraphs[1].Text);
    }

    [Fact]
    public void StructuredText_LazilyCachesBoundingBoxes()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.RotateAt(90, 100, 100)
                .Font(Base14.Helvetica, 12)
                .TextMove(0, 0)
                .Text("A");
        }

        var structured = page.GetStructuredText(doc.Context);
        var character = structured.Characters.Single();
        var word = structured.Words.Single();
        var line = structured.Lines.Single();

        var charBox = character.BoundingBox;
        var wordBox = word.BoundingBox;
        var lineBox = line.BoundingBox;
        var rotationDelta = Math.Min(Math.Abs(character.Rotation - 90d), Math.Abs(character.Rotation - 270d));

        Assert.True(rotationDelta < 5d);
        Assert.True(charBox.Height() > 0d || charBox.Width() > 0d);
        Assert.Same(charBox, character.BoundingBox);
        Assert.Same(wordBox, word.BoundingBox);
        Assert.Same(lineBox, line.BoundingBox);
    }

    [Fact]
    public void StructuredText_ExposesRelativeCoordinatesUsingCropBoxAndRotation()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        page.CropBox = new PdfRectangle(new PdfArray { 50, 100, 250, 400 });
        page.Rotate = 90;
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 140)
                .Text("A");
        }

        var structured = page.GetStructuredText(doc.Context);
        var character = structured.Characters.Single();

        Assert.Equal(300d, structured.RelativePageBox.Width(), 6);
        Assert.Equal(200d, structured.RelativePageBox.Height(), 6);
        Assert.InRange(character.RelativePosition.X, 39d, 41d);
        Assert.InRange(character.RelativePosition.Y, 177d, 179d);
        Assert.True(character.RelativeBoundingBox.LLx >= 0d);
        Assert.True(character.RelativeBoundingBox.LLy >= 0d);
        Assert.True(character.RelativeBoundingBox.URx <= structured.RelativePageBox.URx + 0.001d);
        Assert.True(character.RelativeBoundingBox.URy <= structured.RelativePageBox.URy + 0.001d);
    }

    [Fact]
    public void StructuredText_MergesOutOfOrderWordFragmentsWithinALine()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(94, 700)
                .Text("ld")
                .TextMove(72, 700)
                .Text("Wor");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions { Order = TextOrder.Reading });

        Assert.Single(structured.Lines);
        Assert.Single(structured.Words);
        Assert.Equal("World", structured.Words[0].Text);
        Assert.Equal("World", structured.Lines[0].Text);
    }

    [Fact]
    public void StructuredText_CanExposeRawAndDeduplicatedWords()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Echo")
                .TextMove(72, 700)
                .Text("Echo");
        }

        var raw = page.GetStructuredText(doc.Context);
        var deduplicated = page.GetStructuredText(doc.Context, new StructuredTextOptions { Mode = StructuredTextMode.Deduplicated });

        Assert.Equal(2, raw.RawWords.Count);
        Assert.Single(deduplicated.DeduplicatedWords);
        Assert.Equal("Echo Echo", raw.GetText(TextOrder.Reading, StructuredTextMode.Raw));
        Assert.Equal("Echo", deduplicated.GetText(TextOrder.Reading, StructuredTextMode.Deduplicated));
    }

    [Fact]
    public void StructuredText_DoesNotDeduplicateSeparatedRepeatedWords()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Echo")
                .TextMove(140, 700)
                .Text("Echo");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions { Mode = StructuredTextMode.Deduplicated });

        Assert.Equal(2, structured.DeduplicatedWords.Count);
        Assert.Equal("Echo Echo", structured.GetText(TextOrder.Reading, StructuredTextMode.Deduplicated));
    }

    [Fact]
    public void StructuredText_DeduplicatedModeStillUsesSelectedGrouper()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Echo")
                .TextMove(72, 700)
                .Text("Echo");
        }

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions
        {
            Mode = StructuredTextMode.Deduplicated,
            Grouper = new OneWordPerLineGrouper()
        });

        Assert.Single(structured.DeduplicatedWords);
        Assert.Single(structured.GetLines(TextOrder.Reading, StructuredTextMode.Deduplicated));
        Assert.Equal("Echo", structured.GetText(TextOrder.Reading, StructuredTextMode.Deduplicated));
    }

    [Fact]
    public void StructuredText_HandlesTracemonkeyPageOneMoreReasonably()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdf = Path.Combine(tp, "pdfs", "pdfjs", "tracemonkey.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var page = doc.Pages[0];

        var structured = page.GetStructuredText(doc.Context, new StructuredTextOptions { Order = TextOrder.Reading });
        var readingLines = structured.GetLines(TextOrder.Reading, StructuredTextMode.Raw);
        var readingParagraphs = structured.GetParagraphs(TextOrder.Reading, StructuredTextMode.Raw);

        Assert.True(readingLines[0].Words.Count > 1);
        Assert.NotEmpty(readingParagraphs);
        Assert.True(readingParagraphs.Count <= readingLines.Count);
        Assert.Contains(readingParagraphs, x => x.Text.Contains("Abstract", StringComparison.Ordinal));
    }

    private sealed class OneWordPerLineGrouper : IStructuredTextGrouper
    {
        public StructuredTextGroupingResult Group(StructuredTextGroupingInput input)
        {
            var lines = input.Words
                .Select(x => (IReadOnlyList<StructuredWord>)new[] { x })
                .ToList();
            var paragraphs = lines
                .Select(x => (IReadOnlyList<IReadOnlyList<StructuredWord>>)new[] { x })
                .ToList();
            return new StructuredTextGroupingResult
            {
                ContentLines = lines,
                ReadingLines = lines,
                ContentParagraphs = paragraphs,
                ReadingParagraphs = paragraphs
            };
        }
    }
}
