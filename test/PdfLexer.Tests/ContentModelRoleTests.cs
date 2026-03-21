using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;
using PdfLexer.Content;

namespace PdfLexer.Tests
{
    public class ContentModelRoleTests
    {
        private sealed class UnsupportedNode : IContentNode<double>
        {
            public ContentType Type => ContentType.MarkedContent;

            public PdfRect<double> GetBoundingBox()
            {
                return new PdfRect<double> { LLx = 0, LLy = 0, URx = 1, URy = 1 };
            }
        }

        [Fact]
        public void FormContent_ThrowsOnUnsupportedOperations()
        {
            var form = new FormContent<double> {
                GraphicsState = new GfxState<double>(),
                Stream = new PdfStream()
            };

            var rect = new PdfRect<double> { LLx = 0, LLy = 0, URx = 0, URy = 0 };

            Assert.Throws<NotSupportedException>(() => ((IContentItem<double>)form).CopyArea(rect));
            Assert.Throws<NotSupportedException>(() => ((IContentItem<double>)form).Split(rect));
            Assert.Throws<NotSupportedException>(() => form.ClipExcept(rect));
            Assert.Throws<NotSupportedException>(() => form.ClipFrom(new GfxState<double>()));
        }

        [Fact]
        public void MarkedContentGroup_ExposesChildrenCorrectly()
        {
            var tag = new MarkedContent(new PdfName("Test"));
            var group = new MarkedContentGroup<double>(tag)
            {
                GraphicsState = new GfxState<double>()
            };
            
            IWritableFont font = Standard14Font.GetHelvetica();
            var text = TextContent<double>.Create("Hello", font, 12.0);
            
            group.Children.Add(text);
            
            Assert.Single(group.Children);
            Assert.Equal(text, group.Children[0]);
            
            var container = (IContentContainer<double>)group;
            Assert.Single(container.Children);
            Assert.Equal(text, container.Children.First());
        }

        [Fact]
        public void ContentModelWriter_HandlesMixedNodes()
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage();
            
            var tag = new MarkedContent(new PdfName("Test"));
            var group = new MarkedContentGroup<double>(tag)
            {
                GraphicsState = new GfxState<double>()
            };
            
            IWritableFont font = Standard14Font.GetHelvetica();
            var text = TextContent<double>.Create("Hello", font, 12.0);
            group.Children.Add(text);
            
            var content = new List<IContentNode<double>> { group, text };
            
            using (var writer = page.GetWriter<double>())
            {
                writer.AddContent(content);
            }
            
            var decoded = page.DumpDecodedContents();
            Assert.Contains("/Test BMC", decoded);
            Assert.Contains("EMC", decoded);
            Assert.Contains("(Hello) Tj", decoded);
        }

        [Fact]
        public void ContentModelWriter_ThrowsForNonWritableNode()
        {
            using var doc = PdfDocument.Create();
            var page = doc.AddPage();

            using var writer = page.GetWriter<double>();
            Assert.Throws<InvalidOperationException>(() => writer.AddContent(new UnsupportedNode()));
        }
    }
}
