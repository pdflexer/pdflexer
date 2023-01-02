using PdfLexer.Content;
using PdfLexer.IO;
using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using System.Buffers;

namespace PdfLexer;

/// <summary>
/// Pdf stream object
/// </summary>
public class PdfStream : PdfObject
{
    public PdfStream()
    {
        Dictionary = new PdfDictionary();
        _contents = PdfStreamContents.Empty;
        streamModified = true;
    }

    public PdfStream(PdfStreamContents contents)
    {
        Dictionary = new PdfDictionary();
        _contents = contents;
        streamModified = true;
    }

    public PdfStream(PdfDictionary dictionary, PdfStreamContents contents)
    {
        Dictionary = dictionary;
        _contents = contents;
        streamModified = true;
    }
    /// <summary>
    /// Dictionary portion of the Stream object.
    /// </summary>
    public PdfDictionary Dictionary { get; }
    /// <summary>
    /// Stream portion of the Pdf Object
    /// </summary>
    public PdfStreamContents Contents
    {
        get => _contents;
        set
        {
            streamModified = true;
            _contents = value;
        }
    }
    internal bool streamModified { get; set; }
    internal PdfStreamContents _contents { get; set; }
    public override PdfObjectType Type => PdfObjectType.StreamObj;
    public override bool IsModified => Dictionary.IsModified || streamModified;

    // /Length required
    // /Filter /DecodeParms -> if filters
    // /F, /FFilter, /FDecodeParms -> external file

    /// <summary>
    /// clones stream / dict shallow
    /// </summary>
    /// <returns></returns>
    public PdfStream CloneShallow()
    {
        return new PdfStream(Dictionary.CloneShallow(), _contents);
    }
}

/// <summary>
/// Contents of a Pdf stream.
/// </summary>
public abstract class PdfStreamContents
{
    public static PdfStreamContents Empty { get; } = new PdfByteArrayStreamContents(Array.Empty<byte>());
    /// <summary>
    /// Reads data of the stream.
    /// </summary>
    /// <param name="destination"></param>
    public abstract Stream GetEncodedData();
    /// <summary>
    /// Copies contents to the provided stream.
    /// </summary>
    /// <param name="destination"></param>
    public abstract void CopyEncodedData(Stream destination);
    /// <summary>
    /// Length of the stream (compressed, if applicable).
    /// </summary>
    public abstract int Length { get; }
    /// <summary>
    /// Filter entry for Dict.
    /// </summary>
    internal IPdfObject? Filters { get; set; }
    /// <summary>
    /// DecodeParms entry for Dict.
    /// </summary>
    internal IPdfObject? DecodeParams { get; set; }
    /// <summary>
    /// Decoded data cache
    /// </summary>
    internal byte[]? DecodedData { get; private set; }

    internal ParsingContext? Context { get; set; }
    /// <summary>
    /// Updates the stream dictionary with this streams filter information
    /// </summary>
    /// <param name="dict"></param>
    internal virtual void UpdateStreamDictionary(PdfDictionary dict)
    {
        dict.Remove(PdfName.DecodeParms);
        dict.Remove(PdfName.Filter);
        if (DecodeParams != null)
        {
            dict[PdfName.DecodeParms] = DecodeParams;
        }
        if (Filters != null)
        {
            dict[PdfName.Filter] = Filters;
        }
        dict[PdfName.Length] = new PdfIntNumber(Length);
    }

    public DecodedStreamContents GetDecodedBuffer() => GetDecodedBuffer(true);

