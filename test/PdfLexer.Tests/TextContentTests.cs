using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using PdfLexer.Content.Model;
using PdfLexer.DOM;
using PdfLexer.Fonts;
using PdfLexer.Writing;

namespace PdfLexer.Tests
{
    public class TextContentTests
    {
        [Fact]
        public void TextProperty_ReturnsCorrectString()
        {
            // Setup a mock TextContent with segments
            // Since we can't easily mock Glyph/TextSegment efficiently without helpers, 
            // let's use the new Create method to help us!
            
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("Hello World", font, 12.0);
            
            Assert.Equal("Hello World", content.Text);
        }
        
        [Fact]
        public void Create_GeneratesCorrectStructure()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("Test", font, 10.0);
            
            Assert.Single(content.Segments);
            Assert.Equal(10, content.GraphicsState.FontSize);
            Assert.Equal("Test", content.Text);
            
            // Check glyphs
            var segment = content.Segments[0];
            Assert.True(segment.Glyphs.Count >= 4);
            // Assert.Equal("T", segment.Glyphs[0].Glyph.Char.ToString()); // indices shift with kerns
        }

        [Fact]
        public void Create_WritesAndReadsBack()
        {
            using var doc = PdfDocument.Create();
            var page = new PdfPage(new PdfDictionary());
            doc.Pages.Add(page);
            
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("Round Trip", font, 14.0);
            
            using (var writer = page.GetWriter<double>())
            {
                writer.AddContent(content);
            }

            var txt = page.DumpDecodedContents();

            using var ms = new System.IO.MemoryStream();
            doc.SaveTo(ms);
            
            using var readDoc = PdfDocument.Open(ms.ToArray());           
            var readContents = readDoc.Pages[0].GetContentModel<double>();
            
            Assert.Single(readContents);
            var readTextContent = Assert.IsType<TextContent<double>>(readContents[0]);

            Assert.Equal("Round Trip", readTextContent.Text); 
        }
    }
}
