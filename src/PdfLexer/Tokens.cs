namespace PdfLexer
{
    public enum PdfTokenType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        StringObj,
        NameObj,
        ArrayStart,
        DictionaryStart,
        IndirectRef,
        ArrayEnd,
        DictionaryEnd,
        Trailer,
        StartStream,
        EndStream,
        StartObj,
        EndObj,
        Xref,
        WildCard // dummy
    }

    public enum PdfObjectType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        StringObj,
        NameObj,
        ArrayObj,
        DictionaryObj,
        StreamObj,
        IndirectObj,
        IndirectRefObj
    }
}