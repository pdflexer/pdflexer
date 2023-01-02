using System;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Encryption;

internal interface IDecryptionHandler
{
    ReadOnlySpan<byte> Decrypt(ulong id, CryptoType type, ReadOnlySpan<byte> data, Span<byte> writeBuffer);
    Stream Decrypt(ulong id, CryptoType type, Stream data);
    Stream DecryptCryptStream(ulong id, PdfDictionary? decodeParams, Stream data);
}

public enum CryptoType
{
    Strings,
    Streams,
    Embedded
}