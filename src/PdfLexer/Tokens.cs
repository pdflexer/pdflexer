namespace PdfLexer
{
    public enum PdfTokenType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        NameObj,
        StringObj,
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
        StartXref,
        WildCard // dummy
    }

    public enum PdfObjectType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        NameObj,
        StingObj,
        ArrayObj,
        DictionaryObj,
        StreamObj,
        IndirectObj,
        IndirectRefObj
    }
}