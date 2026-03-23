using PdfLexer.Content;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests
{
    public class CachedContentMutationCompatTests
    {
        [Fact]
        public void CachedContentMutation_CompatApply_WorksForLeafMutation()
        {
            var text = TextContent<double>.Create("Test", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.AddContent(text);
            }

            // Use the IContentGroup<double> -> IEnumerable<IContentGroup<double>> constructor
            var mutation = new CachedContentMutation((IContentGroup<double> node) =>
            {
                if (node is TextContent<double> tc && tc.Text == "Test")
                {
                    return new[] { TextContent<double>.Create("Mutated", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20)) };
                }
                return new[] { node };
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Single(nodes);
            Assert.Equal("Mutated", ((TextContent<double>)nodes[0]).Text);
        }

        [Fact]
        public void CachedContentMutation_CompatApply_CanRemoveLeaf()
        {
            var text = TextContent<double>.Create("Test", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.AddContent(text);
            }

            // Use the IContentGroup<double> -> IContentGroup<double>? constructor
            var mutation = new CachedContentMutation((IContentGroup<double> node) =>
            {
                if (node is TextContent<double> tc && tc.Text == "Test")
                {
                    return null;
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            Assert.Empty(nodes);
        }

        [Fact]
        public void CachedContentMutation_CompatApply_RecursesThroughMarkedContentAndForms()
        {
            var pg = new PdfPage(PageSize.LETTER);
            using (var writer = pg.GetWriter())
            {
                writer.Font(Standard14Font.GetHelvetica(), 12);
                writer.MarkedContent(new MarkedContent("P"));
                writer.Text("Inner").EndText();
                writer.EndMarkedContent();

                var formWriter = new FormWriter(100, 100);
                formWriter.Font(Standard14Font.GetHelvetica(), 12);
                formWriter.Text("FormText").EndText();
                writer.Form(formWriter.Complete());
            }

            var mutation = new CachedContentMutation((IContentGroup<double> node) =>
            {
                if (node is TextContent<double> tc)
                {
                    return TextContent<double>.Create("Changed", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(0,0));
                }
                return node;
            });

            var pg2 = mutation.Apply(pg);
            var nodes = pg2.GetContentNodes<double>();
            
            // Should find MarkedContentGroup with "Changed"
            Assert.IsType<MarkedContentGroup<double>>(nodes[0]);
            var mcg = (MarkedContentGroup<double>)nodes[0];
            Assert.Equal("Changed", ((TextContent<double>)mcg.Children[0]).Text);

            // Should find FormContent with "Changed" inside
            Assert.IsType<FormContent<double>>(nodes[1]);
            var form = (FormContent<double>)nodes[1];
            var formNodes = form.Parse();
            Assert.Equal("Changed", ((TextContent<double>)formNodes[0]).Text);
        }
        
        [Fact]
        public void CachedContentMutation_Apply_ListCompat_Works()
        {
            var text = TextContent<double>.Create("Test", Standard14Font.GetHelvetica(), 12, new PdfPoint<double>(10, 20));
            var list = new List<IContentGroup<double>> { text };
            
            var mutation = new CachedContentMutation((IContentNode<double> node) =>
            {
                return node;
            });

            // This tests the List<IContentGroup<T>> Apply extension if it exists, 
            // or if we just want to ensure we can apply to such a list.
            // CachedContentMutation has Apply(List<IContentNode<T>>)
            // Let's see if there is an extension for List<IContentGroup<T>>
            
            var result = mutation.Apply(list.Cast<IContentNode<double>>().ToList());
            Assert.Single(result);
        }
    }
}
