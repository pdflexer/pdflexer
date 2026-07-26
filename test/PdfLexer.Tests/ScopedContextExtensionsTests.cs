using System;
using System.Linq;
using PdfLexer.DOM;
using PdfLexer.Writing;
using Xunit;

namespace PdfLexer.Tests;

public class ScopedContextExtensionsTests
{
    [Fact]
    public void Structure_scopes_create_exact_hierarchy_and_return_children()
    {
        var builder = new StructuralBuilder();
        IStructureContext paragraph = null!;
        IStructureContext custom = null!;

        var section = builder.AddSection("Results", section =>
        {
            paragraph = section.AddParagraph("Summary", p =>
                p.AddSpan("Résumé", span => span.ActualText("Summary"), lang: "fr-CA"));
            section.AddList(list =>
            {
                list.AddListItem(item =>
                {
                    item.AddLabel("1.", label => { });
                    item.AddListBody("First", body => { });
                });
            });
            section.AddTable("Scores", table =>
            {
                table.AddTableHead(head => head.AddRow(row =>
                {
                    row.AddHeaderCell(cell => cell.TableScope(StructureScope.Column), colSpan: 2);
                }));
                table.AddTableBody(body => body.AddRow(row =>
                {
                    row.AddDataCell(cell => cell.TableHeaders("score"), rowSpan: 2);
                }));
                table.AddTableFoot(foot => foot.AddRow(row => row.AddDataCell(cell => { })));
            });
            custom = section.AddElement(
                "Custom",
                element => element.ElementId("custom-id"),
                title: "Custom title",
                language: "de-DE");
        });

        Assert.Same(section.GetNode(), builder.GetRoot().Children.Single());
        Assert.Same(paragraph.GetNode(), section.GetNode().Children[0]);
        Assert.Equal(new[] { "P", "L", "Table", "Custom" }, section.GetNode().Children.Select(x => x.Type));
        Assert.Equal(new[] { "Lbl", "LBody" }, section.GetNode().Children[1].Children[0].Children.Select(x => x.Type));
        Assert.Equal("fr-CA", paragraph.GetNode().Children.Single().Language);
        Assert.Equal("de-DE", custom.GetNode().Language);
        Assert.Equal("custom-id", custom.GetNode().ID);

        var tableNode = section.GetNode().Children[2];
        Assert.Equal(new[] { "THead", "TBody", "TFoot" }, tableNode.Children.Select(x => x.Type));
        Assert.Equal(2, tableNode.Children[0].Children[0].Children[0].Attributes[0].Get<PdfIntNumber>(PdfName.ColSpan)!.Value);
        Assert.Equal(2, tableNode.Children[1].Children[0].Children[0].Attributes[0].Get<PdfIntNumber>(PdfName.RowSpan)!.Value);
    }

    [Fact]
    public void Configure_returns_original_context_and_supports_content()
    {
        using var doc = PdfDocument.Create();
        var page = doc.AddPage();
        using var writer = new PageWriter<double>(page);
        var paragraph = doc.Structure.AddParagraph("Summary");

        var configured = paragraph.Configure(p =>
        {
            p.ActualText("Accessible summary");
            p.WriteContent(writer, _ => { });
        });

        Assert.Same(paragraph, configured);
        Assert.Equal("Accessible summary", paragraph.GetNode().ActualText);
        Assert.Single(paragraph.GetNode().ContentItems);
    }

    [Fact]
    public void Null_callbacks_are_rejected_before_child_creation()
    {
        var builder = new StructuralBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AddSection("Never added", null!));
        Assert.Throws<ArgumentNullException>(() => builder.Configure(null!));
        Assert.Empty(builder.GetRoot().Children);
    }

    [Fact]
    public void Callback_exceptions_propagate_without_rolling_back_child()
    {
        var builder = new StructuralBuilder();
        var error = new InvalidOperationException("stop");

        var thrown = Assert.Throws<InvalidOperationException>(() =>
            builder.AddSection("Attached", _ => throw error));

        Assert.Same(error, thrown);
        Assert.Single(builder.GetRoot().Children);
        Assert.Equal("Attached", builder.GetRoot().Children[0].Title);
    }

    [Fact]
    public void Legacy_back_and_scoped_construction_produce_same_tree()
    {
        var legacy = new StructuralBuilder();
        legacy.AddSection("S")
            .AddList()
            .AddListItem()
            .AddLabel("1.").Back()
            .AddListBody("Body");

        var scoped = new StructuralBuilder();
        scoped.AddSection("S", section =>
            section.AddList(list =>
                list.AddListItem(item =>
                {
                    item.AddLabel("1.", _ => { });
                    item.AddListBody("Body", _ => { });
                })));

        Assert.Equal(Describe(legacy.GetRoot()), Describe(scoped.GetRoot()));
    }

    private static string Describe(StructureNode node) =>
        $"{node.Type}:{node.Title}[{string.Join(",", node.Children.Select(Describe))}]";
}

public class ScopedOutlineContextExtensionsTests
{
    [Fact]
    public void Outline_scopes_preserve_nesting_options_bookmarks_and_returns()
    {
        var builder = new PdfLexer.DOM.OutlineBuilder();
        var color = new[] { 0.1, 0.2, 0.3 };
        IOutlineContext nested = null!;

        var chapter = builder.AddSection("Chapter", chapter =>
        {
            chapter.AddBookmark("Overview");
            nested = chapter.AddSection("Details", details =>
                details.AddBookmark("Result"), isOpen: false, color: color, style: 2);
        });

        Assert.Same(chapter, chapter.Configure(_ => { }));
        Assert.Equal(2, chapter.FindNode("Chapter")!.Children.Count);
        var detailsNode = chapter.FindNode("Details")!;
        Assert.False(detailsNode.IsOpen);
        Assert.Same(color, detailsNode.Color);
        Assert.Equal(2, detailsNode.Style);
        Assert.Single(detailsNode.Children);
        Assert.Equal("Result", detailsNode.Children[0].Title);
        Assert.Same(nested, nested.Configure(_ => { }));
        Assert.Equal(new[] { "Overview", "Result" }, builder.EnumerateLeaves().Select(x => x.Title));
    }

    [Fact]
    public void Outline_callback_errors_follow_structure_scope_semantics()
    {
        var builder = new PdfLexer.DOM.OutlineBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.AddSection("Missing", null!));
        Assert.Empty(builder.GetRoot().Children);

        Assert.Throws<InvalidOperationException>(() =>
            builder.AddSection("Attached", _ => throw new InvalidOperationException()));
        Assert.Single(builder.GetRoot().Children);
    }
}
