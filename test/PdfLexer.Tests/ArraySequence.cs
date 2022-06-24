using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer.Tests
{
    // from https://gist.github.com/stevejgordon/3bf2164f4eac80a5d6f718061a62cf90#file-memorysegment-cs
    internal class MemorySegment<T> : ReadOnlySequenceSegment<T>
    {
        public MemorySegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
        {
            var segment = new MemorySegment<T>(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };

            Next = segment;

            return segment;
        }
    }
}
