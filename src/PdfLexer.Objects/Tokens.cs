namespace PdfLexer.Objects
{
    public enum PdfTokenType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        StringObj,
        NameObj,
        ArrayStart,
        IndirectRef,
        DictionaryStart,
        ArrayEnd,
        DictionaryEnd,
        Trailer
    }
}