    /// <summary>
    /// Gets the decoded data for this stream.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    internal DecodedStreamContents GetDecodedBuffer(bool cache=false)
    {
        if (cache && StreamBufferCache.StaticCache.Value != null)
        {
            return StreamBufferCache.StaticCache.Value.GetOrDecodeBuffer(this);
        }
        if (DecodedData != null)
        {
            return new ArrayContents(DecodedData);
        }

        // TODO make this smarter
        var size = Math.Min(1_048_000, Length * 15);
        var rented = ArrayPool<byte>.Shared.Rent(size);
        using var str = GetDecodedStream();
        var l = str.Fill(rented);
        if (l == rented.Length)
        {
            // slow fallback
            var ms = new MemoryStream();
            str.CopyTo(ms);
            var totalLength = l + (int)ms.Length;
            var second = ArrayPool<byte>.Shared.Rent(totalLength);
            Buffer.BlockCopy(rented, 0, second, 0, rented.Length);
            ArrayPool<byte>.Shared.Return(rented);
            ms.Position = 0;
            ms.TryFillArray(second, (int) ms.Length, l);
            return new RentedArrayContents(second, totalLength);
        }
        return new RentedArrayContents(rented, l);
    }


    /// <summary>
    /// Gets the decoded data for this stream.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public byte[] GetDecodedData()
    {
        if (DecodedData != null)
        {
            return DecodedData;
        }
        using var stream = GetDecodedStream();
        if (stream is MemoryStream ms)
        {
            DecodedData = ms.ToArray();
        }
        else
        {
            if (stream.CanSeek)
            {
                DecodedData = new byte[stream.Length];
                FillData(stream);
            }
            else
            {
                // don't know length, have to copy
                using var copy = Context?.GetTemporaryStream() ?? new MemoryStream();
                stream.CopyTo(copy);
                DecodedData = new byte[copy.Length];
                copy.Seek(0, SeekOrigin.Begin);
                FillData(copy);
            }
        }

        return DecodedData;

        void FillData(Stream str)
        {
            int pos = 0;
            int read;
            while ((read = str.Read(DecodedData, pos, (int)str.Length - pos)) > 0)
            {
                pos += read;
            }
        }
    }
    /// <summary>
    /// Gets the decoded data as a stream.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public Stream GetDecodedStream()
    {
        if (DecodedData != null)
        {
            return new MemoryStream(DecodedData);
        }

        var source = GetEncodedData();

        if (Filters == null)
        {
            if (Context?.IsEncrypted ?? false)
            {
                source = Context.Decryption.Decrypt(Context.CurrentReference, Encryption.CryptoType.Streams, source);
            }
            return source;
        }

        var obj = Filters.Resolve();
        var parms = DecodeParams?.Resolve();
        if (obj.Type == PdfObjectType.ArrayObj)
        {
            var arr = obj.GetValue<PdfArray>();

            // decrypt only if no crypt filter
            if (!arr.Any(x=> x.GetAsOrNull<PdfName>() == "/Crypt"))
            {
                if (Context?.IsEncrypted ?? false)
                {
                    source = Context.Decryption.Decrypt(Context.CurrentReference, Encryption.CryptoType.Streams, source);
                }
            }

            var parmArray = parms?.GetValue<PdfArray>();
            for (var i = 0; i < arr.Count; i++)
            {
                var filter = arr[i].GetValue<PdfName>();
                var dict = parmArray != null && parmArray.Count > i ? parmArray[i] : null;
                source = DecodeSingle(filter, source,
                    dict?.Type == PdfObjectType.NullObj ? null : dict?.GetValue<PdfDictionary>());
            }
            return source;
        }
        else
        {
            var filter = obj.GetValue<PdfName>();
            if (filter != "/Crypt" && (Context?.IsEncrypted ?? false))
            {
                source = Context.Decryption.Decrypt(Context.CurrentReference, Encryption.CryptoType.Streams, source);
            }

            PdfDictionary? currentParms = null;

            switch (DecodeParams?.Type)
            {
                case PdfObjectType.DictionaryObj:
                    currentParms = DecodeParams.GetValue<PdfDictionary>();
                    break;
                case PdfObjectType.ArrayObj:
                    var arr = DecodeParams.GetValue<PdfArray>();
                    if (arr.Resolve().Type == PdfObjectType.DictionaryObj)
                    {
                        currentParms = arr.GetValue<PdfDictionary>();
                    }
                    break;
            }
            return DecodeSingle(filter, source, currentParms ?? new PdfDictionary());
        }

        Stream DecodeSingle(PdfName filterName, Stream input, PdfDictionary? decodeParams)
        {
            var decode = ParsingContext.GetDecoder(filterName);
            if (Context != null)
            {
                return decode.Decode(input, decodeParams, Context.Error);
            } else
            {
                return decode.Decode(input, decodeParams);
            }
        }
    }
}

