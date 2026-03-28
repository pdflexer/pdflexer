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

public class SemanticExtractTests
{
    public SemanticExtractTests()
    {
        CMaps.AddKnownPdfCMaps();
    }

    [Fact]
    public void SemanticExtract_GroupsLinesAndParagraphs()
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

        var semantic = page.GetSemanticExtract(doc.Context);

        Assert.Equal(3, semantic.Lines.Count);
        Assert.Equal(2, semantic.Paragraphs.Count);
        Assert.Equal("First paragraph line one", semantic.Lines[0].Text);
        Assert.Equal("First paragraph line two", semantic.Lines[1].Text);
        Assert.Equal("Second paragraph", semantic.Paragraphs[1].Text);
    }

    [Fact]
    public void SemanticExtract_PreservesCompactSourceReferences()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha Beta");
        }

        var semantic = page.GetSemanticExtract(doc.Context);
        var character = semantic.Characters.First();
        var word = semantic.Words.First();
        var line = semantic.Lines.First();
        var paragraph = semantic.Paragraphs.First();

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
    public void SemanticExtract_KeepsSequenceOrderingSeparateFromCompactSourceReferences()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha");
        }

        var semantic = page.GetSemanticExtract(doc.Context);
        var characterRefs = semantic.Characters.Select(x => x.SourceReference).Distinct().ToList();
        var characterOrder = semantic.Characters.Select(x => x.Position).ToList();

        Assert.Single(characterRefs);
        Assert.Equal(5, characterOrder.Count);
        Assert.Equal("Alpha", semantic.Words.Single().Text);
        Assert.Single(semantic.Words.Single().SourceReferences);
    }

    [Fact]
    public void SemanticExtract_CanReturnContentAndReadingOrderText()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Order = TextOrder.Reading });

        Assert.StartsWith("Lower line first", semantic.GetText(TextOrder.Content));
        Assert.StartsWith("Upper line second", semantic.GetText(TextOrder.Reading));
        Assert.Equal("Upper line second", semantic.Lines[0].Text);
    }

    [Fact]
    public void SemanticExtract_AllowsCustomGrouperToOverrideLayout()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using (var writer = page.GetWriter())
        {
            writer.Font(Base14.Helvetica, 12)
                .TextMove(72, 700)
                .Text("Alpha Beta");
        }

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
        {
            Grouper = new OneWordPerLineGrouper()
        });

        Assert.Equal(2, semantic.Lines.Count);
        Assert.Equal("Alpha", semantic.Lines[0].Text);
        Assert.Equal("Beta", semantic.Lines[1].Text);
        Assert.Equal("Alpha" + Environment.NewLine + Environment.NewLine + "Beta", semantic.GetText());
    }

    [Fact]
    public void SemanticExtract_ContentOrderGrouperKeepsReadingCollectionsInContentOrder()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
        {
            Order = TextOrder.Reading,
            Grouper = new ContentOrderSemanticTextGrouper()
        });

        Assert.StartsWith("Lower line first", semantic.GetText(TextOrder.Reading));
        Assert.Equal("Lower line first", semantic.GetLines(TextOrder.Reading, SemanticTextMode.Raw)[0].Text);
    }

    [Fact]
    public void SemanticExtract_DocstrumLikeGrouperReadsColumnsBeforeMovingRight()
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

        var heuristic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Order = TextOrder.Reading });
        var docstrumLike = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
        {
            Order = TextOrder.Reading,
            Grouper = new DocstrumLikeSemanticTextGrouper()
        });

        Assert.Equal(
            new[] { "Left one", "Right one", "Left two", "Right two" },
            heuristic.GetLines(TextOrder.Reading, SemanticTextMode.Raw).Select(x => x.Text).ToArray());
        Assert.Equal(
            new[] { "Left one", "Left two", "Right one", "Right two" },
            docstrumLike.GetLines(TextOrder.Reading, SemanticTextMode.Raw).Select(x => x.Text).ToArray());
    }

    [Fact]
    public void SemanticExtract_RebuildsReadingParagraphsFromReadingOrderedLines()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Order = TextOrder.Reading });
        var readingParagraphs = semantic.GetParagraphs(TextOrder.Reading, SemanticTextMode.Raw);

        Assert.Equal(2, readingParagraphs.Count);
        Assert.Equal("Top paragraph line one" + Environment.NewLine + "Top paragraph line two", readingParagraphs[0].Text);
        Assert.Equal("Bottom paragraph", readingParagraphs[1].Text);
    }

    [Fact]
    public void SemanticExtract_LazilyCachesBoundingBoxes()
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

        var semantic = page.GetSemanticExtract(doc.Context);
        var character = semantic.Characters.Single();
        var word = semantic.Words.Single();
        var line = semantic.Lines.Single();

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
    public void SemanticExtract_ExposesRelativeCoordinatesUsingCropBoxAndRotation()
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

        var semantic = page.GetSemanticExtract(doc.Context);
        var character = semantic.Characters.Single();

        Assert.Equal(300d, semantic.RelativePageBox.Width(), 6);
        Assert.Equal(200d, semantic.RelativePageBox.Height(), 6);
        Assert.InRange(character.RelativePosition.X, 39d, 41d);
        Assert.InRange(character.RelativePosition.Y, 177d, 179d);
        Assert.True(character.RelativeBoundingBox.LLx >= 0d);
        Assert.True(character.RelativeBoundingBox.LLy >= 0d);
        Assert.True(character.RelativeBoundingBox.URx <= semantic.RelativePageBox.URx + 0.001d);
        Assert.True(character.RelativeBoundingBox.URy <= semantic.RelativePageBox.URy + 0.001d);
    }

    [Fact]
    public void SemanticExtract_MergesOutOfOrderWordFragmentsWithinALine()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Order = TextOrder.Reading });

        Assert.Single(semantic.Lines);
        Assert.Single(semantic.Words);
        Assert.Equal("World", semantic.Words[0].Text);
        Assert.Equal("World", semantic.Lines[0].Text);
    }

    [Fact]
    public void SemanticExtract_CanExposeRawAndDeduplicatedWords()
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

        var raw = page.GetSemanticExtract(doc.Context);
        var deduplicated = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Mode = SemanticTextMode.Deduplicated });

        Assert.Equal(2, raw.RawWords.Count);
        Assert.Single(deduplicated.DeduplicatedWords);
        Assert.Equal("Echo Echo", raw.GetText(TextOrder.Reading, SemanticTextMode.Raw));
        Assert.Equal("Echo", deduplicated.GetText(TextOrder.Reading, SemanticTextMode.Deduplicated));
    }

    [Fact]
    public void SemanticExtract_DoesNotDeduplicateSeparatedRepeatedWords()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Mode = SemanticTextMode.Deduplicated });

        Assert.Equal(2, semantic.DeduplicatedWords.Count);
        Assert.Equal("Echo Echo", semantic.GetText(TextOrder.Reading, SemanticTextMode.Deduplicated));
    }

    [Fact]
    public void SemanticExtract_DeduplicatedModeStillUsesSelectedGrouper()
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

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions
        {
            Mode = SemanticTextMode.Deduplicated,
            Grouper = new OneWordPerLineGrouper()
        });

        Assert.Single(semantic.DeduplicatedWords);
        Assert.Single(semantic.GetLines(TextOrder.Reading, SemanticTextMode.Deduplicated));
        Assert.Equal("Echo", semantic.GetText(TextOrder.Reading, SemanticTextMode.Deduplicated));
    }

    [Fact]
    public void SemanticExtract_HandlesTracemonkeyPageOneMoreReasonably()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var pdf = Path.Combine(tp, "pdfs", "pdfjs", "tracemonkey.pdf");
        using var doc = PdfDocument.Open(File.ReadAllBytes(pdf));
        var page = doc.Pages[0];

        var semantic = page.GetSemanticExtract(doc.Context, new SemanticExtractOptions { Order = TextOrder.Reading });
        var readingLines = semantic.GetLines(TextOrder.Reading, SemanticTextMode.Raw);
        var readingParagraphs = semantic.GetParagraphs(TextOrder.Reading, SemanticTextMode.Raw);

        Assert.True(readingLines[0].Words.Count > 1);
        Assert.NotEmpty(readingParagraphs);
        Assert.True(readingParagraphs.Count <= readingLines.Count);
        Assert.Contains(readingParagraphs, x => x.Text.Contains("Abstract", StringComparison.Ordinal));
    }

    private sealed class OneWordPerLineGrouper : ISemanticTextGrouper
    {
        public SemanticTextGroupingResult Group(SemanticTextGroupingInput input)
        {
            var lines = input.Words
                .Select(x => (IReadOnlyList<SemanticWord>)new[] { x })
                .ToList();
            var paragraphs = lines
                .Select(x => (IReadOnlyList<IReadOnlyList<SemanticWord>>)new[] { x })
                .ToList();
            return new SemanticTextGroupingResult
            {
                ContentLines = lines,
                ReadingLines = lines,
                ContentParagraphs = paragraphs,
                ReadingParagraphs = paragraphs
            };
        }
    }
}
