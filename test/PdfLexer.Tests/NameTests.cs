using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class NameTests
    {
        [InlineData("/Test ", "Test", 5)]
        [InlineData("/Test  ", "Test", 5)]
        [InlineData(" /Test", "Test", 5)]
        [InlineData("/Test\r", "Test", 5)]
        [InlineData("/Test\n", "Test", 5)]
        [InlineData("/Test\r\n", "Test", 5)]
        [InlineData("/Test#20Test ", "Test Test", 12)]
        [InlineData("/PANTONE#205757#20CV ", "PANTONE 5757 CV", -1)]
        [InlineData("/paired#28#29parentheses ", "paired()parentheses", -1)]
        [InlineData("/The_Key_of_F#23_Minor ", "The_Key_of_F#_Minor", -1)]
        [InlineData("/A#42 ", "AB", -1)]
        // /ºÚÌå-GBKp-EUC-H
        [Theory]
        public void It_Gets_Name_Span(string input, string output, int length)
        {
            if (length == -1) { length = input.Trim().Length; }
            var bytes = Encoding.ASCII.GetBytes(input);
            var start = PdfSpanLexer.TryReadNextToken(bytes, out PdfTokenType type, out int measured);
            Assert.True(start > -1);
            Assert.Equal(PdfTokenType.NameObj, type);

            Assert.Equal(length, measured);
            var parser = new NameParser(new ParsingContext());
            var result = parser.Parse(new ReadOnlySpan<byte>(bytes).Slice(start, measured));
            Assert.Equal(output, result.Value);
        }

        [Fact]
        public void It_Caches()
        {
            var key = "/Key";
            var key2 = "/yeK";
            var keyBytes = Encoding.ASCII.GetBytes(key);
            var key2Bytes = Encoding.ASCII.GetBytes(key2);
            var parser = new NameParser(new ParsingContext(new ParsingOptions { CacheNames = true }));
            var result = parser.Parse(keyBytes);
            var result2 = parser.Parse(keyBytes);
            Assert.True(Object.ReferenceEquals(result, result2));
            var result3 = parser.Parse(key2Bytes);
            Assert.False(Object.ReferenceEquals(result, result3));
            Assert.Equal(key.Substring(1), result.Value);
            Assert.Equal(key2.Substring(1), result3.Value);
        }

        [Fact]
        public void It_Reads_Non_ASCII()
        {
            var root = PathUtil.GetPathFromSegmentOfCurrent("test");
            var data = File.ReadAllBytes(Path.Combine(root, "raw-name.txt"));
            var parser = new NameParser(new ParsingContext(new ParsingOptions { CacheNames = false }));
            var result = parser.Parse(data);
            var serializer = new NameSerializer();
            Span<byte> buffer = new byte[100];
            var c = serializer.GetBytes(result, buffer);
            Assert.True(data.SequenceEqual(buffer.Slice(0, c).ToArray()));
        }

        [InlineData("/Test", "Test")]
        [InlineData("/Test#20Test", "Test Test")]
        [InlineData("/PANTONE#205757#20CV", "PANTONE 5757 CV")]
        [InlineData("/paired#28#29parentheses", "paired()parentheses")]
        [InlineData("/The_Key_of_F#23_Minor", "The_Key_of_F#_Minor")]
        [InlineData("/#23", "#")]
        [InlineData("/Empty", "")]
        [Theory]
        public void It_Writes_Name(string expected, string input)
        {
            var output = Serialize_With_Span(input);
            Assert.Equal(expected, output);
            var strOut = Serialize_With_Stream(input);
            Assert.Equal(expected, strOut);
        }

        private string Serialize_With_Span(string input)
        {
            var serializer = new NameSerializer();
            var data = new Span<byte>(new byte[50]);
            var count = serializer.GetBytes(new PdfName(input), data);

            return Encoding.UTF8.GetString(data.Slice(0, count));
        }

        private string Serialize_With_Stream(string input)
        {
            var serializer = new NameSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(new PdfName(input), ms);

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}