/// <summary>
/// Contents of a Pdf stream.
/// </summary>
internal class PdfExistingStreamContents : PdfStreamContents
{
    internal IPdfDataSource Source { get; }
    internal long Offset { get; }
    public PdfExistingStreamContents(IPdfDataSource source, long offset, int length)
    {
        Source = source;
        Context = source.Context;
        Offset = offset;
        Length = length;
    }

    public override int Length { get; }
    /// <summary>
    /// Copies contents to the provided stream.
    /// </summary>
    /// <param name="destination"></param>
    public override void CopyEncodedData(Stream destination)
    {
        if (Source.Context.IsEncrypted)
        {
            throw new NotSupportedException("Pdf encryption is not supported for copying.");
        }
        Source.CopyData(Offset, Length, destination);
    }

    public override Stream GetEncodedData()
    {
        return Source.GetDataAsStream(Offset, Length);
    }
}

/// <summary>
/// Contents of a Pdf stream.
/// </summary>
internal class PdfXRefStreamContents : PdfStreamContents
{
    internal IPdfDataSource Source { get; }
    internal XRefEntry XRef { get; }
    public PdfXRefStreamContents(IPdfDataSource source, XRefEntry xref, int predictedLength)
    {
        Source = source;
        XRef = xref;
        Length = predictedLength;
    }

    public override int Length { get; }
    /// <summary>
    /// Copies contents to the provided stream.
    /// </summary>
    /// <param name="destination"></param>
    public override void CopyEncodedData(Stream destination)
    {
        if (Source.Context.IsEncrypted)
        {
            throw new NotSupportedException("Pdf encryption is not supported.");
        }
        using var str = Source.Context.GetStreamOfContents(XRef, CommonUtil.GetFirstFilterFromList(Filters), Length);
        str.CopyTo(destination);
    }

    public override Stream GetEncodedData()
    {
        return Source.Context.GetStreamOfContents(XRef, CommonUtil.GetFirstFilterFromList(Filters), Length);
    }
}

/// <summary>
/// Contents of a Pdf stream.
/// </summary>
public class PdfByteArrayStreamContents : PdfStreamContents
{
    public byte[] Contents;
    public PdfByteArrayStreamContents(byte[] contents)
    {
        Contents = contents;
    }

    public PdfByteArrayStreamContents(byte[] contents, PdfName filter, PdfDictionary? decodeParams)
    {
        Contents = contents;
        Filters = new PdfArray { filter };
        if (decodeParams != null)
        {
            DecodeParams = new PdfArray { decodeParams };
        }
    }

    /// <summary>
    /// Length of the stream (compressed, if applicable).
    /// </summary>
    public override int Length => Contents?.Length ?? 0;

    /// <summary>
    /// Copies contents to the provided stream.
    /// </summary>
    /// <param name="destination"></param>
    public override void CopyEncodedData(Stream destination)
    {
        destination.Write(Contents);
    }

    public override Stream GetEncodedData() => new MemoryStream(Contents);
}

/// <summary>
/// Contents of a Pdf stream represented by an external file.
/// TODO
/// </summary>
public class PdfFileStreamContents : PdfStreamContents
{
    internal IPdfObject Specification;
    public PdfFileStreamContents(IPdfObject specification)
    {
        Specification = specification;
    }

