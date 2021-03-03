using System.Buffers;
using System.IO;
using System.Text;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class BoolTests
    {
        [InlineData("false", false, 0, 5)]
        [InlineData(" false ", false, 1, 5)]
        [InlineData("true ", true, 0, 4)]
        [InlineData(" true ", true, 1, 4)]
        [InlineData("true  ", true, 0, 4)]

        [Theory]
        public void It_Gets_Bool_Span(string input, bool value, int start, int length)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var s = PdfSpanLexer.TryReadNextToken(bytes, out PdfTokenType type, out int l);
            Assert.Equal(PdfTokenType.BooleanObj, type);
            Assert.True(start > -1);
            Assert.Equal(length, l);
            var parser = new BoolParser();

            var result = parser.Parse(bytes, start, length);
            Assert.Equal(value, result.Value);

            var serializer = new BoolSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(result, ms);
            Assert.Equal(input.Substring(start, length), Encoding.ASCII.GetString(ms.ToArray()));
        }
    }
}