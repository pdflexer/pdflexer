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
    public class TextContentTests
    {
        [Fact]
        public void TextProperty_ReturnsCorrectString()
        {
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

            using var ms = new System.IO.MemoryStream();
            doc.SaveTo(ms);
            
            using var readDoc = PdfDocument.Open(ms.ToArray());           
            var readContents = readDoc.Pages[0].GetContentModel<double>();
            
            Assert.Single(readContents);
            var readTextContent = Assert.IsType<TextContent<double>>(readContents[0]);

            Assert.Equal("Round Trip", readTextContent.Text); 
        }

        [Fact]
        public void EnumerateCharacters_ReportsCorrectPositions_ForSegmentedText()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(100, 100));
            
            // Add a second segment with a manual shift
            content.Segments[0].Glyphs.Add(new GlyphOrShift<double>(-500)); // shift 500 units (5 points at 10pt font)
            content.Segments[0].Glyphs.AddRange(font.GetGlyphs("B").Select(g => new GlyphOrShift<double>(g.Glyph, 0, g.Bytes)));

            var chars = content.EnumerateCharacters().ToList();
            Assert.Equal(2, chars.Count);
            
            Assert.Equal('A', chars[0].Char);
            Assert.Equal(100, chars[0].XPos);
            
            Assert.Equal('B', chars[1].Char);
            // 'A' width in Helvetica is 667/1000. So at 10pt it's 6.67 units.
            // Plus 500/1000 * 10 = 5 units shift.
            // Total shift = 6.67 + 5 = 11.67.
            // Expected X = 100 + 11.67 = 111.67
            Assert.InRange(chars[1].XPos, 111.6, 111.7);
        }

        [Fact]
        public void EnumerateCharacters_ReportsCorrectPositions_ForNewLine()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(100, 100));
            
            // Add a newline segment
            var gs2 = content.Segments[0].GraphicsState with {
                TextLeading = 12.0
            };
            content.Segments.Add(new TextSegment<double> {
                GraphicsState = gs2,
                NewLine = true,
                Glyphs = font.GetGlyphs("B").Select(g => new GlyphOrShift<double>(g.Glyph, 0, g.Bytes)).ToList()
            });

            var chars = content.EnumerateCharacters().ToList();
            Assert.Equal(2, chars.Count);
            Assert.Equal('A', chars[0].Char);
            Assert.Equal(100, chars[0].XPos);
            Assert.Equal(100, chars[0].YPos);
            
            Assert.Equal('B', chars[1].Char);
            Assert.Equal(100, chars[1].XPos);
            Assert.Equal(88, chars[1].YPos); // 100 - 12
        }

        [Fact]
        public void EnumerateCharacters_ReportsCorrectPositions_ForRotatedText()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            // Rotate 90 degrees: [0 1 -1 0 100 100]
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(100, 100));
            content.LineMatrix = new GfxMatrix<double>(0, 1, -1, 0, 100, 100);

            var chars = content.EnumerateCharacters().ToList();
            Assert.Equal(1, chars.Count);
            Assert.Equal(100, chars[0].XPos);
            Assert.Equal(100, chars[0].YPos);
            
            // Add B. It should be at (100, 106.67)
            content.Segments[0].Glyphs.AddRange(font.GetGlyphs("B").Select(g => new GlyphOrShift<double>(g.Glyph, 0, g.Bytes)));
            chars = content.EnumerateCharacters().ToList();
            Assert.Equal(2, chars.Count);
            Assert.Equal(100, chars[1].XPos);
            Assert.InRange(chars[1].YPos, 106.6, 106.7);
        }

        [Fact]
        public void TextContent_GetBoundingBox_MultiSegment()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(10, 100));
            
            // Add a second segment via NewLine
            content.Segments.Add(new TextSegment<double> {
                GraphicsState = content.Segments[0].GraphicsState with {
                    TextLeading = 50.0
                },
                NewLine = true,
                Glyphs = font.GetGlyphs("B").Select(g => new GlyphOrShift<double>(g.Glyph, 0, g.Bytes)).ToList()
            });

            var bb = content.GetBoundingBox();
            Assert.True(bb.LLx >= 10.0); 
            Assert.True(bb.URy >= 100.0); // "A" at 100
            Assert.True(bb.LLy < 60.0); // "B" at 100 - 50 = 50.
        }

        [Fact]
        public void TextContent_Transform_UpdatesCharacterPositions()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(10, 10));
            
            content.Transform(new GfxMatrix<double>(1, 0, 0, 1, 5, 5));
            
            var chars = content.EnumerateCharacters().ToList();
            // Original position 10,10. Transform adds 5,5.
            Assert.Equal(15.0, chars[0].XPos);
            Assert.Equal(15.0, chars[0].YPos);
        }

        [Fact]
        public void TextContent_CopyArea_FiltersGlyphs()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A B", font, 10.0, new PdfPoint<double>(10, 10));
            
            // 'A' at 10. Space follows. 'B' follows.
            // Let's use a large gap to be sure.
            content.Segments[0].Glyphs.Insert(1, new GlyphOrShift<double>(-2000)); // 20pt gap
            
            var rect = new PdfRect<double>(0, 0, 20, 30); // Should cover 'A' but not 'B'
            
            var copied = (TextContent<double>)content.CopyArea(rect);
            Assert.NotNull(copied);
            Assert.Equal("A", copied.Text.Trim());
        }

        [Fact]
        public void TextContent_Split_FiltersGlyphs()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A B", font, 10.0, new PdfPoint<double>(10, 10));
            content.Segments[0].Glyphs.Insert(1, new GlyphOrShift<double>(-2000)); // 20pt gap
            
            var rect = new PdfRect<double>(0, 0, 20, 30); // Should cover 'A' but not 'B'
            
            var (inside, outside) = content.Split(rect);
            
            Assert.NotNull(inside);
            Assert.NotNull(outside);
            
            Assert.Equal("A", ((TextContent<double>)inside).Text.Trim());
            Assert.Equal("B", ((TextContent<double>)outside).Text.Trim());
        }

        [Fact]
        public void TextContent_EnumerateCharacters_ExpandsMultiCharGlyphs()
        {
            // Manually create a multi-char glyph
            var glyph = new Glyph {
                MultiChar = "lig",
                w0 = 1000
            };
            
            var segment = new TextSegment<double> {
                GraphicsState = new GfxState<double> { 
                    FontSize = 10.0,
                    Text = new TxtState<double> {
                        TextLineMatrix = new GfxMatrix<double>(1, 0, 0, 1, 10, 10),
                        TextMatrix = new GfxMatrix<double>(1, 0, 0, 1, 10, 10)
                    }
                },
                Glyphs = new List<GlyphOrShift<double>> { new GlyphOrShift<double>(glyph, 0, 1) }
            };
            
            var content = new TextContent<double> {
                LineMatrix = new GfxMatrix<double>(1, 0, 0, 1, 10, 10),
                Segments = new List<TextSegment<double>> { segment }
            };
            
            var chars = content.EnumerateCharacters().ToList();
            // Should be 3 chars: 'l', 'i', 'g'
            Assert.Equal(3, chars.Count);
            Assert.Equal('l', chars[0].Char);
            Assert.Equal('i', chars[1].Char);
            Assert.Equal('g', chars[2].Char);
        }
        [Fact]
        public void TextContent_GetGlyphBoundingBoxes_TracksAllGlyphs()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("AB", font, 10.0, new PdfPoint<double>(10, 10));
            
            var bboxes = content.GetGlyphBoundingBoxes().ToList();
            Assert.Equal(2, bboxes.Count);
            
            Assert.InRange(bboxes[0].LLx, 10.0, 10.2);
            Assert.InRange(bboxes[0].LLy, 9.9, 10.1);
            Assert.True(bboxes[0].URx > bboxes[0].LLx);
            
            Assert.True(bboxes[1].LLx > bboxes[0].LLx);
        }

        [Fact]
        public void TextContent_TransformInitial_UpdatesCharacterPositions()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(10, 10));
            
            // TransformInitial adds 5,5 to the baseline matrix.
            content.TransformInitial(new GfxMatrix<double>(1, 0, 0, 1, 5, 5));
            
            var chars = content.EnumerateCharacters().ToList();
            Assert.Equal(15.0, chars[0].XPos);
            Assert.Equal(15.0, chars[0].YPos);
        }

        [Fact]
        public void TextContent_ClipExcept_UpdatesAllSegments()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(10, 10));
            
            // Add a second segment
            content.Segments.Add(new TextSegment<double> {
                GraphicsState = content.Segments[0].GraphicsState,
                Glyphs = font.GetGlyphs("B").Select(g => new GlyphOrShift<double>(g.Glyph, 0, g.Bytes)).ToList()
            });

            var rect = new PdfRect<double>(0, 0, 100, 100);
            content.ClipExcept(rect);
            
            foreach (var segment in content.Segments)
            {
                Assert.NotNull(segment.GraphicsState.Clipping);
                Assert.Single(segment.GraphicsState.Clipping);
            }
        }

        [Fact]
        public void TextContent_ClipFrom_AppendsClippingToAllSegments()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            var content = TextContent<double>.Create("A", font, 10.0, new PdfPoint<double>(10, 10));
            
            var otherGs = new GfxState<double>();
            var rect = new PdfRect<double>(5, 5, 20, 20);
            otherGs = otherGs.ClipExcept(rect);
            
            content.ClipFrom(otherGs);
            
            foreach (var segment in content.Segments)
            {
                Assert.NotNull(segment.GraphicsState.Clipping);
                Assert.Single(segment.GraphicsState.Clipping);
            }
        }

        [Fact]
        public void TextContent_Split_PreservesSpacingAcrossRemovedGlyphs()
        {
            IWritableFont font = Standard14Font.GetHelvetica();
            // "A B C" with spaces
            var content = TextContent<double>.Create("A B C", font, 10.0, new PdfPoint<double>(10, 10));
            
            // 'A' at 10.
            // space at ~16.67.
            // 'B' at ~19.45. (space width in Helvetica is 278)
            // space at ~26.12.
            // 'C' at ~28.9.
            
            // Rect to include 'A' and 'C' but NOT 'B' is tricky if it's one rect.
            // Split uses a single rect. So let's split 'A' from 'B C'.
            var rect = new PdfRect<double>(0, 0, 18, 30); // Should cover 'A' and the first space maybe?
            
            var (inside, outside) = content.Split(rect);
            
            var insideText = (TextContent<double>)inside;
            var outsideText = (TextContent<double>)outside;
            
            Assert.Equal("A", insideText.Text.Trim());
            Assert.Equal("B C", outsideText.Text.Trim());
            
            // Check that 'B C' preserved its relative position.
            // In 'outsideText', the first glyph should be 'B' but it should have a preceding shift
            // representing the 'A' and the space that were removed.
            
            var chars = outsideText.EnumerateCharacters().ToList();
            Assert.Equal('B', chars[0].Char);
            Assert.InRange(chars[0].XPos, 19.4, 19.5);
        }
    }
}
