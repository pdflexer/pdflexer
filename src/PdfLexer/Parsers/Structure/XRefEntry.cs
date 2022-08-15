using PdfLexer.IO;
using PdfLexer.Serializers;

namespace PdfLexer.Parsers.Structure;

public enum XRefType
{
    Normal,
    Compressed
}

public struct XRef
{
    public XRef(int objectNumber, int generation)
    {
        ObjectNumber = objectNumber;
        Generation = generation;
    }
    public int ObjectNumber { get; internal set; }
    public int Generation { get; internal set; }
    public override int GetHashCode()
    {
        return unchecked(ObjectNumber.GetHashCode() + Generation.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) { return false; }
        if (obj is XRef key)
        {
            return key.ObjectNumber.Equals(ObjectNumber) && key.Generation.Equals(Generation);
        }
        return false;
    }

    public override string ToString()
    {
        return $"{ObjectNumber} {Generation}";
    }

    public ulong GetId() => ((ulong)ObjectNumber << 16) | ((uint)Generation & 0xFFFF);
    public static ulong GetId(int objectNumber, int generation) => ((ulong)objectNumber << 16) | ((uint)generation & 0xFFFF);

    public static bool operator ==(XRef left, XRef right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(XRef left, XRef right)
    {
        return !(left == right);
    }
}

public class XRefEntry
{
    public XRefType Type { get; internal set; }
    public XRef Reference { get; internal set; }
    public bool IsFree { get; internal set; }
    /// <summary>
    /// Offset of the start of object. If <see cref="Type"/> is Compressed, this may be 0 if the 
    /// object stream has not been read.
    /// </summary>
    public long Offset { get; internal set; }
    /// <summary>
    /// Maximum length of the object data.
    /// </summary>
    internal int MaxLength { get; set; }
    /// <summary>
    /// Type of object 
    /// </summary>
    internal PdfTokenType KnownObjType { get; set; } = PdfTokenType.Unknown;
    /// <summary>
    /// Start of obj data from start of xref
    /// </summary>
    internal int KnownObjStart { get; set; }
    /// <summary>
    /// Object length excluding endobj (or stream start)
    /// </summary>
    internal int KnownObjLength { get; set; }
    /// <summary>
    /// Start of stream data offset from start of xref
    /// </summary>
    internal int KnownStreamStart { get; set; }
    /// <summary>
    /// Confirmed lenght of stream data
    /// </summary>
    internal int KnownStreamLength { get; set; }
    /// <summary>
    /// Object number of the object stream the referenced object is contained in.
    /// </summary>
    internal int ObjectStreamNumber { get; set; }
    /// <summary>
    /// Index in the object stream.
    /// </summary>
    internal int ObjectIndex { get; set; }
    /// <summary>
    /// Data source containing the referenced data.
    /// This will be the main document source for uncompressed objects.
    /// For compressed objects it will be a wrapper around the object stream
    /// INTERNAL NOTE: May be null during initial doc parsing
    /// </summary>
    internal IPdfDataSource Source { get; set; } = null!;

    internal WeakReference<IPdfDataSource>? CachedSource { get; set; }
    /// <summary>
    /// Gets the object this entry points to.
    /// </summary>
    /// <returns>IPdfObject</returns>
    internal IPdfObject GetObject() 
    {
        try
        {
            return Source.GetIndirectObject(this);
        }
        catch (PdfLexerException e)
        {
            Source.Context.Error($"XRef offset for {Reference} was not valid: " + e.Message);
            if (!StructuralRepairs.TryRepairXRef(Source.Context, this, out var repaired))
            {
                return PdfNull.Value;
            }
            Source.Context.Error("XRef offset repairs to " + repaired.Offset);
            Source.Context.XRefs[repaired.Reference.GetId()] = repaired;
            return Source.GetIndirectObject(repaired);
        }
    } 
    /// <summary>
    /// Copies the data for the object this XRef points to the provided stream.
    /// Excludes object header and trailer (obj/endobj).
    /// </summary>
    /// <param name="destination">Stream to write to</param>
    internal void CopyUnwrappedData(WritingContext destination) {
        try
        {
            if (CachedSource?.TryGetTarget(out var source) ?? false)
            {
                source.CopyIndirectObject(this, destination);
            } else
            {
                Source.CopyIndirectObject(this, destination);
            }
        }
        catch (PdfLexerException e)
        {
            Source.Context.Error($"XRef offset for {Reference} was not valid: " + e.Message);
            if (!StructuralRepairs.TryRepairXRef(Source.Context, this, out var repaired))
            {
                return; // will be null;
            }
            Source.Context.Error("XRef offset repairs to " + repaired.Offset);
            Source.Context.XRefs[repaired.Reference.GetId()] = repaired;
            Source.CopyIndirectObject(repaired, destination);
        }
    }
}
