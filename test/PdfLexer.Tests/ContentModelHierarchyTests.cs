using System.IO;
using System.Linq;
using Xunit;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Writing;

namespace PdfLexer.Tests
{
    public class ContentModelHierarchyTests
    {
        [Fact]
        public void VerifyHierarchicalParsing()
        {
            // 1. Create a PDF in memory with known marked content structure
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            {
                using var ctx = page.GetWriter();
                // Structure:
                // P (Tag)
                //   Span (Tag)
                //     Hello (Text)
                //   World (Text) 

                ctx.MarkedContent(new MarkedContent("P"));
                ctx.MarkedContent(new MarkedContent("Span"));
                ctx.Font(Base14.Helvetica, 12.0);
                ctx.Text("Hello").EndText();
                ctx.EndMarkedContent(); // Closes Span
                ctx.Text("World").EndText();
                ctx.EndMarkedContent(); // Closes P
            }

            using var ms = new MemoryStream();
            doc.SaveTo(ms);
            var bytes = ms.ToArray();

            // 2. Parse it back
            using var readDoc = PdfDocument.Open(bytes);
            var contents = readDoc.Pages[0].GetContentModel();

            // 3. Verify Structure
            Assert.Single(contents); // Should be 1 group (the outer P)
            Assert.IsType<MarkedContentGroup<double>>(contents[0]);
            var pGroup = (MarkedContentGroup<double>)contents[0];
            Assert.Equal("P", pGroup.Tag.Name.Value);

            Assert.Equal(2, pGroup.Children.Count); // Span, World(Text)

            Assert.IsType<MarkedContentGroup<double>>(pGroup.Children[0]);
            var spanGroup = (MarkedContentGroup<double>)pGroup.Children[0];
            Assert.Equal("Span", spanGroup.Tag.Name.Value);

            Assert.IsType<TextContent<double>>(spanGroup.Children[0]);
            // (Optional: verify text content "Hello")

            Assert.IsType<TextContent<double>>(pGroup.Children[1]);
            // (Optional: verify text content "World")

            // 4. Verify Flatten()
            var flat = contents.Flatten().ToList();
            Assert.Equal(2, flat.Count); // Hello, World
            Assert.IsType<TextContent<double>>(flat[0]);
            Assert.IsType<TextContent<double>>(flat[1]);

            // 5. Round Trip Write
            using var doc2 = PdfDocument.Create();
            var page2 = new PdfPage(new PdfDictionary());
            doc2.Pages.Add(page2);
            {
                using var p2w = page2.GetWriter();
                p2w.AddContent(contents);
            }

            using var ms2 = new MemoryStream();
            doc2.SaveTo(ms2);
            var bytes2 = ms2.ToArray();

            // 6. Re-Parse
            using var readDoc2 = PdfDocument.Open(bytes2);
            var contents2 = readDoc2.Pages[0].GetContentModel();

            Assert.Single(contents2);
            var pGroup2 = (MarkedContentGroup<double>)contents2[0];
            Assert.Equal("P", pGroup2.Tag.Name.Value);
            Assert.Equal(2, pGroup2.Children.Count);
        }

        [Fact]
        public void VerifyEmptyTags()
        {
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);

            {
                using var ctx = page.GetWriter();
                ctx.MarkedContent(new MarkedContent("P"));
                ctx.MarkedContent(new MarkedContent("Span")); // Empty span
                ctx.EndMarkedContent();
                ctx.EndMarkedContent();
            }

            using var ms = new MemoryStream();
            doc.SaveTo(ms);

            using var readDoc = PdfDocument.Open(ms.ToArray());
            var contents = readDoc.Pages[0].GetContentModel();

            Assert.Single(contents);
            var p = (MarkedContentGroup<double>)contents[0];
            Assert.Single(p.Children);
            var span = (MarkedContentGroup<double>)p.Children[0];
            Assert.Empty(span.Children); // Empty tag preserved!
        }
    }
}
