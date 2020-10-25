using PdfLexer.Objects.Types;

namespace PdfLexer.Objects.Lazy
{
    public struct PdfLazyObject
    {
        public PdfLazyObject(PdfObjectType type, int offset, int length)
        {
            Type = type;
            Offset = offset;
            Length = length;
        }
        public PdfObjectType Type { get; }
        public int Offset { get; }
        public int Length { get; }
    }

    public static class LazyObjectExts {
        public static T Resolve<T>(this PdfLazyObject obj) where T : IPdfObject
        {
            return default(T);
        }
    }
}