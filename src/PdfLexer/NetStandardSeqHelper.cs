using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace PdfLexer
{
    #if !NET50
    public static class NetStandardSeqHelper
    {
        public static long GetOffset(ReadOnlySequence<byte> sequence, SequencePosition position)
        {
            return sequence.Length - sequence.Slice(position).Length;
        }
    }
    #endif
}
