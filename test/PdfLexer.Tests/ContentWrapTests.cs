using System;
using System.Collections.Generic;
using System.Linq;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using Xunit;

namespace PdfLexer.Tests;

public class ContentWrapTests
{
    [Fact]
    public void Wrap_CanInsertWrapperInsideSingleMarkedContentGroup()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.MarkedContent(new MarkedContent("Span"));
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Alpha").EndText();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Beta").EndText();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Gamma").EndText();
            writer.EndMarkedContent();
        }

        var nodes = page.GetContentNodes<double>();
        var outer = Assert.IsType<MarkedContentGroup<double>>(Assert.Single(nodes));
        var beta = Assert.IsType<TextContent<double>>(outer.Children[1]);

        var wrapper = outer.Wrap(new[] { beta }, new MarkedContent("P"));

        Assert.Equal(3, outer.Children.Count);
        Assert.IsType<TextContent<double>>(outer.Children[0]);
        Assert.Same(wrapper, outer.Children[1]);
        Assert.IsType<TextContent<double>>(outer.Children[2]);
        Assert.Equal("Beta", Assert.IsType<TextContent<double>>(Assert.Single(wrapper.Children)).Text);
    }

    [Fact]
    public void Wrap_SplitsExistingMarkedContentBoundariesToKeepSelectionContiguous()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.MarkedContent(new MarkedContent("Outer"));
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Alpha").EndText();
            writer.MarkedContent(new MarkedContent("Inner"));
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Beta").EndText();
            writer.EndMarkedContent();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Gamma").EndText();
            writer.EndMarkedContent();
        }

        var nodes = page.GetContentNodes<double>();
        var outer = Assert.IsType<MarkedContentGroup<double>>(Assert.Single(nodes));
        var inner = Assert.IsType<MarkedContentGroup<double>>(outer.Children[1]);
        var beta = Assert.IsType<TextContent<double>>(Assert.Single(inner.Children));
        var gamma = Assert.IsType<TextContent<double>>(outer.Children[2]);

        var wrapper = outer.Wrap(new IContentItem<double>[] { beta, gamma }, new MarkedContent("Sect"));

        Assert.Equal(2, outer.Children.Count);
        Assert.Equal("Alpha", Assert.IsType<TextContent<double>>(outer.Children[0]).Text);
        Assert.Same(wrapper, outer.Children[1]);
        Assert.Equal(new[] { "Beta", "Gamma" }, wrapper.Children.OfType<TextContent<double>>().Select(x => x.Text).ToArray());
    }

    [Fact]
    public void Wrap_RejectsLeavesFromAnotherPageTree()
    {
        using var doc = PdfDocument.Create();
        var firstPage = doc.AddPage(PageSize.LETTER);
        var secondPage = doc.AddPage(PageSize.LETTER);

        using (var writer = firstPage.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("First").EndText();
        }

        using (var writer = secondPage.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Second").EndText();
        }

        var firstNodes = firstPage.GetContentNodes<double>();
        var secondNodes = secondPage.GetContentNodes<double>();
        var firstLeaf = Assert.IsType<TextContent<double>>(Assert.Single(firstNodes));
        var secondLeaf = Assert.IsType<TextContent<double>>(Assert.Single(secondNodes));

        var error = Assert.Throws<InvalidOperationException>(() =>
            firstNodes.Wrap(new IContentItem<double>[] { firstLeaf, secondLeaf }, new MarkedContent("P")));

        Assert.Contains("belong", error.Message);
    }

    [Fact]
    public void Wrap_WriterBracketsOnlyTheSelectedOperators()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage(PageSize.LETTER);

        using (var writer = page.GetWriter())
        {
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Alpha").EndText();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Beta").EndText();
            writer.Font(Standard14Font.GetHelvetica(), 12).Text("Gamma").EndText();
        }

        var content = page.GetContentModel<double>();
        var beta = Assert.IsType<TextContent<double>>(content[1]);
        var wrapper = content.Wrap(
            new[] { beta },
            new MarkedContent("P")
            {
                InlineProps = new PdfDictionary
                {
                    [PdfName.MCID] = new PdfIntNumber(7)
                }
            });

        Assert.Equal("Beta", Assert.IsType<TextContent<double>>(Assert.Single(wrapper.Children)).Text);

        var rewritten = doc.AddPage(PageSize.LETTER);
        using (var writer = rewritten.GetWriter())
        {
            writer.AddContent(content);
        }

        var text = rewritten.DumpDecodedContents();
        var bdcIndex = text.IndexOf("/P <</MCID 7>> BDC", System.StringComparison.Ordinal);
        var reparsed = rewritten.GetContentNodes<double>();

        Assert.Equal(3, reparsed.Count);
        Assert.Equal("Alpha", Assert.IsType<TextContent<double>>(reparsed[0]).Text);
        var reparsedWrapper = Assert.IsType<MarkedContentGroup<double>>(reparsed[1]);
        Assert.Equal("P", reparsedWrapper.Tag.Name.Value);
        Assert.Equal("Beta", Assert.IsType<TextContent<double>>(Assert.Single(reparsedWrapper.Children)).Text);
        Assert.Equal("Gamma", Assert.IsType<TextContent<double>>(reparsed[2]).Text);
        Assert.True(bdcIndex >= 0);
        Assert.Equal(1, CountOccurrences(text, "/P <</MCID 7>> BDC"));
        Assert.Equal(1, CountOccurrences(text, "EMC"));
        Assert.Equal(3, CountOccurrences(text, "Tj"));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
