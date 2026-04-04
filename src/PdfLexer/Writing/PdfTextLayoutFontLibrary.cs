using PdfLexer.Fonts;
using PdfLexer.TextLayout;

namespace PdfLexer.Writing;

public sealed record PdfTextLayoutFontFace(
    TextFontFace LayoutFace,
    IWritableFont WritableFont);

public sealed class PdfTextLayoutFontLibrary
{
    private readonly Dictionary<string, PdfTextLayoutFontFace> _faces;

    public PdfTextLayoutFontLibrary(IEnumerable<PdfTextLayoutFontFace> faces)
    {
        ArgumentNullException.ThrowIfNull(faces);
        _faces = faces.ToDictionary(x => x.LayoutFace.FaceId, StringComparer.Ordinal);
    }

    public TextFontLibrary CreateLayoutLibrary()
        => new TextFontLibrary(_faces.Values.Select(x => x.LayoutFace));

    public bool TryGetWritableFont(string faceId, out IWritableFont? font)
    {
        if (_faces.TryGetValue(faceId, out var face))
        {
            font = face.WritableFont;
            return true;
        }

        font = null;
        return false;
    }
}
