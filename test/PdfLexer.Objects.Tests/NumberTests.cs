using System.Text;
using PdfLexer.Objects.Parsers;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class NumberTests
    {
        [InlineData("-1", -1)]
        [InlineData("+1", 1)]
        [InlineData("100", 100)]
        [InlineData("0", 0)]
        [Theory]
        public void It_Parses_Ints(string input, int value)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var val = NumberParser.ParseInt(bytes);
            Assert.Equal(value, val);
        }

        [InlineData("34.5", 34.5)]
        [InlineData("-3.62", -3.62)]
        [InlineData("+123.6", 123.6)]
        [InlineData("4.", 4.0)]
        [InlineData("-.002", -.002)]
        [InlineData("0.0", 0.0)]
        [Theory]
        public void It_Parses_Floats(string input, float value)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var val = NumberParser.ParseReal(bytes);
            Assert.Equal(value, val);
        }
    }
}