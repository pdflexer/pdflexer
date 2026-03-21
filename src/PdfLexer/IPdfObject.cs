using System;
using System.Runtime.CompilerServices;

namespace PdfLexer;


/// <summary>
/// Interface implemented by all PdfObjects parsed from Pdfs.
/// </summary>
public interface IPdfObject
{
    /// <summary>
    /// The underlying type of this Pdf Object.
    /// Note: this may be <see cref="PdfObjectType.IndirectRefObj"/>.
    /// Use IPdfObject.GetPdfObjType() to always return the
    /// direct object type.
    /// </summary>
    public PdfObjectType Type { get; }
    /// <summary>
    /// Signifies if this pdf object has been lazy parsed.
    /// Not needed by library users if access to objects
    /// is performed through IPdfObject.GetAs<T>()
    /// </summary>
    public bool IsLazy { get; }
    /// <summary>
    /// Signifies if this object has been modified.
    /// </summary>
    public bool IsModified { get; }
}

public abstract class PdfObject : IPdfObject
{
    public abstract PdfObjectType Type { get; }
    public virtual bool IsLazy { get => false; }
    public virtual bool IsModified { get => false; }
}

public static class PdfObjectExtensions
{
    /// <summary>
    /// Gets the pdf object type of the direct object. This will
    /// return the type of the referenced object if <see cref="item"/>
    /// is an <see cref="PdfObjectType.IndirectRefObj"/>.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static PdfObjectType GetPdfObjType(this IPdfObject item)
    {
        item = item.Resolve();
        return item.Type;
    }

    /// <summary>
    /// Returns the underlying PdfObject type.
    /// If <see cref="item"/> is an indirect reference
    /// the direct object is returned.
    /// </summary>
    /// <typeparam name="T">Type of PdfObject</typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="PdfLexerObjectMismatchException">Excetion if <see cref="item"/> is not of type <see cref="T"/></exception>
    [Obsolete("Use GetAs<T>() for object-level typed access.")]
    public static T GetValue<T>(this IPdfObject item) where T : IPdfObject => item.GetAs<T>();

    /// <summary>
    /// Returns the underlying PdfObject type.
    /// If <see cref="item"/> is an indirect reference
    /// the direct object is returned.
    /// </summary>
    /// <typeparam name="T">Type of PdfObject</typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="PdfLexerObjectMismatchException">Excetion if <see cref="item"/> is not of type <see cref="T"/></exception>
    [Obsolete("Use GetAsOrNull<T>() for object-level optional typed access.")]
    public static T? GetValueOrNull<T>(this IPdfObject item) where T : IPdfObject => item.GetAsOrNull<T>();

    /// <summary>
    /// Resolves the current object if needed and returns it as <typeparamref name="T"/>.
    /// Use this for object-level typed access after you already have an <see cref="IPdfObject"/>.
    /// </summary>
    /// <typeparam name="T">Expected direct PDF object type.</typeparam>
    /// <param name="item">The current PDF object.</param>
    /// <returns>The resolved object cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="PdfLexerObjectMismatchException">
    /// Thrown when the resolved object is not of type <typeparamref name="T"/>.
    /// </exception>
    public static T GetAs<T>(this IPdfObject item) where T : IPdfObject
    {
        item = item.Resolve();

        if (item is T retyped)
        {
            return retyped;
        }
        throw new PdfLexerObjectMismatchException($"Mismatched data type requested, got {item.Type} expected {typeof(T)}");
    }

    /// <summary>
    /// Resolves the current object if needed and returns it as <typeparamref name="T"/>,
    /// or <see langword="null"/> when the resolved object is not of that type.
    /// Use this for optional object-level typed access after you already have an <see cref="IPdfObject"/>.
    /// </summary>
    /// <typeparam name="T">Expected direct PDF object type.</typeparam>
    /// <param name="item">The current PDF object.</param>
    /// <returns>
    /// The resolved object cast to <typeparamref name="T"/>, or <see langword="null"/>
    /// if the resolved object is not of that type.
    /// </returns>
    public static T? GetAsOrNull<T>(this IPdfObject item) where T : IPdfObject
    {
        item = item.Resolve();

        if (item is T retyped)
        {
            return retyped;
        }
        return default(T);
    }

    public static IPdfObject Resolve(this IPdfObject item)
    {
        if (item.IsLazy)
        {
            var lz = (PdfLazyObject)item;
            return lz.Resolve();
        }

        if (item.Type == PdfObjectType.IndirectRefObj)
        {
            var ir = (PdfIndirectRef)item;
            return ir.GetObject();
        }
        return item;
    }


    public static PdfIndirectRef Indirect(this IPdfObject? item)
    {
        if (item == null) { return PdfIndirectRef.Create(PdfNull.Value); }
        if (item.Type == PdfObjectType.IndirectRefObj)
        {
            return (PdfIndirectRef)item;
        }
        return PdfIndirectRef.Create(item);
    }
}
