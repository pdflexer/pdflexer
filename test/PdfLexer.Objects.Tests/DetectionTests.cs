using System.Text;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class DetectionTests
    {
        [InlineData(" (Test) ", 1, PdfTokenType.StringObj)]
        [InlineData("<1111> ", 0, PdfTokenType.StringObj)]
        [InlineData("<", -1, PdfTokenType.NullObj)]
        [InlineData("<<", 0, PdfTokenType.DictionaryStart)]
        [InlineData(" true ", 1, PdfTokenType.BooleanObj)]
        [InlineData("  false ", 2, PdfTokenType.BooleanObj)]
        [InlineData("null", 0, PdfTokenType.NullObj)]
        [InlineData("0", -1, PdfTokenType.NullObj)]
        [InlineData("1.0", 0, PdfTokenType.NumericObj)]
        [InlineData(" 1.0", 1, PdfTokenType.NumericObj)]
        [InlineData("1 0 R", 0, PdfTokenType.IndirectRef)]
        [InlineData(" 1 0 R", 1, PdfTokenType.IndirectRef)]
        [InlineData("1 0", -1, PdfTokenType.NullObj)]
        [InlineData("101 0.1", 0, PdfTokenType.NumericObj)]
        [InlineData("901 10", -1, PdfTokenType.NullObj)]
        [InlineData("  %901 10", -1, PdfTokenType.NullObj)]
        [InlineData("  %asdf%\r\n.1 ", 10, PdfTokenType.NumericObj)]
        [InlineData("  %asdf%\n.1 ", 9, PdfTokenType.NumericObj)]
        [InlineData("  %asdf\n(.1 10 R", 8, PdfTokenType.StringObj)]
        [InlineData("[", 0, PdfTokenType.ArrayStart)]
        [InlineData("1<", 0, PdfTokenType.NumericObj)]
        [Theory]
        public void It_Detects_Objects(string input, int start, PdfTokenType type)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var pos = CommonUtil.FindNextToken(bytes, out PdfTokenType foundType);
            Assert.Equal(start, pos);
            if (pos > -1)
            {
                Assert.Equal(type, foundType);
            }
        }
    }
}