namespace PdfLexer.Fonts;

public enum UnknownCharHandling
{
    Error,
    Skip,
    WriteNull
}

/// <summary>
/// A font file that has been loaded and can be used to create a writable font
/// for adding text content to a pdf page.
/// </summary>
public interface IPdfEmbeddableFont
{
    /// <summary>
    /// Creates a writable font using the default encoding of the font file.
    /// </summary>
    /// <param name="handling"></param>
    /// <returns></returns>
    IWritableFont GetDefaultEncodedFont(UnknownCharHandling handling = default);

    /// <summary>
    /// Creates a writable font using indentity encoding of the font file.
    /// </summary>
    /// <param name="handling"></param>
    /// <returns></returns>
    IWritableFont GetType0EncodedFont(UnknownCharHandling handling = default);
    /// <summary>
    /// Creates a writable font using a custom encoding that is based on the provided characters.
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="handling"></param>
    /// <returns></returns>
    IWritableFont GetCustomEncodedFont(IEnumerable<char> characters, UnknownCharHandling handling = default);
}