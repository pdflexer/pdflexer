using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer.Legacy
{
    [Obsolete]
    public interface IStreamedParser<T> where T : IPdfObject
    {
        /// <summary>
        /// Parses PDF object of type T from a portion of the provided sequence.
        /// </summary>
        /// <param name="data">Buffer to read from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Number of bytes used.</param>
        /// <returns>Parsed object</returns>
        T Parse(ReadOnlySpan<byte> data, int start, out int length);
        /// <summary>
        /// Parses PDF object of type T from a portion of the provided sequence.
        /// </summary>
        /// <param name="sequence">Sequence to parse from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of object</param>
        /// <returns>Parsed object</returns>
        T Parse(PipeReader reader);
        /// <summary>
        /// Parses PDF object of type T from a portion of the provided sequence.
        /// </summary>
        /// <param name="sequence">Sequence to parse from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of object</param>
        /// <returns>Parsed object</returns>
        ValueTask<T> ParseAsync(PipeReader reader, CancellationToken cancellationToken = default);
    }
}
