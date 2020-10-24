using System.Text;
using PdfLexer.Objects.Parsers;
using Xunit;

namespace PdfLexer.Objects.Tests
{
    public class BoolTests
    {
        [InlineData("false", true, false, 5)]
        [InlineData("true ", true, true, 4)]
        [InlineData("true  ", true, true, 4)]
        [InlineData("fals", false, true, 4)]
        [InlineData("tru", false, true, 4)]
        [Theory]
        public void It_Gets_Bool(string input, bool succeeded, bool value, int bytesUsed)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var gotit = BoolParser.GetBool(bytes, out bool result, out int used);
            Assert.Equal(succeeded, gotit);
            if (succeeded)
            {
                Assert.Equal(value, result);
                Assert.Equal(bytesUsed, used);
            }
        }
    }
}