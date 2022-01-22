using System;
using System.Threading;
using PdfLexer.Parsers.Structure;

namespace PdfLexer
{
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
        /// is performed through IPdfObject.GetValue<T>()
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
        public static T GetValue<T>(this IPdfObject item) where T : IPdfObject
        {
            item = item.Resolve();

            if (item is T retyped)
            {
                return retyped;
            }

            throw new PdfLexerObjectMismatchException($"Mismatched data type requested, got {item.Type} expected {typeof(T)}");
        }

        /// <summary>
        /// ADVANCED USAGE
        /// MAY BECOME INTERNAL
        /// Resolves a lazy or indirect object to the parsed direct object.
        /// If object is not lazy or indirect, the object is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
    }
}
