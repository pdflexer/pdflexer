namespace PdfLexer.Filters;

public interface IDecoder
{
    /// <summary>
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filterParams"></param>
    /// <returns></returns>
    Stream Decode(Stream stream, PdfDictionary? filterParams);

    Stream Decode(Stream stream, PdfDictionary? filterParams, Action<string> errInfo) =>
        Decode(stream, filterParams);
}

public interface IEncoder
{
    /// <summary>
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filterParams"></param>
    /// <returns></returns>
    (Stream Data, PdfName Filter, PdfDictionary? Params) Encode(Stream source);
}
