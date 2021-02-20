using System;
using System.IO;
using System.Text;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
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
            var parser = new NumberParser(new ParsingContext());
            var result = parser.Parse(bytes);
            var num  = result as PdfIntNumber;
            Assert.NotNull(num);
            Assert.Equal(value, num.Value);
        }

        [InlineData("34.5", 34.5)]
        [InlineData("-3.62", -3.62)]
        [InlineData("+123.6", 123.6)]
        [InlineData("4.", 4.0)]
        [InlineData("-.002", -.002)]
        [InlineData("0.0", 0.0)]
        [Theory]
        public void It_Parses_Floats(string input, decimal value)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var parser = new DecimalParser();
            var result = parser.Parse(bytes);
            var num  = result as PdfDecimalNumber;
            Assert.NotNull(num);
            Assert.Equal(value, num.Value);
        }

        // [InlineData("34.5", 34.5, typeof(PdfDecimalNumber))]
        [InlineData("1000000000000", 1000000000000, typeof(PdfLongNumber))]
        [InlineData("100", 100, typeof(PdfIntNumber))]
        [Theory]
        public void It_Caches_Tokens(string input, decimal value, Type expected)
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            var parser = new NumberParser(new ParsingContext() { CacheNumbers = true });
            var result = parser.Parse(bytes);
            
            Assert.Equal(expected, result.GetType());
            if (result is PdfDecimalNumber dn)
            {
                Assert.Equal(value, dn.Value);
            }
            else if (result is PdfIntNumber integer)
            {
                Assert.Equal(value, integer.Value);
            } else if (result is PdfLongNumber ln)
            {
                Assert.Equal(value, ln.Value);
            }
            
            if (expected == typeof(PdfIntNumber))
            {
                var again = parser.Parse(bytes);
                Assert.True(Object.ReferenceEquals(result, again));
            }
        }

        [InlineData(1, "1")]
        [InlineData(100, "100")]
        [InlineData(-100, "-100")]
        [InlineData(2147483647, "2147483647")]
        [InlineData(-2147483647, "-2147483647")]
        [Theory]
        public void It_Writes_Ints(int number, string expected)
        {
            var output = Serialize_With_Span(number);
            Assert.Equal(expected, output);
            var strOut = Serialize_With_Stream(number);
            Assert.Equal(expected, strOut);
        }

        private string Serialize_With_Span(int input)
        {
            var serializer = new NumberSerializer(new ParsingContext());
            var data = new Span<byte>(new byte[50]);
            var count = serializer.GetBytes(new PdfIntNumber(input), data);

            return Encoding.ASCII.GetString(data.Slice(0, count));
        }

        private string Serialize_With_Stream(int input)
        {
            var serializer = new NumberSerializer(new ParsingContext());
            var ms = new MemoryStream();
            serializer.WriteToStream(new PdfIntNumber(input), ms);

            return Encoding.ASCII.GetString(ms.ToArray());
        }

        [InlineData(1.0, "1")]
        [InlineData(-100.00, "-100")]
        [InlineData(-100.01, "-100.01")]
        [InlineData(100.01, "100.01")]

        [Theory]
        public void It_Writes_Decimals(decimal number, string expected)
        {
            var output = Serialize_With_Span(number);
            Assert.Equal(expected, output);
            var strOut = Serialize_With_Stream(number);
            Assert.Equal(expected, strOut);
        }

        private string Serialize_With_Span(decimal input)
        {
            var serializer = new NumberSerializer(new ParsingContext());
            var data = new Span<byte>(new byte[50]);
            var count = serializer.GetBytes(new PdfDecimalNumber(input), data);

            return Encoding.ASCII.GetString(data.Slice(0, count));
        }

        private string Serialize_With_Stream(decimal input)
        {
            var serializer = new NumberSerializer(new ParsingContext());
            var ms = new MemoryStream();
            serializer.WriteToStream(new PdfDecimalNumber(input), ms);

            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }
}