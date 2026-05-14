using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PdfLexer.Lexing;
using Xunit;

namespace PdfLexer.Tests
{
    public class PipeScannerTests
    {
        [Fact]
        public void ReadTo_Finds_Token_Split_Across_Reads()
        {
            var scanner = CreateScanner("abc trailer <<>>", 8);

            var data = scanner.ReadTo(Encoding.ASCII.GetBytes("trailer"));

            Assert.Equal("abc ", Encoding.ASCII.GetString(data.ToArray()));
            Assert.Equal(4, scanner.GetOffset());
            Assert.Equal(PdfTokenType.Trailer, scanner.Peek());
            scanner.SkipCurrent();
            Assert.Equal(PdfTokenType.DictionaryStart, scanner.Peek());
        }

        [Fact]
        public void ReadTo_Finds_Token_After_Multiple_Reads()
        {
            var scanner = CreateScanner("aaaaaaaaaaaaaaaa trailer <<>>", 8);

            var data = scanner.ReadTo(Encoding.ASCII.GetBytes("trailer"));

            Assert.Equal("aaaaaaaaaaaaaaaa ", Encoding.ASCII.GetString(data.ToArray()));
            Assert.Equal(17, scanner.GetOffset());
            Assert.Equal(PdfTokenType.Trailer, scanner.Peek());
        }

        [Fact]
        public void TrySkipToToken_Finds_Token_Split_Across_Reads()
        {
            var scanner = CreateScanner("xxxx endstream tail", 8);

            Assert.True(scanner.TrySkipToToken(Encoding.ASCII.GetBytes("endstream"), 1));

            Assert.Equal(5, scanner.GetOffset());
            Assert.Equal(PdfTokenType.EndStream, scanner.Peek());
        }

        [Fact]
        public void ScanToToken_Clears_Cached_Token()
        {
            var scanner = CreateScanner("123 endstream /Name", 8);
            Assert.Equal(PdfTokenType.NumericObj, scanner.Peek());

            Assert.True(scanner.ScanToToken(Encoding.ASCII.GetBytes("endstream")));
            scanner.SkipCurrent();

            Assert.Equal(13, scanner.GetOffset());
            Assert.Equal(PdfTokenType.NameObj, scanner.Peek());
        }

        [Fact]
        public void ScanToToken_Preserves_Back_Context_When_PrevBuffer_Requested()
        {
            // 14 A's of filler, then "\n1 0 obj BBB" — emulates a repair scan looking
            // for `obj` and needing to back-scan through "1 0 " (the obj header).
            var scanner = CreateScanner("AAAAAAAAAAAAAA\n1 0 obj BBB", 4);

            Assert.True(scanner.ScanToToken(Encoding.ASCII.GetBytes("obj"), 20));
            Assert.Equal(19, scanner.GetOffset());

            // ScanBackTokens(2, 20) must see "1 0" and the newline before it.
            var back = scanner.ScanBackTokens(2, 20);
            Assert.NotEqual(-1, back);
        }

        [Fact]
        public void Read_Throws_When_Length_Exceeds_Data()
        {
            var scanner = CreateScanner("12345", 2);

            PdfLexerException ex = null;
            try
            {
                scanner.Read(10);
            }
            catch (PdfLexerException e)
            {
                ex = e;
            }
            Assert.NotNull(ex);
        }

        private static PipeScanner CreateScanner(string data, int maxChunkSize)
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var stream = new ChunkedReadStream(buffer, maxChunkSize);
            var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(bufferSize: maxChunkSize, minimumReadSize: 1));
            return new PipeScanner(new ParsingContext(), reader);
        }

        private sealed class ChunkedReadStream : MemoryStream
        {
            private readonly int maxChunkSize;

            public ChunkedReadStream(byte[] buffer, int maxChunkSize) : base(buffer)
            {
                this.maxChunkSize = maxChunkSize;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return base.Read(buffer, offset, Math.Min(count, maxChunkSize));
            }

            public override int Read(Span<byte> buffer)
            {
                return base.Read(buffer.Slice(0, Math.Min(buffer.Length, maxChunkSize)));
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                return new ValueTask<int>(base.Read(buffer.Span.Slice(0, Math.Min(buffer.Length, maxChunkSize))));
            }
        }
    }
}
