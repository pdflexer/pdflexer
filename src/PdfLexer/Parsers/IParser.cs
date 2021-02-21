using PdfLexer.IO;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace PdfLexer.Parsers
{
    public abstract class Parser<T> : IParser<T> where T : IPdfObject
    {
        public abstract T Parse(ReadOnlySpan<byte> buffer);

        public virtual T Parse(ReadOnlySpan<byte> buffer, int start, int length)
            => Parse(buffer.Slice(start, length));

        public virtual T Parse(in ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                return Parse(sequence.FirstSpan);
            }

            var len = (int)sequence.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(len);
            sequence.CopyTo(buffer);
            Span<byte> buff = buffer;
            var result = Parse(buff.Slice(0,len));
            ArrayPool<byte>.Shared.Return(buffer);
            return result;
        }

        public virtual T Parse(in ReadOnlySequence<byte> sequence, long start, int length) =>
            Parse(sequence.Slice(start, length));
    }
    public interface IParser<out T> where T : IPdfObject
    {
        /// <summary>
        /// Parses PDF object of type T from the provided buffer.
        /// </summary>
        /// <param name="buffer">Buffer to parse from.</param>
        /// <returns>Parsed object.</returns>
        T Parse(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Parses PDF object of type T from a portion of the provided buffer.
        /// </summary>
        /// <param name="buffer">Buffer to parse from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of object</param>
        /// <returns>Parsed object.</returns>
        T Parse(ReadOnlySpan<byte> buffer, int start, int length);

        /// <summary>
        /// Parses PDF object of type T from the provided sequence.
        /// </summary>
        /// <param name="sequence">Sequence to parse from</param>
        /// <returns>Parsed object</returns>
        T Parse(in ReadOnlySequence<byte> sequence);

        /// <summary>
        /// Parses PDF object of type T from a portion of the provided sequence.
        /// </summary>
        /// <param name="sequence">Sequence to parse from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of object</param>
        /// <returns>Parsed object</returns>
        T Parse(in ReadOnlySequence<byte> sequence, long start, int length);
    }

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
