namespace PdfLexer
{
    public enum PdfTokenType
    {
        NullObj, // Obj tokens get lexed as the entire token
        BooleanObj,
        NumericObj,
        DecimalObj,
        NameObj, 
        StringStart, // Obj start token get lexed as just the start char(s)
        ArrayStart,
        DictionaryStart,
        ArrayEnd, // tokens below here 
        DictionaryEnd,
        IndirectRef, 
        Trailer,
        StartStream,
        EndStream,
        EndString,
        StartObj,
        EndObj,
        Xref,
        StartXref,
        WildCard, // dummy
        Unknown
    }

    public enum PdfObjectType
    {
        NullObj,
        BooleanObj,
        NumericObj,
        DecimalObj,
        NameObj,
        StringObj,
        ArrayObj,
        DictionaryObj,
        StreamObj,
        IndirectRefObj
    }
}