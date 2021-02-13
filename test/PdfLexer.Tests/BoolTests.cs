using System.Buffers;
using System.Text;
using PdfLexer.Parsers;
using Xunit;

namespace PdfLexer.Tests
{
    public class BoolTests
    {
        [InlineData("false", true, false, 5)]
        [InlineData(" false ", true, false, 5)]
        [InlineData("true ", true, true, 4)]
        [InlineData(" true ", true, true, 4)]
        [InlineData("true  ", true, true, 4)]
        [InlineData("fals", false, true, 4)]
        [InlineData("tru", false, true, 4)]
        [Theory]
        public void It_Gets_Bool_Span(string input, bool succeeded, bool value, int bytesUsed)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var start = PdfSpanLexer.TryReadNextToken(bytes, out PdfTokenType type, out int length);
            if (!succeeded)
            {
                Assert.Equal(-1, start);
                return;
            }
            Assert.Equal(PdfTokenType.BooleanObj, type);
            Assert.True(start > -1);
            Assert.Equal(bytesUsed, length);
            var parser = new BoolParser();
            var result = parser.Parse(bytes, start, length);
            Assert.Equal(value, result.Value);
        }

#if NET50
        [InlineData("false", true, false, 5)]
        [InlineData(" false ", true, false, 5)]
        [InlineData("true ", true, true, 4)]
        [InlineData(" true ", true, true, 4)]
        [InlineData("true  ", true, true, 4)]
        [InlineData("fals", false, true, 4)]
        [InlineData("tru", false, true, 4)]
        [Theory]
        public void It_Gets_Bool_Sequence(string input, bool succeeded, bool value, int bytesUsed)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            var read = reader.TryReadNextToken(succeeded, out var type, out var start);
            if (!succeeded)
            {
                Assert.False(read);
                return;
            }

            Assert.Equal(PdfTokenType.BooleanObj, type);
            var length = seq.GetOffset(reader.Position) - seq.GetOffset(start);

            Assert.True(read);
            Assert.Equal(bytesUsed, length);
            var parser = new BoolParser();
            var data = seq.Slice(start, reader.Position);
            var result = parser.Parse(ref data);
            Assert.Equal(value, result.Value);
        }
#endif
    }
}