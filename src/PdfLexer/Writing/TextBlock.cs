using System.Numerics;

namespace PdfLexer.Writing;

internal class TextBlock<T> where T : struct, IFloatingPoint<T>
{
    public required byte[] Op { get; set; }
    public required T Width { get; set; }
}
