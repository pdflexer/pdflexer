using System.Text;
using PdfLexer.Parsers;
using Xunit;

namespace PdfLexer.Tests
{
    public class IndirectTests
    {
        [InlineData("16 0 R", 16, 0)]
        [InlineData("1 1 R", 1, 1)]
        [InlineData("129 16 R", 129, 16)]
        [Theory]
        public void It_Parses_Refs(string input, int objNum, int gen)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var iRef = IndirectParser.ParseIndirectReference(bytes, 0);
            Assert.Equal(iRef.ObjectNumber, objNum);
            Assert.Equal(iRef.Generation, gen);
        }
    }
}