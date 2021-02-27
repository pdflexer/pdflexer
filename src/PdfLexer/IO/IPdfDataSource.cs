using PdfLexer.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PdfLexer.IO
{
    public interface IPdfDataSource
    {
        /// <summary>
        /// Total bytes contained in the PDF data source
        /// </summary>
        long TotalBytes { get; }
        /// <summary>
        /// If the buffer returned by <see cref="FillData"/> contains all data till end of
        /// data source. Optimization. 
        /// </summary>
        bool ReturnsCompleteData { get; }
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
        /// Reader MUST NOT dispose stream.
        /// </summary>
        /// <param name="startPosition">Position of stream in current data source</param>
        /// <returns>Stream</returns>
        Stream GetStream(long startPosition);
        /// <summary>
        /// Fills a buffer with the requested data.
        /// </summary>
        /// <param name="startPosition">Starting position in data source for buffer</param>
        /// <param name="requiredBytes">Number of required bytes.</param>
        /// <param name="buffer">Filled buffer of at least required bytes size.</param>
        void FillData(long startPosition, int requiredBytes, out ReadOnlySpan<byte> buffer);
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
        ParsingContext Context {get;}
    }
}
