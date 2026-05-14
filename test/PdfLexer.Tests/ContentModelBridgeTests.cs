using System.Collections.Generic;
using System.Linq;
using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class ContentModelBridgeTests
{
    [Fact]
    public void ParsedContentItems_ExposeIdentityAndProvenance()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        var formWriter = new FormWriter(40, 40);
        formWriter.Font(Standard14Font.GetHelvetica(), 12).Text("InForm").EndText();
        var form = formWriter.Complete();

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello").EndText();
            writer.Rect(10, 10, 20, 20).Fill();
            writer.MarkedContent(new MarkedContent("P"));
            writer.Text("Inside").EndText();
            writer.EndMarkedContent();
            writer.Form(form);
        }

        var nodes = page.GetContentNodes<double>();

        var text = Assert.IsType<TextContent<double>>(nodes[0]);
        var path = Assert.IsType<PathSequence<double>>(nodes[1]);
        var group = Assert.IsType<MarkedContentGroup<double>>(nodes[2]);
        var formContent = Assert.IsType<FormContent<double>>(nodes[3]);

        Assert.NotNull(text.ParsedItemId);
        Assert.NotNull(text.SourceReference);
        Assert.NotNull(path.ParsedItemId);
        Assert.NotNull(path.SourceReference);
        Assert.NotNull(group.ParsedItemId);
        Assert.NotNull(group.SourceReference);
        Assert.NotNull(formContent.ParsedItemId);
        Assert.NotNull(formContent.SourceReference);
    }

    [Fact]
    public void Bridge_ResolvesStructuredWordBackToTextContent()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Hello World").EndText();
        }

        var nodes = page.GetContentNodes<double>();
        var structured = page.GetStructuredText(doc.Context);
        var word = structured.RawWords.Single(x => x.Text == "Hello");

        var resolved = ContentModelBridge.FindItems(nodes, word);

        var item = Assert.Single(resolved);
        var text = Assert.IsType<TextContent<double>>(item);
        Assert.Equal("Hello World", text.Text);
    }

    [Fact]
    public void Bridge_RoundTrip_PreservesLogicalOperatorPositions()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Alpha").EndText();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Beta").EndText();
            writer.Rect(10, 10, 20, 20).Fill();
        }

        var beforeNodes = page.GetContentNodes<double>();
        var beforeStructured = page.GetStructuredText(doc.Context);
        var beforeResolvedTexts = beforeStructured.RawWords
            .Select(x => ((TextContent<double>)ContentModelBridge.FindItems(beforeNodes, x).Single()).Text)
            .ToList();

        var rewritten = doc.AddPage(PageSize.LETTER);
        using (var writer = rewritten.GetWriter())
        {
            writer.AddContent(beforeNodes);
        }

        var afterNodes = rewritten.GetContentNodes<double>();
        var afterStructured = rewritten.GetStructuredText(doc.Context);
        var afterResolvedTexts = afterStructured.RawWords
            .Select(x => ((TextContent<double>)ContentModelBridge.FindItems(afterNodes, x).Single()).Text)
            .ToList();
        var afterWordPositions = afterStructured.RawWords
            .Select(x => ContentModelBridge.FindItems(afterNodes, x).Single().ParsedItemId!.Value.StartOperatorIndex)
            .ToList();

        Assert.Equal(beforeStructured.RawWords.Select(x => x.Text), afterStructured.RawWords.Select(x => x.Text));
        Assert.Equal(beforeResolvedTexts, afterResolvedTexts);
        Assert.True(afterWordPositions.SequenceEqual(afterWordPositions.OrderBy(x => x)));
    }

    [Fact]
    public void ContentModel_NoOpParseWriteCycle_PreservesStructure()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        var formWriter = new FormWriter(40, 40);
        formWriter.Rect(0, 0, 10, 10).Fill();
        var form = formWriter.Complete();

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Text").EndText();
            writer.Rect(5, 5, 10, 10).Fill();
            writer.MarkedContent(new MarkedContent("P"));
            writer.Text("Inside").EndText();
            writer.EndMarkedContent();
            writer.Form(form);
        }

        var parsed = page.GetContentNodes<double>();

        var rewritten = doc.AddPage(PageSize.LETTER);
        using (var writer = rewritten.GetWriter())
        {
            writer.AddContent(parsed);
        }

        var reparsed = rewritten.GetContentNodes<double>();

        Assert.Equal(Describe(parsed), Describe(reparsed));
    }

    private static List<string> Describe(IEnumerable<IContentNode<double>> nodes)
    {
        var results = new List<string>();
        foreach (var node in nodes)
        {
            switch (node)
            {
                case TextContent<double> text:
                    results.Add($"Text:{text.Text}");
                    break;
                case PathSequence<double> path:
                    results.Add($"Path:{path.Paths.Count}:{path.Closing?.GetType().Name}");
                    break;
                case MarkedContentGroup<double> group:
                    results.Add($"MCG:{group.Tag.Name.Value}[{string.Join(",", Describe(group.Children))}]");
                    break;
                case FormContent<double>:
                    results.Add("Form");
                    break;
                default:
                    results.Add(node.GetType().Name);
                    break;
            }
        }

        return results;
    }
}
