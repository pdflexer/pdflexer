using System.Buffers;
using System.Text;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class DetectionTests
    {
        [InlineData(" (Test) ", 1, 6, PdfTokenType.StringObj)]
        [InlineData("<1111> ", 0, 6, PdfTokenType.StringObj)]
        [InlineData("<", -1, 0, PdfTokenType.NullObj)]
        [InlineData("<<", 0, 2, PdfTokenType.DictionaryStart)]
        [InlineData(" true ", 1, 4, PdfTokenType.BooleanObj)]
        [InlineData("  false ", 2, 5, PdfTokenType.BooleanObj)]
        [InlineData("null", 0, 4, PdfTokenType.NullObj)]
        [InlineData("0", -1, 0, PdfTokenType.NullObj)]
        [InlineData("0", 0, 1, PdfTokenType.NumericObj)]
        [InlineData("1.0", 0, 3, PdfTokenType.NumericObj)]
        [InlineData(" 1.0", 1, 3, PdfTokenType.NumericObj)]
        [InlineData("1 0 R", 0, 1, PdfTokenType.NumericObj)]
        [InlineData(" 1 0 R", 1, 1, PdfTokenType.NumericObj)]
        [InlineData("1 0", 0, 1, PdfTokenType.NumericObj)]
        [InlineData("101 0.1", 0, 3, PdfTokenType.NumericObj)]
        [InlineData("901 10", 0, 3, PdfTokenType.NumericObj)]
        [InlineData("  %901 10", -1, 2, PdfTokenType.NumericObj)]
        [InlineData("  %asdf%\r\n.1 ", 10, 2, PdfTokenType.NumericObj)]
        [InlineData("  %asdf%\n.1 ", 9, 2, PdfTokenType.NumericObj)]
        [InlineData("  %asdf\n(.1 10 R", -1, 0, PdfTokenType.StringObj)]
        [InlineData("[", 0, 1, PdfTokenType.ArrayStart)]
        [InlineData("1<", 0, 1, PdfTokenType.NumericObj)]
        [Theory]
        public void It_Detects_Objects(string input, int startPos, int expectedLength, PdfTokenType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var sequence = new ReadOnlySequence<byte>(bytes);
            var reader = PipeReader.Create(stream);
            var result = PdfTokenizer.TryReadNextToken(sequence, startPos != -1, out var tokenType, out var start, out var length);
            if (startPos > -1)
            {
                Assert.True(result);
                Assert.Equal(type, tokenType);
                Assert.Equal(startPos, start);
                Assert.Equal(expectedLength, length);
            }
            else
            {
                Assert.False(result);
            }
        }
    }
}