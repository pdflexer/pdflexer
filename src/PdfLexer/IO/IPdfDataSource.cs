using PdfLexer.Parsers;
using PdfLexer.Parsers.Structure;
using PdfLexer.Serializers;

namespace PdfLexer.IO;

public interface IPdfDataSource : IDisposable
{
    bool Disposed { get; }
    /// <summary>
    /// Total bytes contained in the PDF data source
    /// </summary>
    long TotalBytes { get; }
    /// <summary>
    /// Returns data already in memory.
    /// </summary>
    /// <param name="startPosition">Starting position in data source for buffer</param>
    /// <param name="requiredBytes">Number of required bytes.</param>
    /// <param name="buffer">Filled buffer of at least required bytes size.</param>
    void GetData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer);
    Stream GetDataAsStream(long startPosition, int desiredBytes);
    /// <summary>
    /// If the data source supports cloning for concurrent processing.
    /// </summary>
    bool SupportsCloning { get; }
    /// <summary>
    /// Clones the current data source for use in concurrent processing.
    /// If all operations on data source are thread safe, source can return itself.
    /// </summary>
    /// <returns></returns>
    IPdfDataSource Clone();
    /// <summary>
    /// Gets a readable stream.
    /// </summary>
    /// <param name="startPosition">Position of stream in current data source</param>
    /// <returns>Stream</returns>
    Stream GetStream(long startPosition);
    /// <summary>
    /// Copies data from data source to stream.
    /// </summary>
    /// <param name="startPosition">Start position for copy.</param>
    /// <param name="requiredBytes">Number of bytes to copy</param>
    /// <param name="stream">Stream to copy to.</param>
    void CopyData(long startPosition, int requiredBytes, Stream stream);
    /// <summary>
    /// Associated parsing context.
    /// </summary>
    ParsingContext Context { get; }
    /// <summary>
    /// Gets the object for the given xref
    /// </summary>
    /// <param name="xref"></param>
    /// <returns></returns>
    IPdfObject GetIndirectObject(XRefEntry xref);
    /// <summary>
    /// Copies content for the given xref to the destination
    /// </summary>
    /// <param name="xref"></param>
    /// <param name="destination"></param>
    void CopyIndirectObject(XRefEntry xref, WritingContext destination);

}
