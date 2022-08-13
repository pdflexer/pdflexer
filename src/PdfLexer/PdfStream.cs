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
    }

    public PdfStream(PdfStreamContents contents)
    {
        Dictionary = new PdfDictionary();
        Contents = contents;
    }

    public PdfStream(PdfDictionary dictionary, PdfStreamContents contents)
    {
        Dictionary = dictionary;
        Contents = contents;
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
}

/// <summary>
/// Contents of a Pdf stream.
/// </summary>
public abstract class PdfStreamContents
{
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

    /// <summary>
    /// Gets the decoded data for this stream.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    internal DecodedStreamContents GetDecodedBuffer()
    {
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
            DecodedData = new byte[l + ms.Length];
            Buffer.BlockCopy(rented, 0, DecodedData, 0, rented.Length);
            int pos = rented.Length;
            int toRead = (int)ms.Length;
            ms.Seek(0, SeekOrigin.Begin);
            int total = 0;
            int read;
            while ((read = ms.Read(DecodedData, pos, toRead - total)) > 0)
            {
                total += read;
            }
            ArrayPool<byte>.Shared.Return(rented);
            return new ArrayContents(DecodedData);
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
        if (Context?.IsEncrypted ?? false)
        {
            throw new NotSupportedException("Pdf encryption is not supported.");
        }
        if (DecodedData != null)
        {
            return new MemoryStream(DecodedData);
        }
        if (Filters == null)
        {
            return GetEncodedData();
        }

        var source = GetEncodedData();


        var obj = Filters.Resolve();
        var parms = DecodeParams?.Resolve();
        if (obj.Type == PdfObjectType.ArrayObj)
        {
            var arr = obj.GetValue<PdfArray>();
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
            throw new NotSupportedException("Pdf encryption is not supported.");
        }
        Source.CopyData(Offset, Length, destination);
    }

    public override Stream GetEncodedData()
    {
        if (Source.Context.IsEncrypted)
        {
            throw new NotSupportedException("Pdf encryption is not supported.");
        }
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
        if (Source.Context.IsEncrypted)
        {
            throw new NotSupportedException("Pdf encryption is not supported.");
        }
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

    public PdfByteArrayStreamContents(byte[] contents, PdfName filter, PdfDictionary decodeParams)
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
    public abstract void Dispose();
}

internal class RentedArrayContents : DecodedStreamContents
{
    private byte[] _data;
    private int _length;

    public RentedArrayContents(byte[] data, int length)
    {
        _data = data;
        _length = length;
    }
    public override void Dispose()
    {
        ArrayPool<byte>.Shared.Return(_data);
        _data = null;
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
    public override void Dispose()
    {
        _data = null;
    }

    public override ReadOnlySpan<byte> GetData()
    {
        if (_data == null) { throw new ObjectDisposedException("GetData() called on disposed DecodedStreamContents."); }
        return _data;
    }
}
