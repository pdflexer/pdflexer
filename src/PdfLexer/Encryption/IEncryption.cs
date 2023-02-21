namespace PdfLexer.Encryption;

internal interface IDecryptionHandler
{
    ReadOnlySpan<byte> Decrypt(ParsingContext ctx, ulong id, CryptoType type, ReadOnlySpan<byte> data, Span<byte> writeBuffer);
    Stream Decrypt(ParsingContext ctx, ulong id, CryptoType type, Stream data);
    Stream DecryptCryptStream(ParsingContext ctx, ulong id, PdfDictionary? decodeParams, Stream data);
}

public enum CryptoType
{
    Strings,
    Streams,
    Embedded
}