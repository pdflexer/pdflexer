using PdfLexer.Parsers.Structure;
using System.Runtime.CompilerServices;

namespace PdfLexer;

/// <summary>
/// PDF reference to a indirect object.
/// </summary>
public abstract class PdfIndirectRef : PdfObject
{
    /// <summary>
    /// Gets the referenced PDF object.
    /// </summary>
    /// <returns>PDF object</returns>
    public virtual IPdfObject GetObject() => GetObject(ParsingContext.Current);
    /// <summary>
    /// Gets the referenced PDF object.
    /// </summary>
    /// <returns>PDF object</returns>
    public abstract IPdfObject GetObject(ParsingContext ctx);
    /// <summary>
    /// Creates a new reference to a PDF object.
    /// </summary>
    /// <param name="obj">Object to point to</param>
    /// <returns>PdfIndirectRef</returns>
    public static PdfIndirectRef Create(IPdfObject obj) => new NewIndirectRef(obj);
    internal virtual bool IsOwned(int sourceId, bool attemptOwnership)
    {
        if (SourceId == 0 && attemptOwnership)
        {
            SourceId = sourceId;
            return true;
        }
        return SourceId == sourceId;
    }

    internal virtual XRef Reference { get; set; }
    internal virtual int SourceId { get; set; }
    internal virtual bool DeferWriting { get; set; }
    public override PdfObjectType Type => PdfObjectType.IndirectRefObj;
}

/// <summary>
/// Indirect reference from an existing document.
/// </summary>
internal class ExistingIndirectRef : PdfIndirectRef
{
    public PdfDocument Document { get; }
    public ExistingIndirectRef(PdfDocument doc, long objectNumber, int generation)
    {

        Document = doc;
        Reference = new XRef 
        {
            ObjectNumber = (int)objectNumber,
            Generation = generation
        };
    }

    public ExistingIndirectRef(PdfDocument doc, XRef reference)
    {
        Document = doc;
        Reference = reference;
    }
    public override IPdfObject GetObject(ParsingContext ctx) 
    {
        if (Object == null)
        {
            Object = ctx.GetIndirectObject(Document, Reference);
        }
        return Object;
    } 
    internal override bool IsOwned(int sourceId, bool attemptOwnership) => SourceId == sourceId;

    internal IPdfObject? Object {get;set;}

    internal override int SourceId { get => Document.DocumentId ; set => throw new PdfLexerException("Updating source id on existing indirect reference is not supported."); }

    public override int GetHashCode()
    {
        return unchecked(SourceId + Reference.Generation + Reference.ObjectNumber).GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not ExistingIndirectRef oir)
        {
            return false;
        }

        return SourceId == oir.SourceId && Reference.Equals(oir.Reference);
    }

    public override string ToString()
    {
        return Reference.ToString();
    }
}


/// <summary>
/// Indirect reference created by the library to be written at a later point.
/// </summary>
internal class NewIndirectRef : PdfIndirectRef
{
    public override IPdfObject GetObject(ParsingContext ctx) => Object;

    public IPdfObject Object { get; internal set; }

    internal bool IsDefined => Reference.ObjectNumber != 0;

    public NewIndirectRef(IPdfObject obj)
    {
        Object = obj;
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(Object);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not NewIndirectRef oor)
        {
            return false;
        }

        if (Object.Equals(oor.Object))
        {
            return true;
        }
        return false;
    }
}

// dummy object representing the R in a indirect reference.
// this is just used to simplify parsing dictionary and arrays
internal class IndirectRefToken : IPdfObject
{
    public IndirectRefToken()
    {

    }
    public static IndirectRefToken Value { get; } = new IndirectRefToken();
    public bool IsIndirect => throw new NotImplementedException();
    public PdfObjectType Type => throw new NotImplementedException();
    public bool IsLazy => throw new NotImplementedException();
    public bool IsModified => throw new NotImplementedException();
}


internal class WrittenIndirectRef : PdfIndirectRef
{
    public WrittenIndirectRef(PdfIndirectRef reference)
    {
        Reference = reference.Reference;
        SourceId = reference.SourceId;
    }

    public override IPdfObject GetObject(ParsingContext ctx)
    {
        throw new NotSupportedException();
    }

    internal override bool IsOwned(int sourceId, bool attemptOwnership) => sourceId == SourceId;
}