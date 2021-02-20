using System;
using System.Buffers;
using System.IO;
using System.Text;
using PdfLexer.Lexing;
using PdfLexer.Parsers;
using PdfLexer.Serializers;
using Xunit;

namespace PdfLexer.Tests
{
    public class NameTests
    {
#if NET50
        [InlineData("/Test ", "/Test", -1)]
        [InlineData("/Test  ", "/Test", -1)]
        [InlineData(" /Test", "/Test", 5)]
        [InlineData("/Test\r", "/Test", 5)]
        [InlineData("/Test\n", "/Test", 5)]
        [InlineData("/Test\r\n", "/Test", 5)]
        [InlineData("/Test#20Test ", "/Test Test", -1)]
        [InlineData("/PANTONE#205757#20CV ", "/PANTONE 5757 CV", -1)]
        [InlineData("/paired#28#29parentheses ", "/paired()parentheses", -1)]
        [InlineData("/The_Key_of_F#23_Minor ", "/The_Key_of_F#_Minor", -1)]
        [InlineData("/A#42 ", "/AB", -1)]
        [Theory]
        public void It_Gets_Name_Sequence(string input, string output, int length)
        {
            if (length == -1) { length = input.Trim().Length; }
            var bytes = Encoding.ASCII.GetBytes(input);
            var seq = new ReadOnlySequence<byte>(bytes);
            var reader = new SequenceReader<byte>(seq);
            var read = reader.TryReadNextToken(true, out var type, out var start);
            Assert.True(read);

            Assert.Equal(PdfTokenType.NameObj, type);

            var measured = seq.GetOffset(reader.Position) - seq.GetOffset(start);
            Assert.Equal(length, measured);
            var parser = new NameParser();
            var sliced = seq.Slice(start, reader.Position);
            var result = parser.Parse(in sliced);
            Assert.Equal(output, result.Value);
        }

#endif

        [InlineData("/Test ", "/Test", 5)]
        [InlineData("/Test  ", "/Test", 5)]
        [InlineData(" /Test", "/Test", 5)]
        [InlineData("/Test\r", "/Test", 5)]
        [InlineData("/Test\n", "/Test", 5)]
        [InlineData("/Test\r\n", "/Test", 5)]
        [InlineData("/Test#20Test ", "/Test Test", 12)]
        [InlineData("/PANTONE#205757#20CV ", "/PANTONE 5757 CV", -1)]
        [InlineData("/paired#28#29parentheses ", "/paired()parentheses", -1)]
        [InlineData("/The_Key_of_F#23_Minor ", "/The_Key_of_F#_Minor", -1)]
        [InlineData("/A#42 ", "/AB", -1)]
        [Theory]
        public void It_Gets_Name_Span(string input, string output, int length)
        {
            if (length == -1) { length = input.Trim().Length; }
            var bytes = Encoding.ASCII.GetBytes(input);
            var start = PdfSpanLexer.TryReadNextToken(bytes, out PdfTokenType type, out int measured);
            Assert.True(start > -1);
            Assert.Equal(PdfTokenType.NameObj, type);

            Assert.Equal(length, measured);
            var parser = new NameParser();
            var result = parser.Parse(new ReadOnlySpan<byte>(bytes).Slice(start, measured));
            Assert.Equal(output, result.Value);
        }

        [InlineData("/Test", "/Test")]
        [InlineData("/Test#20Test", "/Test Test")]
        [InlineData("/PANTONE#205757#20CV", "/PANTONE 5757 CV")]
        [InlineData("/paired#28#29parentheses", "/paired()parentheses")]
        [InlineData("/The_Key_of_F#23_Minor", "/The_Key_of_F#_Minor")]
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

            return Encoding.ASCII.GetString(data.Slice(0, count));
        }

        private string Serialize_With_Stream(string input)
        {
            var serializer = new NameSerializer();
            var ms = new MemoryStream();
            serializer.WriteToStream(new PdfName(input), ms);

            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }
}