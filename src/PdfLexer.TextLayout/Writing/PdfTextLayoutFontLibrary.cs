using PdfLexer.Fonts;
using PdfLexer.TextLayout;
using PdfLexer.Writing;

namespace PdfLexer.Writing;

public sealed record PdfTextLayoutFontFace(
    TextFontFace LayoutFace,
    IWritableFont WritableFont);

public sealed class PdfTextLayoutFontLibrary
{
    private readonly Dictionary<string, PdfTextLayoutFontFace> _faces;
    private IWritableFont? _unorderedListMarkerFont;

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

        if (string.Equals(faceId, TextLayoutBuiltInFaces.UnorderedListMarkerFaceId, StringComparison.Ordinal))
        {
            font = _unorderedListMarkerFont ??= Standard14Font.GetHelvetica();
            return true;
        }

        font = null;
        return false;
    }
}