    /// <summary>
    /// Length of the stream (compressed, if applicable).
    /// </summary>
    public override int Length => 0;

    /// <summary>
    /// Copies contents to the provided stream.
    /// </summary>
    /// <param name="destination"></param>
    public override void CopyEncodedData(Stream destination)
    {
        throw new NotImplementedException();
    }

    public override Stream GetEncodedData() => throw new NotImplementedException();
}

public abstract class DecodedStreamContents : IDisposable
{
    public abstract ReadOnlySpan<byte> GetData();
    public virtual bool TryGetLexInfo(ParsingContext ctx, [NotNullWhen(true)]out RentedStreamLexInfo? contents) { contents = null; return false; }
    public void Dispose()
    {
        Users -= 1;
        if (Users > 0)
        {
            return;
        }
        DisposeFinal();
    }
    internal int Users = 0;
    public ContentStreamScanner GetScanner(ParsingContext ctx)
    {
        if (TryGetLexInfo(ctx, out var cached))
        {
            return new ContentStreamScanner(ctx, GetData(), cached);
        }
        //var data = GetData();
        //var li = RentedStreamLexInfo.Create(ctx, data);
        return new ContentStreamScanner(ctx, GetData());
    }

    public abstract void DisposeFinal();
}

internal class RentedArrayContents : DecodedStreamContents
{
    private byte[] _data;
    private int _length;
    private RentedStreamLexInfo? _cached;

    public RentedArrayContents(byte[] data, int length)
    {
        _data = data;
        _length = length;
    }
    public override void DisposeFinal()
    {
        if (_data != null)
        {
            ArrayPool<byte>.Shared.Return(_data);
        }
        _data = null!;
        if (_cached != null)
        {
            _cached.Dispose();
        }
        _cached = null;
    }

    public override bool TryGetLexInfo(ParsingContext ctx, [NotNullWhen(true)] out RentedStreamLexInfo? contents)
    {
        if (_cached != null) { contents = _cached;  return true; }
        var data = GetData();
        var span = ContentStreamScanner.FillListWithLexedItems(ctx, data, out var arr);
        _cached = new RentedStreamLexInfo(arr, span.Length);
        contents = _cached;
        return true;
    }

    public override ReadOnlySpan<byte> GetData()
    {
        if (_data == null) { throw new ObjectDisposedException("GetData() called on disposed DecodedStreamContents.");  }
        ReadOnlySpan<byte> spanned = _data;
        return spanned.Slice(0, _length);
    }
}

internal class ArrayContents : DecodedStreamContents
{
    private byte[] _data;

    public ArrayContents(byte[] data)
    {
        _data = data;
    }
    public override void DisposeFinal()
    {
        _data = null!;
    }

    public override ReadOnlySpan<byte> GetData()
    {
        if (_data == null) { throw new ObjectDisposedException("GetData() called on disposed DecodedStreamContents."); }
        return _data;
    }
}

public class StreamBufferCache : IDisposable
{
    internal static AsyncLocal<StreamBufferCache?> StaticCache = new AsyncLocal<StreamBufferCache?>();
    private Dictionary<PdfStreamContents, DecodedStreamContents> cache = new();
    public StreamBufferCache()
    {
        StaticCache.Value = this;
    }

    internal DecodedStreamContents GetOrDecodeBuffer(PdfStreamContents stream)
    {
        if (cache.TryGetValue(stream, out var decoded))
        {
            decoded.Users += 1;
            return decoded;
        }

        var buffer = stream.GetDecodedBuffer(false);
        cache[stream] = buffer;
        buffer.Users = 2; // cache and requestor
        return buffer;
    }

    public void Clear()
    {
        Dispose();
    }

    public void Dispose()
    {
        foreach (var item in cache.Values)
        {
            item.Dispose();
        }
        cache.Clear();
        StaticCache.Value = null;
    }
}