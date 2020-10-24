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
}