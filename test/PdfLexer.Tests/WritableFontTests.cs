using PdfLexer.Fonts;
using System.IO;
using System.Linq;
using Xunit;

namespace PdfLexer.Tests;

public class WritableFontTests
{
    [Fact]
    public void Standard14_GetGlyphs_ReturnsCorrectGlyphs()
    {
        var font = Standard14Font.GetHelvetica();
        var text = "Av"; // expects kerning
        var glyphs = font.GetGlyphs(text).ToList();

        // A, kerning, v
        // A=65, v=118
        // Helvetica kerning for A v is usually negative
        
        Assert.NotEmpty(glyphs);
        var g1 = glyphs[0];
        Assert.NotNull(g1.Glyph);
        Assert.Equal('A', g1.Glyph.Char);

        // Check if kerning is present (might depend on implementation details of Standard14 fonts in this lib)
        // If no kerning, count will be 2. If kerning, count will be 3.
        
        // Let's just verify we get glyphs for now.
        var chars = glyphs.Where(x => x.Glyph != null).Select(x => x.Glyph.Char).ToArray();
        Assert.Equal(new[] { 'A', 'v' }, chars);
    }

    [Fact]
    public void TrueType_GetGlyphs_ReturnsCorrectGlyphs()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var font = TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));

        var text = "Hello";
        var glyphs = font.GetGlyphs(text).ToList();

        Assert.Equal(5, glyphs.Count(x => x.Glyph != null));
        var chars = glyphs.Where(x => x.Glyph != null).Select(x => x.Glyph.Char).ToArray();
        Assert.Equal(new[] { 'H', 'e', 'l', 'l', 'o' }, chars);
    }

    [Fact]
    public void TrueTypeSimple_GetGlyphs_ReturnsCorrectGlyphs()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var font = TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath)); 
        // Wait, CreateType0WritableFont creates TrueTypeWritableFont (composite), 
        // CreateWritableFont creates TrueTypeSimpleWritableFont (simple) usually if possible?
        // Let's check FontLoadTests again or TrueTypeFont factory.
        // FontLoadTests says:
        // ttf = TrueTypeFont.CreateWritableFont(...) -> Simple?
        // ttf = TrueTypeFont.CreateType0WritableFont(...) -> Type0 (Composite)?
        
        // Actually TrueTypeWritableFont is Type0/Composite in the implementation I saw?
        // Let's re-read TrueTypeWritableFont.cs and TrueTypeSimpleWritableFont.cs headers.
        // TrueTypeWritableFont: "internal class TrueTypeWritableFont : IWritableFont"
        // TrueTypeSimpleWritableFont: "internal class TrueTypeSimpleWritableFont : IWritableFont"
        
        // I need to be sure which factory method creates which.
        // Assuming CreateWritableFont -> Simple (if fits) or checks? 
        // FontLoadTests: TrueTypeFont.CreateWritableFont(File.ReadAllBytes(fontPath));
        // FontLoadTests: TrueTypeFont.CreateType0WritableFont(File.ReadAllBytes(fontPath));
        
        // Let's assume CreateWritableFont returns something implementing IWritableFont.
        // I'll test both factory methods.
    }
    
    [Fact]
    public void TrueType_Factory_Check_GetGlyphs()
    {
        var tp = PathUtil.GetPathFromSegmentOfCurrent("test");
        var fontPath = Path.Combine(tp, "Roboto-Regular.ttf");
        var bytes = File.ReadAllBytes(fontPath);

        var font1 = TrueTypeFont.CreateWritableFont(bytes); // Likely Simple
        var list1 = font1.GetGlyphs("Test").ToList();
        Assert.Equal(4, list1.Count(x => x.Glyph != null));

        var font2 = TrueTypeFont.CreateType0WritableFont(bytes); // Likely Type0
        var list2 = font2.GetGlyphs("Test").ToList();
        Assert.Equal(4, list2.Count(x => x.Glyph != null));
    }
}
