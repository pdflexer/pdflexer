using PdfLexer.DOM;

namespace PdfLexer;

public sealed partial class PdfDocument
{
    // Used by transactional authoring surfaces to snapshot without invoking the lazy getter.
    internal StructuralBuilder? ExistingStructure => _structure;
}
